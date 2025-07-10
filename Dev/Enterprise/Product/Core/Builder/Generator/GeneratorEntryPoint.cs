using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.Builder.Generator.RestoreDatabase;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Builder.Generator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
	public class GeneratorEntryPoint
	{
		public GeneratorEntryPoint(GeneratorOutputDirectory outputDirectory)
		{
			this.outputDirectory = outputDirectory;
		}

		readonly GeneratorOutputDirectory outputDirectory;

		#region ParseArgsAndExecute

		public int Execute(GeneratorArguments arguments)
		{
			Db.InitializeDatabaseDetails(
				serverName: Db.GetMachineNameIfLocal(arguments.ServerName),
				databaseName: arguments.DatabaseName);

			if (string.IsNullOrEmpty(arguments.Option))
			{
				System.Windows.Forms.Application.Run(new GeneratorForm(outputDirectory));
				return 0;
			}

			return PerformAction(arguments);
		}

		#region Redirect Output

		protected void AddResultLine(string line)
		{
			GenerationResults += line.Replace('\t', ' ') + System.Environment.NewLine;
		}

		protected string GenerationResults = "";

		#endregion

		#endregion

		#region PerformAction

		protected int PerformAction(GeneratorArguments arguments)
		{
			var uppercaseOption = arguments.Option.ToUpperInvariant();

			switch (uppercaseOption)
			{
				case CommandLineOptions.SetupNewSchemaAutoRegen:
					return DoAutoRegen(arguments.Argument, outputDirectory, arguments.Argument2, showGui: true);

				case CommandLineOptions.CommandLineSetupNewSchemaAutoRegen:
					return DoAutoRegen(arguments.Argument, outputDirectory, arguments.Argument2, showGui: false);

				case CommandLineOptions.RestoreDatabase:
					return DoRestoreDatabase();

				case CommandLineOptions.CommandLineRestoreDatabase:
					return DoCommandLineRestoreDatabase();

				case CommandLineOptions.GenerateOneBizObj:
					GenerateSpecificFile(arguments.Argument);
					break;

				case CommandLineOptions.GenerateBizObjectsForSolution:
					GenerateFilesForSolution(arguments.Argument);
					break;

				case CommandLineOptions.GenerateSchemaColumnList:
					GenerateSchemaList();
					break;

				case CommandLineOptions.GenerateModelViewObjects:
					GenerateModelView(arguments.Argument);
					break;

				default:
					MessageBox.Show("Invalid option: " + arguments.Option);
					return ErrorReturnCode;
			}

			return 0;
		}

		#endregion

		#region DoAutoRegen

		public const string AutoRegenDb = "OdysseyAutoRegen";

		protected int DoAutoRegen(string sourceControlAction, GeneratorOutputDirectory directoryOut, string dataRegenType, bool showGui)
		{
			if (!sourceControlAction.Equals(SSCheckOut, StringComparison.OrdinalIgnoreCase))
			{
				MessageBox.Show(null, "Invalid AutoRegen argument: " + sourceControlAction, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
				return ErrorReturnCode;
			}

			RegenActions regenType = RegenActions.FullRegen;
			if (dataRegenType.IsNullOrEmpty() || dataRegenType.Equals(DataRegen, StringComparison.OrdinalIgnoreCase))
			{
				regenType = RegenActions.FullRegen;
			}
			else if (dataRegenType.Equals(NoDataRegen, StringComparison.OrdinalIgnoreCase))
			{
				regenType = RegenActions.SchemaRegen;
			}
			else if (dataRegenType.Equals(SkipSchemaRegen, StringComparison.OrdinalIgnoreCase))
			{
				regenType = RegenActions.DataRegen;
			}
			else
			{
				MessageBox.Show(null, "Invalid DataRegen argument: \"" + dataRegenType + "\"must be one of: \n \"" + DataRegen + "\" \n \"" + SkipSchemaRegen + "\" ", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
				return ErrorReturnCode;
			}

			return DoSetupNewSchemaAction(sourceControlAction, isAutoRegen: true, showGui, directoryOut, regenType);
		}

		#endregion

		#region DoSetupNewSchemaAction

		protected int DoSetupNewSchemaAction(string sourceControlAction, bool isAutoRegen, bool showGui, GeneratorOutputDirectory directoryOut, RegenActions regenType)
		{
			bool success = false;

			try
			{
				NewSchemaDirector director = new NewSchemaDirector(directoryOut);

				switch (sourceControlAction.ToUpperInvariant())
				{
					case SSUndoCheckOut:
						UndoCheckout(directoryOut);
						success = true;
						break;

					case SSMakeWritable:
						success = director.DoSetup(isAutoRegen, showGui, regenType);
						break;

					case SSCheckOut:
						success = director.DoSetup(isAutoRegen, showGui, regenType);
						break;

					default:
						MessageBox.Show(null, "Invalid setup argument: " + sourceControlAction, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

						success = false;
						break;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
#if DEBUG
				if (!NUnit.Framework.TestingState.IsRunningTests)
#endif
				{
					MessageBox.Show(null, e.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
				}
				success = false;
			}

			return (success) ? 0 : 1;
		}

		void UndoCheckout(GeneratorOutputDirectory outputDirectory)
		{
			AddExtraFilesToCheckinLog(outputDirectory.CheckinLog);
			outputDirectory.UndoCheckout();
		}

		void AddExtraFilesToCheckinLog(string checkinLogPath)
		{
			List<string> files = GetExistingCheckingLogFileList(checkinLogPath);

			AddDataUpgradeFilesToCheckinLog(files);
			AddDataPurgeScriptRepositoryToCheckinLog(files);
			AddBiConfigFilesToCheckinLog(files);

			File.WriteAllLines(checkinLogPath, files.ToArray());
		}

		public void AddSpecifiedFilesToCheckinLog(string checkinLogPath, List<string> specifiedFiles)
		{
			List<string> files = GetExistingCheckingLogFileList(checkinLogPath);

			files.AddRange(specifiedFiles);

			File.WriteAllLines(checkinLogPath, files.ToArray());
		}

		void AddDataPurgeScriptRepositoryToCheckinLog(List<string> files)
		{
			string[] purgeScriptFiles = Directory.GetFiles(Constants.DataPurgeScriptFolderPath, "*.cs");

			foreach (string purgeScriptFile in purgeScriptFiles)
			{
				if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(purgeScriptFile))
				{
					files.Add(purgeScriptFile);
				}
			}
		}

		void AddBiConfigFilesToCheckinLog(List<string> files)
		{
			foreach (string file in Constants.BiConfigurationFileCollection)
			{
				if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(file))
				{
					files.Add(file);
				}
			}

			if (Directory.Exists(Constants.BiScriptsFilePath))
			{
				foreach (string file in Directory.GetFiles(Constants.BiScriptsFilePath, "*", SearchOption.AllDirectories))
				{
					if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(file))
					{
						files.Add(file);
					}
				}
			}

			foreach (string registrationDirectory in Constants.BiRegistrationPathCollection)
			{
				if (Directory.Exists(registrationDirectory))
				{
					foreach (string file in Directory.GetFiles(registrationDirectory, "*", SearchOption.AllDirectories))
					{
						if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(file))
						{
							files.Add(file);
						}
					}
				}
			}
		}

		void AddDataUpgradeFilesToCheckinLog(List<string> files)
		{
			AllTasksSetupController dataTasksSetupController = AllTasksSetupController.New();

			if (dataTasksSetupController.IsCheckedOutByMe)
			{
				files.Add(dataTasksSetupController.DataVersionFile);
				files.AddRange(dataTasksSetupController.CheckedOutTaskDataFilePathList);
			}
		}

		List<string> GetExistingCheckingLogFileList(string checkinLogPath)
		{
			List<string> result = new List<string>();

			if (File.Exists(checkinLogPath))
			{
				string[] filesToCheckout = File.ReadAllLines(checkinLogPath);

				if (filesToCheckout != null && filesToCheckout.Length > 0)
				{
					result.AddRange(filesToCheckout);
				}
			}

			return result;
		}

		#endregion

		#region DoRestoreDatabase

		protected int DoRestoreDatabase()
		{
			return DbRestoreForm.OpenAndRun() ? 0 : 1;
		}

		protected int DoCommandLineRestoreDatabase()
		{
			var succeeded = CommandLineDatabaseRestorer.Run();
			return succeeded ? 0 : 1;
		}

		#endregion

		#region Generate Specific File

		protected void GenerateSpecificFile(string fileName)
		{
			try
			{
				var generator = new BizObjGenerator(outputDirectory);
				generator.AddReportLineEvent(new GeneratorEvent(AddResultLine));
				generator.AddErrorLineEvent(new GeneratorEvent(AddResultLine));
				generator.GenerateSpecificFile(fileName);
				outputDirectory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddResultLine(System.Environment.NewLine + "ERROR:" + System.Environment.NewLine + e);
			}

			if (!string.IsNullOrEmpty(GenerationResults) && !NUnit.Framework.TestingState.IsRunningTests)
			{
				ZFormModaliser.ShowDialogAndDispose(new ZMessageBoxWithFixedSizeWithoutMultilingualString(GenerationResults, "Regeneration of '" + fileName + "'..", MessageBoxButtons.OK, MessageBoxIcon.None));
			}
		}

		#endregion

		#region Generate files for Solution

		protected void GenerateFilesForSolution(string solutionName)
		{
			try
			{
				var generator = new BizObjGenerator(outputDirectory);
				generator.AddReportLineEvent(new GeneratorEvent(AddResultLine));
				generator.AddErrorLineEvent(new GeneratorEvent(AddResultLine));
				generator.GenerateAllFilesForSolution(solutionName);
				outputDirectory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddResultLine(System.Environment.NewLine + "ERROR:" + System.Environment.NewLine + e);
			}

			if (!string.IsNullOrEmpty(GenerationResults) && !NUnit.Framework.TestingState.IsRunningTests)
			{
				MessageBox.Show(GenerationResults, "Regeneration of all files in the " + solutionName + " solution..");
			}
		}

		#endregion

		#region Generate Schema List

		protected void GenerateSchemaList()
		{
			try
			{
				AutoSchemaGenerator generator = new AutoSchemaGenerator(outputDirectory);
				generator.AddReportLineEvent(new GeneratorEvent(AddResultLine));
				generator.AddErrorLineEvent(new GeneratorEvent(AddResultLine));
				generator.Generate();
				outputDirectory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddResultLine(System.Environment.NewLine + "ERROR:" + System.Environment.NewLine + e);
			}

			if (!string.IsNullOrEmpty(GenerationResults) && !NUnit.Framework.TestingState.IsRunningTests)
			{
				MessageBox.Show(GenerationResults, "Regeneration of Z Schema List..");
			}
		}

		#endregion

		#region Generate ModelViews

		void GenerateModelView(string filename)
		{
			var generator = new ModelViewObjectGenerator(outputDirectory);

			if (string.IsNullOrWhiteSpace(filename))
			{
				generator.Generate();
			}
			else
			{
				generator.GenerateSpecificFile(filename);
			}

			outputDirectory.Save();
		}

		#endregion

		#region Constants

		internal const int ErrorReturnCode = 1;
		internal const string DashCharacter = "-";

		internal const string SSMakeWritable = "-MAKEWRITABLE";
		internal const string SSCheckOut = "-CHECKOUT";
		internal const string SSUndoCheckOut = "-UNDOCHECKOUT";

		public const string DataRegen = "-WITHDATAREGEN";
		public const string NoDataRegen = "-NODATAREGEN";
		public const string SkipSchemaRegen = "-SKIPSCHEMAREGEN";

		#endregion
	}
}
