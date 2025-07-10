using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Definitions;
using Dat.Integration;
using Enterprise.Dat.Implementation.Preconditions;
using Enterprise.Startup;
using Enterprise.Upgrades.Installers;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation
{
	public class TestClient : ITestClient
	{
		public TestClient(TestAdapterContext adapterContext, ITaskLogger logger)
		{
			context = adapterContext;
			this.logger = logger;
		}

		public string[] CheckPrerequisites()
		{
			var errors = new List<string>();
			Installers.InstallAllDependencies(@"\\datfiles.wtg.zone\DAT\ServerInstallCD\Template\Distribution");

			var preconditions = new IPrecondition[]
			{
					new SqlServerDependentPrecondition(),
			};

			foreach (var precondition in preconditions)
			{
				if (!precondition.CheckPreconditionMet())
				{
					errors.Add(precondition.ErrorMessage);
				}
			}

			if (errors.Count == 0)
			{
				RestoreDB();
			}

			return errors.ToArray();
		}

		public int GetMachineCapabilities()
		{
			return (int)InstalledSoftwareDetection.GetInstalledSoftware();
		}

		public ITestRunner StartTestRunner(string sourcePath, string binPath)
		{
			// We use the location of the root build.xml file since the sourcePath parameter is the location of the current target under test. For submodules this is the location in the repository of its build.xml file.
			// However all tests assume a location relative to the root target. If we didn't do this, tests would need to work with different relative paths when run on DAT versus locally.
			var rootSourcePath = BuildXmlFileHelper.GetRootBuildXmlFilePath(sourcePath) ?? sourcePath;
			if (!rootSourcePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				rootSourcePath += Path.DirectorySeparatorChar;
			}

			if (!string.IsNullOrEmpty(context.SupplementaryContentPath))
			{
				TestCase.SetSupplementaryContentPath(context.SupplementaryContentPath);
			}

			using (logger.RecordTask("Main"))
			{
				ExecuteStartup(DatabaseName, rootSourcePath, logger, IsDat);
				return new TestRunner();
			}
		}

		public static void ExecuteStartup(string databaseName, string sourcePath, ITaskLogger logger, bool isDat)
		{
			LocalDBConnection.DatIsTesting = isDat;
			var serverName = LocalDBConnection.GetServerName();

			int result = 4147;
			Exception exceptionInMain = null;
			var thread = new Thread(() =>
			{
				try
				{
					var args = new List<string>() { serverName, databaseName, ApplicationArguments.OptionTestAdapter, ApplicationArguments.OptionDatEnterprisePath + sourcePath, ApplicationArguments.OptionNoSplash };
					if (isDat)
					{
						args.Add(ApplicationArguments.OptionDat);
					}
					result = ApplicationStartupDirector.Main(args.ToArray());
				}
				catch (Exception ex)
				{
					exceptionInMain = ex;
				}

				DatForm.InitializedEvent.Set();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			DatForm.InitializedEvent.WaitOne();
			if (exceptionInMain != null)
			{
				throw new InvalidOperationException("Exception in Main", exceptionInMain);
			}
			else if (result == ExitCodes.DbUpgraderDirectorFailure)
			{
				DbFileDeleter.HandleDbFileExistsException(logger, DatDbUpgraderDirector.UpgradeLog);
				throw new InvalidOperationException(DatDbUpgraderDirector.UpgradeLog);
			}
			else if (result != 4147 || DatForm.Instance == null)
			{
				throw new InvalidOperationException("StartEnterprise failed with exit code " + result);
			}
		}

		public bool IsDat { get; set; } = true;

		void RestoreDB()
		{
			using (logger.RecordTask("Restoring DB"))
			using (var connection = LocalDBConnection.GetConnection())
			using (ErrorReporter.SetTemporaryInstanceForTest(new LoggerBasedErrorReporter(logger)))
			{
				connection.Open();
				new DbRestorer(connection, logger).RestoreCurrentDbs();
			}
		}

		string DatabaseName
		{
			get
			{
				string databaseName;
				if (!context.Options.TryGetValue(SystemAdapterOptions.DatabaseName, out databaseName))
				{
					databaseName = DatConfiguration.OdysseyTestDatabase;
				}
				return databaseName;
			}
		}

		readonly TestAdapterContext context;
		readonly ITaskLogger logger;
	}
}
