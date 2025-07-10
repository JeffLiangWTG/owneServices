using System.Threading.Tasks;
using CargoWise.Data;
using CWNUnit.TestAdapter;
using Enterprise.Dat.Implementation;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.Startup;
using Enterprise.Startup.Tasks;
using Enterprise.ZArchitecture.Core;

namespace CWNetCoreTest.TestAdapter
{
	public static class Initialization
	{
		public static void ConfigureCargoWise(ITestOptions testOptions)
		{
			ConfigureCargoWiseOptions(testOptions);

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0301:Simplify collection initialization", Justification = "ApplicationArguments signature is ambiguous with the collection initialization expression.")]
		static void ConfigureCargoWiseOptions(ITestOptions testOptions)
		{
			if (!Initialized)
			{
				var datIsTesting = DatIsTesting;

				var arguments = new ApplicationArguments(System.Array.Empty<string>());

				if (string.IsNullOrEmpty(arguments.ServerName))
				{
					LocalDBConnection.DatIsTesting = datIsTesting;
					arguments.ServerName = LocalDBConnection.GetServerName(); // pick up sql server instance in the same way with CW1 Enterprise.Dat.Adapter
				}

				if (string.IsNullOrEmpty(arguments.DatabaseName))
				{
					arguments.DatabaseName = testOptions.DatabaseName;
					if (datIsTesting)
					{
						arguments.DatabaseName = "OdysseyDat";
					}
				}
				Initialized = true;

				Initialiser.InitialiseWinForms();
				ExecuteStartupTask(arguments, new SetupDbConnection());
				ExecuteStartupTask(arguments, new ProductBrandingRegistryDeterminerTask(() => { }));
				SqlSynonymNameResolver.Initialize();

				Env.LoginController.LoginLocationAutomatically(Env.LoginController.LoginUserDeveloper());
			}
		}

		static void ExecuteStartupTask(CommandLineArguments arguments, IApplicationStartupTask applicationStartupTask)
		{
			if (applicationStartupTask.ShouldExecute(arguments))
			{
				if (!applicationStartupTask.Execute(arguments))
				{
					throw new TaskCanceledException();
				}
			}
		}

		public static bool GetDatIsTesting()
		{
			return DatIsTesting;
		}
		static bool DatIsTesting => bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting;

		static bool Initialized { get; set; }
	}
}
