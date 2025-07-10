using System;
using System.IO;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Builder.Generator
{
	public static class GeneratorStartupDirector
	{
		[STAThread]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		static int Main(string[] args)
		{
			try
			{
				var arguments = GeneratorArguments.Parse(args);

				if (!string.IsNullOrEmpty(arguments.CWSharedPath))
				{
					if (!Directory.Exists(arguments.CWSharedPath))
					{
						throw new ArgumentException("The specified CargoWise/Shared path is not a directory.");
					}

					SourceControl.WithAdditionalRepository(arguments.CWSharedPath);
				}
				else if (RequiresCWShared(arguments.Option))
				{
					throw new ArgumentException("CargoWise/Shared path must be specified.");
				}

				var outputDirectory = new GeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, BuildConstants.LocalEnterprisePath, arguments.CWSharedPath);
				Db.DisableSchemaVersionCheckPermanently();
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();
				ObjectFactory.GetType<Integration.Initialisation.IInitialiser>().GetMethod("InitialiseWinForms").Invoke(null, new object[] { Type.Missing });
				return new GeneratorEntryPoint(outputDirectory).Execute(arguments);
			}
#pragma warning disable ENT0001
			catch (Exception ex) // IsCriticalExceptionIsHandled Reason = Top level with console line out error.
#pragma warning restore ENT0001
			{
				ConsoleActivator.EnsureConsole();
				Console.Error.WriteLine(ex.ToString());
				return -1;
			}
		}

		static bool RequiresCWShared(string option)
		{
			if (string.IsNullOrEmpty(option))
			{
				// If it's needed, the GUI can ask the user for it.
				return false;
			}

			return Match(option, CommandLineOptions.CommandLineSetupNewSchemaAutoRegen)
				|| Match(option, CommandLineOptions.GenerateSchemaColumnList)
				|| Match(option, CommandLineOptions.GenerateBizObjectsForSolution)
				|| Match(option, CommandLineOptions.GenerateOneBizObj)
				|| Match(option, CommandLineOptions.SetupNewSchemaAutoRegen)
				|| Match(option, CommandLineOptions.GenerateModelViewObjects);
		}

		static bool Match(string x, string y) => x.Equals(y, StringComparison.OrdinalIgnoreCase);
	}
}
