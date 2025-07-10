using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.CSharp;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	class ControllerUpgradeTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestNoUpgradeRequired()
		{
			var preTestProvider = Env.GetCurrentProvider();
			using (Db.Connection.BeginTransactionWithManager(() =>
			{
				var postTestProvider = Env.GetCurrentProvider();
				if (postTestProvider != preTestProvider)
				{
					preTestProvider.Enable();
					postTestProvider.Dispose();
				}
			}))
			{
				using (InstallationEnvironmentForTest.TempDirectoryForTest())
				{
					Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));
					var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
					upgradeManager.UploadUpgradePackage(TestPackagePath, ReleaseInfo.Instance.VersionNumber.ToVersion(), DateTime.UtcNow, "CUR", "", null);
					var upgrader = new ControllerUpgrade(serviceStopRequestConsumerMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
					upgrader.UpgradeSoftwareIfNeeded();
					AssertNoUpgrade(upgrader);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		[ExpectNoExceptions]
		public void TestUpgrade()
		{
			using (Db.Connection.BeginTransactionWithManager())
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(TestPackagePath, new Version(1, 2, 3, 4), DateTime.UtcNow, "CUR", "", null);
				var serviceStopperMock = new Mock<IServiceStopRequestConsumer>();
				serviceStopperMock
					.Setup(consumer => consumer.WaitForServiceStopRequest(It.IsAny<TimeSpan>()))
					.Returns(() =>
					{
						Process.GetProcessesByName("CurrentVersionWriter").ForEach(p => p.WaitForExit(30000));
						return true;
					});
				var upgrader = new ControllerUpgrade(serviceStopperMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
				upgrader.UpgradeSoftwareIfNeeded();
				AssertUpgrade(upgrader);
				serviceStopperMock.Verify(consumer => consumer.WaitForServiceStopRequest(It.IsAny<TimeSpan>()), Times.AtLeastOnce);
				serviceStopperMock.Verify(consumer => consumer.WaitForServiceStopRequest(TimeSpan.FromMinutes(2)), Times.AtLeastOnce);
			}
		}

		public void TestDatabaseVersionIsIncorrect()
		{
			DatabaseVersionIsIncorrect(
				connection => DbRegistry.DatabaseMajorSchemaVersion.SaveValue(-1, connection),
				"The Database Version does not match the Application Version");
		}

		public void TestDatabaseVersionIsInRestoreState()
		{
			DatabaseVersionIsIncorrect(
				connection => DbRegistry.DatabaseMinorSchemaVersion.SaveValue(-1, connection),
				"The Database is in restore state.");
		}

		void DatabaseVersionIsIncorrect(Action<DbConnection> versionAction, string expectedMessage)
		{
			// Arrange
			using (Db.Connection.BeginTransactionWithManager())
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(TestPackagePath, ReleaseInfo.Instance.VersionNumber.ToVersion(), DateTime.UtcNow, "CUR", "", null);

				versionAction(Db.Connection);

				var serviceStopperMock = new Mock<IServiceStopRequestConsumer>();
				var upgrader = new ControllerUpgrade(serviceStopperMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);

				// Act
				upgrader.UpgradeSoftwareIfNeeded();

				// Assert
				AssertNoExceptionThrown(() =>
				{
					serviceEmergencyExitMock.Verify(exit => exit.ExitApplicationUnsafe(It.IsAny<string>()), Times.Once);
					serviceEmergencyExitMock.Verify(exit => exit.ExitApplicationUnsafe(It.Is<string>(s => s.Contains(expectedMessage, StringComparison.InvariantCultureIgnoreCase))), Times.Once);
					serviceEmergencyExitMock.Verify(exit => exit.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				});
			}
		}

		[UseSnapshotProtection]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		[ExpectNoExceptions]
		public void TestUpgradeAfterSchemaChangeAndReconnect()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));

				var serviceStopperMock = new Mock<IServiceStopRequestConsumer>();
				serviceStopperMock
					.Setup(m => m.WaitForServiceStopRequest(It.IsAny<TimeSpan>()))
					.Returns(true)
					.Callback(() => { Process.GetProcessesByName("CurrentVersionWriter").ForEach(p => p.WaitForExit(30000)); });
				var upgrader = new ControllerUpgrade(serviceStopperMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
				upgrader.UpgradeSoftwareIfNeeded();
				AssertNoUpgrade(upgrader);

				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(TestPackagePath, new Version(1, 2, 3, 4), DateTime.UtcNow, "CUR", "", null);

				using (var adminConnection = Db.NewAdminConnection())
				{
					NUnit.Framework.Assert.That(adminConnection.AcquireLockout(), Is.EqualTo(DbLockoutState.AquiredLockout));
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}

				upgrader.UpgradeSoftwareIfNeeded();
				AssertUpgrade(upgrader);
				serviceStopperMock.VerifyAll();
			}
		}

		[UseSnapshotProtection]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		[ExpectNoExceptions]
		public void TestUpgradeAfterTransformationChangeAndReconnect()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));

				var serviceStopperMock = new Mock<IServiceStopRequestConsumer>();
				serviceStopperMock
					.Setup(m => m.WaitForServiceStopRequest(It.IsAny<TimeSpan>()))
					.Returns(true)
					.Callback(() => { Process.GetProcessesByName("CurrentVersionWriter").ForEach(p => p.WaitForExit(30000)); });
				var upgrader = new ControllerUpgrade(serviceStopperMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
				upgrader.UpgradeSoftwareIfNeeded();
				AssertNoUpgrade(upgrader);

				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(TestPackagePath, new Version(1, 2, 3, 4), DateTime.UtcNow, "CUR", "", null);

				using (var adminConnection = Db.NewAdminConnection())
				{
					NUnit.Framework.Assert.That(adminConnection.AcquireLockout(), Is.EqualTo(DbLockoutState.AquiredLockout));
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						DbRegistry.DatabaseMajorTransformationVersion.SaveValue(DbRegistry.DatabaseMajorTransformationVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}

				upgrader.UpgradeSoftwareIfNeeded();
				AssertUpgrade(upgrader);
				serviceStopperMock.VerifyAll();
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestUpgradeSoftwareIfNeeded_HandlesDatabaseUpgradeInProgress_CanResetAbandonedLockout()
		{
			// Arrange
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var controllerUpgrade = new ControllerUpgrade(serviceStopRequestConsumerMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(TestPackagePath, ReleaseInfo.Instance.VersionNumber.ToVersion(), DateTime.UtcNow, "CUR", "", null);

				using (CreateAbandonedLockOnDatabaseAndUpdateVersion())
				{
					NUnit.Framework.Assert.Multiple(() =>
					{
						// Act
						AssertNoExceptionThrown(() => controllerUpgrade.UpgradeSoftwareIfNeeded());

						// Assert
						using (var adminConnection = Db.NewAdminConnection())
						{
							adminConnection.IsUpgradeCheckDisabled = true;
							NUnit.Framework.Assert.That(adminConnection.CheckLockoutState(), Is.EqualTo(DbLockoutState.NoLockout), "the abandoned lockout is reset");
						}
					});
				}
			}

			IDisposable CreateAbandonedLockOnDatabaseAndUpdateVersion()
			{
				// Note, closing this connection after acquiring a lock with it means the lockout becomes invalid.
				using (var adminConnection = Db.NewAdminConnection())
				{
					NUnit.Framework.Assert.That(adminConnection.AcquireLockout(), Is.EqualTo(DbLockoutState.AquiredLockout));
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					adminConnection.ExecuteNonQuery(@"
UPDATE
	dbo.StmData
SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue)) + 10))
WHERE
	SD_Name = @name;
",
						command => command.AddParameterBasedOnDbColumn("@name", "DATABASE_SCHEMA_VERSION", StmDataSchema.SD_Name));
				}

				return new DisposableAction(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.IsUpgradeCheckDisabled = true;
						adminConnection.ResetLockout();
					}
				});
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestUpgradeResetsInvalidLockout()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var preTestProvider = Env.GetCurrentProvider();
				try
				{
					Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));

					var upgrader = new ControllerUpgrade(serviceStopRequestConsumerMock.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
					upgrader.UpgradeSoftwareIfNeeded();
					AssertNoUpgrade(upgrader);

					var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
					upgradeManager.UploadUpgradePackage(TestPackagePath, ReleaseInfo.Instance.VersionNumber.ToVersion(), DateTime.UtcNow, "CUR", "", null);

					using (var adminConnection = Db.NewAdminConnection())
					{
						NUnit.Framework.Assert.That(adminConnection.AcquireLockout(), Is.EqualTo(DbLockoutState.AquiredLockout));
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					}

					var preTestOpenRetries = Db.Connection.OpenRetries;
					try
					{
						Db.Connection.OpenRetries = 0;
						upgrader.UpgradeSoftwareIfNeeded();
					}
					finally
					{
						Db.Connection.OpenRetries = preTestOpenRetries;
					}

					Db.Connection.EnsureIsOpen();
				}
				catch
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						if (adminConnection.CheckLockoutState() == DbLockoutState.InvalidLockout)
						{
							adminConnection.ResetLockout();
						}
					}
					Db.Connection.EnsureIsOpen();

					throw;
				}
				finally
				{
					var postTestProvider = Env.GetCurrentProvider();
					if (postTestProvider != preTestProvider)
					{
						preTestProvider.Enable();
						postTestProvider.Dispose();
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		[ExpectNoExceptions]
		public void TestUpgradeFinishesOnControllerStop()
		{
			using (Db.Connection.BeginTransactionWithManager())
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				var upgrade = upgradeManager.UploadUpgradePackage(TestPackagePath, new Version(1, 2, 3, 4), DateTime.UtcNow, "CUR", "", null);
				var installPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(upgrade.Version));
				upgradeManager.InstallUpgradePackage(upgrade, installPath).Dispose();
				CreateDeadlockedCurrentVersionWriter(installPath);
				var mockController = new Mock<IServiceStopRequestConsumer>();
				mockController
					.Setup(m => m.WaitForServiceStopRequest(It.IsAny<TimeSpan>()))
					.Returns(true);
				var upgrader = new ControllerUpgrade(mockController.Object, serviceEmergencyExitMock.Object, eventLoggerMock.Object);
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				upgrader.UpgradeSoftwareIfNeeded();
				stopwatch.Stop();

				var allProcess = Process.GetProcessesByName("CurrentVersionWriter");
				bool alreadyFinishedProcessing = true;
				allProcess.ForEach(p => alreadyFinishedProcessing &= p.HasExited);
				allProcess.ForEach(p => p.WaitForExit(30000));

				mockController.VerifyAll();
				NUnit.Framework.Assert.That(!alreadyFinishedProcessing, Is.True, "UpgradeSoftwareIfNeeded should have exited before CurrentVersionWriter finished");
				mockController.VerifyAll();
			}
		}

		static void CreateDeadlockedCurrentVersionWriter(string exeFilePath)
		{
			using var compiler = new CSharpCodeProvider();
			var options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.GenerateExecutable = true;
			options.OutputAssembly = Path.Combine(exeFilePath, "CurrentVersionWriter.exe");
			var result = compiler.CompileAssemblyFromSource(options,
				@"
public static class Program
{
	public static void Main(string[] cmd)
	{
		System.Threading.Thread.Sleep(5000);
	}
}
");
			var output = new string[result.Output.Count];
			result.Output.CopyTo(output, 0);
			NUnit.Framework.Assert.That(result.Errors.Count, Is.EqualTo(0), string.Join("\r\n", output));
		}

		static string TestPackagePath
		{
			get => Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp");
		}

		[ExpectNoExceptions]
		static void AssertNoUpgrade(ControllerUpgrade upgrader)
		{
			NUnit.Framework.Assert.That(upgrader.lastReportedErrorMessage, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(Directory.GetDirectories(InstallationEnvironment.Instance.BaseInstallPath).Length, Is.EqualTo(0));
			NUnit.Framework.Assert.That(Directory.GetFiles(InstallationEnvironment.Instance.BaseInstallPath).Length, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		static void AssertUpgrade(ControllerUpgrade upgrader)
		{
			NUnit.Framework.Assert.That(upgrader.lastReportedErrorMessage, Is.EqualTo(default(string)));
			var directories = Directory.GetDirectories(InstallationEnvironment.Instance.BaseInstallPath);
			NUnit.Framework.Assert.That(directories.Length, Is.EqualTo(1));
			NUnit.Framework.Assert.That(Path.GetFileName(directories[0]), Is.EqualTo("1.2.3.4"));
			File.Exists(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			eventLoggerMock = new Mock<IEventLogger>();
			serviceEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
			serviceStopRequestConsumerMock = new Mock<IServiceStopRequestConsumer>(MockBehavior.Strict);
		}

		Mock<IEventLogger> eventLoggerMock;
		Mock<IApplicationEmergencyExit> serviceEmergencyExitMock;
		Mock<IServiceStopRequestConsumer> serviceStopRequestConsumerMock;
	}
}
