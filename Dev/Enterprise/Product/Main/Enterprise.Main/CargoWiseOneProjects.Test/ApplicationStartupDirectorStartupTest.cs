using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Initialisation;
using Enterprise.Startup.Tasks;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class ApplicationStartupDirectorStartupTest : TestCase
	{
		#region Startup Tasks

		public void TestGuiStartupTasks()
		{
			var expectedTasks = new[]
			{
				nameof(StartupNotification.InitializationTask),
				nameof(ApplicationStartupDirector.EnterStartupErrorHandler),
				nameof(StartupInitEnableMemoryManager),
				nameof(Initialiser.InitialiseWinFormsTask),
				nameof(SetupDbConnection),
				nameof(StartupCheckAvailableDiskSpace),
				nameof(StartupInitAutoTesting),
				nameof(StartupCheckRunWithoutLoader),
				nameof(InstallCurrentVersionTask),
				nameof(StartupCheckClientDll),
				nameof(DbUpgraderDirector),
				nameof(LogUsageOfApplicationTask),
				nameof(RemoveOldUpgradePackagesTask),
				nameof(RemoveOldInstallationsTask),
				nameof(TempFileCleanupTask),
				nameof(ResourceStringsUpdaterTask),
				nameof(StartupDocManagerCommandLineHandler),
				nameof(StartupInitShowInSystray),
				nameof(StartupInitEnterpriseUrlHandlerService),
				nameof(RemoteDesktopServicesInitializationTask),
				nameof(LoginDirector),
				nameof(ApplicationStartupDirector.EndStartupErrorHandler),
				nameof(DotNetRecorderTaskScheduler),
				nameof(StartDat),
				nameof(ScreenResolutionChecker),
				nameof(StartupOpenMainFormTask),
				nameof(StartBlazorWinFormsInterop),
				nameof(ServiceManagerCommunicationMonitoringTask),
				nameof(DbConnectionCleanerTask),
				nameof(ConcludeStartupTask),
				nameof(ValidateDbArguments),
				nameof(ValidatePurgeDataRunStatus)
			};

			var commandLineArgs = new[]
			{
				Db.ServerName,
				Db.DatabaseName,
				"-SDir:foo",
				ApplicationArguments.OptionNoSplash,
			};

			TestStartupTasks(commandLineArgs, expectedTasks);
		}

		public void TestNonInteractiveStartupTasks()
		{
			var expectedTasks = new[]
			{
				nameof(StartupNotification.InitializationTask),
				nameof(ApplicationStartupDirector.EnterStartupErrorHandler),
				nameof(ApplicationStartupDirector.EnterScheduledUpgraderErrorHandler),
				nameof(Initialiser.InitialiseWinFormsTask),
				nameof(SetupDbConnection),
				nameof(InstallCurrentVersionTask),
				nameof(StartupCheckClientDll),
				nameof(ScheduledUpgraderDirector),
				nameof(RemoveOldUpgradePackagesTask),
				nameof(RemoveOldInstallationsTask),
				nameof(TempFileCleanupTask),
				nameof(ResourceStringsUpdaterTask),
				nameof(SlowBackgroundApplicationStartupTask),
				nameof(ExitApplicationTask),
				nameof(ValidateDbArguments),
				nameof(ValidatePurgeDataRunStatus)
			};

			var commandLineArgs = new[]
			{
				Db.ServerName,
				Db.DatabaseName,
				"-SDir:foo",
				ApplicationArguments.OptionNoSplash,
				ApplicationArguments.OptionScheduledDbUpgrader,
			};

			TestStartupTasks(commandLineArgs, expectedTasks);
		}

		void TestStartupTasks(string[] commandLineArgs, IEnumerable<string> expectedTasks)
		{
			// Arrange
			var startupTasks = new List<string>();
			var startupDirectorMock = new Mock<ApplicationStartupDirector> { CallBase = true };
			var startupDirector = startupDirectorMock.Object;
			var excludedTasks = new[] { typeof(DbConnectionCleanerTask), };
			var skipTestExecuteTasks = new[]
			{
				typeof(SetupDbConnection),
				typeof(ExitApplicationTask),
			};

			using (AbstractApplicationStartupTask.SetupShouldExecute_ForTest((args, task) =>
			{
				startupTasks.Add(task.GetType().Name);
				return !excludedTasks.Contains(task.GetType());
			}))
			using (AbstractApplicationStartupTask.SetupDoExecute_ForTest((args, task) =>
			{
				if (task is ConcludeStartupTask)
				{
					return false;
				}

				return skipTestExecuteTasks.Contains(task.GetType()) ? null : true;
			}))
			{
				// Act
				_ = startupDirector.StartEnterprise(commandLineArgs);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedTasks, startupTasks);
			}
		}

		protected override void TearDown()
		{
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}
		#endregion
	}
}
