namespace g
{
    // Last update: 6/28/2024 7:11 PM

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Program
    {
        public static void Main()
        {
            Game.Start();
        }
    }

    public static class GameEvents
    {
        public static event Action OnPlayerSkillGain;
        public static void RaisePlayerSkillGain() => OnPlayerSkillGain?.Invoke();
    }

    // Game
    public static class Game
    {
        public static Player Player { get; set; } = new Player();
        public static Room CurrentRoom { get; set; }

        public static void PrintTitle()
        {
            string text = $"\n\n\t{TH.SetColor(Colors.DarkCyan)} _______  _______  _______ _________ _______  _        _______  {TH.SetColor()}    __     \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}(  ____ )(  ___  )(  ____ )\\__   __/(  ___  )( \\      (  ____ \\{TH.SetColor()}    /__\\    \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}| (    )|| (   ) || (    )|   ) (   | (   ) || (      | (    \\/{TH.SetColor()}   ( \\/ )   \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}| (____)|| |   | || (____)|   | |   | (___) || |      | (_____ {TH.SetColor()}    \\  /    \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}|  _____)| |   | ||     __)   | |   |  ___  || |      (_____  ) {TH.SetColor()}   /  \\/\\  \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}| (      | |   | || (\\ (      | |   | (   ) || |            ) | {TH.SetColor()}  / /\\  /  \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}| )      | (___) || ) \\ \\__   | |   | )   ( || (____/\\/\\____) |{TH.SetColor()}  (  \\/  \\  \r\n" +
                            $"\t{TH.SetColor(Colors.DarkCyan)}|/       (_______)|/   \\__/   )_(   |/     \\|(_______/\\_______) {TH.SetColor()}  \\___/\\/  \r\n" +
                            $"\t{TH.SetColor(Colors.DarkRed)} _______  _______  _        _______ _________ _______  _______  _______    \r\n" +
                            $"\t(       )(  ___  )( (    /|(  ____ \\\\__   __/(  ____ \\(  ____ )(  ____ \\   \r\n" +
                            $"\t| () () || (   ) ||  \\  ( || (    \\/   ) (   | (    \\/| (    )|| (    \\/   \r\n" +
                            $"\t| || || || |   | ||   \\ | || (_____    | |   | (__    | (____)|| (_____    \r\n" +
                            $"\t| |(_)| || |   | || (\\ \\) |(_____  )   | |   |  __)   |     __)(_____  )   \r\n" +
                            $"\t| |   | || |   | || | \\   |      ) |   | |   | (      | (\\ (         ) |   \r\n" +
                            $"\t| )   ( || (___) || )  \\  |/\\____) |   | |   | (____/\\| ) \\ \\__/\\____) |   \r\n" +
                            $"\t|/     \\|(_______)|/    )_)\\_______)   )_(   (_______/|/   \\__/\\_______)   \r\n                                                                           \n";
            TH.WriteL($"{text}{TH.SetColor()}", true);
        }

        public static void PrintHud()
        {
            TH.Clear();
            string playerText = (Player == null) ? "" : $" Player Name: {Player.Name} | Health: {Player.PrintHealth()} | {Player.PrintStats()} | Buffs: {Player.PrintBuffs()} | Gold: {TH.SetColor(Colors.Yellow)}{Player.Gold}g{TH.SetColor()}\n";

            TH.WriteL($"+--------------------------------------------------------------------------------------------+\n" +
                $" Current Room: {CurrentRoom.Name} | Turn {StatsTracker.Turns} | Total Enemies Slain: {StatsTracker.Victims.Count}\n" +
                $"{playerText}" +
                $"{((CurrentRoom.Enemy == null) ? " You are alone in the room.\n" :
                $"{(CurrentRoom.Enemy.IsMerchant ? $" Character: {CurrentRoom.Enemy.Name}" : $" Enemy: {CurrentRoom.Enemy.Name}")}" +
                $"| Health: {CurrentRoom.Enemy.PrintHealth()} | {CurrentRoom.Enemy.PrintStats()} | Buffs: {CurrentRoom.Enemy.PrintBuffs()} {CurrentRoom.Enemy.PrintIntent(Player.CanSeeIntent())} \n")}" +
                $"+--------------------------------------------------------------------------------------------+\n", true);
        }

        private static void Setup()
        {
            GameEvents.OnPlayerSkillGain += HandlePLayerSkillGain;
            StatsTracker.Reset();
            PrintTitle();
            TH.Menu("Do you want to skip the intro?", ["Yes", "No"], out int skipIntro, includeCancel: false, true);

            if (skipIntro == 1)
            {
                WriteIntro();
            }


            CurrentRoom = new Room() { ExhaustedSearch = true, IsStore = false, Name = "Locked Dungeon Cell" };
            BuildPlayer(false);
            Player.GiveUsableItem(ItemFactory.CreateUsableItem(eUsableItem.SmallHealthPotion));
            Player.GiveUsableItem(ItemFactory.CreateUsableItem(eUsableItem.ShopPortal));
            Player.GiveUsableItem(ItemFactory.CreateUsableItem(eUsableItem.IntentPotion));

            MoveToNewRoom();
        }

        public static void Start()
        {
            Console.ResetColor();
            PrintTitle();
            TH.WriteL("\n\n\t\tA small prototype by r\n\n", true);
            TH.Write("\t\t");
            TH.WaitAndClear("[Press Enter To Start]");
            int choice = -1;
            do
            {
                PrintTitle();
                TH.Menu("Main Menu:", ["Play", "How To Play", "About", "Exit"], out choice, false, true);
                switch (choice)
                {
                    case 0:
                        PlayGame();
                        break;
                    case 1:
                        HowToPlay();
                        break;
                    case 2:
                        About();
                        break;
                    case 3:
                        break;
                }
            } while (choice != 3);


            TH.WriteL("Thank you for playing! Press Any Key to exit the game.");
            Console.ReadKey();
        }

        public static void HowToPlay()
        {
            string header = $"{TH.SetColor(Colors.DarkCyan)}\t          _______            _________ _______    _______  _        _______          \r\n" +
                                                             $"\t|\\     /|(  ___  )|\\     /|  \\__   __/(  ___  )  (  ____ )( \\      (  ___  )|\\     /|\r\n" +
                                                             $"\t| )   ( || (   ) || )   ( |     ) (   | (   ) |  | (    )|| (      | (   ) |( \\   / )\r\n" +
                                                             $"\t| (___) || |   | || | _ | |     | |   | |   | |  | (____)|| |      | (___) | \\ (_) / \r\n" +
                                                             $"\t|  ___  || |   | || |( )| |     | |   | |   | |  |  _____)| |      |  ___  |  \\   /  \r\n" +
                                                             $"\t| (   ) || |   | || || || |     | |   | |   | |  | (      | |      | (   ) |   ) (   \r\n" +
                                                             $"\t| )   ( || (___) || () () |     | |   | (___) |  | )      | (____/\\| )   ( |   | |   \r\n" +
                                                             $"\t|/     \\|(_______)(_______)     )_(   (_______)  |/       (_______/|/     \\|   \\_/   {TH.SetColor()}";
            TH.WriteL($"{header}\n\n", true);
            TH.WriteL($"{TH.CT(Colors.DarkCyan, $"Portals")} & {TH.CT(Colors.DarkRed, "Monsters")} is a simple roguelike in which you are pitted in 1-on-1 turn-based combat against\n" +
                      $"an army of never-ending {TH.CT(Colors.DarkRed, "foes")}. Each room is an enclosed arena, guaranteed to have a {TH.CT(Colors.DarkCyan, "portal")} to an\n" +
                      "unknown destination. Where will you end up?\n\n" +
                      $"The main gameplay is based around Rock Paper Scissors, but instead called \"{TH.CT(Colors.Red, "Strike")}\", \"{TH.CT(Colors.Green, "Counter")}\" and \"{TH.CT(Colors.Blue, "Spell")}\".\n\n\n" +
                      $"{TH.CT(Colors.Red, "Strike")} beats {TH.CT(Colors.Blue, "spell")} - {TH.CT(Colors.Red, "Strike")} is quick and can hit someone slowly casting their {TH.CT(Colors.Blue, "spell")}.\n\n" +
                      $"{TH.CT(Colors.Green, "Counter")} beats {TH.CT(Colors.Red, "strike")} - Preparing for a {TH.CT(Colors.Red, "strike")}, you can react accordingly and {TH.CT(Colors.Green, "punish")} their attack.\n\n" +
                      $"{TH.CT(Colors.Blue, "Spell")} beats {TH.CT(Colors.Green, "counter")} - Those who are {TH.CT(Colors.Green, "waiting to counter")} allow the opposing caster time to cast their {TH.CT(Colors.Blue, "spell")}.\n\n");
            TH.WaitAndClear();

            TH.WriteL($"{header}\n\n", true);
            TH.WriteL("The game has some additional interesting mechanics. For instance when players tie on a choice:\n\n" +
                $"{TH.CT(Colors.Red, "Strike")} - If both opponents {TH.CT(Colors.Red, "strike")} at the same time, they {TH.CT(Colors.Red, "clash")}, and the respective damage of both blows is\n\t added up and dealt to each player.\n\n" +
                $"{TH.CT(Colors.Green, "Counter")} - If each opponent {TH.CT(Colors.Green, "counters")}, they {TH.CT(Colors.Green, "bolster")} their defenses by the amount of Dexterity they have.\n\n" +
                $"{TH.CT(Colors.Blue, "Spell")} - When both opponenets cast a {TH.CT(Colors.Blue, "spell")}, the magic attacks {TH.CT(Colors.Blue, "swirl together")}, eminating a stackable aura \n\tthat increases the damage of the next successful spell for each character by 1.\n\n");
            TH.WaitAndClear();

            TH.WriteL($"{header}\n\n", true);
            TH.WriteL("Here are some helpful things to know about the in-game displays.\n\n" +
                $"Characters have fixed health as shown like so: ({TH.CT(Colors.Red, "♥")})({TH.CT(Colors.Red, "♥")})( ).\nThis means the character has 3 HP total and has taken 1 point of damage.\n\n" +
                $"If a character has armor it will look like this: ({TH.CT(Colors.Red, "♥")})({TH.CT(Colors.Red, "♥")})( )[{TH.CT(Colors.Cyan, "+")}][{TH.CT(Colors.Cyan, "+")}].\nThis means the character has 2 points of {TH.CT(Colors.Cyan, "armor")}.\n\n" +
                $"{TH.CT(Colors.Cyan, "Armor")} is temporary additional HP that there is no limit on. When taking damage, {TH.CT(Colors.Cyan, "armor")} is used before HP.\n" +
                $"If a character has 2 {TH.CT(Colors.Cyan, "armor")} and takes 3 damage, they will lose all {TH.CT(Colors.Cyan, "armor")} and recieve 1 damage to their \navailable HP.\n\n" +
                $"For each character you will see a \"S/D/M:\" followed by numbers like so \"{TH.CT(Colors.Red, "1")}/{TH.CT(Colors.Green, "1")}/{TH.CT(Colors.Blue, "1")}\".\nThis stands for {TH.CT(Colors.Red, "Strength")}/{TH.CT(Colors.Green, "Dexterity")}/{TH.CT(Colors.Blue, "Magic")} and represents the amount of damage those attacks do.\n\n" +
                $"You will also see a \"Buffs: []\". When a character has a buff, it will show in the [], like this: [{TH.CT(Colors.Blue, "$")}].\nIf they have a stackable buff, it'll show something like [$x2].\n" +
                $"\nCurrently there is only one buff in the game, but if more are added, they will appear in this list:\n" +
                $"- {TH.CT(Colors.Blue, "$")} = {TH.CT(Colors.Blue, "Spell")} damage (from both players casting a {TH.CT(Colors.Blue, "spell")})\n\n");
            TH.WaitAndClear();

            TH.WriteL($"{header}\n\n", true);
            TH.WriteL($"You will also see an \"Intent: [?]\" next to the {TH.CT(Colors.DarkRed, "enemy")}. There's an item that lets you view what\nthey plan to do for their turn.\n\n" +
                $"If you use it, it'll show something like this [i->{TH.CT(Colors.Blue, "M")}].\nThis means the enemy is planning on using an item and then using their {TH.CT(Colors.Blue, "magic")} combat ability ({TH.CT(Colors.Blue, "spell")}).\n\n" +
                $"{TH.CT(Colors.Red, "S = Strength (Strike)")}, {TH.CT(Colors.Green, "D = Dexterity (Counter)")}, {TH.CT(Colors.Blue, "M = Magic (Spell)")}, i = Item\n\n" +
                "And like in the example above, if you see i=>, that means they're using an item into an ability.\n\nIf it's just i, then they're planning on only using an item that will end their turn.\n\n");
            TH.WaitAndClear();

            TH.WriteL($"{header}\n\n", true);
            TH.WriteL($"The last thing to mention is there is no ending. The game will continue on until you lose!\n\n" +
                $"Every 5 enemies you slay, you will gain a stat point to assign to either {TH.CT(Colors.Red, "Strength")}, {TH.CT(Colors.Green, "Dexterity")}, or {TH.CT(Colors.Blue, "Magic")}.\n\n" +
                "I think that's it for the basics! If you have any additional questions, reach out! I hope you enjoy the game.\n\n");
            TH.WaitAndClear("[Press Enter To Go Back To Main Menu]");
        }

        public static void About()
        {
            string header = $"{TH.SetColor(Colors.DarkCyan)}\t _______  ______   _______          _________\r\n" +
                                                             $"\t(  ___  )(  ___ \\ (  ___  )|\\     /|\\__   __/\r\n" +
                                                             $"\t| (   ) || (   ) )| (   ) || )   ( |   ) (   \r\n" +
                                                             $"\t| (___) || (__/ / | |   | || |   | |   | |   \r\n" +
                                                             $"\t|  ___  ||  __ (  | |   | || |   | |   | |   \r\n" +
                                                             $"\t| (   ) || (  \\ \\ | |   | || |   | |   | |   \r\n" +
                                                             $"\t| )   ( || )___) )| (___) || (___) |   | |   \r\n" +
                                                             $"\t|/     \\||/ \\___/ (_______)(_______)   )_(   \r\n                                             ";

            TH.WriteL($"{header}{TH.SetColor()}\n\n", true);
            TH.WriteL("This is a simple game prototype built by r (rhunyc) in a single Program.cs file.\n\n" +
                "The intent with this, if it proves to be a fun enough concept, is to port it into a game engine with graphics \nand sound.\n\n" +
                "One thing to note is that is VERY rough around the edges and most definitely NOT balanced (aside from my rough\n pass at it).\n\n" +
                "Expect weird grammar and the occasional typo. It is not meant to be a polished or relatively finished game.\n\n" +
                "Just a small bite-sized thing to get the idea across. :) \n\n" +
                "There is room for a lot of additional content like more enemies, abilities, items, rooms, and gameplay mechanics.\n" +
                "I think it could be fun to add some sense of progression, like unlocks and an end goal. Something like fighting \nbosses and trying to make it to an end point of some sort.\n\n" +
                "Anyways, thanks for checking it out. Please share any feedback, good or bad. :)\n\n(Headers and title generated with: https://patorjk.com/software/taag/)\n");
            TH.WaitAndClear("[Press Enter To Go Back To Main Menu]");
        }

        public static void PlayGame()
        {
            int restart = 0;
            do
            {
                Setup();
                while (!Player.IsDead)
                {
                    Play();
                }

                PrintHud();
                StatsTracker.PrintFinalStats();
                TH.Menu("\nWould you like to restart?", ["Yes", "No"], out restart, includeCancel: false);
            }
            while (restart == 0);
        }

        private static void WriteIntro()
        {
            TH.WriteStoryText([
                "You wake up from what feels like the hardest sleep of your life.\n",
                $"Rubbing your eyes, you can't help but to notice what appears to be a {TH.CT(Colors.DarkCyan,"dimly lit portal")}, swirling with \nall kinds of energy.\n",
                "You think to yourself \"This can't be real... I must be asleep. This has to be a dream.\"\n",
                $"Looking around more as you snap out of your grogginess, you realize you are in a locked dungeon cell. You see a \ntable with a note on it.\n",
                "You get up to read the note... It says:\n",
                "\"Greetings.. Unfortunately you're trapped in a vicious cycle.\"\n",
                "\"You must travel from portal to portal, in a series of unforseen locations.\"\n",
                "\"I've done my best to free you from your existing circumstances, but unfortunately this is the best I can do...\"\n",
                "\"The only path forward is through the portals. I don't know what lies on the other side.\"\n",
                "\"I pray you find respite. -b\"\n",
                "Dropping the letter, you feel a sense of dread start to well up in your chest.\n",
                $"With a heavy sigh, you start stepping towards the portal..\n"
            ]);
        }


        public static void HandlePLayerSkillGain()
        {
            if (!Player.IsDead)
            {
                TH.Clear();
                PrintHud();
                TH.WriteL("You've defeated 5 enemies, gaining a skill point!");
                TH.WaitAndClear();
                PrintHud();
                PlayerSkillGain(true);
            }
        }
        private static void PlayerSkillGain(bool isInGame = false)
        {
            TH.Menu("Which stat would you like to increase by 1?", [$"{TH.CT(Colors.Red, "Strength")}", $"{TH.CT(Colors.Green, "Dexterity")}", $"{TH.CT(Colors.Blue, "Magic")}"], out int c, false);
            if (isInGame) { PrintHud(); }
            switch (c)
            {
                case 0:
                    Player.Strength++;
                    TH.WriteL($"You feel {TH.SetColor(Colors.Red)}stronger{TH.SetColor()}.");
                    break;
                case 1:
                    Player.Dexterity++;
                    TH.WriteL($"You feel {TH.SetColor(Colors.Green)}quicker{TH.SetColor()}.");
                    break;
                case 2:
                    Player.Magic++;
                    TH.WriteL($"Your {TH.SetColor(Colors.Blue)}magical aura grows{TH.SetColor()}.");
                    break;
            }
            TH.WaitAndClear();
        }

        private static void Play()
        {
            StatsTracker.Turns++; CombatAction enemyAction = null; eUsableItem enemyItem = eUsableItem.Nothing;
            if (CurrentRoom.Enemy != null && !CurrentRoom.Enemy.IsDead && !Player.IsDead)
            {
                RollEnemyTurn(CurrentRoom.Enemy, out enemyAction, out enemyItem);
            }

            PlayerTurn(out bool skipsEnemyTurn, out CombatAction playerAction);

            if (!skipsEnemyTurn && CurrentRoom.Enemy != null && !CurrentRoom.Enemy.IsDead && !Player.IsDead)
            {
                if (enemyItem != eUsableItem.Nothing)
                {
                    PrintHud();
                    CurrentRoom.Enemy.UseItem(enemyItem, CurrentRoom.Enemy);
                    TH.WaitForEnter();
                }

                ResolveCombat(playerAction, enemyAction);
            }

            if (Player.SeesIntent) Player.SeesIntent = false;
        }

        private static void PlayerTurn(out bool skipsEnemyTurn, out CombatAction combatAction)
        {
            bool turnSpent = false; skipsEnemyTurn = false;

            do
            {
                combatAction = null;

                PrintHud();
                List<string> choices = new List<string>();
                bool enemyPresent = CurrentRoom.Enemy != null && !CurrentRoom.Enemy.IsDead;

                if (enemyPresent)
                {
                    if (CurrentRoom.Enemy.IsMerchant)
                    {
                        if (CurrentRoom.Enemy.UsableItems.Count > 0)
                        {
                            choices.Add("Shop");
                        }

                        if (!CurrentRoom.Enemy.ExhaustedSearch)
                        {
                            choices.Add($"Steal From* ({Player.CalculateStealChance(CurrentRoom.Enemy)} % Chance)");
                        }

                        choices.Add($"Combat");
                        choices.Add("Inventory");
                        choices.Add($"Leave Room*");
                    }
                    else
                    {
                        choices.Add($"Combat");
                        if (!CurrentRoom.Enemy.ExhaustedSearch)
                        {
                            choices.Add($"Steal From* ({Player.CalculateStealChance(CurrentRoom.Enemy)}% Chance)");
                        }

                        if (!CurrentRoom.ExhaustedSearch)
                        {
                            choices.Add($"Search Room*");
                        }

                        choices.Add("Inventory");
                        choices.Add($"Escape Room* ({CurrentRoom.Enemy.EvasionChance}% Chance)");
                    }
                }
                else
                {
                    if (!CurrentRoom.ExhaustedSearch)
                    {
                        choices.Add($"Search Room*");
                    }

                    choices.Add("Inventory");
                    choices.Add($"Leave Room*");
                }

                TH.Menu($"What would you like to do? (* ends turn)", choices.ToArray(), out int c, includeCancel: false);
                string choice = choices[c];
                PrintHud();

                switch (choice.Split(' ')[0].Split('*')[0])
                {
                    case "Shop":

                        if (TH.Menu($"Choose an item to buy ({TH.CT(Colors.Yellow, $"Available Gold: {Player.Gold}")}:", CurrentRoom.Enemy.GetItemsForSaleAsChoice(out eUsableItem[] itemChoices), out int selectedItem))
                        {
                            PrintHud();
                            Player.TryBuyItem(itemChoices[selectedItem], CurrentRoom.Enemy);
                            TH.WaitForEnter();
                        }
                        break;
                    case "Steal":
                        if (Helper.RollDice(Player.CalculateStealChance(CurrentRoom.Enemy)))
                        {
                            if (CurrentRoom.Enemy.UsableItems.Count <= 0)
                            {
                                TH.WriteL($"You were able to stealthily search the {CurrentRoom.Enemy.Name}'s belongings, unfortunately they had no items.");
                                TH.WaitForEnter();
                            }
                            else
                            {
                                TH.WriteL($"Your guile allows you to quickly steal an item from the {CurrentRoom.Enemy.Name} {(CurrentRoom.Enemy.IsMerchant ? "without them noticing" : "")}!");
                                TH.WaitForEnter();
                                PrintHud();
                                TH.Menu($"Choose an item to steal:", CurrentRoom.Enemy.GetItemsAsChoice(out eUsableItem[] stealingChoices), out int stolenItem, includeCancel: false);
                                PrintHud();
                                Player.StealItem(CurrentRoom.Enemy, stealingChoices[stolenItem]);
                                TH.WaitForEnter();
                            }

                            CurrentRoom.Enemy.ExhaustedSearch = CurrentRoom.Enemy.UsableItems.Count <= 0;
                        }
                        else
                        {
                            TH.WriteL($"In an attempt to search the {CurrentRoom.Enemy.Name} for valuables, you slipped up and they caught you before you could get a good look at what they're carrying.");
                            TH.WaitForEnter();
                            if (CurrentRoom.Enemy.IsMerchant)
                            {
                                PrintHud();
                                TH.WriteL($"The {CurrentRoom.Enemy.Name} looks at you with a dissapointed expression. They have lost trust in you as a buyer.");
                                TH.WaitForEnter();
                                PrintHud();
                                TH.WriteL("You are now an enemy.");
                                CurrentRoom.Enemy.IsMerchant = false;
                            }
                        }
                        turnSpent = true;
                        break;

                    case "Combat":
                        if (TH.Menu("Choose your action:", Player.GetActionsAsChoice(), out int a))
                        {
                            combatAction = Player.Actions[a];
                            turnSpent = true;
                        }
                        break;
                    case "Search":
                        if (CurrentRoom.TrySearch(out ItemUsable item))
                        {
                            Player.GiveUsableItem(item);
                        }
                        TH.WriteL((item == null) ? "After looking more, you realize there's no items left in the room." : $"You found a {item.Name}! You add it to your inventory.");
                        turnSpent = true;
                        TH.WaitForEnter();
                        break;
                    case "Leave":
                        MoveToNewRoom();
                        skipsEnemyTurn = true;
                        turnSpent = true;
                        break;
                    case "Escape":
                        if (Helper.RollDice(CurrentRoom.Enemy.EvasionChance))
                        {
                            TH.WriteL($"You were able dodge past the {CurrentRoom.Enemy.Name} and jump to portal to the next room.");
                            TH.WaitForEnter();
                            skipsEnemyTurn = true;
                            MoveToNewRoom();
                        }
                        else
                        {
                            TH.WriteL($"You tried rolling past the {CurrentRoom.Enemy.Name}, but it caught you and tossed you back to where you were.");
                            TH.WaitForEnter();
                        }

                        turnSpent = true;
                        break;
                    case "Inventory":
                        if (Player.UsableItems.Count == 0)
                        {
                            TH.WriteL("You have no items in your inventory.");
                            TH.WaitForEnter();
                        }
                        else
                        {
                            if (TH.Menu("Choose an item to use: (* ends turn)", Player.GetItemsAsChoice(out eUsableItem[] itemsToChooseFrom), out int i))
                            {
                                PrintHud();
                                eUsableItem chosenItem = itemsToChooseFrom[i];
                                turnSpent = Player.UseItem(chosenItem, (ItemFactory.SelfUseItems.Contains(chosenItem) ? Player : CurrentRoom.Enemy));

                                if (CurrentRoom.Enemy != null && CurrentRoom.Enemy.IsBanished)
                                {
                                    CurrentRoom.Enemy = null;
                                }

                                TH.WaitForEnter();

                                if (chosenItem == eUsableItem.ShopPortal)
                                {
                                    PrintHud();
                                    skipsEnemyTurn = true;
                                    MoveToNewRoom(eRoom.Shop);

                                }
                            }
                        }

                        break;
                }
            } while (!turnSpent);
        }

        private static void RollEnemyTurn(Enemy enemy, out CombatAction enemyAction, out eUsableItem item)
        {
            enemyAction = null; item = eUsableItem.Nothing;
            bool itemEndsTurn = false;


            if (enemy.Health < enemy.MaxHealth && Helper.RollDice(65))
            {
                enemy.DetermineIfUseHeal(out item, out itemEndsTurn);
            }

            if (!enemy.IsMerchant)
            {
                enemyAction = (!itemEndsTurn) ? Helper.GetRandomItemFromArray(enemy.Actions.ToArray()) : null;
            }

            string itemIntent = (item != eUsableItem.Nothing) ? "i" : "";
            string actionIntent = (enemyAction == null) ? "" : enemyAction.GetIntentSymbol();

            enemy.Intent = itemIntent + (String.IsNullOrEmpty(itemIntent) ? "" : "=>") + actionIntent;
        }

        private static void ResolveCombat(CombatAction playerAction, CombatAction enemyAction)
        {
            PrintHud();
            Enemy currentEnemy = CurrentRoom.Enemy;

            if ((playerAction == null && enemyAction == null) || currentEnemy == null)
            {
                return;
            }
            else if (playerAction == null || enemyAction == null)
            {
                CombatAction a = playerAction ?? enemyAction;
                HandleSingleAttack(a, (a.Owner == Player) ? currentEnemy : Player);
            }
            else
            {
                TH.WriteL($"You chose to {playerAction.Name} and the {currentEnemy.Name} chose to {enemyAction.Name}.");

                if (playerAction.WinAgainst == enemyAction.ActionType)
                {
                    TH.WriteL($"Your {playerAction.Name} beat the {currentEnemy.Name}'s {enemyAction.Name}, dealing {playerAction.PrintDamage()}!");
                    TH.WaitAndClear();
                    currentEnemy.TakeDamage(playerAction);
                }
                else if (playerAction.LoseAgainst == enemyAction.ActionType)
                {
                    TH.WriteL($"The {currentEnemy.Name}'s {enemyAction.Name} beat your {playerAction.Name}, dealing {enemyAction.PrintDamage()}.");
                    TH.WaitAndClear();
                    Player.TakeDamage(enemyAction);
                }
                else
                {
                    switch (playerAction.ActionType)
                    {
                        case eAction.Strike:
                            int totalDamage = playerAction.Damage + enemyAction.Damage;
                            TH.WriteL($"Both of your strikes clashed with such intense force, dealing the combined damage of each {TH.SetColor(Colors.Red)}slash{TH.SetColor()} ({totalDamage}) to both you and the {currentEnemy.Name}!");
                            TH.WaitAndClear();
                            Player.TakeDamage(new CombatAction() { ActionType = eAction.Clash, Owner = currentEnemy, Damage = totalDamage, Name = "Clash" });
                            currentEnemy.TakeDamage(new CombatAction() { ActionType = eAction.Clash, Owner = Player, Damage = totalDamage, Name = "Clash" });
                            break;
                        case eAction.Counter:
                            TH.WriteL($"As each opponent awaits an incoming attack, they {TH.SetColor(Colors.Green)}fortify{TH.SetColor()} their defenses.");
                            TH.WaitAndClear();
                            PrintHud();
                            Player.AddCounterArmor();
                            currentEnemy.AddCounterArmor();
                            TH.WaitAndClear();
                            break;
                        case eAction.Spell:
                            TH.WriteL($"The spells swirl together, creating an aura the buffs each opponent's next {TH.SetColor(Colors.Blue)}spell{TH.SetColor()}!");
                            TH.WaitAndClear();
                            Player.GiveBuff(eCombatBuff.SpellDamage);
                            currentEnemy.GiveBuff(eCombatBuff.SpellDamage);
                            break;
                    }
                }
            }
        }

        public static void HandleSingleAttack(CombatAction attack, Character otherCharacter)
        {
            switch (attack.ActionType)
            {
                case eAction.Strike:
                    TH.WriteL($"While {otherCharacter.PerspectiveText("you", $"the {otherCharacter.Name}")} were unready, {attack.Owner.PerspectiveText("you", $"the {attack.Owner.Name}")} {TH.SetColor(Colors.Red)}struck{TH.SetColor()} for{attack.PrintDamage()}.");
                    TH.WaitAndClear();
                    otherCharacter.TakeDamage(attack);
                    break;
                case eAction.Counter:
                    TH.WriteL($"{attack.Owner.PerspectiveText("You", $"The {attack.Owner.Name}")} prepared for a strike that never came, {TH.SetColor(Colors.Green)}fortifying{TH.SetColor()} {attack.Owner.PerspectiveText("your", $"their")} defense!");
                    TH.WaitAndClear();
                    attack.Owner.AddCounterArmor();
                    break;
                case eAction.Spell:
                    TH.WriteL($"While {otherCharacter.PerspectiveText("you were", $"the {otherCharacter.Name} was")} busy, {attack.Owner.PerspectiveText("you were able to cast your", $"the {attack.Owner.Name} was able to cast their")} {TH.SetColor(Colors.Blue)}spell{TH.SetColor()} uninterupted, dealing {attack.PrintDamage()}!");
                    TH.WaitAndClear();
                    otherCharacter.TakeDamage(attack);
                    break;
            }

            if (!otherCharacter.IsDead && otherCharacter is Enemy e && e.IsMerchant)
            {
                TH.WriteL($"The {e.Name} looks at you with a dissapointed expression. {TH.SetColor(Colors.DarkGray)}They have lost trust in you as a buyer.{TH.SetColor()}");
                TH.WaitForEnter();
                PrintHud();
                TH.WriteL($"You are now an {TH.SetColor(Colors.DarkRed)}enemy{TH.SetColor()}.");
                e.IsMerchant = false;
            }
        }

        private static void MoveToNewRoom(eRoom roomToMoveTo = eRoom.Empty)
        {
            bool randomRoom = roomToMoveTo == eRoom.Empty;
            Room prevRoom = CurrentRoom;
            if (prevRoom == null)
            {
                CurrentRoom = (randomRoom) ? RoomFactory.GetRandomRoom() : RoomFactory.GetSpecificRoom(roomToMoveTo);
            }
            else if (!randomRoom)
            {
                CurrentRoom = RoomFactory.GetSpecificRoom(roomToMoveTo);
            }
            else
            {
                if (randomRoom)
                {
                    do
                    {
                        CurrentRoom = RoomFactory.GetRandomRoom();
                    }
                    while (CurrentRoom.Name == prevRoom.Name);
                }
            }

            StatsTracker.RoomVisited(CurrentRoom);
            TH.WriteL(CurrentRoom.RoomEnterText(prevRoom));
            TH.WaitForEnter();
        }

        public enum eAbility
        {  // Some abilities, we'll do these later
            // offensive
            DoubleStrike, // Next strike deals twice damage
            PowerSurge,  // Next spell deals twice damage
            Focus, // Next counter deals twice damage

            // defensive
            Fortify, // reduce damage by 1 for two turns
            Regenerate, // heal 1 hp for two turns            
        }

        private static void BuildPlayer(bool skipSetup = false)
        {
            Player = new Player() { Health = 6, IsPlayer = true, MaxHealth = 6, Name = "???", Strength = 1, Dexterity = 1, Magic = 1, Actions = ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell]) };

            if (skipSetup)
            {
                for (int i = 0; i < 2; i++)
                {
                    PlayerSkillGain();
                }
                PrintHud();
                return;
            }

            Player.Name = TH.PromptAndGetTextAnswer($"Before you step into the {TH.SetColor(Colors.DarkCyan)}portal{TH.SetColor()}, what is your name?");
            TH.Clear();
            TH.WriteL($"Excellent, thank you for answering that {Player.Name}.");
            TH.WaitAndClear();

            TH.WriteL($"Now, let's set up your character. You have 2 points to distribute between {TH.SetColor(Colors.Red)}Strength{TH.SetColor()}, {TH.SetColor(Colors.Green)}Dexterity{TH.SetColor()}, and {TH.SetColor(Colors.Blue)}Magic{TH.SetColor()}.");
            TH.WaitAndClear();

            for (int i = 0; i < 2; i++)
            {
                PlayerSkillGain();
            }

            PrintHud();
        }
    }

    // COMBAT
    public enum eCombatBuff { SpellDamage }

    public static class AllBuffs
    {
        public static eCombatBuff[] AllCombatBuffs = [eCombatBuff.SpellDamage];
    }

    public enum eAction { Strike, Counter, Spell, Clash }

    public class CombatAction
    {
        public eAction ActionType { get; set; }
        public eAction WinAgainst { get; set; }
        public eAction LoseAgainst { get; set; }

        private string _Name;
        public string Name
        {
            get
            {
                char color = Colors.Default;
                switch (ActionType)
                {
                    case eAction.Strike:
                        color = Colors.Red;
                        break;
                    case eAction.Counter:
                        color = Colors.Green;
                        break;
                    case eAction.Spell:
                        color = Colors.Blue;
                        break;
                }

                return $"{TH.SetColor(color)}{_Name}{TH.SetColor()}";
            }
            set => _Name = value;
        }
        public Character Owner { get; set; }

        private bool _IsBuffed = false;
        private int _Damage;
        public int Damage
        {
            get
            {
                return CalculateDamage();
            }
            set
            {
                _Damage = value;
            }
        }

        public CombatAction CreateCopy()
        {
            return new CombatAction()
            {
                ActionType = ActionType,
                WinAgainst = WinAgainst,
                LoseAgainst = LoseAgainst,
                Name = Name,
                Damage = Damage,
            };
        }

        public string PrintDamage()
        {
            int dmg = Damage;
            return (dmg <= 0) ? "" : $" ({dmg}{(_IsBuffed ? "*" : "")} dmg)";
        }

        public int CalculateDamage()
        {
            if (Owner == null) { return _Damage; }
            int damage = _Damage;

            switch (ActionType)
            {
                case eAction.Spell:
                    damage += Owner.Magic;
                    damage += Owner.CombatBuffs.Where(b => b == eCombatBuff.SpellDamage).Count();
                    _IsBuffed = (damage > Owner.Magic);
                    break;
                case eAction.Strike:
                    damage += Owner.Strength;
                    break;
                case eAction.Counter:
                    damage += Owner.Dexterity;
                    break;
            }

            return damage;
        }

        public string GetIntentSymbol()
        {
            switch (ActionType)
            {
                case eAction.Spell:
                    return $"{TH.SetColor(Colors.Blue)}M{TH.SetColor()}";
                case eAction.Strike:
                    return $"{TH.SetColor(Colors.Red)}S{TH.SetColor()}";
                case eAction.Counter:
                    return $"{TH.SetColor(Colors.Green)}D{TH.SetColor()}";
            }

            return "";
        }

        public void ResolveBuffs()
        {
            if (ActionType == eAction.Spell)
            {
                while (Owner.CombatBuffs.Contains(eCombatBuff.SpellDamage))
                {
                    Owner.RemoveBuff(eCombatBuff.SpellDamage);
                }
            }
        }
    }

    public static class ActionsFactory
    {
        public static CombatAction[] Actions { get; set; } = [
            new() { ActionType = eAction.Strike, Name = "Strike", Damage = 0, WinAgainst = eAction.Spell, LoseAgainst = eAction.Counter },
            new() { ActionType = eAction.Counter, Name = "Counter", Damage = 0, WinAgainst = eAction.Strike, LoseAgainst = eAction.Spell },
            new() { ActionType = eAction.Spell, Name = "Spell", Damage = 0, WinAgainst = eAction.Counter, LoseAgainst = eAction.Strike }
        ];

        public static List<CombatAction> CreateActions(eAction[] action) => Actions.Where(a => action.Contains(a.ActionType)).Select(a => a.CreateCopy()).ToList();
    }

    //Room
    public class Room
    {
        public eRoom eRoom { get; set; }

        private string _Name;
        public string Name
        {
            get { return $"{TH.SetColor(Colors.DarkCyan)}{_Name}{TH.SetColor()}"; }
            set { _Name = value; }
        }
        public List<ItemUsable> Items { get; set; } = new();
        public Enemy Enemy { get; set; } = null;
        public int Gold { get; set; } = 0;
        public bool ExhaustedSearch { get; set; } = false;
        public bool IsStore { get; set; }

        private UsableItemSpawn[] PossibleItems { get; set; } = [];
        private EnemySpawn[] PossibleEnemies { get; set; } = [];
        private int PossibleGold { get; set; }

        public Room(string name = "", UsableItemSpawn[] possibleItems = null, EnemySpawn[] possibleEnemies = null, int possibleGold = 0, bool isRoom = false, eRoom room = eRoom.Empty)
        {
            Name = name;
            PossibleEnemies = possibleEnemies ?? PossibleEnemies;
            PossibleItems = possibleItems ?? PossibleItems;
            PossibleGold = possibleGold;
            IsStore = false;
            eRoom = room;
        }

        public Room BuildRoom() => new()
        {
            Name = Name,
            Gold = Helper.GetRandomNumber(PossibleGold),
            Enemy = EnemyFactory.SpawnRandomEnemy(PossibleEnemies),
            Items = ItemFactory.SpawnUsableItems(PossibleItems),
            IsStore = IsStore,
            eRoom = eRoom
        };


        // returns true if it counts as an action, at first if you exhaust it will deplete your turn, but subsequent attempts won't burn a turn;
        public bool TrySearch(out ItemUsable item)
        {
            if (Items.Count == 0)
            {
                item = null;
                ExhaustedSearch = true;
                return false;
            }

            item = Helper.GetRandomItemFromArray<ItemUsable>(Items.ToArray());
            Items.Remove(item);
            return true;
        }

        public string RoomEnterText(Room prevRoom)
        {
            bool isFirstRoom = prevRoom == null;
            string[] firstRoom = [$"You start your journey entering the portal for the first time. After stepping through it, you are now in a {Name}.", $"With a deep breath, you step through the portal. Suddenly you appear in a {Name}."];
            string[] travelText = (isFirstRoom) ? [] : [$"You enter the portal, leaving the {prevRoom.Name} and have now entered what appears to be a {Name}.",
                $"Looking around the {prevRoom.Name} one last time, you step into the portal and are now in a {Name}.",
                $"Ready to leave the {prevRoom.Name}, you quickly enter the portal to land in a {Name}.",
                $"Without hesitation, you jump into the portal. After a flash of light you find yourself in a {Name}."
                ];
            string[] merchantText = (Enemy == null || !Enemy.IsMerchant) ? [] : [$" In it, there is a single {Enemy.Name} {(Enemy.UsableItems.Count > 0 ? "with some items for sale." : "who sadly has no items for sale. What kind of store is this?")}"];
            string[] enemyText = (Enemy == null || Enemy.IsMerchant) ? [] : [$" In it, there is a single {Enemy.Name} infront of a portal, sizing you up, ready to fight.", $" Quickly you notice a {Enemy.Name} staring at you. They want to fight.", $" Instantly you make eye contact with the {Enemy.Name} blocking you from escape. You can sense their aggression.", $" The {Enemy.Name} in the room is pacing back and forth, as if they've been waiting to fight."];
            string[] emptyText = [$" Thankfully there's nobody else in the {Name}, just the lit portal across the way.", $" Luckily you realize you're alone.", $" Fortunately, there's no enemies, granting you a bit of respite."];

            string rText = (prevRoom == null) ? Helper.GetRandomItemFromArray(firstRoom) : Helper.GetRandomItemFromArray(travelText);
            rText += (this.Enemy == null) ? Helper.GetRandomItemFromArray(emptyText) : (this.Enemy.IsMerchant) ? Helper.GetRandomItemFromArray(merchantText) : Helper.GetRandomItemFromArray(enemyText);
            return rText;
        }
    }

    public enum eRoom { Empty, DarkAlleyway, DimlyLitCellar, ForestClearing, MushroomDwelling, CastleThroneRoom, Shop }
    public static class RoomFactory
    {
        private static UsableItemSpawn[] StandardRoomDrops = [new(eUsableItem.SmallHealthPotion, 40),
            new(eUsableItem.SmallHealthPotion, 20),
            new(eUsableItem.MediumHealthPotion, 10),
            new(eUsableItem.MediumHealthPotion, 5),
            new(eUsableItem.ArmorShard, 30),
            new(eUsableItem.ArmorShard, 5),
            new(eUsableItem.BanishmentSpellScroll, 5),
            new(eUsableItem.ShopPortal, 10)];

        static Room[] Rooms { get; set; } = [
            new(name: "Dark Alleyway",
                possibleItems: StandardRoomDrops,
                possibleEnemies: [new(eEnemy.Skeleton, 10), new(eEnemy.Goblin, 100)]),
            new(name: "Dimly Lit Cellar",
                possibleItems: StandardRoomDrops,
                possibleEnemies: [new(eEnemy.Wolf, 30), new(eEnemy.Goblin, 90)]),
            new(name: "Forest Clearing",
                possibleItems: StandardRoomDrops,
                possibleEnemies: [new(eEnemy.Goblin, 10), new(eEnemy.MushroomKnight, 30), new(eEnemy.Wolf, 80)]),
            new(name: "Mushroom Dwelling",
                possibleItems: StandardRoomDrops,
                possibleEnemies: [new(eEnemy.MushroomKnight, 90)]),
            new(name: "Castle Throne Room",
                possibleItems: [new(eUsableItem.SmallHealthPotion, 30), new(eUsableItem.SmallHealthPotion, 30), new(eUsableItem.SmallHealthPotion, 30)],
                possibleEnemies: [new(eEnemy.PossessedSoldier, 40), new(eEnemy.MushroomKnight, 70)]),
            new(name: "Shop", room: eRoom.Shop,
                possibleEnemies: [new(eEnemy.Merchant, 100)],
                possibleItems: [new(eUsableItem.BanishmentSpellScroll, 10), new(eUsableItem.MediumHealthPotion, 90), new(eUsableItem.ArmorShard, 95), new(eUsableItem.ShopPortal, 100)])
        ];

        public static Room GetRandomRoom() => Helper.GetRandomItemFromArray(Rooms).BuildRoom();

        public static Room GetSpecificRoom(eRoom room) => Rooms.Where(r => r.eRoom == room).FirstOrDefault().BuildRoom();
    }

    //Character
    public abstract class Character
    {
        public string _Name;
        public string Name
        {
            get
            {
                char color = (IsPlayer) ? Colors.Green : (IsMerchant) ? Colors.Green : Colors.DarkRed;
                return (IsDead) ? $"{TH.SetColor(Colors.DarkGray)}{_Name}{TH.SetColor()}" : $"{TH.SetColor(color)}{_Name}{TH.SetColor()}";
            }
            set => _Name = value;
        }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; } = 0;
        public int Gold { get; set; } = 0;

        public int Strength { get; set; } = 1;
        public int Dexterity { get; set; } = 1;
        public int Magic { get; set; } = 1;

        public bool IsDead { get; set; } = false;
        public bool IsPlayer { get; set; } = false;
        public bool IsMerchant { get; set; }
        public bool IsBanished { get; set; } = false;
        public Dictionary<eUsableItem, List<ItemUsable>> UsableItems { get; private set; } = new() { };

        private List<CombatAction> _Actions = new();
        public List<CombatAction> Actions
        {
            get { return _Actions; }
            set
            {
                _Actions = value;
                foreach (CombatAction a in _Actions)
                {
                    a.Owner = this;
                }
            }
        }

        public List<eCombatBuff> CombatBuffs { get; set; } = new();
        public void GiveBuff(eCombatBuff buff) => CombatBuffs.Add(buff);
        public void RemoveBuff(eCombatBuff buff) => CombatBuffs.Remove(buff);
        public void GiveUsableItem(ItemUsable item)
        {
            if (UsableItems.ContainsKey(item.UsableItem))
            {
                UsableItems[item.UsableItem].Add(item);
            }
            else
            {
                UsableItems.Add(item.UsableItem, new List<ItemUsable>() { item });
            }
        }
        public void GiveItems(List<ItemUsable> items)
        {
            foreach (ItemUsable item in items)
            {
                GiveUsableItem(item);
            }
        }

        public void RemoveItem(ItemUsable item)
        {
            if (UsableItems.ContainsKey(item.UsableItem))
            {
                UsableItems[item.UsableItem].Remove(item);

                if (UsableItems[item.UsableItem].Count == 0)
                {
                    UsableItems.Remove(item.UsableItem);
                }
            }
        }

        public bool UseItem(eUsableItem item, Character target)
        {
            bool usesTurn = false;

            if (UsableItems.ContainsKey(item))
            {
                usesTurn = !UsableItems[item][0].InstantUse;

                if (!UsableItems[item][0].Use(this, target))
                {
                    usesTurn = false;
                }
            }

            return usesTurn;
        }

        public void StealItem(Character target, eUsableItem item)
        {
            ItemUsable stolenItem = target.UsableItems[item].FirstOrDefault();
            target.RemoveItem(stolenItem);
            GiveUsableItem(stolenItem);

            TH.WriteL($"You slyly put the stolen {stolenItem.Name} in your inventory.");
        }

        public void AddArmor(int armorToAdd)
        {
            Armor += armorToAdd;
            TH.WriteL($"{PerspectiveText("You", $"The {Name}")} gained {armorToAdd} armor.");
        }

        public void AddCounterArmor() => AddArmor(1 + (Dexterity / 2));


        public void Heal(int healedPoints)
        {
            healedPoints = (Health + healedPoints > MaxHealth) ? MaxHealth - Health : healedPoints;
            Health += healedPoints;
            TH.WriteL($"{PerspectiveText("You", $"The {Name}")} recovered {healedPoints} hp.");
        }
        public void TakeDamage(CombatAction enemyAttack)
        {
            int dmg = enemyAttack.Damage;

            if (Armor > 0)
            {
                if (Armor >= dmg)
                {
                    Armor -= dmg;
                    return;
                }
                else
                {
                    dmg -= Armor;
                    Armor = 0;
                }
            }

            Health -= dmg;
            enemyAttack.ResolveBuffs();
            Game.PrintHud();

            if (Health <= 0)
            {
                TH.WriteL($"{PerspectiveText($"You die", $"The {Name} was killed")} as a result!");
                TH.WaitForEnter();
                TH.Clear();
                IsDead = true;
                Death(enemyAttack.Owner);
                return;
            }
        }

        public string PerspectiveText(string playerText, string enemyText)
        {
            return (IsPlayer) ? playerText : enemyText;
        }

        public string PrintHealth()
        {
            int remainder = MaxHealth - Health;
            string healthBar = "";

            for (int i = 0; i < Health; i++)
            {
                healthBar += $"({TH.SetColor(Colors.Red)}♥{TH.SetColor()})";
            }

            for (int i = 0; i < remainder; i++)
            {
                healthBar += "( )";
            }

            return (Health > 0) ? $"{healthBar}{PrintArmor()}" : $"{TH.SetColor(Colors.DarkGray)}Dead{TH.SetColor()}";
        }

        public string PrintArmor()
        {
            string armor = "";
            for (int i = 0; i < Armor; i++)
            {
                armor += $"[{TH.SetColor(Colors.Cyan)}+{TH.SetColor()}]";
            }

            return armor;
        }

        public string PrintBuffs()
        {
            string buffs = "";

            foreach (eCombatBuff b in AllBuffs.AllCombatBuffs)
            {
                int num = CombatBuffs.Where(cb => cb == b).Count();
                if (num > 0)
                {
                    string mod = (num > 1) ? $"x{num}" : "";
                    string r = "";

                    switch (b)
                    {
                        case eCombatBuff.SpellDamage:
                            r = $"{TH.SetColor(Colors.Blue)}${TH.SetColor()}";
                            break;
                    }

                    buffs += $"[{r}{mod}]";
                }
            }

            return String.IsNullOrEmpty(buffs) ? $"{TH.SetColor(Colors.DarkGray)}None{TH.SetColor()}" : buffs;
        }

        public string[] GetItemsAsChoice(out eUsableItem[] itemsToChooseFrom)
        {
            List<string> choices = new(); List<eUsableItem> itemChoices = new();

            foreach (KeyValuePair<eUsableItem, List<ItemUsable>> kvp in UsableItems)
            {
                choices.Add($"{(kvp.Value[0].InstantUse ? "" : "*")}{kvp.Value[0].Name} [x{kvp.Value.Count}] - {kvp.Value[0].Description}");
                itemChoices.Add(kvp.Key);
            }

            itemsToChooseFrom = itemChoices.ToArray();
            return choices.ToArray();
        }

        public void Banish()
        {
            TH.WriteL($"Suddenly the sky tears open above {PerspectiveText("you", $"the {Name}")} and, in a flash, dozens of arms reach out and grab {PerspectiveText("you, forcibly banishing you", "them, foricbly banishing them")} to the void. The tear seals as quickly as it appeared.");
            IsBanished = true;
            if (!IsPlayer)
            {
                StatsTracker.EnemySentToVoid(this as Enemy);
            }
        }

        public virtual void Death(Character otherChar) { }

        public int CalculateStealChance(Character otherChar)
        {
            int stealChance = 20 + ((Dexterity - otherChar.Dexterity) * 5);
            return stealChance < 0 ? 0 : stealChance > 100 ? 100 : stealChance;
        }

        public string PrintStats() => $"S/D/M: {TH.SetColor(IsDead ? Colors.DarkGray : Colors.Red)}{Strength}{TH.SetColor()}/{TH.SetColor(IsDead ? Colors.DarkGray : Colors.Green)}{Dexterity}{TH.SetColor()}/{TH.SetColor(IsDead ? Colors.DarkGray : Colors.Blue)}{Magic}{TH.SetColor()}";
    }

    //Player
    public class Player : Character
    {
        public bool SeesIntent { get; set; } = false;
        public bool CanSeeIntent()
        {
            return SeesIntent;
        }

        public string[] GetActionsAsChoice()
        {
            List<string> choices = new List<string>();

            foreach (CombatAction a in Actions)
            {
                choices.Add($"{a.Name}{a.PrintDamage()}");
            }

            return choices.ToArray();
        }

        public bool TryBuyItem(eUsableItem itemToBuy, Character merchant)
        {
            ItemUsable boughtItem = merchant.UsableItems[itemToBuy].FirstOrDefault();

            if (Gold < boughtItem.BuyPrice)
            {
                TH.WriteL($"You cannot afford the {boughtItem.Name}.");
                return false;
            }

            string[] mr = [$"smiles briefly as you grab the {boughtItem.Name} before looking back down at their book.",
                "utters a brief noise of what you percieve to be gratitude.",
                "grabs your gold nonchalantly and nods as their attention doesn't leave the book they're invested in.",
                "glances up at you, \"Thanks, traveler.\"",
                $"stares at you blankly as you take the {boughtItem.Name}. Is this... their way of trying to sell you on another item to buy?"];

            merchant.RemoveItem(boughtItem);
            GiveUsableItem(boughtItem);

            merchant.Gold += boughtItem.BuyPrice;
            Gold -= boughtItem.BuyPrice;

            TH.WriteL($"You purchased the {boughtItem.Name} for {TH.SetColor(Colors.Yellow)}{boughtItem.BuyPrice} gold{TH.SetColor()}. The {merchant.Name} {Helper.GetRandomItemFromArray(mr)}");

            return true;
        }
    }

    //ENEMY
    public class Enemy : Character
    {
        public eEnemy eEnemy { get; set; }
        private UsableItemSpawn[] PossibleItems { get; set; } = [];
        private int PossibleGold { get; set; }
        public int EvasionChance { get; set; }
        public string Intent { get; set; }
        public bool ExhaustedSearch { get; set; }

        public Enemy() { }
        public Enemy(eEnemy enemy, string name = "", int health = 0, int possibleGold = 0, UsableItemSpawn[] possibleItems = null,
                    List<CombatAction> actions = null, int evasionChance = 0, int strength = 1, int dexterity = 1, int magic = 1, int armor = 0, bool isMerchant = false)
        {
            eEnemy = enemy;
            Name = name;
            MaxHealth = health;
            Health = health;
            PossibleGold = possibleGold;
            PossibleItems = possibleItems == null ? [] : possibleItems;
            Actions = actions == null ? new() : actions;
            EvasionChance = evasionChance;
            Strength = strength;
            Dexterity = dexterity;
            Magic = magic;
            Armor = armor;
            IsMerchant = isMerchant;
        }

        public Enemy CreateCopy()
        {
            Enemy e = new Enemy()
            {
                eEnemy = eEnemy,
                Name = _Name,
                Health = Health,
                MaxHealth = MaxHealth,
                Actions = Actions,
                EvasionChance = EvasionChance,
                Strength = Strength,
                Dexterity = Dexterity,
                Magic = Magic,
                Armor = Armor,
                IsMerchant = IsMerchant,
            };

            e.GiveItems(ItemFactory.SpawnUsableItems(PossibleItems));
            e.Gold = Helper.GetRandomNumber(PossibleGold);

            return e;
        }

        public override void Death(Character otherChar)
        {

            StatsTracker.EnemyDefeated(this);
            if (otherChar != null && !otherChar.IsDead && Gold > 0)
            {
                Game.PrintHud();
                TH.WriteL($"The {Name} drops a {TH.SetColor(Colors.Yellow)}gold pouch{TH.SetColor()}. You grab it and take the {TH.SetColor(Colors.Yellow)}{Gold} gold{TH.SetColor()} in it.");
                TH.WaitForEnter();
                TH.Clear();
                StatsTracker.TotalGoldEarned += Gold;
                otherChar.Gold += Gold;
            }

            if (otherChar is Player p && StatsTracker.Victims.Count % 5 == 0)
            {
                GameEvents.RaisePlayerSkillGain();
            }
        }

        public bool DetermineIfUseHeal(out eUsableItem item, out bool endsTurn)
        {
            item = eUsableItem.Nothing; endsTurn = false;

            if (Health == MaxHealth)
            {
                return false;
            }

            if (Health == 1)
            {
                if (UsableItems.ContainsKey(eUsableItem.MediumHealthPotion))
                {
                    item = eUsableItem.MediumHealthPotion;
                }

                if (UsableItems.ContainsKey(eUsableItem.SmallHealthPotion))
                {
                    item = eUsableItem.SmallHealthPotion;
                }
            }

            if (MaxHealth - Health <= 2 && UsableItems.ContainsKey(eUsableItem.SmallHealthPotion))
            {
                item = eUsableItem.SmallHealthPotion;
            }

            if (UsableItems.ContainsKey(eUsableItem.SmallHealthPotion) && UsableItems.ContainsKey(eUsableItem.MediumHealthPotion))
            {
                item = Helper.GetRandomItemFromArray([eUsableItem.SmallHealthPotion, eUsableItem.MediumHealthPotion]);
            }

            item = (UsableItems.ContainsKey(eUsableItem.SmallHealthPotion)) ? eUsableItem.SmallHealthPotion : UsableItems.ContainsKey(eUsableItem.MediumHealthPotion) ? eUsableItem.MediumHealthPotion : eUsableItem.Nothing;

            if (item != eUsableItem.Nothing)
            {
                endsTurn = !UsableItems[item].FirstOrDefault().InstantUse;
                return true;
            }

            return false;
        }

        public string[] GetItemsForSaleAsChoice(out eUsableItem[] itemsToChooseFrom)
        {
            List<string> choices = new(); List<eUsableItem> itemChoices = new();

            foreach (KeyValuePair<eUsableItem, List<ItemUsable>> kvp in UsableItems)
            {
                choices.Add($"({TH.SetColor(Colors.Yellow)}{kvp.Value[0].BuyPrice}g{TH.SetColor()}) {kvp.Value[0].Name} [x{kvp.Value.Count}] - {kvp.Value[0].Description}");
                itemChoices.Add(kvp.Key);
            }

            itemsToChooseFrom = itemChoices.ToArray();
            return choices.ToArray();
        }

        public string PrintIntent(bool canSeeIntent)
        {
            return (IsDead || IsMerchant) ? "" : $"| Intent: [{(canSeeIntent ? Intent : "?")}]";
        }
    }

    public enum eEnemy { Goblin, Wolf, MushroomKnight, Skeleton, Bandit, PossessedSoldier, Merchant }
    public static class EnemyFactory
    {
        static List<Enemy> Enemies = new() {
            new(eEnemy.Goblin, name: "Goblin", health: 3, possibleGold: 15, evasionChance: 40, strength: 1, dexterity: 1, magic: 1,
                possibleItems: [new(eUsableItem.SmallHealthPotion,70)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.Wolf, name: "Wolf", health: 4, possibleGold: 30, evasionChance: 25, strength: 2,
                possibleItems: [new(eUsableItem.SmallHealthPotion, 80)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.MushroomKnight, name: "Mushroom Knight", health: 2, possibleGold: 60, evasionChance: 35, magic: 4, armor: 2,
                possibleItems: [new(eUsableItem.SmallHealthPotion,30),new(eUsableItem.SmallHealthPotion,30),new(eUsableItem.SmallHealthPotion,100)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.Skeleton, name: "Skeleton", health: 3, possibleGold: 60, evasionChance: 35, magic: 2, armor: 1, strength: 2,
                possibleItems: [new(eUsableItem.SmallHealthPotion,10),new(eUsableItem.SmallHealthPotion,30),new(eUsableItem.SmallHealthPotion,80)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.Bandit, name: "Bandit", health: 2, possibleGold: 100, evasionChance: 10, dexterity: 6, armor: 2,
                possibleItems: [new(eUsableItem.SmallHealthPotion,30),new(eUsableItem.SmallHealthPotion,100)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.PossessedSoldier, name: "Possessed Soldier", health: 5, possibleGold: 200, evasionChance: 10, strength: 2, dexterity: 2, armor: 3,
                possibleItems: [new(eUsableItem.SmallHealthPotion,30),new(eUsableItem.SmallHealthPotion,20), new (eUsableItem.SmallHealthPotion,10)],
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
            new(eEnemy.Merchant, name: "Merchant", health: 6, possibleGold: 600, evasionChance: 5, strength: 4, dexterity: 4, armor: 3, isMerchant:true,
                possibleItems: ItemFactory.PossibleMerchantItems,
                actions: ActionsFactory.CreateActions([eAction.Strike, eAction.Counter, eAction.Spell])),
        };

        public static Enemy SpawnEnemy(eEnemy enemy) => Enemies.FirstOrDefault(e => e.eEnemy == enemy)?.CreateCopy();
        public static Enemy SpawnRandomEnemy(EnemySpawn[] possibleEnemies)
        {
            foreach (EnemySpawn es in possibleEnemies)
            {
                if (Helper.RollDice(es.SpawnChance)) { return SpawnEnemy(es.Enemy); }
            }

            return null;
        }
    }

    public class EnemySpawn : Spawnable
    {
        public eEnemy Enemy { get; set; }
        public int SpawnChance { get; set; }

        public EnemySpawn(eEnemy enemy, int spawnChance)
        {
            Enemy = enemy;
            SpawnChance = spawnChance;
        }
    }

    // ITEMS
    public abstract class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public eItemType ItemType { get; set; }
        public int SellPrice { get; set; }
        public int BuyPrice { get; set; }
        public abstract bool Use(Character c, Character t);
    }

    //Usable Item
    public class ItemUsable : Item
    {
        public eUsableItem UsableItem { get; set; }
        public bool InstantUse { get; set; } = false;
        public ItemUsable() => ItemType = eItemType.Usable;

        public ItemUsable(eUsableItem eUsableItem, string name, string description, bool instantUse = false, int sellPrice = 0, int buyPrice = 0)
        {
            ItemType = eItemType.Usable;
            UsableItem = eUsableItem;
            Name = name;
            Description = description;
            InstantUse = instantUse;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
        }

        public ItemUsable CreateCopy()
        {
            char c = Colors.Default;
            switch (UsableItem)
            {
                case eUsableItem.SmallHealthPotion:
                case eUsableItem.MediumHealthPotion:
                    c = Colors.DarkGreen;
                    break;
                case eUsableItem.ArmorShard:
                    c = Colors.Cyan;
                    break;
                case eUsableItem.IntentPotion:
                    c = Colors.Blue;
                    break;
                case eUsableItem.BanishmentSpellScroll:
                case eUsableItem.ShopPortal:
                    c = Colors.DarkMagenta;
                    break;
            }

            return new ItemUsable()
            {
                Name = $"{TH.SetColor(c)}{Name}{TH.SetColor()}",
                Description = Description,
                ItemType = ItemType,
                UsableItem = UsableItem,
                InstantUse = InstantUse,
                BuyPrice = BuyPrice,
                SellPrice = SellPrice
            };
        }

        public override bool Use(Character c, Character t)
        {
            if (t == null)
            {
                TH.WriteL($"There's no enemy to use that item on.");
                return false;
            }
            else if (t.IsDead)
            {
                TH.WriteL($"You can't use that item on a dead enemy.");
                return false;
            }
            else if (t.Health == t.MaxHealth && ItemFactory.HealingItems.Contains(this.UsableItem))
            {
                TH.WriteL($"{t.PerspectiveText("You", $"The {t.Name}")} {t.PerspectiveText("are", "is")} already at full health.");
                return false;
            }

            if (UsableItem == eUsableItem.BanishmentSpellScroll && (Game.CurrentRoom.Enemy == null || Game.CurrentRoom.Enemy.IsDead || Game.CurrentRoom.Enemy.IsMerchant))
            {
                TH.WriteL("It'd be a waste to use this item now.");
                return false;
            }

            TH.WriteL($"{c.PerspectiveText("You", $"The {c.Name}")} used a {Name}.");

            switch (UsableItem)
            {
                case eUsableItem.SmallHealthPotion:
                    t.Heal(2);
                    break;
                case eUsableItem.MediumHealthPotion:
                    t.Heal(4);
                    break;
                case eUsableItem.ArmorShard:
                    t.AddArmor(2);
                    break;
                case eUsableItem.BanishmentSpellScroll:
                    t.Banish();
                    break;
                case eUsableItem.IntentPotion:
                    if (Game.CurrentRoom.Enemy == null || Game.CurrentRoom.Enemy.IsDead)
                    {
                        TH.WriteL("It'd be a waste to use this item now.");
                    }
                    if (t is Player p)
                    {
                        p.SeesIntent = true;
                    }
                    break;
            }

            c.RemoveItem(this);
            return true;
        }
    }

    public enum eItemType { Equippable, Usable }
    public class UsableItemSpawn : Spawnable
    {
        public eUsableItem UsableItem { get; set; }
        public int SpawnChance { get; set; }

        public UsableItemSpawn(eUsableItem usableItem, int spawnChance)
        {
            UsableItem = usableItem;
            SpawnChance = spawnChance;
        }
    }

    public enum eUsableItem { SmallHealthPotion, MediumHealthPotion, ShopPortal, BanishmentSpellScroll, ArmorShard, IntentPotion, Nothing }
    public static class ItemFactory
    {
        public static eUsableItem[] SelfUseItems = [eUsableItem.SmallHealthPotion, eUsableItem.MediumHealthPotion, eUsableItem.ShopPortal, eUsableItem.ArmorShard, eUsableItem.IntentPotion];
        public static eUsableItem[] HealingItems = [eUsableItem.SmallHealthPotion, eUsableItem.MediumHealthPotion];

        public static UsableItemSpawn[] PossibleMerchantItems = [
            new(eUsableItem.BanishmentSpellScroll, 5),
            new(eUsableItem.ShopPortal, 15),
            new(eUsableItem.MediumHealthPotion, 50),
            new(eUsableItem.MediumHealthPotion, 50),
            new(eUsableItem.MediumHealthPotion, 50),
            new(eUsableItem.ArmorShard, 50),
            new(eUsableItem.ArmorShard, 50),
            new(eUsableItem.ArmorShard, 50),
            new(eUsableItem.SmallHealthPotion, 80),
            new(eUsableItem.SmallHealthPotion, 80),
            new(eUsableItem.SmallHealthPotion, 80),
            new(eUsableItem.SmallHealthPotion, 80)
        ];

        static List<ItemUsable> UsableItems = new List<ItemUsable>() {
            new(eUsableItem.SmallHealthPotion, "Herb", "Instantly Heals 2 HP.", true, buyPrice: 30),
            new(eUsableItem.MediumHealthPotion, "Potion", "Heals 5 HP.", buyPrice: 50),
            new(eUsableItem.ShopPortal, "Shop Portal", "Instantly teleports to a shop.", true, buyPrice: 100),
            new(eUsableItem.BanishmentSpellScroll, "Banishment Spell Scroll", "Instantly teleport an enemy to the void.", buyPrice: 250),
            new(eUsableItem.ArmorShard, "Armor Shard", "Instantly grants 2 armor", true, buyPrice: 40),
            new(eUsableItem.IntentPotion, "Potion of Intent", "Allows you to see an enemy's intent for the current turn.", true, buyPrice: 60)
        };

        public static ItemUsable CreateUsableItem(eUsableItem item) => UsableItems.FirstOrDefault(i => i.UsableItem == item)?.CreateCopy();

        public static List<ItemUsable> SpawnUsableItems(UsableItemSpawn[] spawns)
        {
            List<ItemUsable> items = new List<ItemUsable>();

            foreach (UsableItemSpawn s in spawns)
            {
                if (Helper.RollDice(s.SpawnChance)) { items.Add(UsableItems.FirstOrDefault(i => i.UsableItem == s.UsableItem)?.CreateCopy()); };
            }

            return items;
        }
    }

    // GENERIC INTERFACES
    public interface Spawnable { public int SpawnChance { get; set; } }

    //HELPERS
    public static class Helper
    {
        //  Hitchance 0-100 Example: (40% chance = 40) if you roll 0-40 you are a success, otherwise fail
        public static bool RollDice(int hitChance) => hitChance >= new Random().Next(0, 101);
        public static int GetVariantValue(int variance) => new Random().Next(-(variance), variance + 1);
        public static int GetRandomNumber(int maxValue) => new Random().Next(0, maxValue + 1);
        public static int GetRandomNumberFromRange(int min, int maxValue) => new Random().Next(min, maxValue + 1);
        public static T GetRandomItemFromArray<T>(T[] items)
        {
            if (items != null || items.Length == 0)
            {
                return items[new Random().Next(items.Length)];
            }
            else
            {
                throw new Exception("Items array is null or empty. We need at least one item in the array.");
            }
        }
    }

    public static class Colors
    {
        public static char Black = '0';
        public static char Blue = '1';
        public static char Cyan = '2';
        public static char DarkBlue = '3';
        public static char DarkCyan = '4';
        public static char DarkGray = '5';
        public static char DarkGreen = '6';
        public static char DarkMagenta = '7';
        public static char DarkRed = '8';
        public static char DarkYellow = '9';
        public static char Gray = 'a';
        public static char Green = 'b';
        public static char Magenta = 'c';
        public static char Red = 'd';
        public static char White = 'e';
        public static char Yellow = 'f';
        public static char Default = 'g';
    }

    //Text Handler
    public static class TH
    {
        public static Dictionary<char, ConsoleColor> ConsoleColors { get; set; } = new() {
            {Colors.Black, ConsoleColor.Black},
            {Colors.Blue, ConsoleColor.Blue},
            {Colors.Cyan, ConsoleColor.Cyan},
            {Colors.DarkBlue, ConsoleColor.DarkBlue},
            {Colors.DarkCyan, ConsoleColor.DarkCyan},
            {Colors.DarkGray, ConsoleColor.DarkGray},
            {Colors.DarkGreen, ConsoleColor.DarkGreen},
            {Colors.DarkMagenta, ConsoleColor.DarkMagenta},
            {Colors.DarkRed, ConsoleColor.DarkRed},
            {Colors.DarkYellow, ConsoleColor.DarkYellow},
            {Colors.Gray, ConsoleColor.Gray},
            {Colors.Green, ConsoleColor.Green},
            {Colors.Magenta, ConsoleColor.Magenta},
            {Colors.Red, ConsoleColor.Red},
            {Colors.White, ConsoleColor.White},
            {Colors.Yellow, ConsoleColor.Yellow}
        };

        public static string SetColor(char color = 'g')
        {
            return "`" + color;
        }

        public static string CT(char color, string text)
        {
            return $"{SetColor(color)}{text}{SetColor()}";
        }

        private static List<ConsoleKey> NumberKeys = new() { ConsoleKey.D1, ConsoleKey.D2, ConsoleKey.D3, ConsoleKey.D4, ConsoleKey.D5, ConsoleKey.D6, ConsoleKey.D7, ConsoleKey.D8, ConsoleKey.D9, ConsoleKey.D0 };

        public static string PromptAndGetTextAnswer(string question)
        {
            WriteL($"{question}");
            Write(">");
            return Console.ReadLine();
        }

        public static bool Menu(string question, string[] choices, out int choice, bool includeCancel = true, bool indent = false)
        {
            bool choiceMade = false;
            string ind = (indent) ? "\t\t" : "";
            Console.CursorVisible = false;
            WriteL(ind + question, true);
            int check = (includeCancel) ? choices.Length : choices.Length - 1;


            for (int i = 0; i < choices.Length; i++)
            {
                WriteL($"{ind}{i + 1}. {choices[i]}", true);
            }

            if (includeCancel)
            {
                WriteL($"{ind}{choices.Length + 1}. Cancel", true);
            }

            do
            {
                choice = NumberKeys.IndexOf(Console.ReadKey(true).Key);
                choice = (choice > check) ? -1 : choice;
            }
            while (choice < 0);
            TH.Clear();
            Console.CursorVisible = true;

            if (choice < choices.Length)
            {
                choiceMade = true;
            }

            return choiceMade;
        }

        public static void WriteStoryText(string[] lines, bool clearAfterEachLine = true, string clearDelim = "|")
        {
            bool skipCutscene = false;

            foreach (string line in lines)
            {
                if (skipCutscene)
                {
                    break;
                }

                if (!clearAfterEachLine)
                {
                    if (line == clearDelim) { WaitForEnterStory(out skipCutscene); Clear(); continue; }
                    WriteL(line);
                }
                else
                {
                    WriteL(line);
                    WaitForEnterStory(out skipCutscene);
                    Clear();
                }
            }
        }

        public static void Write(string text, bool instantText = false)
        {
            bool lookForColor = false;

            foreach (var letter in text)
            {
                if (letter == '`')
                {
                    lookForColor = true;
                }
                else if (lookForColor)
                {
                    if (ConsoleColors.ContainsKey(letter))
                    {
                        Console.ForegroundColor = ConsoleColors[letter];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    lookForColor = false;
                }
                else
                {
                    Console.Write(letter);
                    if (!instantText) Thread.Sleep(5);
                }
            }

            if (lookForColor)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteL(string text, bool instantText = false)
        {

            Write(text, instantText);

            Console.WriteLine();
            if (!instantText)
            {
                Thread.Sleep(250);
            }
        }

        public static void WaitAndClear(string text = "[Press Enter To Continue]")
        {
            WaitForEnter(text);
            Clear();
        }

        public static void WaitForEnter(string text = "[Press Enter To Continue]")
        {
            bool enterPressed = false;

            Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ConsoleColor.White;

            Write($"{text}", true);
            Console.CursorVisible = false;
            Console.ResetColor();

            while (!enterPressed)
            {
                enterPressed = Console.ReadKey(true).Key == ConsoleKey.Enter;
            }

            Console.CursorVisible = true;
        }

        public static void WaitForEnterStory(out bool skipCutscene)
        {
            bool enterPressed = false; skipCutscene = false;
            Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ConsoleColor.White;

            Write("[Press Enter To Continue, Esc to Skip Story]", true);
            Console.CursorVisible = false;
            Console.ResetColor();

            while (!enterPressed)
            {
                ConsoleKey keyPressed = Console.ReadKey(true).Key;
                if (keyPressed == ConsoleKey.Escape) { skipCutscene = true; break; }
                enterPressed = keyPressed == ConsoleKey.Enter;
            }

            Console.CursorVisible = true;
        }

        public static void Clear() => Console.Clear();
    }

    public static class StatsTracker
    {
        public static int Turns { get; set; }
        public static int TotalGoldEarned { get; set; }
        public static Dictionary<Enemy, int> Victims { get; } = new();
        public static Dictionary<Enemy, int> InTheVoid { get; } = new();
        public static Dictionary<Room, int> RoomsVisited { get; } = new();

        public static void Reset()
        {
            Turns = 0;
            RoomsVisited.Clear();
            TotalGoldEarned = 0;
            Victims.Clear();
            InTheVoid.Clear();
        }

        public static void EnemyDefeated(Enemy e) => AddEnemyToDictionary(e, Victims);
        public static void EnemySentToVoid(Enemy e) => AddEnemyToDictionary(e, InTheVoid);

        public static void AddEnemyToDictionary(Enemy e, Dictionary<Enemy, int> d)
        {
            bool exists = false;
            foreach (Enemy k in d.Keys)
            {
                if (k.eEnemy == e.eEnemy)
                {
                    d[k]++;
                    exists = true;
                }
            }

            if (!exists)
            {
                d.Add(e, 1);
            }
        }

        public static void RoomVisited(Room r) => AddRoomToDictionary(r, RoomsVisited);
        public static void AddRoomToDictionary(Room r, Dictionary<Room, int> d)
        {
            bool exists = false;
            foreach (Room k in d.Keys)
            {
                if (k.Name == r.Name)
                {
                    d[k]++;
                    exists = true;
                }
            }

            if (!exists)
            {
                d.Add(r, 1);
            }
        }

        public static string PrintEnemyList(Dictionary<Enemy, int> d)
        {
            string list = (d.Count > 0) ? "\n" : "";

            foreach (Enemy e in d.Keys)
            {
                list += $" - {e.Name} (x{d[e]})\n";
            }

            return list;
        }

        public static string PrintRoomsList(Dictionary<Room, int> d)
        {
            string list = (d.Count > 0) ? "\n" : "";

            foreach (Room r in d.Keys)
            {
                list += $" - {r.Name} (x{d[r]})\n";
            }

            return list;
        }

        public static void PrintFinalStats()
        {
            TH.WriteL($"Final Stats:\n- Total Turns: {Turns}\n- Rooms Visited: {RoomsVisited.Count}{PrintRoomsList(RoomsVisited)}\n- Enemies Slain: {Victims.Count}{PrintEnemyList(Victims)}\n- Enemies Sent to the Void: {InTheVoid.Count}{PrintEnemyList(InTheVoid)}\n- Total Gold Earned: {TH.SetColor(Colors.Yellow)}{TotalGoldEarned}g{TH.SetColor()}");
        }
    }
}