using System;
using System.IO;
using CargoWise.Bi.Development.Automation;
using CargoWise.Bi.Development.Common;
using CargoWise.Common;

namespace BiRegenSynchroniser
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Console utility usage message")]
		static int Main(string[] args)
		{
			try
			{
				var arguments = ParseArgument(args);

				if (arguments == null)
				{
					ConsoleLog(
@"Invalid parameter provided.
Usage:
    BiRegenSynchronizer -sync <path-to-cwshared>
    BiRegenSynchronizer -undo <path-to-cwshared>

-sync : Synchronise BI files with current schema
-undo : Undo check out of all BI files");
					return 1;
				}

				if (!Directory.Exists(arguments.CWSharedPath))
				{
					ConsoleLog("The specified CWShared path doesn't exist.");
					return 1;
				}

				MainCore(arguments);
				return 0;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ConsoleLog(ex.Message);
				ConsoleLog(ex.StackTrace);
				return 1;
			}
		}

		static void MainCore(CommandLineArgs arguments)
		{
			BiFiles.CWSharedPath = arguments.CWSharedPath;

			switch (arguments.Action)
			{
				case SyncAction.Sync:
					SynchroniseSchema();
					break;
				case SyncAction.Undo:
					UndoCheckout();
					break;
				default:
					break;
			}
		}

		static CommandLineArgs ParseArgument(string[] args)
		{
			if (args.Length != 2)
			{
				return null;
			}

			var action = SyncAction.Unknown;

			switch (args[0].ToUpperInvariant())
			{
				case "-SYNC":
					action = SyncAction.Sync;
					break;
				case "-UNDO":
					action = SyncAction.Undo;
					break;
				default:
					break;
			}

			return new CommandLineArgs(action, args[1]);
		}

		static void SynchroniseSchema()
		{
			var biManager = new BiManager(ConsoleLog);
			biManager.SchemaSynchronisation();
		}

		static void UndoCheckout()
		{
			var biManager = new BiManager(ConsoleLog);
			biManager.UndoCheckout();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "development code only")]
		static void ConsoleLog(string message)
		{
			Console.WriteLine(message);
		}

		enum SyncAction
		{
			Sync = 0,
			Undo,
			Unknown
		}

		sealed class CommandLineArgs
		{
			public CommandLineArgs(SyncAction action, string cwSharedPath)
			{
				Action = action;
				CWSharedPath = cwSharedPath;
			}

			public SyncAction Action { get; }
			public string CWSharedPath { get; }
		}
	}
}
