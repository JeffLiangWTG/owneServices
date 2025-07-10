using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Core;
using Enterprise.DbUpgrader.Assemblies;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Common.CW;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	class UpgradeManagerTest : TestCase
	{
		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestUnhandledExceptionsAreNotLostDueToUnhandledSqlLockLostException()
		{
			// Arrange
			const string expectedInnerExceptionMessage = nameof(TestUnhandledExceptionsAreNotLostDueToUnhandledSqlLockLostException);
			var logger = new StringBuilder();
			Exception threadException = null;

			var upgradeRunnerThread = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						var errorReporter = new Mock<IErrorReporter>();

						SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true, true);

						using var disposableErrorReporter = ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object);

						var sqlException = SqlExceptionBuilder.CreateSqlException(1222, expectedInnerExceptionMessage);
						var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
						PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
						var upgradeManager = new UpgradeManagerForTest(
							new VersionChangeInfoForTest(),
							upgradeInfo,
							forceBiDatabaseUpgrade: false,
							overrideAcquireApplicationLockoutAndRun: false);
						upgradeManager.OverrideShouldAcquireLockoutInTransaction = true;
						upgradeManager.override_KillUserConnections = true;
						upgradeManager.UserChooseToKillOtherDbConnections_ForTest = true;

						upgradeManager.UpgradeEvent += (_, message) => logger.AppendLine(message);
						upgradeManager.UserAction_Offline_ForTest = () =>
						{
							// Act
							Db.AdminConnection.CloseConnection();
							throw sqlException;
						};

						using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
						{
							_ = upgradeManager.Run();
						}
					}
				}
				catch (Exception ex)
				{
					threadException = ex;
				}
			});

			upgradeRunnerThread.Start();
			upgradeRunnerThread.Join();

			var upgradeLog = logger.ToString();
			// Assert
			CombineAssertions(upgradeLog, () =>
			{
				AssertContains(expectedInnerExceptionMessage, upgradeLog);
				AssertNull(threadException);
			});
		}

		[UseSnapshotProtection]
		public void TestOdysseyExceptionsAreReported()
		{
			// Arrange
			var odysseyException = new OdysseyException(nameof(TestOdysseyExceptionsAreReported));

			// Act
			// Assert
			TestRunWithCallbackActionAndAssertions(
				() => { throw odysseyException; },
				() =>
				{
					var innerException = ErrorReporter.LastExceptionReported.InnerException;

					AssertNotNull(innerException);
					AssertType<OdysseyException>(innerException);
					AssertEquals(nameof(TestOdysseyExceptionsAreReported), innerException.Message);
				}
			);
		}

		[UseSnapshotProtection]
		public void TestSchemaChangeExceptionsAreReported()
		{
			// Arrange
			var schemaChangeException = new SchemaChangeException(nameof(TestSchemaChangeExceptionsAreReported));

			// Act
			// Assert
			TestRunWithCallbackActionAndAssertions(
				() => { throw schemaChangeException; },
				() =>
				{
					var innerException = ErrorReporter.LastExceptionReported.InnerException;

					AssertNotNull(innerException);
					AssertType<SchemaChangeException>(innerException);
					AssertEquals(nameof(TestSchemaChangeExceptionsAreReported), innerException.Message);
				}
			);
		}

		[UseSnapshotProtection]
		public void TestBatchRunnerSqlExceptionsAreReported()
		{
			// Arrange
			var batchRunner = new BatchRunner();
			var testCommands = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("GetVersion", "SELECT @@VERSION"),
					new KeyValuePair<string, string>("GetCargoWiseOneVersion", "SELECT @@CargoWiseOneVersion;"),
					new KeyValuePair<string, string>("GetUtcDate", "SELECT GETUTCDATE();"),
				};

			// Act
			// Assert
			TestRunWithCallbackActionAndAssertions(
				() => { batchRunner.RunCollectionOfSqlCommands(Db.Connection, testCommands); },
				() =>
				{
					var innerException = ErrorReporter.LastExceptionReported.InnerException;

					AssertNotNull(innerException);
					AssertType<SqlException>(innerException);
					AssertEquals(@"Must declare the scalar variable ""@@CargoWiseOneVersion"".", innerException.Message);
				}
			);
		}

		[UseSnapshotProtection]
		public void TestArithmeticOverflowConvertingToDataTypeSqlExceptionsAreReported()
		{
			// Arrange
			var sqlArithmeticOverflowException =
				SqlExceptionBuilder.CreateSqlException(8115, nameof(DbErrorType.ArithmeticOverflowConvertingToDataType));

			// Act
			// Assert
			TestRunWithCallbackActionAndAssertions(
				() => { throw sqlArithmeticOverflowException; },
				() =>
				{
					var innerException = ErrorReporter.LastExceptionReported.InnerException;

					AssertNotNull(innerException);
					AssertType<SqlException>(innerException);
					AssertEquals(sqlArithmeticOverflowException, innerException);
				}
			);
		}

		[UseSnapshotProtection]
		public void TestUpgradeLockTimeoutDoesNotSendIssueReport()
		{
			// Arrange
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			var upgraderManagerMock = new Mock<UpgradeManager>(new VersionChangeInfoForTest(), upgradeInfo) { CallBase = true };
			var upgraderManager = upgraderManagerMock.Object;
			var timeoutException = CreateSqlException(new List<(int, string)> { (-2, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.") });
			upgraderManagerMock.Protected()
				.Setup("TryGetUpgradeLock")
				.Throws(timeoutException);
			var logger = new List<string>();
			upgraderManager.UpgradeEvent += (_, message) => logger.Add(message);

			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				// Act
				upgraderManager.Run();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Should not send issue report", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Should not send issue report", string.Empty, ErrorReporter.LastMessageReported);
					AssertContains("log should have busy message", "Database is busy", string.Join(System.Environment.NewLine, logger));
				});
			}
		}

		void TestRunWithCallbackActionAndAssertions(Action upgraderManagerRunAction, Action testAssertions)
		{
			// Arrange
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgraderManagerMock = new Mock<UpgradeManager>(new VersionChangeInfoForTest(), upgradeInfo) { CallBase = true };
			var upgraderManager = upgraderManagerMock.Object;

			// Do nothing for the online part to make the test fast.
			upgraderManager.UserAction_BeforeOnline_ForTest = () =>
			{
				upgraderManager.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgraderManagerMock
				.Setup(x => x.RunOfflineUpgrade())
				.Callback(() => { upgraderManagerRunAction?.Invoke(); });

			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				// Act
				upgraderManager.Run();

				// Assert
				testAssertions?.Invoke();
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseCompatibilityLevel()
		{
			var expectedLevel = Db.Connection.ServerVersionNumber.CompatibilityLevel;
			var allDbs = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);

			foreach (var db in allDbs)
			{
				AssertEquals("Compatibility level for DB = " + db, expectedLevel, GetCurrentCompatibility(db));
			}
		}

		[UseSnapshotProtection]
		public void TestDbStatistics()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				// Mock only the method we need, to simplify the test. 
				var mockPreparer = new Mock<IUpgradePreparation>();
				mockPreparer.Setup(x => x.TurnOffMainDbStatistics())
					.Callback(() => new UpgradePreparation(Db.AdminConnection).TurnOffMainDbStatistics());

				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgrader.override_KillUserConnections = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.MockUpgradePreparer = mockPreparer.Object;
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.DisableAutoStatistics = true;
				var currentStatisticsSettingsDuringUpgrade = "";
				upgrader.UserAction_ForTest = () =>
					{
						var upgStats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);
						currentStatisticsSettingsDuringUpgrade = upgStats.Current.ToString();

						throw new OperationCanceledException("Upgrade aborted by user");
					};

				StatisticsSwitch.Instance.SetDbStatisticsSettings(Db.AdminConnection, Db.DatabaseName, true, true, false);

				var stats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);
				AssertEquals("Precondition", "ON-ON-OFF", stats.Current.ToString());

				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunOfflineUpgrade());

				stats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);
				AssertEquals("Statistics has been switched OFF during upgrade", "OFF-OFF-OFF", currentStatisticsSettingsDuringUpgrade);
				AssertEquals("Statistics has been switched back and put ASYNC ON", "ON-ON-ON", stats.Current.ToString());
			}
		}

		[UseSnapshotProtection]
		public void TestStatisticsCollectionIsSuspendedDuringDBUpgradeAndResumeAfterwards()
		{
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmUsageQueue; DELETE dbo.StmUsage;");

			var numberOfTimesMonitorActionHasRun = 0;

			Action<int> monitor = (int i) =>
			{
				numberOfTimesMonitorActionHasRun++;
				ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = nameof(EnabledState.Detailed);
				PerformanceStatisticsCollector.ResetInstance();
				using (PerformanceStatisticsCollector.StartMonitoring($"action_TestStatisticsCollectionIsSuspendedDuringDBUpgradeAndResumeAfterwards{i}"))
				{
					Thread.Sleep(50);
				}
				ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = nameof(EnabledState.Disabled);
				PerformanceStatisticsCollector.AttemptFlush(true);
				WaitForFlushToComplete();
			};

			var factory = new BusinessObjectFactory();

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgrader.override_KillUserConnections = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();
				upgrader.UserAction_Offline_ForTest = () =>
				{
					upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), false);
				};
				upgrader.UserAction_ForTest = () =>
				{
					monitor(1);
				};

				AssertEquals("Pre-conditon: no statistics have been collected yet", 0, numberOfTimesMonitorActionHasRun);
				AssertEquals("Pre-conditon: no statistics have been collected yet", 0, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "1"));
				AssertEquals("Pre-conditon: no statistics have been collected yet", 0, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "2"));
				upgrader.RunOfflineUpgrade();
				AssertEquals("Monitoring action should have been called once", 1, numberOfTimesMonitorActionHasRun);
				AssertEquals("No stastistics have been collected during the upgrade", 0, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "1"));
				AssertEquals("No stastistics have been collected during the upgrade", 0, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "2"));
			}

			monitor(2);
			AssertEquals("Monitoring action should have been called a second time", 2, numberOfTimesMonitorActionHasRun);
			AssertEquals("No stastistics have been collected for action 1", 0, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "1"));
			AssertEquals("Statistics have been properly collected for action 2", 1, GetIsSuspendedDuringDBUpgradeStmUsageActionCount(factory, "2"));
		}

		static void WaitForFlushToComplete()
		{
			// Wait for the pending saves to complete, with a maximum wait time
			SpinWait.SpinUntil(() => TestPerformanceStatisticsCollector.PendingSaves == 0, TimeSpan.FromSeconds(10));

			// Ensure that all pending saves have completed
			AssertEquals(0, TestPerformanceStatisticsCollector.PendingSaves);
		}

		[UseSnapshotProtection]
		public void TestForceBiDatabaseUpgrade()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var versionInfo = new VersionChangeInfoForTest { DbReferenceVersion_Clr = SqlClrAssembliesVersion.Application };
				var upgrader = new UpgradeManagerForTest(versionInfo, upgradeInfo, forceBiDatabaseUpgrade: false);
				var assembliesUpgraderMock = new Mock<BaseUpgrader>(upgrader, Db.Connection, SqlClrAssembliesVersion.Application) { CallBase = true };
				assembliesUpgraderMock
					.Setup(x => x.IsUpgradeRequired)
					.Returns(false);
				upgrader.OverrideAssembliesUpgrader = assembliesUpgraderMock.Object;
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				upgrader.InitialiseRequiredUpgraders_Exposed();

				CombineAssertions("Case (ForceBiDatabaseUpgrade = false)", () =>
				{
					AssertEquals("Number of required upgrade actions", 3, upgrader.OfflineActionList_Exposed.Count);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "SQL Assemblies Upgrade", expected: false);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database Schema Upgrade", expected: false);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database View, Procedure, Function & Trigger Upgrade", expected: false);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database objects synchronising", expected: false);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Key Cycler", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Cleanup temporary resources", expected: false);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "PT Key creation", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "OFX Key creation", expected: true);
				});

				upgrader = new UpgradeManagerForTest(versionInfo, upgradeInfo, forceBiDatabaseUpgrade: true);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();

				CombineAssertions("Case (ForceBiDatabaseUpgrade = true)", () =>
				{
					AssertEquals("Number of required upgrade actions", 8, upgrader.OfflineActionList_Exposed.Count);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "SQL Assemblies Upgrade", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database Schema Upgrade", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database View, Procedure, Function & Trigger Upgrade", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database objects synchronising", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Key Cycler", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Cleanup temporary resources", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "PT Key creation", expected: true);
					AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "OFX Key creation", expected: true);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestDroppedTriggersRestoredInTransformWhenIsRequired_ScriptFalse()
		{
			//Arrange
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var versionInfo = new VersionChangeInfoForTest
				{
					OverrideIsRequired_Script = false,
					OverrideIsRequired_Transformation = true,
				};
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManagerForTest(versionInfo, upgradeInfo, forceBiDatabaseUpgrade: false);

				var mockAssembliesUpgrader = new Mock<BaseUpgrader>(upgrader, Db.AdminConnection, SqlClrAssembliesVersion.Application);
				mockAssembliesUpgrader.SetupGet(x => x.IsUpgradeRequired).Returns(false);
				upgrader.OverrideAssembliesUpgrader = mockAssembliesUpgrader.Object;
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				//Act
				upgrader.InitialiseRequiredUpgraders_Exposed();

				//Assert
				AssertUpgradeActionRequired(upgrader.OfflineActionList_Exposed, "Database View, Procedure, Function & Trigger Upgrade", expected: true);
			}
		}

		[UseSnapshotProtection]
		public void TestLoadAuditConnectionWhenNotRegistered()
		{
			try
			{
				Globals.IsTest_ForTest.Value = false;

				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
					PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
					var versionInfo = new VersionChangeInfoForTest { DbReferenceVersion_Clr = SqlClrAssembliesVersion.Application };
					var upgrader = new UpgradeManagerForTest(versionInfo, upgradeInfo, forceBiDatabaseUpgrade: false);
					upgrader.ShouldLoadBiServerIfNotRegistered = false;
					upgrader.SetConnection_ForTest(Db.AdminConnection);

					var logger = new List<string>();
					upgrader.UpgradeEvent += (_, message) => logger.Add(message);

					var response = upgrader.Run();

					AssertEquals(false, response.Successful);
					AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
					AssertCollectionContains(string.Join("\r\n", logger), $"Product Key verification failed. It’s not safe to deploy Audit database to BI server [{Db.ServerName}]. Remove BiAuditServer value from registry and try again.", logger);
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
			}
		}

		[UseSnapshotProtection]
		public void TestLoadDatawarehouseConnectionWhenNotRegistered()
		{
			try
			{
				Globals.IsTest_ForTest.Value = false;

				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				using (BiServers.TemporarilySetAuditServerToNull())
				{
					var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
					PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
					var versionInfo = new VersionChangeInfoForTest { DbReferenceVersion_Clr = SqlClrAssembliesVersion.Application };
					var upgrader = new UpgradeManagerForTest(versionInfo, upgradeInfo, forceBiDatabaseUpgrade: false);
					upgrader.ShouldLoadBiServerIfNotRegistered = false;
					upgrader.SetConnection_ForTest(Db.AdminConnection);

					var logger = new List<string>();
					upgrader.UpgradeEvent += (_, message) => logger.Add(message);

					var response = upgrader.Run();

					AssertEquals(false, response.Successful);
					AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
					AssertCollectionContains(string.Join("\r\n", logger), $"Product Key verification failed. It’s not safe to deploy EDW database to BI server [{Db.ServerName}]. Remove BiDataWarehouseServer value from registry and try again.", logger);
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
			}
		}

		[UseSnapshotProtection]
		public void TestCouldNotObtainExclusiveLockIsNotReportedToIssueManager()
		{
			// Arrange
			var errorsToThrow = new List<(int, string)> {
					(1807, "Could not obtain exclusive lock on database"),
					(1802, "CREATE DATABASE failed"),
					};
			var ex = CreateSqlException(errorsToThrow);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestCannotRenameTableBecauseItIsPublishedForReplication()
		{
			// Arrange
			var ex = CreateSqlException(new List<(int, string)> { (15051, "Cannot rename the table because it is published for replication.") });
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestModifyFileEncounteredOperatingSystemError()
		{
			// Arrange
			var ex = CreateSqlException(new List<(int, string)> { (5149, "MODIFY FILE encountered operating system error 112(There is not enough space on the disk.)") });
			var outerEx = new Exception("failed", ex);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestHandleException_DbInSingleUserMode_ShouldNotReportIssue()
		{
			// Assert
			var ex = CreateSqlException(new List<(int, string)> { (18461, "Login failed for user 'OdysseyAdmin'. Reason: Server is in single user mode. Only one administrator can connect at this time.") });
			var outerEx = new Exception("failed", ex);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestHandleException_DbFilegroupIsFull_ShouldNotReportIssue()
		{
			// Arrange
			var ex = CreateSqlException(new List<(int, string)> { (1105, "Could not allocate space for object in database because the 'PRIMARY' filegroup is full") });
			var outerEx = new Exception("failed", ex);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestHandleException_LogFileIsFull_ShouldNotReportIssue()
		{
			// Arrange
			var ex = CreateSqlException(new List<(int, string)> { (9002, "The transaction log for database 'Odyssey_SD001' is full due to 'ACTIVE_TRANSACTION'.") });
			var outerEx = new Exception("failed", ex);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(ex);
			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Message == ex.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestHandleExceptionHandleSqlInfraException()
		{
			// Arrange
			var sqlException = SqlExceptionBuilder.CreateSqlException(-1, 0, 0, Db.ServerName, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0);

			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(sqlException);

			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == sqlException.Message));
			AssertEquals("Should not send ReportOnce", string.Empty, ErrorReporter.LastKeyReported);
		}

		[UseSnapshotProtection]
		public void TestKillingUserConnectionsHasExceededTimeout()
		{
			AssertKillingUserConnectionsHasExceededTimeout_LoggedAndNotReported(fromTask: false);
		}

		[UseSnapshotProtection]
		public void TestKillingUserConnectionsHasExceededTimeout_FromTask()
		{
			AssertKillingUserConnectionsHasExceededTimeout_LoggedAndNotReported(fromTask: true);
		}

		[UseSnapshotProtection]
		public void TestHandleException_OnlinePreSchemaUpgraderEnvironmentException()
		{
			// Arrange
			var sqlException = CreateSqlException(new List<(int, string)> { (-2, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.") });
			var environmentException = new OnlinePreSchemaUpgraderEnvironmentException("Some environment error", sqlException);
			var exceptionFromBaseUpgrader = new Exception("Database Schema Online Upgrade failed.\r\n" + environmentException.Message, environmentException);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(exceptionFromBaseUpgrader);

			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == exceptionFromBaseUpgrader.Message));
			AssertEquals("We should not ReportOnce OnlinePreSchemaUpgraderEnvironmentException", null, ErrorReporter.LastExceptionReported);
		}

		[UseSnapshotProtection]
		public void TestHandleException_Win32Timeout_ShouldNotReportIssue()
		{
			// Arrange
			var ex = new Win32Exception(258, "The wait operation timed out");
			var outerEx = new Exception("failed", ex);
			var aggregateEx = new AggregateException("aggregated", outerEx);
			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(aggregateEx);

			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message == aggregateEx.Message));
			AssertEquals("Should not send ReportOnce", 0, ErrorReporter.TotalErrorCount);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This is native connection since DbConnection has retry setup which needs to be avoided to validate the test")]
		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_KillsExtraConnection_WithNoLockout()
		{
			IDbConnection connection = null;

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.UserAction_BeforeOnline_ForTest = () =>
				{
					var newOnlineActionList = new UpgradeActionList();
					upgrader.SetOnlineActionList_ForTest(newOnlineActionList);
				};
				upgrader.UserAction_Offline_ForTest = () =>
				{
					upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), false);
				};
				try
				{
					//This is native connection since DbConnection has retry setup which needs to be avoided to validate the test
					connection = new SqlConnection(new SqlConnectionStringBuilder
					{
						ApplicationName = "Upgrade Manager Test",
						ConnectTimeout = (int)TimeSpan.FromSeconds(300).TotalSeconds,
						DataSource = Db.ServerName,
						InitialCatalog = Db.DatabaseName,
						IntegratedSecurity = true,
						Pooling = false,
						ConnectRetryCount = 0,
						TrustServerCertificate = true,
					}.ToString());
					connection.Open();
					// As it takes a bit of time to complete the login and save login_time in sys.dm_exec_sessions,
					// so delay some time here to make sure the login_time is earlier than the login cutoff time during upgrade.
					Thread.Sleep(TimeSpan.FromMilliseconds(100));

					upgrader.requiresLockout = false;
					upgrader.RunOfflineUpgrade();
					AssertExceptionThrown<SqlException>("Extra DB Connection Killed During Upgrade With Native Connection", () =>
					{
						IDbCommand cmd = connection.CreateCommand();
						cmd.CommandText = "SELECT @@version";
						var result = cmd.ExecuteScalar();
					});
				}
				finally
				{
					if (connection != null && ConnectionState.Open == connection.State)
					{
						connection.Close();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_KillsExtraConnection_LegacyLockout()
			=> AssertRunOfflineUpgrade_KillsExtraConnection(lockoutInTransaction: false);

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_KillsExtraConnection_TransactionLockout()
			=> AssertRunOfflineUpgrade_KillsExtraConnection(lockoutInTransaction: true);

		void AssertRunOfflineUpgrade_KillsExtraConnection(bool lockoutInTransaction)
		{
			DbConnection mainConnection = null;

			using (Globals.SetIsUserInteractiveForTest(false))
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.UserAction_BeforeOnline_ForTest = () =>
				{
					upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
				};
				upgrader.UserAction_Offline_ForTest = () =>
				{
					var newOfflineActionList = new UpgradeActionList();
					upgrader.SetOffLineActionList_ForTest(newOfflineActionList, true);
					AssertExceptionThrown<DatabaseUpgradeInProgressException>("Extra DB Connection Killed During Upgrade", () =>
					{
						mainConnection.ExecuteScalar("SELECT @@version");
					});
				};

				try
				{
					mainConnection = Db.NewExtraConnectionToMainDb();
					mainConnection.EnsureIsOpen();
					upgrader.requiresLockout = true;
					// Act
					upgrader.RunOfflineUpgrade();
				}
				finally
				{
					if (mainConnection != null && ConnectionState.Open == mainConnection.State)
					{
						mainConnection.CloseConnection();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_UserConfirmation_LegacyLockout()
			=> AssertRunOfflineUpgrade_UserConfirmation(lockoutInTransaction: false);

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_UserConfirmation_TransactionLockout()
			=> AssertRunOfflineUpgrade_UserConfirmation(lockoutInTransaction: true);

		void AssertRunOfflineUpgrade_UserConfirmation(bool lockoutInTransaction)
		{
			SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, false, true);
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false);
				upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.requiresLockout = true;
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.UserHasCancelledToKillOtherDbConnections_ForTest = true;
				upgrader.UserAction_ForTest = () =>
				{
					throw new OperationCanceledException("Upgrade aborted by user");
				};

				AssertExceptionThrown<DeniedByUserException>(() => upgrader.RunOfflineUpgrade());
				Assert(UnitTestUserNotification.Instance.PreviousMessages.Length == 1);

				using (Env.Instance.SetTemporaryUserContext(null, Guid.Empty, Guid.Empty))
				{
					AssertExceptionThrown<DeniedByUserException>(() => upgrader.RunOfflineUpgrade());

					var original = Globals.IsUserInteractive;
					try
					{
						Globals.IsUserInteractive = false;
						AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunOfflineUpgrade());
					}
					finally
					{
						Globals.IsUserInteractive = original;
					}

					AssertNoExceptionThrown(() => upgrader.Run());
					AssertEquals("Upgrade canceled by user. Failed to lock application access to the database [" + Db.DatabaseName + "] because applications are still running by other user.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupSemaphores_LegacyLockout()
			=> AssertCleanupSemaphores(lockoutInTransaction: false, simulateFailure: false);

		[UseSnapshotProtection]
		public void TestCleanupSemaphores_TransactionLockout()
			=> AssertCleanupSemaphores(lockoutInTransaction: true, simulateFailure: false);

		[UseSnapshotProtection]
		public void TestCleanupSemaphoresOnFailure_LegacyLockout()
			=> AssertCleanupSemaphores(lockoutInTransaction: false, simulateFailure: true);

		[UseSnapshotProtection]
		public void TestCleanupSemaphoresOnFailure_TransactionLockout()
			=> AssertCleanupSemaphores(lockoutInTransaction: true, simulateFailure: true);

		void AssertCleanupSemaphores(bool lockoutInTransaction, bool simulateFailure)
		{
			const string countSemaphore = @"SELECT count(*) FROM dbo.StmServiceSemaphore";

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			using (Db.AdminConnection.BeginTransactionWithManager())
			{
				var connection = Db.AdminConnection;
				// Arrange
				connection.ExecuteNonQuery(@"
INSERT dbo.StmServiceHeartBeat(SV_PK, SV_ExpiresAtUtc, SV_WorkstationName, SV_ProcessID, SV_HeartbeatType, SV_ParentId, SV_CreateTimeUtc, SV_ParentTableCode)
	VALUES ('29D8BE56-7463-460F-B2D0-F03B930D7224', '2014-07-11 05:40:13.587', 'SYD-ABCD-1', 8824, 'ENT', 'ABE1D8D8-A709-4BFA-88E3-53997AA925E2', '2014-07-11 05:34:43.557', 'GS')
INSERT dbo.StmServiceSemaphore(SS_PK, SS_ServiceClass, SS_LockInfo, SS_UseCount, SS_AcquiredTimeUtc, SS_SV)
	VALUES ('e94eb468-f340-471b-8813-00ab08c9dee2', 'ACT', 'EnterprisePendingUserAction:d3e6aa3e-b695-4466-bbab-c9aae6134621', 1, '2014-07-13 23:55:33.960', '29D8BE56-7463-460F-B2D0-F03B930D7224')"
				);
				connection.CommitTransaction();

				var semaphoreCount = (int)connection.ExecuteScalar(countSemaphore);
				AssertGreaterThan("Semaphores exist in database", semaphoreCount, 0);

				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.requiresLockout = true;
				upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.SetConnection_ForTest(connection);

				using (Globals.SetIsUserInteractiveForTest(false))
				{
					if (simulateFailure)
					{
						upgrader.UserAction_Offline_ForTest = () =>
						{
							throw new OperationCanceledException("Upgrade aborted by user");
						};

						// Act
						AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunOfflineUpgrade());
					}
					else
					{
						upgrader.UserAction_Offline_ForTest = () =>
						{
							upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), true);
						};

						// Act
						upgrader.RunOfflineUpgrade();
					}
				}

				// Assert
				var newSemaphoreCount = (int)Db.Connection.ExecuteScalar(countSemaphore);
				AssertEquals("Semaphores have been removed", 0, newSemaphoreCount);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupSqlMutexLocks_LegacyLockout()
			=> AssertCleanupSqlMutexLocks(lockoutInTransaction: false, simulateFailure: false);

		[UseSnapshotProtection]
		public void TestCleanupSqlMutexLocks_TransactionLockout()
			=> AssertCleanupSqlMutexLocks(lockoutInTransaction: true, simulateFailure: false);

		[UseSnapshotProtection]
		public void TestCleanupSqlMutexLocksOnFailure_LegacyLockout()
			=> AssertCleanupSqlMutexLocks(lockoutInTransaction: false, simulateFailure: true);

		[UseSnapshotProtection]
		public void TestCleanupSqlMutexLocksOnFailure_TransactionLockout()
			=> AssertCleanupSqlMutexLocks(lockoutInTransaction: true, simulateFailure: true);

		void AssertCleanupSqlMutexLocks(bool lockoutInTransaction, bool simulateFailure)
		{
			// Arrange
			var sqlMutexProvider = new SqlMutexLockProvider();
			sqlMutexProvider.GetUnobservedLock("TST", "test lock 1", TimeSpan.FromSeconds(15), "USR");
			sqlMutexProvider.GetUnobservedLock("TST", "test lock 2", TimeSpan.FromSeconds(120), "USR");
			sqlMutexProvider.GetUnobservedLock("TST", "test lock 3", TimeSpan.FromSeconds(300), "USR");

			const string sqlCountMutexLocks = @"SELECT count(*) FROM dbo.StmServiceMutex";
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			using (Db.AdminConnection.BeginTransactionWithManager())
			{
				// Arrange
				var connection = Db.AdminConnection;
				var mutexLocksCount = (int)connection.ExecuteScalar(sqlCountMutexLocks);
				AssertGreaterThanOrEqualTo("Sql mutex locks exist in database", mutexLocksCount, 3);

				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.requiresLockout = true;
				upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.SetConnection_ForTest(connection);

				using (Globals.SetIsUserInteractiveForTest(false))
				{
					if (simulateFailure)
					{
						upgrader.UserAction_Offline_ForTest = () =>
						{
							throw new OperationCanceledException("Upgrade aborted by user");
						};

						// Act
						AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunOfflineUpgrade());
					}
					else
					{
						upgrader.UserAction_Offline_ForTest = () =>
						{
							upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), true);
						};

						// Act
						upgrader.RunOfflineUpgrade();
					}
				}

				// Assert
				var newMutexLocksCount = (int)Db.Connection.ExecuteScalar(sqlCountMutexLocks);
				AssertEquals("Sql mutex locks have been cleaned", 0, newMutexLocksCount);
			}
		}

		[UseSnapshotProtection]
		public void TestRunParallelUpgrades()
		{
			SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, false, true);
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				var initialUpgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(initialUpgradeInfo.PK, initialUpgradeInfo.Version, "RDY");
				var initialUpgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), initialUpgradeInfo, false, true);
				var mock = new Mock<SoftwareUpgrader>();
				mock.CallBase = true;
				var readySignal = new AutoResetEvent(false);
				ValidationResponse secondResponse = null;

				mock.Setup(m => m.RunCurrentVersionWriter(It.IsAny<UpgradeInfo>(), It.IsAny<IUpgradeManager>()))
					.Callback(() => readySignal.Set());
				SoftwareUpgrader.OverridableInstance.Value = mock.Object;

				var runner2Thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var otherVersion = ReleaseInfo.Instance.VersionNumber.AddPatch(1);
						var secondUpgradeInfo = new UpgradeInfo(Guid.NewGuid(), otherVersion.ToVersion());
						PackageHelper.InsertUpgradePackage(secondUpgradeInfo.PK, secondUpgradeInfo.Version, "RDY");
						var secondUpgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), secondUpgradeInfo, false, true);

						readySignal.WaitOne();
						secondResponse = secondUpgrader.Run();
					}
				});

				runner2Thread.Start();
				var initialUpgradeResponse = initialUpgrader.Run();
				if (!runner2Thread.Join(TimeSpan.FromSeconds(30)))
				{
					Fail("RunCurrentVersionWriter wasn't called after 30 seconds");
				}
				var lockInfo = DataUtils.LoadDbExtendedProperty(Db.Connection, Constants.SystemUpgrade.DbUpgradeLockExtPty);

				CombineAssertions(() =>
				{
					AssertEquals(true, initialUpgradeResponse.Successful);
					AssertEquals(null, initialUpgradeResponse.Information);

					AssertNotNull(secondResponse);
					AssertEquals(false, secondResponse.Successful);
					Assert(secondResponse.Information.StartsWith("Database is locked for upgrade"));
					Assert(lockInfo, lockInfo.StartsWith("User: 'CargoWise Support' started a system upgrade at utc time:"));
				});
				mock.VerifyAll();
			}
		}

		[UseSnapshotProtection]
		public void TestHooksAreInitializedAndUninitialized()
		{
			// Arrange
			var clientHookMock = new Mock<IClientHook>();
			var clientHookLoaderMock = new Mock<IClientHookLoader>();
			clientHookLoaderMock.SetupGet(loader => loader.ClientHook).Returns(clientHookMock.Object);

			var sequence = new MockSequence();
			clientHookMock
				.InSequence(sequence)
				.SetupGet(hook => hook.HasCompanySpecificOverrides)
				.Returns(true);
			clientHookMock
				.InSequence(sequence)
				.SetupSet<bool>(hook => hook.IsUpgrading = true);
			clientHookMock
				.InSequence(sequence)
				.SetupGet(hook => hook.IsInitialised)
				.Returns(false);
			clientHookMock
				.InSequence(sequence)
				.Setup(hook => hook.Initialise(true));
			clientHookMock
				.InSequence(sequence)
				.SetupGet(hook => hook.HasCompanySpecificOverrides)
				.Returns(true);
			clientHookMock
				.InSequence(sequence)
				.SetupSet<bool>(hook => hook.IsUpgrading = false);
			clientHookMock
				.InSequence(sequence)
				.SetupGet(hook => hook.IsInitialised)
				.Returns(true);
			clientHookMock
				.InSequence(sequence)
				.Setup(hook => hook.Uninitialise());

			using (ObjectFactory.Substitute(clientHookLoaderMock.Object))
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, overrideAcquireApplicationLockoutAndRun: true);
				SoftwareUpgrader.OverridableInstance.Value = new Mock<SoftwareUpgrader>().Object;

				// Act
				upgrader.Run();

				// Assert
				AssertNoExceptionThrown(() =>
				{
					clientHookLoaderMock.VerifyGet(loader => loader.ClientHook, Times.AtLeastOnce);
					clientHookMock.Verify(hook => hook.Uninitialise(), Times.Once);
				});
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestClientHooksInitializedOnMainThread()
		{
			var clientHookMock = new Mock<IClientHook>();
			clientHookMock.SetupGet(hook => hook.HasCompanySpecificOverrides).Returns(true);
			var inSyncInvoke = false;
			var clientHookLoaderMock = new Mock<IClientHookLoader>();
			clientHookLoaderMock.SetupGet(loader => loader.ClientHook).Returns(() =>
			{
				if (!inSyncInvoke)
				{
					throw new InvalidOperationException("ClientHook.get should be called from the main thread");
				}
				return clientHookMock.Object;
			});
			clientHookMock.Setup(hook => hook.Initialise(true)).Callback(() =>
			{
				if (!inSyncInvoke)
				{
					throw new InvalidOperationException("IClientHook.Initialize should be called from the main thread");
				}
			});
			var syncInvokeHook = new Mock<ISynchronizeInvoke>();
			syncInvokeHook.Setup(s => s.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>())).Callback<Delegate, object[]>((d, a) =>
			{
				inSyncInvoke = true;
				d.DynamicInvoke(a);
				inSyncInvoke = false;
			});
			using (ObjectFactory.Substitute(clientHookLoaderMock.Object))
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, overrideAcquireApplicationLockoutAndRun: true);
				upgrader.SyncInvoke = syncInvokeHook.Object;
				SoftwareUpgrader.OverridableInstance.Value = new Mock<SoftwareUpgrader>().Object;
				upgrader.Run();
			}
		}

		[UseSnapshotProtection]
		public void TestGetConfirmationToKillOtherDbConnectionsOnMainThread()
		{
			var isInvoked = false;
			var syncInvokeHook = new Mock<ISynchronizeInvoke>();
			syncInvokeHook.Setup(s => s.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>())).Returns<Delegate, object[]>((d, a) =>
			{
				isInvoked = true;
				return d.DynamicInvoke(a);
			});
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false, overrideAcquireApplicationLockoutAndRun: true);
			upgrader.SyncInvoke = syncInvokeHook.Object;
			SoftwareUpgrader.OverridableInstance.Value = new Mock<SoftwareUpgrader>().Object;
			upgrader.UserChooseToKillOtherDbConnections_ForTest = true;

			var mockUserInteraction = new Mock<IDbUpgraderUserInteraction>();
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(typeof(IDbUpgraderUserInteraction))).Returns(mockUserInteraction.Object);
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				upgrader.GetConfirmationToKillOtherDbConnections();
			}
			AssertEquals("GetConfirmation should be invoked with SyncInvoke", expected: true, isInvoked);
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_DeleteSchedulesOfDecommissionedServiceTask_LegacyLockout()
			=> AssertRunOfflineUpgrade_DeleteSchedulesOfDecommissionedServiceTask(lockoutInTransaction: false);

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_DeleteSchedulesOfDecommissionedServiceTask_TransactionLockout()
			=> AssertRunOfflineUpgrade_DeleteSchedulesOfDecommissionedServiceTask(lockoutInTransaction: true);

		void AssertRunOfflineUpgrade_DeleteSchedulesOfDecommissionedServiceTask(bool lockoutInTransaction)
		{
			const string scheduleTypeCode1 = "CCC";
			const string scheduleTypeCode2 = "DDD";
			var cleaner = new ServiceTasksSchedulesCleaner();
			var allCodes = cleaner.GetAllCurrentServiceCodes();

			Assert(!allCodes.Contains(scheduleTypeCode1));
			Assert(!allCodes.Contains(scheduleTypeCode2));

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				// Arrange
				var connection = Db.AdminConnection;
				connection.ExecuteNonQuery($@"
			INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','{scheduleTypeCode1}','Description C','D')
			INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','{scheduleTypeCode2}','Description C','D')");

				AssertEquals(2, connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('{scheduleTypeCode1}','{scheduleTypeCode2}')"));

				SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true);
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.requiresLockout = true;
				upgrader.override_KillUserConnections = true;
				upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = true;
				upgrader.UserAction_Offline_ForTest = () =>
				{
					upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), true);
				};
				upgrader.SetConnection_ForTest(connection);

				// Act
				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					upgrader.RunOfflineUpgrade();
				}
				// Assert
				AssertEquals(0, connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('{scheduleTypeCode1}','{scheduleTypeCode2}')"));
			}
		}

		[UseSnapshotProtection]
		public void TestShouldSetAuditServerToDefault()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				AssertEquals("Unregistered system should not set audit server to default", false, upgrader.ShouldSetAuditServerToDefault(Db.ServerName));

				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				registrationKey.EnterpriseCodeForTest = "WTL";
				registrationKey.IsInternalSystemForTest = true;

				AssertEquals("Internal system should not set audit server to default", false, upgrader.ShouldSetAuditServerToDefault(null));

				registrationKey.IsInternalSystemForTest = false;
				registrationKey.HostedLocationForTest = "SYD";

				AssertEquals("Hosted client system should set audit server to default", true, upgrader.ShouldSetAuditServerToDefault(null));
			}
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderLockTimeout()
		{
			var lockTimeoutBeforeOnline = -2;
			var lockTimeoutDuringOnline = -2;
			var lockTimeoutAfterOnline = -2;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, false);
			upgrader.override_KillUserConnections = true;
			upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
			upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
			upgrader.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = true;

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());

				lockTimeoutBeforeOnline = GetLockTimeout(Db.Connection);
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				lockTimeoutDuringOnline = GetLockTimeout(Db.Connection);
			};

			upgrader.UserAction_AfterOnline_ForTest = () =>
			{
				lockTimeoutAfterOnline = GetLockTimeout(Db.Connection);

				throw new OperationCanceledException("Upgrade aborted by user");
			};

			Env.Registry.UpgradeObserverMaxWaitInSeconds = -1;
			using (TemporarySetInitialLockTimeout(Db.Connection, 15_000))
			{
				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());

				var fullLog = string.Join(System.Environment.NewLine, logger);
				AssertContains("Should change LockTimeout to Infinite"
					, expected: "Switching LockTimeout to Infinite"
					, actualContainingExpected: fullLog);
				AssertContains("Should restore LockTimeout"
					, expected: "Restoring LockTimeout"
					, actualContainingExpected: fullLog);

				AssertEquals("LockTimout Before Online", 15_000, lockTimeoutBeforeOnline);
				AssertEquals("LockTimout During Online", DbConnection.LockTimeout.Infinite, lockTimeoutDuringOnline);
				AssertEquals("LockTimout Aftere Online", 15_000, lockTimeoutAfterOnline);
			}
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderDontUseObserver()
		{
			var useObserver = false;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				useObserver = upgrader.UseObserver_ForTest;

				throw new OperationCanceledException("Upgrade aborted by user");
			};

			Env.Registry.UpgradeObserverMaxWaitInSeconds = -1;

			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
			AssertEquals("Use Observer?", false, useObserver);
			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertNotContains("Should not use observer"
				, expected: "Using On-line Upgrade Observer"
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderUseObserver()
		{
			var useObserver = false;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				useObserver = upgrader.UseObserver_ForTest;

				throw new OperationCanceledException("Upgrade aborted by user");
			};

			Env.Registry.UpgradeObserverMaxWaitInSeconds = 100;

			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
			AssertEquals("Use Observer?", true, useObserver);
			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertContains("Should use observer"
				, expected: "Using On-line Upgrade Observer"
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderObserverInterval()
		{
			var observerIntervalInMs = -2;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				observerIntervalInMs = upgrader.ObserverInterval_ForTest;

				throw new OperationCanceledException("Upgrade aborted by user");
			};

			Env.Registry.UpgradeObserverMaxWaitInSeconds = 0;

			using (TemporarySetInitialLockTimeout(Db.Connection, 15_250))
			{
				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Observer Interval During Online", 0, observerIntervalInMs);

				Env.Registry.UpgradeObserverMaxWaitInSeconds = 10;

				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Observer Interval During Online", 10_000, observerIntervalInMs);

				Env.Registry.UpgradeObserverMaxWaitInSeconds = 100;

				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Observer Interval During Online", 15_250, observerIntervalInMs);
			}

			using (TemporarySetInitialLockTimeout(Db.Connection, DbConnection.LockTimeout.Infinite))
			{
				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Observer Interval During Online", 100_000, observerIntervalInMs);
			}
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderBlocked_NoQueue_ObserverKillsBlockersAfterWait()
		{
			var upgradeSuccesfull = false;
			var blockerSPID_1 = 0;
			var blockerSPID_2 = 0;

			#region Prepare locking environment

			var table_1 = "TestTable_1";
			Db.Connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS dbo.{table_1};
CREATE TABLE dbo.{table_1} (PK int NOT NULL);
INSERT dbo.{table_1} (PK) VALUES(0);
");

			var table_2 = "TestTable_2";
			Db.Connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS dbo.{table_2};
CREATE TABLE dbo.{table_2} (PK int NOT NULL);
INSERT dbo.{table_2} (PK) VALUES(0);
");

			#endregion // Prepare locking environment

			Task blocker2Task = null;

			using (TemporarySetInitialLockTimeout(Db.Connection, DbConnection.LockTimeout.Infinite))
			using (var blocker_1 = Db.NewExtraConnectionToMainDb())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

				var logger = new List<string>();
				upgrader.UpgradeEvent += (_, message) => logger.Add(message);

				upgrader.UserAction_BeforeOnline_ForTest = () =>
				{
					#region Setup blockers

					// setup head blocker
					blockerSPID_1 = blocker_1.SPID;
					// put locks and keep transaction open
					blocker_1.BeginTransaction();
					// put lock for second blocker in a row, and then for DbUpgrader
					blocker_1.ExecuteNonQuery($"UPDATE dbo.{table_1} SET PK = 1", cmd => cmd.CommandTimeout = 1);

					// setup second blocker
					using (var readyToStart = new AutoResetEvent(false))
					{
						blocker2Task = Task.Run(() =>
						{
							using (Db.DisposableActionForDbConnection())
							using (var blocker_2 = Db.NewExtraConnectionToMainDb())
							{
								blockerSPID_2 = blocker_2.SPID;
								blocker_2.RunInTransaction(() =>
								{
									// put lock for DbUpgrader
									blocker_2.ExecuteNonQuery($"UPDATE dbo.{table_2} SET PK = 2", cmd => cmd.CommandTimeout = 1);
									readyToStart.Set();
									// blocked by head blocker
									blocker_2.ExecuteNonQuery($"UPDATE dbo.{table_1} SET PK = 2", cmd => cmd.CommandTimeout = 60);
								});
							}
						});

						readyToStart.WaitOne(); // Ensure second blocker is running
					}

					#endregion // Setup blockers

					// setup DbUpgrader actions
					var newOnlineActionList = new UpgradeActionList();
					newOnlineActionList.Add(() =>
					{
						// blocked by second blocker
						Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.{table_2} ADD NewCol int NULL", cmd => cmd.CommandTimeout = 60);
						// blocked by head blocker
						Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.{table_1} ADD NewCol int NULL", cmd => cmd.CommandTimeout = 60);
					});
					upgrader.SetOnlineActionList_ForTest(newOnlineActionList);
				};

				upgrader.UserAction_AfterOnline_ForTest = () =>
				{
					upgradeSuccesfull =
						Db.Connection.Exists($"FROM dbo.{table_1} WHERE PK = 0") // no updates from blockers
						&& Db.Connection.Exists($"FROM dbo.{table_2} WHERE PK = 0") // no updates from blockers
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_1}', N'U') AND name = N'NewCol'") // DbUpgrader added columns successfully
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_2}', N'U') AND name = N'NewCol'") // DbUpgrader added columns successfully
						;

					throw new OperationCanceledException("Upgrade aborted by user");
				};

				var maxWait = 1;
				Env.Registry.UpgradeObserverMaxWaitInSeconds = maxWait;

				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Upgrade Succesfull?", true, upgradeSuccesfull);

				var fullLog = string.Join(System.Environment.NewLine, logger);
				AssertContains("Blocker_2 should be killed after wait"
					, expected: $"Client SPID {blockerSPID_2} has been killed after wait due to blocking upgrade process (WaitType: LCK_M_SCH_M, ApplicationLockTimeout: -1 millisecond(s), MaxWait: {maxWait} second(s))"
					, actualContainingExpected: fullLog);
				AssertContains("Blocker_1 should be killed immediately (forced)"
					, expected: $"Client SPID {blockerSPID_1} has been killed (forced) due to blocking upgrade process (WaitType: LCK_M_SCH_M, ApplicationLockTimeout: -1 millisecond(s), MaxWait: {maxWait} second(s))"
					, actualContainingExpected: fullLog);

				try
				{
					blocker2Task.Wait();
				}
				catch
				{ }
			}
		}

		[UseSnapshotProtection]
		public void TestDbUpgraderBlocked_HasQueue_ObserverKillsBlockers()
		{
			var upgradeSuccesfull = false;
			var blockerSPID_1 = 0;
			var blockerSPID_2 = 0;

			#region Prepare locking environment

			var table_1 = "TestTable_1";
			Db.Connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS dbo.{table_1};
CREATE TABLE dbo.{table_1} (PK int NOT NULL);
INSERT dbo.{table_1} (PK) VALUES(0);
");

			var table_2 = "TestTable_2";
			Db.Connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS dbo.{table_2};
CREATE TABLE dbo.{table_2} (PK int NOT NULL);
INSERT dbo.{table_2} (PK) VALUES(0);
");

			var table_3 = "TestTable_3";
			Db.Connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS dbo.{table_3};
CREATE TABLE dbo.{table_3} (PK int NOT NULL);
INSERT dbo.{table_3} (PK) VALUES(0);
");

			#endregion // Prepare locking environment

			Task blocker2Task = null;

			using (TemporarySetInitialLockTimeout(Db.Connection, 1_000))
			using (var blocker_1 = Db.NewExtraConnectionToMainDb())
			using (var readyToQueue = new AutoResetEvent(false))
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

				var logger = new List<string>();
				upgrader.UpgradeEvent += (_, message) => logger.Add(message);

				upgrader.UserAction_BeforeOnline_ForTest = () =>
				{
					#region Setup blockers

					// setup head blocker
					blockerSPID_1 = blocker_1.SPID;
					// put locks and keep transaction open
					blocker_1.BeginTransaction();
					// put lock for second blocker in a row, and then for DbUpgrader
					blocker_1.ExecuteNonQuery($"UPDATE dbo.{table_1} SET PK = 1", cmd => cmd.CommandTimeout = 1);

					// setup second blocker
					using (var readyToStart = new AutoResetEvent(false))
					{
						blocker2Task = Task.Run(() =>
						{
							using (Db.DisposableActionForDbConnection())
							using (var blocker_2 = Db.NewExtraConnectionToMainDb())
							using (blocker_2.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
							{
								blockerSPID_2 = blocker_2.SPID;

								blocker_2.BeginTransaction();
								// give some time to a head blocker
								blocker_2.ExecuteNonQuery("WAITFOR DELAY '00:00:01'");
								// put lock for DbUpgrader
								blocker_2.ExecuteNonQuery($"UPDATE dbo.{table_2} SET PK = 2", cmd => cmd.CommandTimeout = 1);
								readyToStart.Set();
								// blocked by head blocker
								blocker_2.ExecuteNonQuery($"UPDATE dbo.{table_1} SET PK = 2", cmd => cmd.CommandTimeout = 60);
							}
						});

						readyToStart.WaitOne(); // Ensure second blocker is running
					}

					#endregion // Setup blockers

					// setup DbUpgrader actions
					var newOnlineActionList = new UpgradeActionList();
					newOnlineActionList.Add(() =>
					{
						Db.Connection.RunInTransaction(() =>
						{
							// put lock for queued process
							Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.{table_3} ADD NewCol int NULL", cmd => cmd.CommandTimeout = 1);
							readyToQueue.Set();

							// blocked by second blocker
							Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.{table_2} ADD NewCol int NULL", cmd => cmd.CommandTimeout = 60);
							// blocked by head blocker
							Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.{table_1} ADD NewCol int NULL", cmd => cmd.CommandTimeout = 60);
						});
					});
					upgrader.SetOnlineActionList_ForTest(newOnlineActionList);
				};

				upgrader.UserAction_AfterOnline_ForTest = () =>
				{
					upgradeSuccesfull =
						Db.Connection.Exists($"FROM dbo.{table_1} WHERE PK = 0") // no updates from blockers
						&& Db.Connection.Exists($"FROM dbo.{table_2} WHERE PK = 0") // no updates from blockers
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_1}', N'U') AND name = N'NewCol'") // DbUpgrader added columns successfully
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_2}', N'U') AND name = N'NewCol'") // DbUpgrader added columns successfully
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_3}', N'U') AND name = N'NewCol'") // DbUpgrader added columns successfully
						&& Db.Connection.Exists($"FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table_3}', N'U') AND name = N'QueuedCol'") // Queued process added column successfully
						;

					throw new OperationCanceledException("Upgrade aborted by user");
				};

				// setup queued client process
				var task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var queued = Db.NewExtraConnectionToMainDb())
					using (queued.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
					{
						readyToQueue.WaitOne();
						// blocked by DbUpgrader
						queued.ExecuteNonQuery($"ALTER TABLE dbo.{table_3} ADD QueuedCol int NULL", cmd => cmd.CommandTimeout = 60);
					}
				});

				var maxWait = 100;
				Env.Registry.UpgradeObserverMaxWaitInSeconds = maxWait;

				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
				AssertEquals("Upgrade Succesfull?", true, upgradeSuccesfull);

				var fullLog = string.Join(System.Environment.NewLine, logger);
				AssertContains("Blocker_2 should be killed immediately"
					, expected: $"Client SPID {blockerSPID_2} has been killed immediately due to blocking upgrade process (WaitType: LCK_M_SCH_M, ApplicationLockTimeout: 1,000 millisecond(s), MaxWait: {maxWait} second(s))"
					, actualContainingExpected: fullLog);
				AssertContains("Blocker_1 should be killed immediately (forced)"
					, expected: $"Client SPID {blockerSPID_1} has been killed (forced) due to blocking upgrade process (WaitType: LCK_M_SCH_M, ApplicationLockTimeout: 1,000 millisecond(s), MaxWait: {maxWait} second(s))"
					, actualContainingExpected: fullLog);

				try
				{
					Task.WaitAll(task, blocker2Task);
				}
				catch
				{
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGetOnlineUpgradeRetryCount_NoExplicit_Default()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				var defaultValue = upgrader.OnlineUpgradeRetryCount;

				// ensure there is no explicit value
				ExtProperty.Database.Delete(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount));

				AssertEquals("OnlineUpgradeRetryCount", defaultValue, upgrader.GetOnlineUpgradeRetryCount());
			}
		}

		[UseSnapshotProtection]
		public void TestGetOnlineUpgradeRetryCount_WrongExplicit_Default()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				var defaultValue = upgrader.OnlineUpgradeRetryCount;
				var explicitValue = "wrong value";

				// setup explicit value
				ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), explicitValue);

				AssertEquals("OnlineUpgradeRetryCount", defaultValue, upgrader.GetOnlineUpgradeRetryCount());
			}
		}

		[UseSnapshotProtection]
		public void TestGetOnlineUpgradeRetryCount_Explicit()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);
				upgrader.SetConnection_ForTest(Db.AdminConnection);

				var defaultValue = upgrader.OnlineUpgradeRetryCount;
				var explicitValue = defaultValue + 1;

				// setup explicit value
				ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), explicitValue.ToString());

				AssertEquals("OnlineUpgradeRetryCount", explicitValue, upgrader.GetOnlineUpgradeRetryCount());
			}
		}

		[UseSnapshotProtection]
		public void TestRetryOnlineUpgradeOnDeadlock_Success()
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			var runCount = 0;

			// setup explicit value
			var retryCount = 2;
			ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), retryCount.ToString());

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				runCount++;

				if (--retryCount >= 0)
				{
					throw new Exception("Wrapped exception", SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(1205, 45, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0))));
				}
				else
				{
					throw new OperationCanceledException("Upgrade aborted by user");
				}
			};

			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());

			AssertEquals("Run count", 3, runCount);

			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertContains("Upgrade process should be re-started"
				, expected: "On-line upgrade has been re-started due to deadlock"
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestRetryOnlineUpgradeOnDeadlock_Fail()
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			var runCount = 0;

			// setup explicit value
			var retryCount = 2;
			ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), retryCount.ToString());

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				runCount++;

				throw new Exception("Wrapped exception", SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1205, 45, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0))));
			};

			var ex = AssertExceptionThrown<Exception>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
			AssertEquals("Deadlock exception should be thrown", true, ex.IsInnermostDeadlock());

			AssertEquals("Run count", 3, runCount);

			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertContains("Upgrade process should be re-started"
				, expected: "On-line upgrade has been re-started due to deadlock"
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestRetryOnlineUpgradeOnExclusiveLock_Success()
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			var runCount = 0;

			// setup explicit value
			var retryCount = 2;
			ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), retryCount.ToString());

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				runCount++;

				if (--retryCount >= 0)
				{
					throw new Exception("Wrapped exception", SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(5030, 45, 13, Db.ServerName, $"The database could not be exclusively locked to perform the operation.", string.Empty, 0))));
				}
				else
				{
					throw new OperationCanceledException("Upgrade aborted by user");
				}
			};

			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());

			AssertEquals("Run count", 3, runCount);

			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertContains("Upgrade process should be re-started"
				, expected: "On-line upgrade has been re-started due to failure to obtain an exclusive lock."
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestRetryOnlineUpgradeOnExclusiveLock_Fail()
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManager(new VersionChangeInfoForTest(), upgradeInfo);

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			var runCount = 0;

			// setup explicit value
			var retryCount = 2;
			ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), retryCount.ToString());

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				runCount++;

				throw new Exception("Wrapped exception", SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(5030, 45, 13, Db.ServerName, $"The database could not be exclusively locked to perform the operation.", string.Empty, 0))));
			};

			var ex = AssertExceptionThrown<Exception>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
			AssertEquals("Exclusive Lock exception should be thrown", true, ex.IsInnermostSpecifiedError(DbErrorType.CouldNotObtainExclusiveLock));

			AssertEquals("Run count", 3, runCount);

			var fullLog = string.Join(System.Environment.NewLine, logger);
			AssertContains("Upgrade process should be re-started"
				, expected: "On-line upgrade has been re-started due to failure to obtain an exclusive lock."
				, actualContainingExpected: fullLog);
		}

		[UseSnapshotProtection]
		public void TestClientDocumentsWhenCurUpgradePackageIsSameVersion()
		{
			var version = ReleaseInfo.Instance.VersionNumber.ToVersion();
			var pk = Guid.NewGuid();

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var versionInfo = new VersionChangeInfoForTest { DbReferenceVersion_Clr = SqlClrAssembliesVersion.Application };
				var upgrader = new UpgradeManagerForTest(versionInfo, null, false);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();
				var actual1 = string.Join("; ", upgrader.OfflineActionList_Exposed.Select(a => a.Name).OrderBy(x => x));

				DataRegistry.Instance.ClientDocumentName = "ABC";
				upgrader = new UpgradeManagerForTest(versionInfo, null, false);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();
				var actual2 = string.Join("; ", upgrader.OfflineActionList_Exposed.Select(a => a.Name).OrderBy(x => x));

				DataRegistry.Instance.ClientDocumentName = "";

				Assert("'CUR' of package should be added", PackageHelper.InsertUpgradePackage(pk, version, "CUR"));

				upgrader = new UpgradeManagerForTest(versionInfo, null, false);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();
				var actual3 = string.Join("; ", upgrader.OfflineActionList_Exposed.Select(a => a.Name).OrderBy(x => x));

				DataRegistry.Instance.ClientDocumentName = "ABC";
				upgrader = new UpgradeManagerForTest(versionInfo, null, false);
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				upgrader.InitialiseRequiredUpgraders_Exposed();
				var actual4 = string.Join("; ", upgrader.OfflineActionList_Exposed.Select(a => a.Name).OrderBy(x => x));

				CombineAssertions(() =>
				{
					AssertEquals("Case No CUR No Client", "Key Cycler; OFX Key creation; PT Key creation", actual1);
					AssertEquals("Case No CUR Has Client", "Key Cycler; OFX Key creation; PT Key creation", actual2);
					AssertEquals("Case Has CUR No Client", "Key Cycler; OFX Key creation; PT Key creation", actual3);
					AssertEquals("Case Has CUR Has Client", "Client Documents and Reports Upgrade; Key Cycler; OFX Key creation; PT Key creation", actual4);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestCDCBecomesEnabledForSelfHosted()
		{
			var isSelfHosted = true;
			var startDbWithCdcEnabled = false;
			var shouldDisableCdc = false;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For self hosted customers where CDC is disabled, it should be auto-enabled."
			);
		}

		[UseSnapshotProtection]
		public void TestCDCRemainsDisabledForSelfHostedIfDisableCdcFlagIsTrue()
		{
			var isSelfHosted = true;
			var startDbWithCdcEnabled = false;
			var shouldDisableCdc = true;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For self hosted customers where CDC is disabled, it should be auto-enabled unless the BiDisableChangeDataCapture registry flag is set to true."
			);
		}

		[UseSnapshotProtection]
		public void TestCDCRemainsEnabledForSelfHosted()
		{
			var isSelfHosted = true;
			var startDbWithCdcEnabled = true;
			var shouldDisableCdc = false;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For self hosted customers with CDC enabled, it should remain enabled."
			);
		}

		[UseSnapshotProtection]
		public void TestCDCBecomesDisabledForSelfHostedIfDisableCdcFlagIsTrue()
		{
			var isSelfHosted = true;
			var startDbWithCdcEnabled = true;
			var shouldDisableCdc = true;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For self hosted customers with CDC enabled, it should remain enabled unless the BiDisableChangeDataCapture registry flag is set to true."
			);
		}

		[UseSnapshotProtection]
		public void TestCDCBecomesEnabledForWiseCloud()
		{
			var isSelfHosted = false;
			var startDbWithCdcEnabled = false;
			var shouldDisableCdc = false;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For Wisecloud hosted customers with CDC disabled, it should auto-enable"
			);
		}

		[UseSnapshotProtection]
		public void TestCDCRemainsDisabledForWiseCloudIfDisableCdcFlagTrue()
		{
			var isSelfHosted = false;
			var startDbWithCdcEnabled = false;
			var shouldDisableCdc = true;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For Wisecloud hosted customers with CDC disabled, it should auto-enable unless the BiDisableChangeDataCapture registry flag is set to true."
			);
		}

		[UseSnapshotProtection]
		public void TestCDCRemainsEnabledForWiseCloud()
		{
			var isSelfHosted = false;
			var startDbWithCdcEnabled = true;
			var shouldDisableCdc = false;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For Wisecloud hosted customers with CDC enabled, it should remain enabled"
			);
		}

		[UseSnapshotProtection]
		public void TestCDCBecomesDisabledForWiseCloudIfDisableCdcFlagIsTrue()
		{
			var isSelfHosted = false;
			var startDbWithCdcEnabled = true;
			var shouldDisableCdc = true;

			TestCdcAutoEnable(isSelfHosted, startDbWithCdcEnabled, shouldDisableCdc,
				combineAssertionsMessage: "For Wisecloud hosted customers with CDC enabled, it should remain enabled unless the BiDisableChangeDataCapture registry flag is set to true."
			);
		}

		[UseSnapshotProtection]
		public void TestEnsureExclusiveLockOfResourcesIfRequired_IsInTransaction_LegacyLockout()
			=> AssertEnsureExclusiveLockOfResourcesIfRequired_IsInTransaction(lockoutInTransaction: false);

		[UseSnapshotProtection]
		public void TestEnsureExclusiveLockOfResourcesIfRequired_IsInTransaction_LockoutInTransaction()
			=> AssertEnsureExclusiveLockOfResourcesIfRequired_IsInTransaction(lockoutInTransaction: true);

		void AssertEnsureExclusiveLockOfResourcesIfRequired_IsInTransaction(bool lockoutInTransaction)
		{
			// Arrange
			var hasLockBeforeUpgrade = false;
			var hasLockDuringUpgrade = false;
			var hasLockAfterUpgrade = false;

			var schemaName = "";
			var tableName = "";
			var tableId = 0;

			var sql = DatabasePadlock.GetObjectsToLockQuery(Db.Connection.CurrentDatabase);
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					schemaName = (string)reader["sch_name"];
					tableName = (string)reader["tab_name"];
					tableId = Convert.ToInt32(reader["tab_id"], CultureInfo.InvariantCulture);
					// cdc.CdcTables can be dropped during the start of the upgrade by Cdc stuff.
					// Avoid using it to verify table locks are taken since it may not exist by then.
					if (!(schemaName == "cdc" && tableName == "CdcTables"))
					{
						break;
					}
				}
			}

			SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true, true);
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
				info.DbReferenceVersion_Schema == SchemaVersion.Application
				&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
				&& info.DbReferenceVersion_Script == ScriptVersion.Application
				&& info.DbReferenceVersion_Transformation == TransformationVersion.ApplicationNumber
				&& info.IsRequired_Schema);
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgrader = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, false, false);

			upgrader.requiresLockout = true;
			upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
			upgrader.OverrideShouldAcquireLockoutInTransaction = lockoutInTransaction;
			upgrader.override_KillUserConnections = true;
			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_BeforeOffline_ForTest = () =>
			{
				hasLockBeforeUpgrade = HasSchemaModificationLock(Db.Connection, tableId);
			};

			upgrader.UserAction_Offline_ForTest = () =>
			{
				upgrader.SetOffLineActionList_ForTest(new UpgradeActionList(), true);
				hasLockDuringUpgrade = HasSchemaModificationLock(Db.Connection, tableId);
				throw new OperationCanceledException("Upgrade aborted by user");
			};

			upgrader.UserAction_AfterOffline_ForTest = () =>
			{
				hasLockAfterUpgrade = HasSchemaModificationLock(Db.Connection, tableId);
			};

			// Act
			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());
			}

			// Assert
			AssertEquals($"Has lock before upgrade? [{schemaName}].[{tableName}] ({tableId})", false, hasLockBeforeUpgrade);
			AssertEquals($"Has lock during upgrade? [{schemaName}].[{tableName}] ({tableId})", true, hasLockDuringUpgrade);
			AssertEquals($"Has lock after upgrade? [{schemaName}].[{tableName}] ({tableId})", false, hasLockAfterUpgrade);

			bool HasSchemaModificationLock(DbConnection connection, int objectId)
			{
				return connection.Exists(@"
FROM
	sys.dm_tran_locks
WHERE 1=1
	AND resource_database_id = DB_ID()
	AND resource_type = N'OBJECT'
	AND resource_associated_entity_id = @tableId
	AND request_mode = N'Sch-M'
	AND request_status = N'GRANT'
	AND request_owner_type = N'TRANSACTION'
"
					, cmd =>
					{
						cmd.AddParameter("@tableId", SqlDbType.BigInt, objectId);
					});
			}
		}

		[UseSnapshotProtection]
		public void TestUpgradeProcessInfo()
		{
			// make sure upgrade log contains the following information:
			// 1. Application version being applied (Applying version [22.5.25.3])
			//	2. Main connection info:
			//		2.1.SPID (Upgrade process SPID: [123])
			//		2.2.Current Host name (CLIENT COMPUTER: [YNLTS57])
			//		2.3.Current Host process id (CLIENT PROCESS: [57504])
			//		2.4.Current Application name (CLIENT PROGRAM: [CargoWiseOne])

			// Arrange
			var currentSpid = 0;
			var hostName = "";
			var hostProcessId = 0;
			var programName = "";

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
				info.DbReferenceVersion_Schema == SchemaVersion.Application
				&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
				&& info.DbReferenceVersion_Script == ScriptVersion.Application
				&& info.DbReferenceVersion_Transformation == TransformationVersion.ApplicationNumber
				&& info.IsRequired_Schema);
			var upgrader = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, false, false);
			upgrader.requiresLockout = true;

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				Db.Connection.ExecuteReader(@"
SELECT
	session_id,
	host_name       = ISNULL(host_name, N''),
	host_process_id = ISNULL(host_process_id, 0),
	program_name    = ISNULL(program_name, N'')
FROM
	sys.dm_exec_sessions
WHERE 1=1
	AND session_id = @@SPID

"
					, (record) =>
					{
						currentSpid = (short)record["session_id"];
						hostName = (string)record["host_name"];
						hostProcessId = (int)record["host_process_id"];
						programName = (string)record["program_name"];
					});

				throw new OperationCanceledException("Upgrade aborted by user");
			};

			// Act
			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());

			// Assert
			var fullLog = string.Join(System.Environment.NewLine, logger);

			CombineAssertions(() =>
			{
				AssertContains("Application version"
					, expected: $"Applying version [{ReleaseInfo.Instance.VersionNumber.ToVersion()}]"
					, actualContainingExpected: fullLog);

				AssertContains("Main connection SPID"
					, expected: $"Upgrade process SPID: [{currentSpid}]"
					, actualContainingExpected: fullLog);

				AssertContains("Current Host name"
					, expected: $"Client computer: [{hostName}]"
					, actualContainingExpected: fullLog);

				AssertContains("Current Host process id"
					, expected: $"Client process: [{hostProcessId}]"
					, actualContainingExpected: fullLog);

				AssertContains("Current Application name"
					, expected: $"Client program: [{programName}]"
					, actualContainingExpected: fullLog);
			});
		}

		[UseSnapshotProtection]
		public void TestUpgradeProcessInfoUpdatedAfterRestart()
		{
			// make sure upgrade process info (Main connection SPID) has been updated in the log after re-starting on-line upgrade

			// Arrange
			var spidBeforeRestart = 0;
			var spidAfterRestart = 0;

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
				info.DbReferenceVersion_Schema == SchemaVersion.Application
				&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
				&& info.DbReferenceVersion_Script == ScriptVersion.Application
				&& info.DbReferenceVersion_Transformation == TransformationVersion.ApplicationNumber
				&& info.IsRequired_Schema);
			var upgrader = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, false, false);
			upgrader.requiresLockout = true;

			var logger = new List<string>();
			upgrader.UpgradeEvent += (_, message) => logger.Add(message);

			var retryCount = 1;
			ExtProperty.Database.Update(Db.Connection, nameof(upgrader.OnlineUpgradeRetryCount), retryCount.ToString());

			upgrader.UserAction_BeforeOnline_ForTest = () =>
			{
				upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
			};

			upgrader.UserAction_Online_ForTest = () =>
			{
				if (--retryCount >= 0)
				{
					spidBeforeRestart = Db.Connection.ExecuteScalar<short>("SELECT @@SPID");

					throw new Exception("Wrapped exception", SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(1205, 45, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0))));
				}
				else
				{
					spidAfterRestart = Db.Connection.ExecuteScalar<short>("SELECT @@SPID");

					throw new OperationCanceledException("Upgrade aborted by user");
				}
			};

			// Act
			AssertExceptionThrown<OperationCanceledException>(() => upgrader.RunUpgradeOnAdminConnectionWithNoTimeout());

			// Assert
			var fullLog = string.Join(System.Environment.NewLine, logger);

			CombineAssertions(() =>
			{
				AssertContains("Main connection SPID before restart"
					, expected: $"Upgrade process SPID: [{spidBeforeRestart}]"
					, actualContainingExpected: fullLog);

				AssertContains("Upgrade process should be re-started"
					, expected: "On-line upgrade has been re-started due to deadlock"
					, actualContainingExpected: fullLog);

				AssertContains("Main connection SPID after restart"
					, expected: $"Upgrade process SPID after restart: [{spidAfterRestart}]"
					, actualContainingExpected: fullLog);
			});
		}

		int GetCurrentCompatibility(string dbName)
		{
			var sqlText = "SELECT compatibility_level FROM sys.databases where name = '" + dbName + "'";
			return (byte)Db.Connection.ExecuteScalar(sqlText);
		}

		static int GetIsSuspendedDuringDBUpgradeStmUsageActionCount(BusinessObjectFactory factory, string postFix)
		{
			return StatisticsTestHelper.GetUsageActions(factory, "action_TestStatisticsCollectionIsSuspendedDuringDBUpgradeAndResumeAfterwards" + postFix).Count();
		}

		void AssertUpgradeActionRequired(UpgradeActionList requiredUpgList, string upgraderActionName, bool expected)
		{
			AssertEquals("[" + upgraderActionName + "] included in the list of required upgrade actions?", expected, requiredUpgList.Any(a => a.Name == upgraderActionName));
		}

		void AssertKillingUserConnectionsHasExceededTimeout_LoggedAndNotReported(bool fromTask)
		{
			// Arrange
			Exception exceptionToTest = null;
			try
			{
				if (fromTask)
				{
					Task.Run(() =>
					{
						DbConnectionKiller.ThrowTimeoutException_Exposed("");
					}).Wait();
				}
				else
				{
					DbConnectionKiller.ThrowTimeoutException_Exposed("");
				}
			}
			catch (Exception e)
			{
				exceptionToTest = e;
			}

			var upgradeLog = new List<(UpgradeEventType Type, string Message)>();
			var upgrader = GetUpgradeManager(upgradeLog);
			upgrader
				.Protected()
				.Setup<SqlApplicationLock>("TryGetUpgradeLock")
				.Throws(exceptionToTest);

			// Act
			upgrader.Object.Run();

			// Assert
			Assert(upgradeLog.Any(x => x.Type == UpgradeEventType.TaskFailed && x.Message.Contains(exceptionToTest.Message)));
			AssertEquals("We should not ReportOnce Timeout exception from DbConnectionKiller", null, ErrorReporter.LastExceptionReported);
		}

		Mock<UpgradeManager> GetUpgradeManager(List<(UpgradeEventType, string)> upgradeLog)
		{
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			var upgrader = new Mock<UpgradeManager>(new VersionChangeInfoForTest(), upgradeInfo) { CallBase = true };
			upgrader
				.Object
				.UpgradeEvent += (e, m) => upgradeLog.Add((e, m));
			return upgrader;
		}

		[UseSnapshotProtection]
		public void TestCatchInvalidOperationExceptionOnUpgradeManagerRun()
		{
			var logger = new List<string>();
			var connection = Db.Connection;
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			var upgraderManagerMock = new Mock<UpgradeManager>(new VersionChangeInfoForTest(), upgradeInfo) { CallBase = true };
			var mainDbAssembliesUpgraderMock = new Mock<MainDbAssembliesUpgrader>(new DummyUpgradeManager(), connection, new VersionLabel(0, 0)) { CallBase = true };
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");

			mainDbAssembliesUpgraderMock.Protected()
				.Setup<VersionLabel>("LatestVersion")
				.Returns(() => new VersionLabel(0, 0));
			upgraderManagerMock.Protected()
				.Setup<BaseUpgrader>("GetAssembliesUpgrader")
				.Returns(() =>
				{
					connection.CloseConnection();
					return mainDbAssembliesUpgraderMock.Object;
				});

			var upgradeManager = upgraderManagerMock.Object;
			upgradeManager.UpgradeEvent += (_, message) => logger.Add(message);

			using (new DisposableAction(ErrorReporter.Clear))
			{
				upgradeManager.Run();

				CombineAssertions(() =>
				{
					Assert("Should not send issue report", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
					Assert("Should not send issue report", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
					AssertContains("Connection to the database was lost while running the upgrade, please try the upgrade again later.", string.Join(System.Environment.NewLine, logger));
				});
			}
		}

		internal static IDisposable TemporarySetInitialLockTimeout(DbConnection mainConnection, int value)
		{
			var isChangesRequired = false;
			var initialValue = DbRegistry.LockTimeout.LoadValue(mainConnection);
			if (initialValue != value)
			{
				isChangesRequired = true;
				DbRegistry.LockTimeout.SaveValue(value, mainConnection);
				mainConnection.CloseConnection();
				mainConnection.EnsureIsOpen();
			}

			return new DisposableAction(() =>
			{
				if (isChangesRequired)
				{
					DbRegistry.LockTimeout.SaveValue(initialValue, mainConnection);
					mainConnection.CloseConnection();
					mainConnection.EnsureIsOpen();
				}
			});
		}

		internal static int GetLockTimeout(DbConnection connection)
		{
			return connection.ExecuteScalar<int>("SELECT @@LOCK_TIMEOUT");
		}

		internal static SqlException CreateSqlException(IEnumerable<(int errorNumber, string errorMessage)> listOfErrors)
		{
			var sqlErrors = new List<SqlError>();
			foreach (var (errorNumber, errorMessage) in listOfErrors)
			{
				sqlErrors.Add(SqlExceptionBuilder.CreateSqlError(errorNumber, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, errorMessage, "", 0));
			}
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(sqlErrors.ToArray());
			return SqlExceptionBuilder.CreateSqlException(errors);
		}

		void CreateSTMData(AdminConnection adminConnection)
		{
			var createTestDbTablesScript = @"
			CREATE TABLE dbo.StmData
			( 
			   [SD_PK] UNIQUEIDENTIFIER NOT NULL,
			   [SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
			   [SD_Owner] UNIQUEIDENTIFIER NULL,
			   [SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
			   [SD_Type] CHAR(3) NOT NULL DEFAULT '',
			   [SD_IsLogged] BIT NOT NULL DEFAULT 0,
			   [SD_BinaryValue] VARBINARY(MAX) NULL,
			   [SD_GuidValue] UNIQUEIDENTIFIER NULL,
			   [SD_IsCancelled] BIT NOT NULL DEFAULT 0,
			   [SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
			   [SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
			   [SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
			   [SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
			   [SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
			);";

			adminConnection.ExecuteNonQuery(createTestDbTablesScript);
		}

		void TestCdcAutoEnable(bool isSelfHosted, bool startDbWithCdcEnabled, bool shouldDisableCdc, string combineAssertionsMessage)
		{
			var testDbName = "UpgradePreparationTest$Database";
			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);
				var wasRunningOnDAT = TestingState.IsRunningOnDAT;
				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						CreateSTMData(testDbConnection);
						var upgPreparation = new UpgradePreparationForTest(testDbConnection);
						var registration = ObjectFactory.Get<IProductRegistration>();
						TestingState.IsRunningOnDAT = false;
						registration.KeyForTest.IsInternalSystemForTest = false;

						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = shouldDisableCdc;
						registration.KeyForTest.HostedLocationForTest = isSelfHosted ? "NCW" : "SYD";

						if (startDbWithCdcEnabled)
						{
							CdcDatabase.Enable(testDbConnection, testDbName);
						}
						else
						{
							CdcDatabase.Disable(testDbConnection, testDbName);
						}

						//when Audit Server is set
						DbRegistry.BiAuditServer.SaveValue("someAuditServerForUpgrade", testDbConnection);

						// Create an upgrade manager and Remove the audit server if needed
						var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
						var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
							info.DbReferenceVersion_Schema == SchemaVersion.Application
							&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
							&& info.DbReferenceVersion_Script == ScriptVersion.Application
							&& info.DbReferenceVersion_Transformation == TransformationVersion.ApplicationNumber
							&& info.IsRequired_Schema);
						var upgrader = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, false, false);
						upgrader.BiDisableChangeDataCapture_OverrideValueForTest = shouldDisableCdc;
						upgrader.SetConnection_ForTest(testDbConnection);
						upgrader.SetAuditServerIfRequired();

						using (Globals.TemporaryOverrideForIsTest(false))
						{
							CombineAssertions(combineAssertionsMessage, () =>
							{
								if (shouldDisableCdc)
								{
									Assert("CDC should be disabled", !upgPreparation.ShouldEnableCdc());
									AssertNullOrEmpty("Audit Server should not be set", DbRegistry.BiAuditServer.LoadValue(testDbConnection));
								}
								else
								{
									Assert("CDC should be enabled", upgPreparation.ShouldEnableCdc());
									AssertNotNullOrEmpty("Audit Server should be set", DbRegistry.BiAuditServer.LoadValue(testDbConnection));
								}
							});
						}
					}
				}
				finally
				{
					TestingState.IsRunningOnDAT = wasRunningOnDAT;
					using (var newConnection = Db.NewAdminConnection())
					{
						AdoTestUtils.DropDbIfExists(newConnection, testDbName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestShouldAcquireLockoutInTransaction()
		{
			CombineAssertions(() =>
			{
				AssertShouldAcquireLockoutInTransaction(false, oldSchemaVersion: UpgradeManager.Prod_LastMajorSchemaVersionThatDisabledLogins);
				AssertShouldAcquireLockoutInTransaction(true, oldSchemaVersion: UpgradeManager.Prod_LastMajorSchemaVersionThatDisabledLogins + 1);
			});

			void AssertShouldAcquireLockoutInTransaction(bool expectedResult, int oldSchemaVersion)
			{
				var versionChangeInfoMock = new Mock<IVersionChangeInfo>();
				versionChangeInfoMock.Setup(x => x.DbReferenceVersion_Schema)
					.Returns(new VersionLabel(oldSchemaVersion, 0));

				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManager(versionChangeInfoMock.Object, upgradeInfo);

				AssertEquals("old schema: " + oldSchemaVersion, expectedResult, upgrader.ShouldAcquireLockoutInTransaction());
			}
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_WithTransactionLockout_LockoutIsReset()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			using (var otherConnection = Db.NewAdminConnection())
			{
				SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true);
				otherConnection.IsUpgradeCheckDisabled = true;
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgrader = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				upgrader.OverrideShouldAcquireLockoutInTransaction = true;
				upgrader.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgrader.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgrader.override_KillUserConnections = true;
				upgrader.SetConnection_ForTest(Db.AdminConnection);
				var logger = new List<string>();
				upgrader.UpgradeEvent += (_, message) => logger.Add(message);
				upgrader.UserAction_BeforeOnline_ForTest = () =>
				{
					upgrader.SetOnlineActionList_ForTest(new UpgradeActionList());
				};
				upgrader.UserAction_Offline_ForTest = () =>
				{
					AssertEquals("Lockout was taken", true, DbLockout.HasLockout(otherConnection));
					var newOfflineActionList = new UpgradeActionList();
					upgrader.SetOffLineActionList_ForTest(newOfflineActionList, true);
				};

				upgrader.requiresLockout = true;

				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					upgrader.RunOfflineUpgrade();
				}
				AssertEquals("Lockout was reset", false, DbLockout.HasLockout(otherConnection));
				var fullLog = string.Join(System.Environment.NewLine, logger);
				AssertContains("Log shows lockout is inside transaction"
					, expected: "Beginning transaction\r\nLocking application access to the database"
					, actualContainingExpected: fullLog);
			}
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgradeShouldNotFailsIfStorageDatabaseDoesNotExist()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			using (Db.DisableSchemaVersionCheck())
			{
				// Arrange
				var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), null, false);
				AssertNotNull(upgradeManager);
				upgradeManager.requiresLockout = true;
				upgradeManager.override_KillUserConnections = true;
				upgradeManager.OverrideShouldAcquireLockoutInTransaction = false;
				upgradeManager.override_DoOfflinePreparationBeforeMainTransaction = false;
				upgradeManager.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgradeManager.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = true;
				upgradeManager.UserAction_Offline_ForTest = () =>
				{
					upgradeManager.SetOffLineActionList_ForTest(new UpgradeActionList(), true);
				};
				upgradeManager.SetConnection_ForTest(Db.AdminConnection);

				// Create new Storage DB, cache all database names, drop the new Storage DB
				using (AdoTestUtils.CreateDbDropExistingDisposable(Db.AdminConnection, Db.DatabaseName + "_SD050"))
				{
					upgradeManager.GetAllDatabases();
				}

				using (Globals.SetIsUserInteractiveForTest(false))
				{
					// Act and Assert
					AssertNoExceptionThrown(() => upgradeManager.RunOfflineUpgrade());
				}
			}
		}

		[UseSnapshotProtection]
		public void TestLegacyLockout_BeginTransactionAndLockResources_WhenLockFails_Throws()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgradeManager.SetConnection_ForTest(connection);
				upgradeManager.override_KillUserConnections = true; // save time
				upgradeManager.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = false;

				AssertExceptionThrown<UpgradeBlockedException>(() => upgradeManager.LegacyLockout_BeginTransactionAndLockResources());
			}
		}

		[UseSnapshotProtection]
		public void TestBeginTransactionsAndSetup_WhenLockFails_Throws()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgradeManager.SetConnection_ForTest(connection);
				upgradeManager.override_KillUserConnections = true; // save time
				upgradeManager.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgradeManager.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = false;

				AssertExceptionThrown<UpgradeBlockedException>(() => upgradeManager.BeginTransactionsAndSetup());
			}
		}

		[UseSnapshotProtection]
		public void TestGetDbExtPtyLockInfo_WithNoLock()
		{
			using (var connectionWritingProperty = Db.NewAdminConnection())
			using (var connectionReadingProperty = Db.NewAdminConnection())
			using (connectionReadingProperty.TemporarySetLockTimeout(TimeSpan.Zero))
			{
				connectionWritingProperty.BeginTransaction();
				UpgradeManager.WriteDbExtPtyLockInfo(connectionWritingProperty);

				var actual = UpgradeManager.GetDbExtPtyLockInfo(connectionReadingProperty, "dummy message");
				AssertContains("Current lock holder", actual);
			}
		}

		[UseSnapshotProtection]
		public void TestOfflineUpgradeWillBeFailedWhenTheUpgradeInfoIsNonexistentInDb()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgradeManager.UserAction_BeforeOnline_ForTest = () =>
				{
					upgradeManager.SetOnlineActionList_ForTest(new UpgradeActionList());
				};
				upgradeManager.requiresLockout = false;
				upgradeManager.override_KillUserConnections = false;
				upgradeManager.OverrideShouldAcquireLockoutInTransaction = true;
				upgradeManager.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgradeManager.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgradeManager.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = true;
				upgradeManager.UserAction_Offline_ForTest = () =>
				{
					upgradeManager.SetOffLineActionList_ForTest(new UpgradeActionList(), false);
				};
				upgradeManager.SetConnection_ForTest(Db.AdminConnection);

				AssertExceptionThrown(typeof(InvalidPackageException), $"Failed to set the status of the upgrade '{upgradeInfo.PK}' to 'CUR' because it does not exist.", () => upgradeManager.RunOfflineUpgrade());
			}
		}

		[UseSnapshotProtection]
		public void TestCheckExistenceOfUpgradeInfoAtBeginningOfDbUpgrade()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				var upgradeManager = new UpgradeManagerForTest(new VersionChangeInfoForTest(), upgradeInfo, forceBiDatabaseUpgrade: false);
				upgradeManager.UserAction_BeforeOnline_ForTest = () =>
				{
					upgradeManager.SetOnlineActionList_ForTest(new UpgradeActionList());
				};
				upgradeManager.SetConnection_ForTest(Db.AdminConnection);

				AssertNoExceptionThrown(() => upgradeManager.Run());

				AssertEquals(typeof(InvalidPackageException), ErrorReporter.LastExceptionReported.InnerException.GetType());
				AssertEquals($"Failed to apply the upgrade '{upgradeInfo.PK}' because it does not exist.", ErrorReporter.LastExceptionReported.InnerException.Message);
				ErrorReporter.Clear();
			}
		}

		[UseSnapshotProtection]
		public void TestRunOfflineUpgrade_RefreshDependentScripts()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				// Arrange
				var connection = Db.AdminConnection;

				var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
				PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
				var versionChangeInfoMock = Mock.Of<IVersionChangeInfo>(info =>
						info.DbReferenceVersion_Schema == SchemaVersion.Application
						&& info.DbReferenceVersion_Clr == SqlClrAssembliesVersion.Application
						&& info.DbReferenceVersion_Script == new VersionLabel(5767, 0)
						&& info.DbReferenceVersion_Transformation == new VersionLabel(0, 0)
						&& info.IsRequired_Schema);

				SetupDBUpgraderUserInteractionMocks(out var mockServiceProvider, true);
				var upgradeManager = new UpgradeManagerForTest(versionChangeInfoMock, upgradeInfo, forceBiDatabaseUpgrade: false);
				upgradeManager.requiresLockout = true;
				upgradeManager.override_KillUserConnections = true;
				upgradeManager.OverrideShouldAcquireLockoutInTransaction = true;
				upgradeManager.override_DoOfflinePreparationBeforeMainTransaction = true;
				upgradeManager.override_DoNonTransactionalStepsAfterFirstMainDbLockout = true;
				upgradeManager.EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest = true;
				upgradeManager.SetConnection_ForTest(connection);
				upgradeManager.DisableRefreshDependentScripts_ForTest = false;
				upgradeManager.UserChooseToKillOtherDbConnections_ForTest = true;

				AssertNull("PRE: RefreshDependentScriptsAction", upgradeManager.RefreshDependentScriptsAction);

				upgradeManager.InitialiseRequiredUpgraders_Exposed();

				AssertNotNull("RefreshDependentScriptsAction is set by InitialiseRequiredUpgraders", upgradeManager.RefreshDependentScriptsAction);

				var callCountForRefreshDependentScriptsAction = 0;
				var refreshDependentScriptsWasCalledInTransaction = false;
				upgradeManager.RefreshDependentScriptsAction = new Action(() =>
				{
					if (connection.IsInTransaction)
					{
						refreshDependentScriptsWasCalledInTransaction = true;
					}
					++callCountForRefreshDependentScriptsAction;
				});

				upgradeManager.SetOffLineActionList_ForTest(new UpgradeActionList(), true);

				// Act
				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					upgradeManager.RunOfflineUpgrade();
				}

				// Assert
				AssertEquals("callCountForRefreshDependentScriptsAction", 1, callCountForRefreshDependentScriptsAction);
				AssertEquals("refreshDependentScriptsWasCalledInTransaction", false, refreshDependentScriptsWasCalledInTransaction);
			}
		}

		public static void SetupDBUpgraderUserInteractionMocks(out Mock<IServiceProvider> mockServiceProvider, bool confirmDisconnectUsers, bool userConfirmation = false, bool retryAction = false)
		{
			var mockUserInteraction = new Mock<IDbUpgraderUserInteraction>();
			mockUserInteraction.Setup(x => x.PromptConfirmDisconnectUsers(It.IsAny<KeyValuePair<string, string>[]>())).Returns(confirmDisconnectUsers);
			mockUserInteraction.Setup(x => x.PromptUserConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).Returns(userConfirmation);
			mockUserInteraction.Setup(x => x.PromptRetryAction(It.IsAny<string>(), It.IsAny<string>())).Returns(retryAction);

			var originalServiceProvider = GlobalServiceProvider.Instance;
			mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
			mockServiceProvider.Setup(x => x.GetService(typeof(IDbUpgraderUserInteraction))).Returns(mockUserInteraction.Object);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BiServers.ClearBiServersCache();
		}

		protected override void TearDown()
		{
			BiServers.ClearBiServersCache();
			base.TearDown();
		}
	}
}
