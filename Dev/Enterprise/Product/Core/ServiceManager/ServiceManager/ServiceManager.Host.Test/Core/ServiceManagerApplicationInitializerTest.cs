using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class ServiceManagerApplicationInitializerTest
	{
		class WithFactoryTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestCallsUpgradeSoftware()
			{
				// Arrange
				var repo = new MockRepository(MockBehavior.Default);
				var upgradeMock = repo.Create<IControllerUpgrade>();

				var initializer = new ServiceManagerApplicationInitializer(upgrader: upgradeMock.Object,
					tcpIpRegistryAdjuster: repo.Create<ITcpIpRegistryAdjuster>().Object,
					hostLogger: repo.Create<IHostLogger>().Object,
					serviceTaskLocksCleaner: Mock.Of<IServiceTaskLocksCleaner>(),
					hostApplicationLockAcquirer: Mock.Of<IHostApplicationLockAcquirer>());

				// Act
				initializer.Initialize();

				// Assert
				upgradeMock.Verify(up => up.UpgradeSoftwareIfNeeded(), Times.AtLeastOnce);
			}

			[ExpectNoExceptions]
			public void TestExecutionOrder()
			{
				// Arrange
				var expectedExecutionList = new List<string>
				{
					"Acquire host application lock",
					"Upgrade",
					"Cleanup service task locks",
					"Adjust TcpIp Registry",
				};
				var executionList = new List<string>();

				var repo = new MockRepository(MockBehavior.Default);
				var upgradeMock = repo.Create<IControllerUpgrade>();
				upgradeMock.Setup(mock => mock.UpgradeSoftwareIfNeeded()).Callback(() => executionList.Add("Upgrade"));

				var tcpIpRegistryAdjuster = repo.Create<ITcpIpRegistryAdjuster>();
				tcpIpRegistryAdjuster.Setup(mock => mock.TryAdjustIfRequired(It.IsAny<IHostLogger>())).Callback(() => executionList.Add("Adjust TcpIp Registry"));

				var hostApplicationLockAcquirerMok = repo.Create<IHostApplicationLockAcquirer>();
				hostApplicationLockAcquirerMok.Setup(mock => mock.AcquireHostApplicationLock()).Callback(() => executionList.Add("Acquire host application lock"));

				var serviceTaskLocksCleanerMock = repo.Create<IServiceTaskLocksCleaner>();
				serviceTaskLocksCleanerMock.Setup(mock => mock.ReleaseLocksFromHost()).Callback(() => executionList.Add("Cleanup service task locks"));

				var initializer = new ServiceManagerApplicationInitializer(upgrader: upgradeMock.Object,
					tcpIpRegistryAdjuster: tcpIpRegistryAdjuster.Object,
					hostLogger: repo.Create<IHostLogger>().Object,
					serviceTaskLocksCleaner: serviceTaskLocksCleanerMock.Object,
					hostApplicationLockAcquirer: hostApplicationLockAcquirerMok.Object);

				// Act
				initializer.Initialize();

				// Assert
				NUnit.Framework.Assert.That(executionList.ToArray(), Is.EqualTo(expectedExecutionList.ToArray()));
				tcpIpRegistryAdjuster.Verify(a => a.TryAdjustIfRequired(It.Is<IHostLogger>(logger => logger != null)), Times.Once());
			}
		}

		class Test : TestCase
		{
			public void TestWrongConstructorParamsCall()
			{
				AssertNoExceptionThrown(() =>
				{
					var upgrader = new Mock<IControllerUpgrade>();
					var serviceStopRequestConsumerMock = new Mock<IServiceStopRequestConsumer>();
					var applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
					var tcpIpRegistryAdjusterMock = new Mock<ITcpIpRegistryAdjuster>();
					var hostLoggerMock = new Mock<IHostLogger>();
					var serviceTaskLocksCleanerMock = Mock.Of<IServiceTaskLocksCleaner>();
					var hostApplicationLockAcquirerMock = Mock.Of<IHostApplicationLockAcquirer>();
					var dbConnectionSetupMock = Mock.Of<IDbConnectionSetup>();

					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceManagerApplicationInitializer(null,tcpIpRegistryAdjusterMock.Object, hostLoggerMock.Object, serviceTaskLocksCleanerMock, hostApplicationLockAcquirerMock));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("upgrader"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceManagerApplicationInitializer(upgrader.Object,null, hostLoggerMock.Object, serviceTaskLocksCleanerMock, hostApplicationLockAcquirerMock));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("tcpIpRegistryAdjuster"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceManagerApplicationInitializer(upgrader.Object,tcpIpRegistryAdjusterMock.Object, null, serviceTaskLocksCleanerMock, hostApplicationLockAcquirerMock));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceManagerApplicationInitializer(upgrader.Object,tcpIpRegistryAdjusterMock.Object, hostLoggerMock.Object, null, hostApplicationLockAcquirerMock));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTaskLocksCleaner"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceManagerApplicationInitializer(upgrader.Object,tcpIpRegistryAdjusterMock.Object, hostLoggerMock.Object, serviceTaskLocksCleanerMock, null));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostApplicationLockAcquirer"));
				});
			}

			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestInitializeAndRunTaskDispatcherFixesWriterLogin()
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					adminConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DROP LOGIN {0}", ((IDbLoginRepair)adminConnection).RestrictedWriterDbLoginName));
				}

				try
				{
					Db.Connection.CloseConnection();
					NUnit.Framework.Assert.That(Db.Connection.State, Is.EqualTo(ConnectionState.Closed));

					using var ctsSource = new CancellationTokenSource();

					var repo = new MockRepository(MockBehavior.Default);

					var upgrader = new ControllerUpgrade(repo.Create<IServiceStopRequestConsumer>().Object, repo.Create<IApplicationEmergencyExit>().Object, repo.Create<IEventLogger>().Object);
					var initializer = new ServiceManagerApplicationInitializer(upgrader: upgrader,
						tcpIpRegistryAdjuster: repo.Create<ITcpIpRegistryAdjuster>().Object,
						hostLogger: repo.Create<IHostLogger>().Object,
						serviceTaskLocksCleaner: Mock.Of<IServiceTaskLocksCleaner>(),
						hostApplicationLockAcquirer: Mock.Of<IHostApplicationLockAcquirer>());

					var preTestOpenRetries = Db.Connection.OpenRetries;
					try
					{
						Db.Connection.OpenRetries = 0;
						initializer.Initialize();
					}
					finally
					{
						Db.Connection.OpenRetries = preTestOpenRetries;
					}
				}
				finally
				{
					Db.Connection.EnsureIsOpen();
				}
			}

			[UseSnapshotProtection]
			[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
			[DatCapabilityRequirement("SOURCE_CODE")]
			[ExpectNoExceptions]
			public void TestInitializeAndRunTaskDispatcherChecksForUpgradeFirst()
			{
				Db.Connection.EnsureIsOpen();

				using (InstallationEnvironmentForTest.TempDirectoryForTest())
				{
					var preTestProvider = Env.GetCurrentProvider();
					try
					{
						Directory.Delete(InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber));

						var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
						upgradeManager.UploadUpgradePackage(TestPackagePath, ReleaseInfo.Instance.VersionNumber.ToVersion(), DateTime.UtcNow, "CUR", "", null);

						using (var adminConnection = Db.NewAdminConnection())
						{
							NUnit.Framework.Assert.That(adminConnection.AcquireLockout(), Is.EqualTo(DbLockoutState.AquiredLockout));
							DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						}

						var repo = new MockRepository(MockBehavior.Default);
						using var ctsSource = new CancellationTokenSource();
						var upgradeFactory = new ControllerUpgrade(repo.Create<IServiceStopRequestConsumer>().Object, repo.Create<IApplicationEmergencyExit>().Object, repo.Create<IEventLogger>().Object);
						var initializer = new ServiceManagerApplicationInitializer(upgrader: upgradeFactory,
							tcpIpRegistryAdjuster: repo.Create<ITcpIpRegistryAdjuster>().Object,
							hostLogger: repo.Create<IHostLogger>().Object,
							serviceTaskLocksCleaner: Mock.Of<IServiceTaskLocksCleaner>(),
							hostApplicationLockAcquirer: Mock.Of<IHostApplicationLockAcquirer>());

						ctsSource.Cancel();

						var preTestOpenRetries = Db.Connection.OpenRetries;
						try
						{
							Db.Connection.OpenRetries = 0;
							initializer.Initialize();
						}
						finally
						{
							Db.Connection.OpenRetries = preTestOpenRetries;
						}

						Db.Connection.EnsureIsOpen();
					}
					catch (Exception ex)
					{
						Debug.Write(ex);
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

			[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
			[ExpectNoExceptions]
			public void TestInitializeShouldInitializeCurrentUser()
			{
				using (ClearUserContext())
				{
					// Arrange
					var serviceManagerApplicationInitializer = new ServiceManagerApplicationInitializer(
						Mock.Of<IControllerUpgrade>(),
						Mock.Of<ITcpIpRegistryAdjuster>(),
						Mock.Of<IHostLogger>(),
						Mock.Of<IServiceTaskLocksCleaner>(),
						Mock.Of<IHostApplicationLockAcquirer>());

					// Action
					serviceManagerApplicationInitializer.Initialize();

					// Assert
					NUnit.Framework.Assert.That(Env.CurrentUser.LoginName, Is.EqualTo(User.ServiceUserName));
				}

				IDisposable ClearUserContext()
				{
					var userContext = EnvProxy.Instance.CurrentUserContext;
					EnvProxy.Instance.ClearUserContext();

					return new DisposableAction(() =>
					{
						EnvProxy.Instance.SetUserContext(userContext);
					});
				}
			}

			[ExpectNoExceptions]
			public void TestInitialize_WhenUpgrade_DoesNotSetServiceUser()
			{
				// Arrange
				var mockUpgrader = new Mock<IControllerUpgrade>();
				mockUpgrader.Setup(x => x.IsUpgrading).Returns(true);

				var initializer = new ServiceManagerApplicationInitializer(
					mockUpgrader.Object,
					Mock.Of<ITcpIpRegistryAdjuster>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IServiceTaskLocksCleaner>(),
					Mock.Of<IHostApplicationLockAcquirer>());

				// Act
				initializer.Initialize();

				// Assert
				NUnit.Framework.Assert.That(Env.CurrentUser?.LoginName, Is.Not.EqualTo(User.ServiceUserName));
			}

			static string TestPackagePath => Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp");
		}
	}
}
