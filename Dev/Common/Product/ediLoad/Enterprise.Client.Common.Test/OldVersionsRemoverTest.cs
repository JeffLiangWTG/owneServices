using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppDomainWrappers.Net;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection.TestFramework;
using CargoWise.IO;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using CargoWise.NGenInstallerProgram;
using Enterprise.Upgrades;
using Microsoft.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.Authenticode;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;
using static Enterprise.Client.Common.Testing.AppManagerTestHelper;

namespace Enterprise.Client.Common.Testing
{
	public class OldVersionsRemoverTest : TestCase
	{
		public void TestCleanupIsSkipped_LatestCleanupWithinCleanupInterval()
		{
			TestLatestCleanupCheckAgainstCleanupInterval(lastStartTime: DateTime.UtcNow.AddHours(-23), currentVersionCleanupIntervalInDays: 1, false);
			TestLatestCleanupCheckAgainstCleanupInterval(lastStartTime: DateTime.UtcNow.AddDays(-6).AddHours(-23), currentVersionCleanupIntervalInDays: 7, false);
		}

		public void TestCleanupIsPerformed_NextCleanupIsOverDue()
		{
			TestLatestCleanupCheckAgainstCleanupInterval(lastStartTime: DateTime.UtcNow.AddDays(-2), currentVersionCleanupIntervalInDays: 1, true);
			TestLatestCleanupCheckAgainstCleanupInterval(lastStartTime: DateTime.UtcNow.AddDays(-8), currentVersionCleanupIntervalInDays: 7, true);
		}

		void TestLatestCleanupCheckAgainstCleanupInterval(DateTime lastStartTime, int currentVersionCleanupIntervalInDays, bool expectCleanup)
		{
			using (var tempDirectory = new TempDirectory())
			{
				var configManager = new CurrentVersionCleanerConfigManager();
				var baseInstallationPath = tempDirectory.DirectoryName;
				var config = configManager.LoadConfiguration(baseInstallationPath) as CurrentVersionCleanerConfig;
				var configurationMock = new Mock<Configuration>();
				var installationMock = new Mock<Installation>(configurationMock.Object);
				var oldVersionsRemoverMock = new Mock<OldVersionsRemover>(installationMock.Object, false, false) { CallBase = true };
				var oldVersionsRemover = oldVersionsRemoverMock.Object;
				var configuration = configurationMock.Object;

				configuration.BaseTargetPath = baseInstallationPath;
				oldVersionsRemoverMock.Setup(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()))
					.Verifiable();
				oldVersionsRemoverMock.Setup(x => x
						.CleanupNGenRoots())
					.Verifiable();

				// Arrange
				config.LastStartTime = lastStartTime;
				config.CurrentVersionCleanupIntervalInDays = TimeSpan.FromDays(currentVersionCleanupIntervalInDays);
				configManager.SaveConfigurationViaAppManager(baseInstallationPath, config);

				// Act
				var installationResults = new InstallationResultCollection();
				oldVersionsRemover.Install(installationResults);

				// Assert
				CombineAssertions("Skip cleanup if LatestCurrentVersionCleanup is within CurrentVersionCleanupIntervalInDays", () =>
				{
					if (expectCleanup)
					{
						Assert(installationResults.All(x => x.IsOK));
						oldVersionsRemoverMock.Verify(x => x
							.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Once);
						oldVersionsRemoverMock.Verify(x => x
							.CleanupNGenRoots(), Times.Once);
					}
					else
					{
						var expectedMessage = Invariant($"Skipped cleanup as the latest cleanup was performed on: {lastStartTime} less than {currentVersionCleanupIntervalInDays} day(s) of the configured intervals.");
						Assert(installationResults.All(x => x.IsOK));
						Assert(installationResults.All(x => x.Message == expectedMessage));
						oldVersionsRemoverMock.Verify(x => x
							.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Never);
						oldVersionsRemoverMock.Verify(x => x
							.CleanupNGenRoots(), Times.Once);
					}
				});
			}
		}

		public void TestCleanupWithoutCleanupConfigFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var baseInstallationPath = tempDirectory.DirectoryName;
				var configurationMock = new Mock<Configuration>();
				var installationMock = new Mock<Installation>(configurationMock.Object);
				var oldVersionsRemoverMock = new Mock<OldVersionsRemover>(installationMock.Object, false, false) { CallBase = true };
				var oldVersionsRemover = oldVersionsRemoverMock.Object;
				var configuration = configurationMock.Object;
				var currentCleanerConfigFilePath = Path.Combine(baseInstallationPath, CurrentVersionCleanerConfigManager.CurrentVersionConfigFileName);

				configuration.BaseTargetPath = baseInstallationPath;
				oldVersionsRemoverMock.Setup(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()))
					.CallBase();
				oldVersionsRemoverMock.Setup(x => x
						.CleanupNGenRoots())
					.Verifiable();

				// Arrange
				var dateTimeUtcNow = DateTime.UtcNow;
				AssertEquals(false, File.Exists(currentCleanerConfigFilePath));

				// Act
				var installationResults = new InstallationResultCollection();
				oldVersionsRemover.Install(installationResults);

				// Assert
				CombineAssertions("CleanCurrentVersion has created the initial config file if not found", () =>
				{
					AssertEquals(true, File.Exists(currentCleanerConfigFilePath));

					oldVersionsRemoverMock.Verify(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Once);
					oldVersionsRemoverMock.Verify(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Once);

					var config = new CurrentVersionCleanerConfigManager().LoadConfiguration(baseInstallationPath);
					AssertNotNull(config);
					AssertGreaterThanOrEqualTo(config.LastStartTime, dateTimeUtcNow);
					AssertEquals(config.NextRuntime, config.LastStartTime + config.CurrentVersionCleanupIntervalInDays);
				});
			}
		}

		public void TestCleanupWithExistingCleanupConfigFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var baseInstallationPath = tempDirectory.DirectoryName;
				var configManager = new CurrentVersionCleanerConfigManager();
				var configurationMock = new Mock<Configuration>();
				var installationMock = new Mock<Installation>(configurationMock.Object);
				var oldVersionsRemoverMock = new Mock<OldVersionsRemover>(installationMock.Object, false, false) { CallBase = true };
				var oldVersionsRemover = oldVersionsRemoverMock.Object;
				var configuration = configurationMock.Object;
				var currentCleanerConfigFilePath = Path.Combine(baseInstallationPath, CurrentVersionCleanerConfigManager.CurrentVersionConfigFileName);

				configuration.BaseTargetPath = baseInstallationPath;
				oldVersionsRemoverMock.Setup(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()))
					.CallBase();
				oldVersionsRemoverMock.Setup(x => x
						.CleanupNGenRoots())
					.Verifiable();

				// Arrange
				var originalConfig = configManager.LoadConfiguration(baseInstallationPath);
				var utcLastRun = DateTime.UtcNow.AddDays(-3);
				originalConfig.LastStartTime = utcLastRun.AddDays(-1);
				originalConfig.NextRuntime = originalConfig.LastStartTime + originalConfig.CurrentVersionCleanupIntervalInDays;

				var savingConfigResult = configManager.SaveConfigurationViaAppManager(baseInstallationPath, originalConfig);
				AssertEquals(AppManagerResultStatus.Success, savingConfigResult.Status);
				AssertEquals(true, File.Exists(currentCleanerConfigFilePath));

				// Act
				var installationResults = new InstallationResultCollection();
				oldVersionsRemover.Install(installationResults);

				// Assert
				CombineAssertions("CleanCurrentVersionConfig file has been loaded and updated.", () =>
				{
					AssertEquals(true, File.Exists(currentCleanerConfigFilePath));

					oldVersionsRemoverMock.Verify(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Once);
					oldVersionsRemoverMock.Verify(x => x
						.CleanCurrentVersion(baseInstallationPath, It.IsAny<ICurrentVersionCleanerConfigWithLogs>()), Times.Once);

					var updatedConfig = configManager.LoadConfiguration(baseInstallationPath);
					AssertNotNull(updatedConfig);
					AssertGreaterThanOrEqualTo(updatedConfig.LastStartTime, utcLastRun);
					AssertEquals(updatedConfig.NextRuntime, updatedConfig.LastStartTime + updatedConfig.CurrentVersionCleanupIntervalInDays);
				});
			}
		}

		public void TestNeedsToInstall()
		{
			MockConfiguration configuration = new MockConfiguration();
			Assert(new OldVersionsRemover(new Installation(configuration)).NeedsToInstall());
		}

		#region TestInstallExcludingDependencies
		public void TestInstallExcludingDependencies()
		{
			var appDomainWrapper = new AppDomainWrapper("TestDeleteOldInstalledVersions");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.Client.Common.Testing",
				ClassName = nameof(OldVersionsRemoverTest),
				MethodName = nameof(InstallExcludingDependenciesStatic),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Client.Common.Test.dll");
			config.MethodParameters = new string[] { config.TempWorkingDirectoryPath };

			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		static void InstallExcludingDependenciesStatic(string tempDir)
		{
			var configuration = new MockConfiguration(@"c:\Temp", tempDir);
			configuration.TargetVersion = new Version("1.2.0");

			var currentPath = Path.Combine(configuration.BaseTargetPath, configuration.TargetVersion.ToString());
			Directory.CreateDirectory(currentPath);
			Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

			var originalAsm = typeof(OldVersionsRemover).Assembly.Location;
			var parentPath = Path.GetDirectoryName(currentPath);
			File.WriteAllText(Path.Combine(currentPath, "current.txt"), "current");
			var lockedPath = Path.Combine(parentPath, "1.0.0");
			Directory.CreateDirectory(lockedPath);
			Directory.SetCreationTimeUtc(lockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
			File.WriteAllText(Path.Combine(lockedPath, "one.txt"), "one");
			File.WriteAllText(Path.Combine(lockedPath, "two.txt"), "two");
			File.Copy(originalAsm, Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
			var unlockedPath = Path.Combine(parentPath, "1.1.0");
			Directory.CreateDirectory(unlockedPath);
			Directory.SetCreationTimeUtc(unlockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
			File.WriteAllText(Path.Combine(unlockedPath, "one.txt"), "one");
			File.WriteAllText(Path.Combine(unlockedPath, "two.txt"), "two");
			File.Copy(originalAsm, Path.Combine(unlockedPath, Path.GetFileName(originalAsm)));

			_ = Assembly.LoadFile(Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
			var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));
			oldVersionsRemover.exceptionLog.Add(new Exception());
			var result = oldVersionsRemover.InstallExcludingDependencies();
			Assert(result.IsWarning);

			AssertContainsExactElementsInAnyOrder(new string[] { currentPath, lockedPath }, Directory.GetDirectories(parentPath));
			AssertContainsExactElementsInAnyOrder(new string[] { Path.Combine(currentPath, "current.txt") }, Directory.GetFiles(currentPath));
			AssertContainsExactElementsInAnyOrder(new string[] { Path.Combine(lockedPath, Path.GetFileName(originalAsm)), Path.Combine(lockedPath, "one.txt"), Path.Combine(lockedPath, "two.txt") }, Directory.GetFiles(lockedPath));
		}
		#endregion

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		public void TestCurrentVersionCleanerIsCalledWithProvidedConfig()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var configuration = new MockConfiguration(@"c:\Temp", tempDirectory);
				configuration.TargetVersion = new Version("1.2.0");
				var (factoryMock, configManagerMock, configMock, cleanerMock) = GetMocks();
				var mockInstallation = new Mock<Installation>(configuration);
				var remover = new OldVersionsRemoverForTest(mockInstallation.Object, factoryMock.Object, configManagerMock.Object);

				// Act
				remover.InstallExcludingDependencies();

				//Assert
				factoryMock.Verify(m => m.GetCurrentVersionCleaner(configMock.Object), Times.Once);
				configManagerMock.Verify(m => m.LoadConfiguration(It.IsAny<string>()), Times.Exactly(2));
				cleanerMock.Verify(m => m.CleanCurrentVersion(), Times.Once);
				factoryMock.VerifyNoOtherCalls();
			}
		}

		static (Mock<ICurrentVersionCleanerFactory>, Mock<ICurrentVersionCleanerConfigManager>, Mock<ICurrentVersionCleanerConfigWithLogs>, Mock<ICurrentVersionCleaner>) GetMocks()
		{
			var configMock = new Mock<ICurrentVersionCleanerConfigWithLogs>();

			var cleanerMock = new Mock<ICurrentVersionCleaner>();
			cleanerMock.Setup(m => m.CleanCurrentVersion());

			var configManagerMock = new Mock<ICurrentVersionCleanerConfigManager>();
			configManagerMock.Setup(x => x
					.SaveConfigurationViaAppManager(It.IsAny<string>(), It.IsAny<ICurrentVersionCleanerConfigWithLogs>()))
				.Returns(new AppManagerResult(AppManagerResultStatus.Success));

			var factoryMock = new Mock<ICurrentVersionCleanerFactory>();
			configManagerMock.Setup(m => m.LoadConfiguration(It.IsAny<string>())).Returns(configMock.Object);
			factoryMock.Setup(m => m.GetCurrentVersionCleaner(It.IsAny<ICurrentVersionCleanerConfig>())).Returns(cleanerMock.Object);

			return (factoryMock, configManagerMock, configMock, cleanerMock);
		}

		[UseSnapshotProtection]
		public void TestOneConnectionPerSqlServerIsCreated()
		{
			// Arrange
			const string tableName = "_E3601EE9-E970-4F63-AF32-8A4AB9D307D1";
			const string databaseName = "Odyssey_E3601EE9-E970-4F63-AF32-8A4AB9D307D1";
			using (var tempDirectory = new TempDirectory())
			{
				var currentVersionFile = CreateCurrentVersionFile(tempDirectory);
				var applicationUsageLogFile = new Mock<ApplicationUsageLogFile>();
				applicationUsageLogFile.Setup(x => x.GetUtcNow()).Returns(DateTime.UtcNow);
				AddApplicationUsageForCurrentVersions(applicationUsageLogFile, currentVersionFile);

				var configManager = new CurrentVersionCleanerConfigManager();
				var currentVersionCleanerConfigMock = Mock.Of<ICurrentVersionCleanerConfigWithLogs>(config =>
					config.LastStartTime == It.IsAny<DateTime>()
					&& config.LogFile == applicationUsageLogFile.Object
					&& config.CurrentVersionFile == currentVersionFile
					&& config.VersionInactiveDurationInDays == TimeSpan.FromDays(14));
				var currentVersionCleaner = new CurrentVersionCleaner(currentVersionCleanerConfigMock);
				var currentVersionCleanerFactoryMock = new Mock<ICurrentVersionCleanerFactory>();
				currentVersionCleanerFactoryMock
					.Setup(factory => factory.GetCurrentVersionCleaner(It.IsAny<ICurrentVersionCleanerConfig>()))
					.Returns(currentVersionCleaner);

				var mockInstallation = new Mock<Installation>(
					new MockConfiguration(tempDirectory, tempDirectory) { TargetVersion = new Version("1.2.0"), });

				var remover = new OldVersionsRemoverForTest(mockInstallation.Object, currentVersionCleanerFactoryMock.Object, configManager);

				CreateTable();
				using (var adminConnection = Db.NewAdminConnection())
				using (CreateDatabase(adminConnection))
				using (CreateTrigger(adminConnection))
				{
					// Act
					remover.InstallExcludingDependencies();
				}

				// Assert
				var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);
				var result = Db.Connection.ExecuteScalar<int>(
					$@"
SELECT
	COUNT(*)
FROM
	[{Db.DatabaseName}].[dbo].[{tableName}]
WHERE 1=1
	AND HostName = @HostName
	AND AppName = @AppName
	AND sUserName = @sUserName
",
					command =>
					{
						command.AddParameter("@DbName", SqlDbType.VarChar, 128, "master");
						command.AddParameter("@HostName", SqlDbType.VarChar, 128, Dns.GetHostName());
						command.AddParameter("@AppName", SqlDbType.VarChar, 128, "CurrentVersionCleaner");
						command.AddParameter("@sUserName", SqlDbType.VarChar, 128, expectedAdminLogin.UserName);
					});
				AssertEquals(1, result);
			}

			void AddApplicationUsageForCurrentVersions(Mock<ApplicationUsageLogFile> applicationUsageLogFile, string currentVersionFilePath)
			{
				var currentVersionFile = new CurrentVersionFile(currentVersionFilePath);
				var logs = currentVersionFile
					.RetrieveAllServerNameDbName()
					.Select(s => s.Split(','))
					.Select(strings => new
					{
						serverNameWithInstance = strings[0],
						databaseName = strings[1],
					})
					.Select(x => new ApplicationUsageLog(x.serverNameWithInstance, x.databaseName))
					.ToList();
				applicationUsageLogFile.Setup(x => x.RetrieveAllTheLogs()).Returns(logs);
				applicationUsageLogFile.Setup(x => x.DeleteLog(It.IsAny<ApplicationUsageLog>()))
					.Callback<ApplicationUsageLog>(x => logs.Remove(x));
			}

			string CreateCurrentVersionFile(string tempDirectory)
			{
				var file = Path.Combine(tempDirectory, "CurrentVersion");
				var currentVersionFile = new CurrentVersionFile(file);
				currentVersionFile.RecordVersionNumber(Dns.GetHostName(), Db.DatabaseName, "10.1");
				currentVersionFile.RecordVersionNumber(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First().ToString(), Db.DatabaseName, "10.1");
				currentVersionFile.RecordVersionNumber("localhost", Db.DatabaseName, "10.1");
				currentVersionFile.RecordVersionNumber("127.0.0.1", Db.DatabaseName, "10.1");
				currentVersionFile.RecordVersionNumber(Dns.GetHostName(), databaseName, "10.1");
				currentVersionFile.RecordVersionNumber(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First().ToString(), databaseName, "10.1");
				currentVersionFile.RecordVersionNumber("localhost", databaseName, "10.1");
				currentVersionFile.RecordVersionNumber("127.0.0.1", databaseName, "10.1");
				return file;
			}

			void CreateTable()
			{
				Db.Connection.ExecuteNonQuery($@"
CREATE TABLE [{Db.DatabaseName}].[dbo].[{tableName}] (
[DbName]            VARCHAR(128)                        NULL,
[HostName]          VARCHAR(128)                        NULL,
[AppName]           VARCHAR(128)                        NULL,
[sUserName]         VARCHAR(128)                        NULL)
");
			}

			IDisposable CreateTrigger(AdminConnection adminConnection)
			{
				const string triggerName = "Logon_Trigger";
				adminConnection.ExecuteNonQuery($@"
CREATE OR ALTER TRIGGER
	{triggerName}
ON
	ALL SERVER FOR LOGON
AS
BEGIN
	IF EXISTS(SELECT 1 FROM [{Db.DatabaseName}].sys.Tables WHERE  Name = N'{tableName}' AND Type = N'U')
	BEGIN
		INSERT INTO [{Db.DatabaseName}].[dbo].[{tableName}]
		SELECT
			DBName           = DB_NAME(),
			HostName         = HOST_NAME(),
			AppName          = APP_NAME(),
			sUserName        = SUSER_SNAME()
		END
	END
;

ENABLE TRIGGER [{triggerName}] ON ALL SERVER
");

				return new DisposableAction(() =>
				{
					adminConnection.ExecuteNonQuery($"DROP TRIGGER [{triggerName}] ON ALL SERVER");
				});
			}

			IDisposable CreateDatabase(AdminConnection adminConnection)
			{
				adminConnection.CreateDatabase(databaseName);

				return new DisposableAction(() => AdoTestUtils.DropDbIfExistsDisposable(adminConnection, databaseName));
			}
		}

		public void TestOnlyOneInstanceOfOldVersionRemoverIsLaunched()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			using (var currentVersionCleanerStartedEvent = new ManualResetEvent(false))
			using (var timeToFinishCurrentVersionCleaner = new ManualResetEvent(false))
			{
				var cleanCurrentVersionHits = 0;

				var configManagerMock = new Mock<ICurrentVersionCleanerConfigManager>();
				var currentVersionCleanerConfigMock = Mock.Of<ICurrentVersionCleanerConfigWithLogs>(config =>
					config.LastStartTime == It.IsAny<DateTime>());
				var currentVersionCleanerMock = new Mock<ICurrentVersionCleaner>();
				currentVersionCleanerMock
					.Setup(cleaner => cleaner.CleanCurrentVersion())
					.Callback(() =>
					{
						Interlocked.Increment(ref cleanCurrentVersionHits);

						currentVersionCleanerStartedEvent.Set();
						timeToFinishCurrentVersionCleaner.WaitOne(TimeSpan.FromSeconds(10));
					});
				var currentVersionCleanerFactoryMock = new Mock<ICurrentVersionCleanerFactory>();
				currentVersionCleanerFactoryMock
					.Setup(factory => factory.GetCurrentVersionCleaner(It.IsAny<ICurrentVersionCleanerConfig>()))
					.Returns(currentVersionCleanerMock.Object);
				configManagerMock
					.Setup(factory => factory.LoadConfiguration(It.IsAny<string>()))
					.Returns(currentVersionCleanerConfigMock);
				configManagerMock.Setup(x => x
						.SaveConfigurationViaAppManager(It.IsAny<string>(), It.IsAny<ICurrentVersionCleanerConfigWithLogs>()))
					.Returns(new AppManagerResult(AppManagerResultStatus.Success));

				var mockInstallation = new Mock<Installation>(
					new MockConfiguration(tempDirectory, tempDirectory) { TargetVersion = new Version("1.2.0"), });

				var firstThread = new Thread(() =>
				{
					var remover = new OldVersionsRemoverForTest(mockInstallation.Object, currentVersionCleanerFactoryMock.Object, configManagerMock.Object);
					remover.InstallExcludingDependencies();
				});
				firstThread.Start();
				currentVersionCleanerStartedEvent.WaitOne();

				var threads = Enumerable.Range(0, 10)
					.Select(i => new Thread(() =>
					{
						var remover = new OldVersionsRemoverForTest(mockInstallation.Object, currentVersionCleanerFactoryMock.Object, configManagerMock.Object);
						remover.InstallExcludingDependencies();
					}))
					.ToList();

				// Act
				foreach (var thread in threads)
				{
					thread.Start();
				}

				var result = threads
					.Select(thread => thread.Join(TimeSpan.FromSeconds(5)))
					.ToList();

				// Assert
				timeToFinishCurrentVersionCleaner.Set();
				CombineAssertions(() =>
				{
					AssertEquals(true, firstThread.Join(TimeSpan.FromSeconds(5)));
					AssertCollectionNotContains(false, result);
					AssertNoExceptionThrown(() => currentVersionCleanerFactoryMock.Verify(currentVersionCleanerFactory => currentVersionCleanerFactory.GetCurrentVersionCleaner(It.IsAny<ICurrentVersionCleanerConfig>()), Times.Once));
					AssertNoExceptionThrown(() => configManagerMock.Verify(currentVersionCleanerFactory => currentVersionCleanerFactory.LoadConfiguration(It.IsAny<string>()), Times.Exactly(2)));
					AssertEquals(1, cleanCurrentVersionHits);
					AssertNoExceptionThrown(() => currentVersionCleanerMock.Verify(cleaner => cleaner.CleanCurrentVersion(), Times.Once));
				});
			}
		}

		public void TestResultWhenInstallIsSkipped()
		{
			// Arrange
			using (var mutex = new UpgraderMutex("Global\\CargoWiseOneOldVersionsRemover"))
			{
				Assert(mutex.IsCreatedNew && mutex.WaitOne(TimeSpan.Zero));

				var installationMock = new Mock<Installation>(new Mock<Configuration>().Object);
				var currentVersionCleanerFactoryMock = new Mock<ICurrentVersionCleanerFactory>();
				var configManagerMock = new Mock<ICurrentVersionCleanerConfigManager>();

				var remover = new OldVersionsRemoverForTest(installationMock.Object, currentVersionCleanerFactoryMock.Object, configManagerMock.Object);

				// Act
				var result = remover.InstallExcludingDependencies();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(true, result.IsOK);
					AssertEquals("Removing old installations skipped, due to the other instance is running", result.Message);
				});
			}
		}

		#region TestInstallExcludingDependencies_ReturnNotAffectCargoWiseFunctionsWarning_AllExceptionsAreFileInUse
		public void TestInstallExcludingDependencies_ReturnNotAffectCargoWiseFunctionsWarning_AllExceptionsAreFileInUse()
		{
			var appDomainWrapper = new AppDomainWrapper("TestDeleteOldInstalledVersions");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.Client.Common.Testing",
				ClassName = nameof(OldVersionsRemoverTest),
				MethodName = nameof(InstallExcludingDependencies_ReturnNotAffectCargoWiseFunctionsWarning_AllExceptionsAreFileInUseStatic),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Client.Common.Test.dll");
			config.MethodParameters = new string[] { config.TempWorkingDirectoryPath };

			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		static void InstallExcludingDependencies_ReturnNotAffectCargoWiseFunctionsWarning_AllExceptionsAreFileInUseStatic(string tempDir)
		{
			var configuration = new MockConfiguration(@"c:\Temp", tempDir);
			configuration.TargetVersion = new Version("1.1.0");

			var currentPath = Path.Combine(configuration.BaseTargetPath, configuration.TargetVersion.ToString());
			Directory.CreateDirectory(currentPath);
			Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

			var originalAsm = typeof(OldVersionsRemover).Assembly.Location;
			var parentPath = Path.GetDirectoryName(currentPath);
			File.WriteAllText(Path.Combine(currentPath, "current.txt"), "current");
			var lockedPath = Path.Combine(parentPath, "1.0.0");
			Directory.CreateDirectory(lockedPath);
			Directory.SetCreationTimeUtc(lockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
			File.WriteAllText(Path.Combine(lockedPath, "one.txt"), "one");
			File.WriteAllText(Path.Combine(lockedPath, "two.txt"), "two");
			File.Copy(originalAsm, Path.Combine(lockedPath, Path.GetFileName(originalAsm)));

			_ = Assembly.LoadFile(Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
			var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));
			var result = oldVersionsRemover.InstallExcludingDependencies();
			AssertEquals(true, result.IsWarning);
			AssertEquals("All exceptions are file in use exception, affect CargoWise Functions return false", false, result.AffectCargoWiseFunctions);
		}
		#endregion

		#region TestInstallExcludingDependencies_ReturnAffectCargoWiseFunctionsWarning_NotAllExceptionsAreFileInUse
		public void TestInstallExcludingDependencies_ReturnAffectCargoWiseFunctionsWarning_NotAllExceptionsAreFileInUse()
		{
			var appDomainWrapper = new AppDomainWrapper("TestDeleteOldInstalledVersions");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.Client.Common.Testing",
				ClassName = nameof(OldVersionsRemoverTest),
				MethodName = nameof(InstallExcludingDependencies_ReturnAffectCargoWiseFunctionsWarning_NotAllExceptionsAreFileInUseStatic),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Client.Common.Test.dll");
			config.MethodParameters = new string[] { config.TempWorkingDirectoryPath };

			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		static void InstallExcludingDependencies_ReturnAffectCargoWiseFunctionsWarning_NotAllExceptionsAreFileInUseStatic(string tempDir)
		{
			var configuration = new MockConfiguration(@"c:\Temp", tempDir);
			configuration.TargetVersion = new Version("1.1.0");

			var currentPath = Path.Combine(configuration.BaseTargetPath, configuration.TargetVersion.ToString());
			Directory.CreateDirectory(currentPath);
			Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

			var originalAsm = typeof(OldVersionsRemover).Assembly.Location;
			var parentPath = Path.GetDirectoryName(currentPath);
			File.WriteAllText(Path.Combine(currentPath, "current.txt"), "current");
			var lockedPath = Path.Combine(parentPath, "1.0.0");
			Directory.CreateDirectory(lockedPath);
			Directory.SetCreationTimeUtc(lockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
			File.WriteAllText(Path.Combine(lockedPath, "one.txt"), "one");
			File.WriteAllText(Path.Combine(lockedPath, "two.txt"), "two");
			File.Copy(originalAsm, Path.Combine(lockedPath, Path.GetFileName(originalAsm)));

			_ = Assembly.LoadFile(Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
			var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));
			oldVersionsRemover.exceptionLog.Add(new Exception("Dummy Exception"));
			var result = oldVersionsRemover.InstallExcludingDependencies();
			AssertEquals(true, result.IsWarning);
			AssertEquals("Not all exceptions are file in use exception, affect CargoWise Functions return true", true, result.AffectCargoWiseFunctions);
		}
		#endregion

		#region TestInstallExcludingDependencies_SilentlyContinue_NotAllExceptionsAreFileInUse
		public void TestInstallExcludingDependencies_SilentlyContinue_NotAllExceptionsAreFileInUse()
		{
			var appDomainWrapper = new AppDomainWrapper("TestDeleteOldInstalledVersions");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.Client.Common.Testing",
				ClassName = nameof(OldVersionsRemoverTest),
				MethodName = nameof(InstallExcludingDependencies_SilentlyContinue_NotAllExceptionsAreFileInUseStatic),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Client.Common.Test.dll");
			config.MethodParameters = new string[] { config.TempWorkingDirectoryPath };

			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		static void InstallExcludingDependencies_SilentlyContinue_NotAllExceptionsAreFileInUseStatic(string tempDir)
		{
			var configuration = new MockConfiguration(@"c:\Temp", tempDir);
			configuration.TargetVersion = new Version("1.1.0");

			var currentPath = Path.Combine(configuration.BaseTargetPath, configuration.TargetVersion.ToString());
			Directory.CreateDirectory(currentPath);
			Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

			var originalAsm = typeof(OldVersionsRemover).Assembly.Location;
			var parentPath = Path.GetDirectoryName(currentPath);
			File.WriteAllText(Path.Combine(currentPath, "current.txt"), "current");
			var lockedPath = Path.Combine(parentPath, "1.0.0");
			Directory.CreateDirectory(lockedPath);
			Directory.SetCreationTimeUtc(lockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
			File.WriteAllText(Path.Combine(lockedPath, "one.txt"), "one");
			File.WriteAllText(Path.Combine(lockedPath, "two.txt"), "two");
			File.Copy(originalAsm, Path.Combine(lockedPath, Path.GetFileName(originalAsm)));

			_ = Assembly.LoadFile(Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
			var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration), silentlyContinueOnError: true);
			oldVersionsRemover.exceptionLog.Add(new Exception("Dummy Exception"));
			var result = oldVersionsRemover.InstallExcludingDependencies();
			AssertEquals(true, result.IsOK);
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		public void TestNewlyAddedFolderNotDeleted()
		{
			using (var tempDirectory = new TempDirectory())
			{
				MockConfiguration configuration = new MockConfiguration(@"c:\Temp", tempDirectory);
				configuration.TargetVersion = new Version("1.2.0");

				var oldPath = Path.Combine(configuration.BaseTargetPath, "1.1.1");
				Directory.CreateDirectory(oldPath);
				Directory.SetCreationTimeUtc(oldPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				File.WriteAllText(Path.Combine(oldPath, "foo.dll"), "whatever");

				var newPath = Path.Combine(configuration.BaseTargetPath, "1.1.2");
				Directory.CreateDirectory(newPath);
				Directory.SetCreationTimeUtc(newPath, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)));
				File.WriteAllText(Path.Combine(newPath, "foo.dll"), "whatever");

				var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));
				oldVersionsRemover.InstallExcludingDependencies();

				AssertContainsExactElementsInAnyOrder(new string[] { newPath }, Directory.GetDirectories(configuration.BaseTargetPath));
			}
		}

		[TestRequiresAdministrativePrivileges("Create event log source")]
		public void TestDeleteOrphanedVersionManyTimes()
		{
			if (!EventLog.SourceExists("CargoWise One"))
			{
				EventLog.CreateEventSource("CargoWise One", "Application");
			}

			var currentPath = Environment.CurrentDirectory;
			var tempDir = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
			using (new TempDirectory(tempDir))
			{
				var orphanedPath = Path.Combine(tempDir, "17.0.0.0");
				var orphanedNGenRoot = Path.Combine(orphanedPath, "CargoWise.NGenRoot.dll");
				var copyPath = Path.GetDirectoryName(GetType().Assembly.Location);
				using (new TempDirectory(orphanedPath))
				{
					File.Copy(Path.Combine(copyPath, "CargoWise.NGenRoot.dll"), orphanedNGenRoot);
					new NGenInstaller(new Installation(new Configuration()), currentPath, orphanedNGenRoot, NGenInstaller.Action.Install).Install(new InstallationResultCollection());
					WaitForNGenInstaller();
				}

				var newInstallPath = Path.Combine(tempDir, "18.0.0.0");
				var newInstallNGenRoot = Path.Combine(newInstallPath, "CargoWise.NGenRoot.dll");
				Directory.CreateDirectory(newInstallPath);
				File.Copy(Path.Combine(copyPath, "CargoWise.NGenRoot.dll"), newInstallNGenRoot);

				var tasks = new List<Task>();

				//Queue up another install to keep NGenInstaller busy to expose the issue that the OldVersionsRemover will happily uninstall the same orphaned install multiple times
				tasks.Add(Task.Run(() => { new NGenInstaller(new Installation(new Configuration()), currentPath, newInstallNGenRoot, NGenInstaller.Action.Install).Install(new InstallationResultCollection()); }));
				Thread.Sleep(500);

				try
				{
					var removeStartTime = DateTime.Now;
					var results = new ConcurrentQueue<InstallationResult>();
					tasks.Add(Task.Run(() => { results.Enqueue(OldVersionsRemoverWithTestBinaries.Run(Environment.CurrentDirectory)); }));
					tasks.Add(Task.Run(() => { results.Enqueue(OldVersionsRemoverWithTestBinaries.Run(Environment.CurrentDirectory)); }));
					tasks.Add(Task.Run(() => { results.Enqueue(OldVersionsRemoverWithTestBinaries.Run(Environment.CurrentDirectory)); }));
					tasks.Add(Task.Run(() => { results.Enqueue(OldVersionsRemoverWithTestBinaries.Run(Environment.CurrentDirectory)); }));
					Task.WaitAll(tasks.ToArray());
					WaitForNGenInstaller();

					foreach (var result in results)
					{
						Assert("OldVersionsRemover.Run had an error", !result.IsError);
						Assert("OldVersionsRemover.Run had a warning", !result.IsWarning);
					}

					var eventLogs = (new EventLog { Source = "CargoWise One" }).Entries.Cast<EventLogEntry>().Where(e => ((e.TimeGenerated >= removeStartTime) && (e.Message.Contains("NGenInstaller Error")))).ToArray();
					AssertEquals("NGenInstaller Processes Reported Error(s)", 0, eventLogs.Length);
				}
				finally
				{
					new NGenInstaller(new Installation(new Configuration()), currentPath, newInstallNGenRoot, NGenInstaller.Action.Uninstall).Install(new InstallationResultCollection());
					WaitForNGenInstaller();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		public void TestUninstallRemovesOldVersions()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var configuration = new MockConfiguration(@"c:\Temp", tempDirectory)
				{
					TargetVersion = new Version("2.2.2"),
				};

				var path = Path.Combine(configuration.BaseTargetPath, "1.1.1");
				Directory.CreateDirectory(path);
				Directory.SetCreationTimeUtc(path, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

				var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));

				// Act
				var result = oldVersionsRemover.InstallExcludingDependencies();

				// Assert
				AssertEquals(true, string.IsNullOrEmpty(result.Message));
			}
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		public void TestAppManagerNotRequiredWhenElevated()
		{
			// Arrange
			var mockAppManager = new Mock<IAppManager>();

			using (var tempDirectory = new TempDirectory())
			using (MockAppManager(mockAppManager.Object))
			using (MockIsAdmin(true))
			{
				Directory.CreateDirectory(Path.Combine(tempDirectory, "1.1.0"));

				var configuration = new MockConfiguration(@"c:\Temp", tempDirectory) { TargetVersion = new Version("1.2.0") };
				var (factoryMock, _, _, _) = GetMocks();
				var mockInstallation = new Mock<Installation>(configuration);
				var remover = new OldVersionsRemoverForTest(mockInstallation.Object, factoryMock.Object, new CurrentVersionCleanerConfigManager(), true);

				// Act
				remover.InstallExcludingDependencies();

				//Assert
				mockAppManager.VerifyNeverInvoked();
			}
		}

		[TestRequiresAdministrativePrivileges("Create folder and copy files to ProgramFilesX86 folder")]
		public void TestCleanupNGenRootsAfterAssembliesHaveBeenDeleted()
		{
			const string oldVersion = "17.8.20.0";
			const string testAssembly = "test.dll";

			var baseTargetPath = Path.Combine(OldVersionsRemover.WiseTechPath, $"{Guid.NewGuid():N}");
			var oldVersionDir = Path.Combine(baseTargetPath, oldVersion);
			var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			try
			{
				// Arrange
				DeleteTargetDirectoryIfExists();
				DeleteCurrentVersionConfigFileIfExists();
				Directory.CreateDirectory(oldVersionDir);

				var installedAssemblyFilePath = Path.Combine(oldVersionDir, testAssembly);
				Assert(!CreateTestAssembly(installedAssemblyFilePath).Any());

				// ngen install test assembly and verify installed
				new NGenCaller(Mock.Of<ILogger>()).InstallOrUninstall(NGenAction.Install, installedAssemblyFilePath);
				AssertEquals(
					$"Native image for {testAssembly} has been successfully installed",
					true,
					QueryInstalledNGenRootsFromWiseTechAssemblies().Any(x => x.Contains(testAssembly)));

				// delete the physical file from file system
				TempDirectory.DeleteDirectory(oldVersionDir);

				// Act
				var configuration = new MockConfiguration(baseTargetPath, binDir);
				var appManager = new AppManagerForTesting();
				configuration.SetAppManagerClient(appManager);
				var oldVersionsRemover = new OldVersionsRemoverForTest(new Installation(configuration));
				_ = oldVersionsRemover.InstallExcludingDependencies();

				// Assert
				AssertEquals(
					$"{testAssembly} has been removed from installed native images by OldVersionsRemover",
					false,
					QueryInstalledNGenRootsFromWiseTechAssemblies().Any(x => x.Contains(testAssembly)));
			}
			finally
			{
				DeleteTargetDirectoryIfExists();
			}

			void DeleteCurrentVersionConfigFileIfExists()
			{
				var currentVersionConfigFile = Path.Combine(binDir, CurrentVersionCleanerConfigManager.CurrentVersionConfigFileName);
				if (File.Exists(currentVersionConfigFile))
				{
					File.Delete(currentVersionConfigFile);
				}
			}

			void DeleteTargetDirectoryIfExists()
			{
				if (Directory.Exists(baseTargetPath))
				{
					TempDirectory.DeleteDirectory(baseTargetPath);
				}
			}

			IEnumerable<string> CreateTestAssembly(string assemblyPath)
			{
				using (var compiler = new CSharpCodeProvider())
				{
					var options = new CompilerParameters();

					options.ReferencedAssemblies.Add("System.dll");
					options.GenerateExecutable = false;
					options.GenerateInMemory = false;
					options.IncludeDebugInformation = true;
					options.OutputAssembly = assemblyPath;

					var code = $@"using System;

namespace Enterprise.Client.Common.Testing
{{
	public static class HelloWorld
	{{
		public static void Shout()
		{{
			Console.WriteLine(""Hello World!"");
		}}
	}}
}}
";
					var result = compiler.CompileAssemblyFromSource(options, code);
					var output = new string[result.Output.Count];
					result.Output.CopyTo(output, 0);

					return output;
				}
			}

			IEnumerable<string> QueryInstalledNGenRootsFromWiseTechAssemblies()
			{
				using (var hklmReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (var roots = hklmReg.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework\v2.0.50727\NGenService\Roots", false))
				{
					if (roots == null)
					{
						throw new InvalidOperationException("OpenSubKey operation failed.");
					}

					return roots
						.GetSubKeyNames()
						.Select(name => name.Replace("/", @"\"))
						.Where(path => path.StartsWith(OldVersionsRemover.WiseTechPath, StringComparison.OrdinalIgnoreCase));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Baseline")]
		public static void WaitForNGenInstaller()
		{
			Process[] processes = null;
			for (int i = 0; i < 1800; ++i)
			{
				processes = Process.GetProcessesByName("CargoWise.NGenInstaller");
				if (processes.Length == 0)
				{
					break;
				}

				Thread.Sleep(100);
			}

			AssertEquals("NGenInstaller Processes Didn't Run To Completion", 0, processes.Length);
		}

		public class OldVersionsRemoverIntegrationTest : TestCase
		{
			[RequiresSoftware(RequiredSoftware.IsVM)]
			[DatCapabilityRequirement("ADMIN")]
			public void TestScheduleConfigurationsAreUpdatedAfterCurrentVersionCleaner()
			{
				var configurationMock = new Mock<Configuration>();
				var installationMock = new Mock<Installation>(configurationMock.Object);
				var currentVersionCleanerFactoryMock = new Mock<ICurrentVersionCleanerFactory>();
				var currentVersionConfigManagerMock = new Mock<CurrentVersionCleanerConfigManager> { CallBase = true };
				var currentVersionConfigManager = currentVersionConfigManagerMock.Object;
				var currentVersionCleanerMock = new Mock<ICurrentVersionCleaner>();
				var oldVersionsRemoverMock = new Mock<OldVersionsRemover>(installationMock.Object, currentVersionCleanerFactoryMock.Object, currentVersionConfigManager, false, false) { CallBase = true };
				var oldVersionsRemover = oldVersionsRemoverMock.Object;
				var configuration = configurationMock.Object;

				var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				var baseDirectoryName = Path.GetFileName(configuration.BaseTargetPath);
				var backupDirectoryName = Invariant($"{baseDirectoryName}_{Guid.NewGuid()}");
				var currentVersionFiles = new[] { "CurrentVersion", "CurrentVersionConfig" };
				var lastStartTime = DateTime.UtcNow.AddDays(-7);

				var currentVersionConfig = new CurrentVersionCleanerConfig
				{
					VersionInactiveDurationInDays = TimeSpan.FromDays(3),
					CurrentVersionCleanupIntervalInDays = TimeSpan.FromDays(1),
					LastStartTime = lastStartTime,
					CurrentVersionFile = Path.Combine(configuration.BaseTargetPath, "CurrentVersion"),
				};

				currentVersionConfigManagerMock.Setup(x => x
						.LoadConfiguration(It.IsAny<string>()))
					.Returns(currentVersionConfig)
					.Verifiable("CurrentVersionConfig is loaded");
				currentVersionCleanerFactoryMock.Setup(x => x
						.GetCurrentVersionCleaner(It.IsAny<ICurrentVersionCleanerConfig>()))
					.Returns(currentVersionCleanerMock.Object)
					.Verifiable("GetCurrentVersionCleaner is invoked");
				currentVersionCleanerMock.Setup(x => x
						.CleanCurrentVersion())
					.Callback(() =>
					{
						var config = new CurrentVersionCleanerConfigManager().LoadConfiguration(configuration.BaseTargetPath);

						AssertNotNull(config);
						AssertGreaterThan(config.LastStartTime, DateTime.MinValue);
						AssertGreaterThan(config.NextRuntime, config.LastStartTime);
					})
					.Verifiable("CleanCurrentVersion must have been invoked.");

				var installationResults = new InstallationResultCollection();

				try
				{
					SetupTestDirectoryWithBackup(currentVersionFiles);
					CombineAssertions("We have created test CW1 environment as if for a production environment", () =>
					{
						Assert(configuration.BaseTargetPath.StartsWith(programFilesPath));
						foreach (var fileName in currentVersionFiles)
						{
							Assert(File.Exists(Path.Combine(configuration.TargetPath, fileName)));
						}

						AssertLessThan(
							"LastStartTime was older than CurrentVersionCleanupIntervalInDays so that clean up is due",
							currentVersionConfig.LastStartTime,
							DateTime.UtcNow.Subtract(currentVersionConfig.CurrentVersionCleanupIntervalInDays));
					});

					// Act
					oldVersionsRemover.Install(installationResults);

					// Assert
					CombineAssertions("CurrentVersionConfig has been updated successfully", () =>
					{
						Assert(installationResults.All(x => x.IsOK));

						var config = new CurrentVersionCleanerConfigManager().LoadConfiguration(configuration.BaseTargetPath);

						AssertNotNull(config);
						AssertGreaterThan(config.LastSuccessTime, DateTime.MinValue);
						AssertGreaterThanOrEqualTo(config.LastSuccessTime, config.LastStartTime);
					});

					void SetupTestDirectoryWithBackup(IEnumerable<string> emptyToCreateInDirectory)
					{
						if (Directory.Exists(configuration.BaseTargetPath))
						{
							var parentPath = Directory.GetParent(configuration.BaseTargetPath).FullName;
							var baseDirectoryBackupPath = Path.Combine(parentPath, backupDirectoryName);
							Directory.Move(configuration.BaseTargetPath, baseDirectoryBackupPath);
						}

						Directory.CreateDirectory(configuration.BaseTargetPath);

						foreach (var fileName in emptyToCreateInDirectory)
						{
							File.WriteAllText(Path.Combine(configuration.BaseTargetPath, fileName), string.Empty);
						}
					}
				}
				finally
				{
					TempDirectory.DeleteDirectory(configuration.BaseTargetPath);

					var parentPath = Directory.GetParent(configuration.BaseTargetPath)?.FullName;
					var baseDirectoryBackupPath = Path.Combine(parentPath, backupDirectoryName);
					if (Directory.Exists(baseDirectoryBackupPath))
					{
						Directory.Move(baseDirectoryBackupPath, configuration.BaseTargetPath);
					}
				}
			}
		}
	}

	class OldVersionsRemoverForTest : OldVersionsRemover
	{
		public OldVersionsRemoverForTest(Installation installation, bool silentlyContinueOnError = false)
			: base(installation, uninstallAll: false, silentlyContinueOnError: silentlyContinueOnError)
		{
		}

		public OldVersionsRemoverForTest(Installation installation, ICurrentVersionCleanerFactory factory, ICurrentVersionCleanerConfigManager configManager, bool uninstallAll = false, bool silentlyContinueOnError = false)
			: base(installation, factory, configManager, uninstallAll, silentlyContinueOnError)
		{
		}

		public new InstallationResult InstallExcludingDependencies()
		{
			return base.InstallExcludingDependencies();
		}

		public new List<Exception> exceptionLog => base.exceptionLog;

		public ICurrentVersionCleanerConfig Config { get; set; }
	}

	public class OldVersionsRemoverWithTestBinaries : OldVersionsRemover
	{
		public OldVersionsRemoverWithTestBinaries(Installation installation)
			: base(installation)
		{
		}

		public static InstallationResult Run(string baseInstallationPath)
		{
			var configuration = new Configuration();
			configuration.BaseTargetPath = baseInstallationPath;
			configuration.TargetVersion = null;
			return new OldVersionsRemoverWithTestBinaries(new Installation(configuration)).InstallExcludingDependencies();
		}

		protected override IEnumerable<string> FilterBinariesList(string[] subKeyNames)
		{
			return subKeyNames.Select(name => name.Replace("/", @"\")).Where(path => path.EndsWith(NGenInstaller.NGenRootsDllName, StringComparison.OrdinalIgnoreCase) && !File.Exists(path));
		}
	}

	class AppManagerForTesting : MockAppManager
	{
		public MutexRequest LastMutexRequest { get; private set; }

		public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
		{
			if (RetryOnce)
			{
				RetryOnce = false;
				return new AppManagerResult(AppManagerResultStatus.Retry);
			}
			else
			{
				LastMutexRequest = request;

				var workingDirectory = (string)((object[])state)[0];
				var targetPath = (string)((object[])state)[1];
				var action = ((object[])state)[2].ToString();
				var delayTimeInMsForTest = ((object[])state)[3].ToString();
				var appDomainWrapper = new AppDomainWrapper("MockAppManagerClientDomain");
				var config = new ProcessConfig
				{
					NamespacePath = "Enterprise.Client.Common.Testing",
					ClassName = nameof(NGenInstallerProxy),
					MethodName = nameof(Invoke)
				};

				config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Client.Common.Test.dll");
				config.MethodParameters = new string[] { assemblyPath, typeName, workingDirectory, targetPath, action, delayTimeInMsForTest };
				var result = appDomainWrapper.RunMethodInProcess48(config);

				var settings = new JsonSerializerSettings
				{
					Converters = new List<JsonConverter> { new AppManagerResultConverter() }
				};

				var deserializedAppManagerResult = JsonConvert.DeserializeObject<AppManagerResult>(result, settings);
				return deserializedAppManagerResult;
			}
		}

		public class AppManagerResultConverter : JsonConverter<AppManagerResult>
		{
			public override AppManagerResult ReadJson(JsonReader reader, Type objectType, AppManagerResult existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				if (reader.TokenType != JsonToken.StartObject)
				{
					throw new JsonSerializationException("Unexpected token type");
				}

				var message = string.Empty;
				var status = AppManagerResultStatus.TimedOut;

				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.EndObject)
					{
						return new AppManagerResult(status, message);
					}

					if (reader.TokenType == JsonToken.PropertyName && reader.Value is string propertyName)
					{
						_ = reader.Read();

						switch (propertyName)
						{
							case "Message":
								message = serializer.Deserialize<string>(reader);
								break;
							case "Status":
								status = serializer.Deserialize<AppManagerResultStatus>(reader);
								break;
						}
					}
				}

				throw new JsonSerializationException("Unexpected end when deserializing");
			}

			public override void WriteJson(JsonWriter writer, AppManagerResult value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}

	[CodeAlive("Called using Enterprise.Client.Common.Testing.AppManagerForTesting.Invoke() in a separate process")]
	class NGenInstallerProxy
	{
		public void Invoke(string assemblyPath, string typeName, string workingDirectory, string targetPath, string action, string delayTimeInMsForTest)
		{
			object state = new object[]
			{
				workingDirectory,
				targetPath,
				(NGenInstaller.Action)int.Parse(action),
				int.Parse(delayTimeInMsForTest)
			};

			var invocable = (IAppManagerInvocable)new NGenInstaller(new Installation(new Configuration()), Mock.Of<IAuthenticodeVerificationService>(), null, null, 0);

			var result = invocable.Invoke(waitedForMutex: false, state);

			var serializedResult = JsonConvert.SerializeObject(result);
#pragma warning disable CW1106 // Required to send data back from the AppDomainWrapper to CargoWise.Loader.Common.Testing.MockAppManager.Invoke()
			Console.WriteLine($"{serializedResult}");
#pragma warning restore CW1106
		}
	}
}
