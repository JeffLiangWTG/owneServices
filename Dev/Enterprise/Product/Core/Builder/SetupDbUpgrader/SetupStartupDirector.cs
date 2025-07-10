using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BuildTools;
using Enterprise.Builder.Generator;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	public static class SetupStartupDirector
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				var parsedArgs = Initialize();
				return RunAction(parsedArgs);
			}
			catch (Exception e)
			{
				LogMessage(e.ToString(), "Unexpected Error.");
				return 1;
			}

			CommandLineArgs Initialize()
			{
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();
				ObjectFactory.GetType<Integration.Initialisation.IInitialiser>().GetMethod("InitialiseWinForms").Invoke(null, new object[] { Type.Missing });

				var arguments = CommandLineArgs.ParseCommandLineArguments(args) ?? throw new ArgumentNullException(nameof(args), "Invalid arguments in GenerateDbUpgraderResources");

				if (string.IsNullOrEmpty(arguments.CWShared) || !Directory.Exists(arguments.CWShared))
				{
					throw new ArgumentNullException(nameof(args), "No valid CWShared path specified.");
				}
				return arguments;
			}

			int RunAction(CommandLineArgs arguments)
			{
				SourceControl.WithAdditionalRepository(arguments.CWShared);

				if (string.IsNullOrEmpty(arguments.SetupAction) && string.IsNullOrEmpty(arguments.SourceSafeAction))
				{
					System.Windows.Forms.Application.Run(new SetupForm(arguments.CWShared));
					return 0;
				}
				else
				{
					return PerformSetupAction(arguments); //DataRegenAction can be empty and will preserve prior behaviour
				}
			}
		}

		static int PerformSetupAction(CommandLineArgs arguments)
		{
			int result = 1;
			bool successful = false;
			bool isAutoRegenSetupAction = IsAutoRegen(arguments.SetupAction);

			switch (arguments.SourceSafeAction)
			{
				case CommandLineArgs.SSUndoCheckOut:
					successful = UndoCheckOutUpgrader();

					break;

				case CommandLineArgs.SSCheckIn:
					if (isAutoRegenSetupAction)
					{
						// Instead of checking in files, CallSetupNewSchema will add DbUpgrader
						// files to list of files to be checked in during an AutoRegen (atomic regen check-in)
						successful = true;
					}
					else
					{
						successful = false;
					}

					break;

				case CommandLineArgs.SSCheckOut:
					var flags = RegenFlags.Compile;

					if (arguments.SetupAction == CommandLineArgs.AutoRegenMinor || arguments.SetupAction == CommandLineArgs.BumpVersionMinor)
					{
						flags |= RegenFlags.MinorVersion;
					}

					if (IsBumpVersion(arguments.SetupAction))
					{
						flags |= RegenFlags.BumpVersionOnly;
					}

					if (IsMerge(arguments.SetupAction))
					{
						flags |= RegenFlags.Merge;
					}

					if (arguments.ShowGui)
					{
						using (SetupForm batchSetupForm = OpenSetupFormForBatch(arguments.CWShared))
						{
							successful = batchSetupForm.DoSetup(flags, out var errorMsg);

							if (!successful)
							{
								LogMessage(errorMsg, "Process completed with errors");
							}
						}
					}
					else
					{
						ConsoleActivator.EnsureConsole();
						var engine = new RegenEngine();

						successful = engine.DoSetup(consoleLogger, flags, arguments.CWShared, out var errorMsg);

						if (!successful)
						{
							consoleLogger.AddReportHeader(errorMsg);
						}
					}

					break;

				default:
					LogMessage($"Invalid Source Control option [" + arguments.SourceSafeAction + "]", "Fail to run action");
					break;
			}

			if (successful)
			{
				// If it's AutoRegen, continue the process with generation of code files
				if (isAutoRegenSetupAction)
				{
					result = CallSetupNewSchema(CommandLineArgs.AutoRegenMajor, arguments.SourceSafeAction, arguments.DataRegenAction, arguments.CWShared, arguments.ShowGui);
				}
				else
				{
					LogMessage("Action Completed", "Process completed successfully");
					result = 0;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		static SetupForm OpenSetupFormForBatch(string cwShared)
		{
			SetupForm resultForm = new SetupForm(cwShared);

			resultForm.HideActionControls();
			resultForm.Show();
			System.Windows.Forms.Application.DoEvents();

			return resultForm;
		}

		static List<string> GetUpgraderFiles()
		{
			List<string> result = new List<string>();

			SetupController setupUpg = new SetupController();
			setupUpg.OnTaskFailed += new SetupUpgraderEvent(OnSetupTaskFailed);

			result.AddRange(setupUpg.GetUpgraderFilesToCheckin());

			return result;
		}

		static bool UndoCheckOutUpgrader()
		{
			SetupController setupUpg = new SetupController();
			setupUpg.OnTaskFailed += new SetupUpgraderEvent(OnSetupTaskFailed);

			return setupUpg.UndoCheckOutUpgraderFiles();
		}

		static int CallSetupNewSchema(string mainAction, string sourceSafeAction, string dataRegenAction, string cwshared, bool showGui)
		{
			using (var outputDirectory = new GeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, BuildConstants.LocalEnterprisePath, cwshared))
			{
				GeneratorEntryPoint generatorEntryPoint = new GeneratorEntryPoint(outputDirectory);

				// if it's Auto Regen - add Upgrader files to CheckinLog
				if (IsAutoRegen(mainAction) && sourceSafeAction == CommandLineArgs.SSCheckIn)
				{
					generatorEntryPoint.AddSpecifiedFilesToCheckinLog(outputDirectory.CheckinLog, GetUpgraderFiles());
				}

				return generatorEntryPoint.Execute(new GeneratorArguments(
					serverName: GetMachineNameAndInstanceName(),
					databaseName: GeneratorEntryPoint.AutoRegenDb,
					option: showGui ? CommandLineOptions.SetupNewSchemaAutoRegen : CommandLineOptions.CommandLineSetupNewSchemaAutoRegen,
					argument: sourceSafeAction,
					argument2: dataRegenAction));
			}
		}

		static string GetMachineNameAndInstanceName()
		{
			var machine = System.Environment.MachineName;
			var result = machine;
			if (File.Exists("instance.txt"))
			{
				var instance = File.ReadAllText("instance.txt");
				result = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", machine, instance);
			}
			consoleLogger.LogLineRaw("Using database server: " + result);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "This is an internal developer tool.")]
		public static void LogMessage(string message, string caption)
		{
			consoleLogger.LogLineRaw(message);
			MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
		}

		[ThreadSafe]
		static readonly ILogger consoleLogger = new ConsoleLogger();

		static bool IsAutoRegen(string argument) => argument == CommandLineArgs.AutoRegenMajor || argument == CommandLineArgs.AutoRegenMinor || argument == CommandLineArgs.Merge;
		static bool IsMerge(string argument) => argument == CommandLineArgs.Merge;
		static bool IsBumpVersion(string argument) => argument == CommandLineArgs.BumpVersionMajor || argument == CommandLineArgs.BumpVersionMinor;
		static void OnSetupTaskFailed(string message) => LogMessage(message, "Fail to run action");
	}
}
