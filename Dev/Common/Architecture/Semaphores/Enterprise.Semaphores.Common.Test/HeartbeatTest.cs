using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class HeartbeatMockTest : TransactionedTestCase
	{
		public void TestHeartBeat()
		{
			Guid heartbeatUniqueId;
			IHeartbeatInfoFactory heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();

			using (HeartbeatWithNoTimerForTesting testHeartbeat = new HeartbeatWithNoTimerForTesting(heartbeatFactory, TimeSpan.FromSeconds(2)))
			{
				heartbeatUniqueId = testHeartbeat.HeartbeatUniqueId_Exposed;
				Assert("Heardbeat isn't registred", !HeartbeatForTesting.IsHeartbeatInDatabase(TestConnection, heartbeatUniqueId));
				Assert("Timer wasn't started", !testHeartbeat.IsTimerAlive);

				testHeartbeat.Register_Exposed();
				Assert("Heardbeat was registred", HeartbeatForTesting.IsHeartbeatInDatabase(TestConnection, heartbeatUniqueId));
				Assert("Timer was started", testHeartbeat.IsTimerAlive);
			}

			Assert("Heartbeat should be stopped", !HeartbeatForTesting.IsHeartbeatInDatabase(TestConnection, heartbeatUniqueId));
		}

		[ExpectNoExceptions]
		public void TestHeartbeatDisposeAndKeepAliveDoesNotCauseExceptions()
		{
			IHeartbeatInfoFactory heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var logoffHandler = Mock.Of<IHeartBeatRemoteLogoff>();
			var heartbeat1 = new HeartbeatForTesting(heartbeatFactory, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), logoffHandler);
			heartbeat1.Dispose();
			heartbeat1.Tick();
		}

		public void TestHeartbeatRefreshDatabaseUpgradeExceptionsAreHandled()
		{
			bool exceptionHandled = false;
			HeartBeatForDBVersionTest testHeartbeat = null;
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockGuidPlugin.CallBase = true;
			mockDbEnv.CallBase = true;
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin).Returns(mockGuidPlugin.Object);
			mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
				.Callback(() => exceptionHandled = true);
			DbEnv.SetDbEnvironment(mockDbEnv.Object);

			using (testHeartbeat = new HeartBeatForDBVersionTest(heartbeatFactory, TimeSpan.FromSeconds(3)))
			{
				testHeartbeat.KeepHeartbeatAlive_Exposed(true);
			}

			Assert(exceptionHandled);
		}

		public void TestHeartbeatRefreshNotCalledWhenDatabaseUpgradedExceptionHasBeenThrown()
		{
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var logoffManagerMock = new Mock<IHeartBeatRemoteLogoff>();
			var semaphoreDbManagerMock = new Mock<ISemaphoreDbManager>();

			using (Db.Connection.SetDatabaseUpgradedExceptionHasBeenThrown_ForTest())
			using (var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, TimeSpan.FromSeconds(3), TimeSpan.FromHours(1), logoffManagerMock.Object, semaphoreDbManagerMock.Object))
			{
				testHeartbeat.KeepHeartbeatAlive_Exposed(true);
			}

			AssertNoExceptionThrown(() =>
			{
				semaphoreDbManagerMock.Verify(x => x
					.RefreshHeartbeatInDatabase(It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<bool>()), Times.Never);
			});
		}

		public void TestUsingDbConnectionInHeartbeat()
		{
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();

			using (var testHeartbeat = new HeartbeatUsingDbConnection(heartbeatFactory, TimeSpan.FromSeconds(3)))
			{
				testHeartbeat.Register_Exposed();
				Thread.Sleep(2000);
			}

			AssertEquals("ExpiresAtUtc was reset?", DateTime.MinValue, HeartbeatUsingDbConnection.ExpiresAtUtc);
		}

		public void TestHearbeatAndUpgradeDurations()
		{
			var logoffHandler = new HearbeatRemoteLogoffForTesting();
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var longDuration = TimeSpan.FromMinutes(2);
			var shortDuration = TimeSpan.FromSeconds(1);
			var dbManager = new SemaphoreDbManagerWithDummyHeartbeat();
			var upgradeChecker = new UpgradeCheckerWithDummyUpgrade();

			//Short heartbeat, long upgrade check
			using (var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, shortDuration, longDuration, logoffHandler, dbManager, upgradeChecker))
			{
				testHeartbeat.Register_Exposed();
				Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
			}

			AssertGreaterThanOrEqualTo("Heartbeat should have occured", dbManager.ExecuteHeartbeatCount, 2);
			dbManager.ExecuteHeartbeatCount = 0;
			AssertEquals("Upgrade check shouldn't have occured, period has not expired", 0, logoffHandler.OnRemoteUpgradeLogoffCalls);

			//Long heartbeat, short upgrade check
			using (var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, longDuration, shortDuration, logoffHandler, dbManager, upgradeChecker))
			{
				testHeartbeat.Register_Exposed();
				Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
			}

			AssertGreaterThanOrEqualTo("Upgrade check should have occured", logoffHandler.OnRemoteUpgradeLogoffCalls, 2);
			logoffHandler.OnRemoteUpgradeLogoffCalls = 0;
			AssertEquals("Heartbeat shouldn't have occured, period has not expired", 0, dbManager.ExecuteHeartbeatCount);

			//Short heartbeat, Short upgrade check
			using (var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, shortDuration, shortDuration, logoffHandler, dbManager, upgradeChecker))
			{
				testHeartbeat.Register_Exposed();
				Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
			}

			AssertGreaterThanOrEqualTo("Heartbeat should have occured", dbManager.ExecuteHeartbeatCount, 2);
			AssertGreaterThanOrEqualTo("Upgrade check should have occured", logoffHandler.OnRemoteUpgradeLogoffCalls, 2);
		}

		public void TestTemporarilyDisableTimers()
		{
			var logoffHandler = new HearbeatRemoteLogoffForTesting();
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var longDuration = TimeSpan.FromMinutes(2);
			var shortDuration = TimeSpan.FromSeconds(1);
			var dbManager = new SemaphoreDbManagerWithDummyHeartbeat();
			var upgradeChecker = new UpgradeCheckerWithDummyUpgrade();

			using (var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, shortDuration, longDuration, logoffHandler, dbManager, upgradeChecker))
			{
				testHeartbeat.Register_Exposed();

				Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
				AssertGreaterThanOrEqualTo("Heartbeat should have occured", dbManager.ExecuteHeartbeatCount, 2);
				AssertEquals("Upgrade check shouldn't have occured, period has not expired", 0, logoffHandler.OnRemoteUpgradeLogoffCalls);

				var firstPassCount = dbManager.ExecuteHeartbeatCount;
				using (testHeartbeat.TemporarilyDisableTimers())
				{
					Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
					AssertEquals("Heartbeat should not have occured as it is disabled", dbManager.ExecuteHeartbeatCount, firstPassCount);
					AssertEquals("Upgrade check shouldn't have occured, period has not expired", 0, logoffHandler.OnRemoteUpgradeLogoffCalls);
				}

				Thread.Yield();
				AssertGreaterThanOrEqualTo("Heartbeat should have occured immediatly after restarting timers", dbManager.ExecuteHeartbeatCount, firstPassCount + 1);

				Thread.Sleep((int)(shortDuration.TotalMilliseconds * 2.5));
				AssertGreaterThanOrEqualTo("Heartbeat should have occured", dbManager.ExecuteHeartbeatCount, firstPassCount + 3);
				AssertEquals("Upgrade check shouldn't have occured, period has not expired", 0, logoffHandler.OnRemoteUpgradeLogoffCalls);
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseUpgradeInProgressExceptionHandled()
		{
			var heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();
			var logoffHandler = Mock.Of<IHeartBeatRemoteLogoff>();
			var adminConnection = Db.NewAdminConnection();
			var semaphoreDbManagerThatAlwaysRefreshes = new SemaphoreDbManagerThatAlwaysRefreshes();
			var upgradeChecker = new UpgradeChecker();
			AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
			DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

			var exceptionHandled = false;
			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockGuidPlugin.CallBase = true;
			mockDbEnv.CallBase = true;
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin).Returns(mockGuidPlugin.Object);
			mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
				.Callback(() => exceptionHandled = true);
			DbEnv.SetDbEnvironment(mockDbEnv.Object);

			var testHeartbeat = new HeartbeatForTesting(heartbeatFactory, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), logoffHandler, semaphoreDbManagerThatAlwaysRefreshes, upgradeChecker);
			try
			{
				testHeartbeat.Tick();
			}
			finally
			{
				adminConnection.ResetLockout();
			}

			Assert(exceptionHandled);
		}
	}

	#region Classes For Testing

	class HearbeatRemoteLogoffForTesting : IHeartBeatRemoteLogoff
	{
		public void OnRemoteUpgradeLogoff(DateTime utc, Func<bool> updateExists)
		{
			++OnRemoteUpgradeLogoffCalls;
		}

		public void OnRemoteLogoff()
		{
			++OnRemoteLogoffCalls;
		}

		public int OnRemoteLogoffCalls { get; set; }
		public int OnRemoteUpgradeLogoffCalls { get; set; }
	}

	class HeartbeatForTesting : Heartbeat
	{
		public HeartbeatForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, TimeSpan upgradeCheckDuration, IHeartBeatRemoteLogoff logoffHandler, ISemaphoreDbManager dbManager = null)
			: this(sessionInfoFactory, heartbeatDuration, upgradeCheckDuration, logoffHandler, dbManager ?? new SemaphoreDbManager(), new UpgradeChecker())
		{
		}

		public HeartbeatForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, TimeSpan upgradeCheckDuration, IHeartBeatRemoteLogoff logoffHandler, ISemaphoreDbManager dbManager, IUpgradeChecker upgradeChecker)
			: base(sessionInfoFactory, heartbeatDuration, upgradeCheckDuration, logoffHandler, dbManager, upgradeChecker)
		{
		}

		public HeartbeatForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, TimeSpan upgradeCheckDuration)
			: this(sessionInfoFactory, heartbeatDuration, upgradeCheckDuration, Mock.Of<IHeartBeatRemoteLogoff>())
		{
		}

		public void Register_Exposed()
		{
			Register();
		}

		public void KeepHeartbeatAlive_Exposed(bool isForegroundThread)
		{
			base.KeepHeartbeatAlive(isForegroundThread);
		}

		public static bool IsHeartbeatInDatabase(DbConnection conn, Guid heartbeatPk)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM dbo.StmServiceHeartBeat WITH(READCOMMITTED)
				WHERE SV_PK = '{0}';",
				heartbeatPk);
			var count = (int)conn.ExecuteScalar(sqlText);

			return (count == 1);
		}

		internal bool IsInDatabase()
		{
			return IsHeartbeatInDatabase(Db.Connection, heartbeatUniqueId);
		}

		public Guid HeartbeatUniqueId_Exposed => heartbeatUniqueId;

		public void Tick()
		{
			KeepHeartbeatAlive(isForegroundThread: false);
		}

		public virtual bool IsTimerAlive => (threadTimer != null);
	}

	class HeartbeatWithNoTimerForTesting : HeartbeatForTesting
	{
		public HeartbeatWithNoTimerForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration)
			: base(sessionInfoFactory, heartbeatDuration, heartbeatDuration)
		{
		}

		protected override void StartKeepAliveTimers()
		{
			timerStartedFlag = true;
		}

		public override bool IsTimerAlive => timerStartedFlag;

		bool timerStartedFlag;
	}

	class HeartbeatWithMockTimerForTesting : HeartbeatForTesting
	{
		public HeartbeatWithMockTimerForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, IHeartBeatRemoteLogoff logoffHandler)
			: base(sessionInfoFactory, heartbeatDuration, heartbeatDuration, logoffHandler)
		{
			expiresAtUtc = DateTime.MinValue;
		}

		public HeartbeatWithMockTimerForTesting(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration)
			: this(sessionInfoFactory, heartbeatDuration, null)
		{
		}

		protected override void StopHeartbeat()
		{
			StopTimers();
			expiresAtUtc = DateTime.MinValue;
		}

		protected override void RegisterCore()
		{
			RefreshHeartbeat();
		}

		void RefreshHeartbeat()
		{
			expiresAtUtc = DateTime.UtcNow; // Do not want to hit the DB in another thread (Test Only)
		}

		public static DateTime ExpiresAtUtc => expiresAtUtc;

		[ThreadStatic]
		static DateTime expiresAtUtc;
	}

	class HeartBeatForDBVersionTest : HeartbeatForTesting
	{
		public HeartBeatForDBVersionTest(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, IHeartBeatRemoteLogoff logoffHandler)
			: base(sessionInfoFactory, heartbeatDuration, heartbeatDuration, logoffHandler, new TestSemaphoreDbManager.TestSemaphoreDbManagerForDBUpgrade(), new UpgradeChecker())
		{
			expiresAtUtc = DateTime.MinValue;
		}

		public HeartBeatForDBVersionTest(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration)
			: this(sessionInfoFactory, heartbeatDuration, Mock.Of<IHeartBeatRemoteLogoff>())
		{
		}

		public DateTime ExpiresAtUtc
		{
			get { return expiresAtUtc; }
		}

		readonly DateTime expiresAtUtc;
	}

	class HeartbeatUsingDbConnection : HeartbeatWithMockTimerForTesting
	{
		public HeartbeatUsingDbConnection(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration)
			: base(sessionInfoFactory, heartbeatDuration, Mock.Of<IHeartBeatRemoteLogoff>())
		{ }

		protected override void KeepHeartbeatAlive(bool isForegroundThead)
		{
			var dbConnection = Db.Connection;
			base.KeepHeartbeatAlive(isForegroundThead);
		}
	}

	class MockHeartbeatInfoFactoryForTesting : IHeartbeatInfoFactory
	{
		#region IHeartbeatInfoFactory Members

		public IHeartbeatInfo New()
		{
			return new HeartbeatInfo(Guid.NewGuid(), MockHostName, MockUserPk, MockFullUserName, MockUserId, MockUserEmail, LogonType.Staff, MockProcessId, MockHeartbeatType, null);
		}

		#endregion

		public static readonly Guid MockUserPk = Guid.NewGuid();

		public const string MockUserId = "#T!";
		public const string MockFullUserName = "TestUser";
		public const string MockUserEmail = "test@test.test";
		public const string MockHeartbeatType = HeartbeatTypes.Enterprise;

		public static readonly string MockHostName = Guid.NewGuid().ToString();
		readonly int MockProcessId = new Random().Next();
	}

	#endregion

	class HeartbeatTest : TestCase
	{
		[UseSnapshotProtection]
		class KeepHeartbeatAliveTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				heartbeatInfoMock = Mock.Of<IHeartbeatInfo>(info =>
					info.HeartbeatId == Guid.Parse("00000000-0000-0000-0000-00000000000A")
					&& info.HostName == $"Host_{nameof(HeartbeatTest)}"
					&& info.UserPk == Guid.Parse("A0000000-0000-0000-0000-000000000000")
					&& info.FullUserName == "TestUser"
					&& info.LogonIdentificationCode == "T!!"
					&& info.EmailAddress == "e@ma.il"
					&& info.LogonType == LogonType.Staff
					&& info.ProcessId == 12345
					&& info.HeartbeatType == HeartbeatTypes.Enterprise
				);
				heartbeatFactoryMock = Mock.Of<IHeartbeatInfoFactory>(factory => factory.New() == heartbeatInfoMock);
				logoffHandlerMock = new Mock<IHeartBeatRemoteLogoff>();
				semaphoreDbManagerMock = new Mock<SemaphoreDbManager> { CallBase = true };
				upgradeCheckerMock = new Mock<UpgradeChecker> { CallBase = true }
					.As<IUpgradeChecker>();
				windowsTimerMock = new Mock<IWindowsTimer>();
			}

			public void TestCallsNothingInCaseOfUpgradeExceptionIsAlreadyThrownInConnection()
			{
				// Arrange
				var timeSpan = TimeSpan.FromSeconds(30);
				using (var heartbeat = new Heartbeat(
					heartbeatFactoryMock,
					timeSpan,
					timeSpan,
					logoffHandlerMock.Object,
					semaphoreDbManagerMock.Object,
					upgradeCheckerMock.Object,
					windowsTimerMock.Object))
				using (SetDatabaseUpgradedExceptionHasBeenThrownInConnection())
				{
					Assert(Db.DatabaseUpgradedExceptionHasBeenThrownInConnection);
					heartbeat.Register();

					// Act
					windowsTimerMock.Raise(timer => timer.Tick += null, EventArgs.Empty);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						upgradeCheckerMock.VerifyNoOtherCalls();
						logoffHandlerMock.VerifyNoOtherCalls();
					});
				}

				IDisposable SetDatabaseUpgradedExceptionHasBeenThrownInConnection()
				{
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;
					return new DisposableAction(() => Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false);
				}
			}

			public void TestExpiryTime()
			{
				// Arrange
				var timeSpan = TimeSpan.FromSeconds(20);
				upgradeCheckerMock
					.Setup(checker => checker.CheckForUpgrade(It.IsAny<bool>(), It.IsAny<TimeSpan>()))
					.Returns(DateTime.MinValue);

				using (var command = Db.Connection.Command($@"
			SELECT
				{StmServiceHeartBeatSchema.Constants.SV_ExpiresAtUtc}
			FROM
				{StmServiceHeartBeatSchema.Constants.SqlSchemaName}.{StmServiceHeartBeatSchema.Constants.TableName}
			WHERE
				{StmServiceHeartBeatSchema.Constants.PK} = @id
			"))
				{
					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object))
					{
						var id = heartbeat.HeartbeatId;
						command.AddParameterBasedOnDbColumn("@id", id, StmServiceHeartBeatSchema.PK);
						AssertEquals("ExpiresAtUtc initialised?", null, command.ExecuteScalar());

						// Act
						heartbeat.Register();

						// Assert
						var initialExpiresAtUtc = WaitForHeartbeatRecord();
						AssertGreaterThan("ExpiresAtUtc initialised?", initialExpiresAtUtc, DateTime.MinValue);

						Thread.Sleep(timeSpan);

						AssertGreaterThan("Heartbeat life should be extended", WaitForHeartbeatRecord(), initialExpiresAtUtc);
					}

					AssertEquals("ExpiresAtUtc was reset?", null, command.ExecuteScalar());

					DateTime WaitForHeartbeatRecord()
					{
						var stopwatch = Stopwatch.StartNew();
						while (stopwatch.Elapsed <= timeSpan)
						{
							var result = command.ExecuteScalar();
							if (result != null)
							{
								return Convert.ToDateTime(result.ToString());
							}
						}

						return DateTime.MinValue;
					}
				}
			}

			public void TestHandlesDatabaseUpgradeExceptionFromHeartbeatRefresh()
			{
				// Arrange
				using (var waitForHeartbeatUpdate = new ManualResetEvent(false))
				{
					var timeSpan = TimeSpan.FromSeconds(20);
					semaphoreDbManagerMock
						.Setup(manager => manager.RefreshHeartbeatInDatabase(It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<bool>()))
						.Callback(() => waitForHeartbeatUpdate.Set())
						.Throws(new Mock<DatabaseUpgradeException>(string.Empty).Object);

					var dbEnvironmentMock = CreateAndConfigureDbEnvironmentMock();
					var dbConnectionGuiPluginMock = new Mock<IDbConnectionGuiPlugin>();
					dbEnvironmentMock
						.SetupGet(environment => environment.ConnectionGuiPlugin)
						.Returns(dbConnectionGuiPluginMock.Object);

					using (DbEnv.SetTemporaryDbEnvironment(dbEnvironmentMock.Object))
					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object))
					{
						// Act
						heartbeat.Register();

						// Assert
						Assert(waitForHeartbeatUpdate.WaitOne(timeSpan));
					}

					CombineAssertions(() =>
					{
						AssertNoExceptionThrown(() => logoffHandlerMock.VerifyNoOtherCalls());
						AssertNoExceptionThrown(() =>
						{
							dbConnectionGuiPluginMock.Verify(plugin => plugin.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once);
							dbConnectionGuiPluginMock.VerifyNoOtherCalls();
						});
						AssertEquals(0, ErrorReporter.TotalErrorCount);
					});
				}
			}

			public void TestHandlesDatabaseUpgradeExceptionFromUpgradeChecker()
			{
				// Arrange
				using (var waitForUpgradeCheck = new ManualResetEvent(false))
				{
					var timeSpan = TimeSpan.FromSeconds(20);
					upgradeCheckerMock
						.Setup(checker => checker.CheckForUpgrade(It.IsAny<bool>(), It.IsAny<TimeSpan>()))
						.Callback(() => waitForUpgradeCheck.Set())
						.Throws(new Mock<DatabaseUpgradeException>(string.Empty).Object);

					var dbEnvironmentMock = CreateAndConfigureDbEnvironmentMock();
					var dbConnectionGuiPluginMock = new Mock<IDbConnectionGuiPlugin>();
					dbEnvironmentMock
						.SetupGet(environment => environment.ConnectionGuiPlugin)
						.Returns(dbConnectionGuiPluginMock.Object);

					using (DbEnv.SetTemporaryDbEnvironment(dbEnvironmentMock.Object))
					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object))
					{
						// Act
						heartbeat.Register();

						// Assert
						Assert(waitForUpgradeCheck.WaitOne(timeSpan));
					}

					CombineAssertions(() =>
					{
						AssertNoExceptionThrown(() => logoffHandlerMock.VerifyNoOtherCalls());
						AssertNoExceptionThrown(() =>
						{
							dbConnectionGuiPluginMock.Verify(plugin => plugin.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once);
							dbConnectionGuiPluginMock.VerifyNoOtherCalls();
						});
						AssertEquals(0, ErrorReporter.TotalErrorCount);
					});
				}
			}

			public void TestRemoteLogoffIfFailedToUpgrade()
			{
				// Arrange
				using (var waitForHeartbeatUpdate = new ManualResetEvent(false))
				{
					var timeSpan = TimeSpan.FromSeconds(3);
					semaphoreDbManagerMock
						.Setup(manager => manager.RefreshHeartbeatInDatabase(It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<bool>()))
						.Returns(() =>
						{
							waitForHeartbeatUpdate.Set();
							return false;
						});

					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object))
					{
						// Act
						heartbeat.Register();

						// Assert
						Assert(waitForHeartbeatUpdate.WaitOne(timeSpan));
						AssertNoExceptionThrown(() =>
						{
							logoffHandlerMock.Verify(logoff => logoff.OnRemoteLogoff(), Times.Once);
							logoffHandlerMock.VerifyNoOtherCalls();
						});
					}
				}
			}

			public void TestUnhandledExceptionsAreReported()
			{
				// Arrange
				using (var waitForHeartbeatUpdate = new ManualResetEvent(false))
				{
					var timeSpan = TimeSpan.FromSeconds(10);
					var dbEnvironmentMock = CreateAndConfigureDbEnvironmentMock();
					var expectedExceptions = new List<Exception>();
					dbEnvironmentMock
						.Setup(environment => environment.IsTimerDisabledDuringDbUpgrade)
						.Callback(() =>
						{
							if (expectedExceptions.Count > 2)
							{
								waitForHeartbeatUpdate.Set();
							}

							throw CreateExpectedException(expectedExceptions);
						});
					var errorReporterMock = new Mock<IErrorReporter>();
					bool result;

					using (DbEnv.SetTemporaryDbEnvironment(dbEnvironmentMock.Object))
					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object,
						windowsTimerMock.Object))
					{
						// Act
						heartbeat.Register();

						// Assert
						Thread.Sleep(timeSpan);
						result = waitForHeartbeatUpdate.WaitOne(timeSpan);
					}

					CombineAssertions(() =>
					{
						AssertEquals("Lock should be released", true, result);

						AssertNoExceptionThrown(() =>
						{
							foreach (var expectedException in expectedExceptions)
							{
								errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), expectedException), Times.Once);
							}

							errorReporterMock.VerifyNoOtherCalls();
						});
						AssertEquals(expectedExceptions.Count, ErrorReporter.TotalErrorCount);
					});
				}

				Exception CreateExpectedException(IList<Exception> expectedExceptions)
				{
					lock (expectedExceptions)
					{
						var number = expectedExceptions.Count + 1;
						var exceptionMock = Mock.Of<ApplicationException>(exception =>
							exception.Message == $"error{number}"
							&& exception.StackTrace == $"StackTrace{number}");
						Mock.Get(exceptionMock).CallBase = true;

						expectedExceptions.Add(exceptionMock);
						return exceptionMock;
					}
				}
			}

			public void TestUnhandledExceptionsDoNotStopTimersAndReportLogoff()
			{
				// Arrange
				using (var waitForHeartbeatUpdate = new ManualResetEvent(false))
				{
					var timeSpan = TimeSpan.FromSeconds(10);
					var errorReporterMock = new Mock<IErrorReporter>();
					var counter = 0;
					semaphoreDbManagerMock
						.Setup(manager => manager.RefreshHeartbeatInDatabase(It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<bool>()))
						.Returns(true);
					semaphoreDbManagerMock
						.Setup(manager => manager.CheckHeartbeatExpiredTimeIsExpired(It.IsAny<Guid>())).Returns(false);
					upgradeCheckerMock
						.Setup(checker => checker.CheckForUpgrade(It.IsAny<bool>(), It.IsAny<TimeSpan>()))
						.Callback(() =>
						{
							if (Interlocked.Increment(ref counter) > 2)
							{
								waitForHeartbeatUpdate.Set();
							}
						})
						.Throws<ApplicationException>();

					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					using (var heartbeat = new Heartbeat(
						heartbeatFactoryMock,
						timeSpan,
						timeSpan,
						logoffHandlerMock.Object,
						semaphoreDbManagerMock.Object,
						upgradeCheckerMock.Object,
						windowsTimerMock.Object))
					{
						// Act
						heartbeat.Register();

						// Assert
						Thread.Sleep(timeSpan);

						CombineAssertions(() =>
						{
							AssertEquals("Lock should be released", true, waitForHeartbeatUpdate.WaitOne(timeSpan));

							AssertNoExceptionThrown(
								() => upgradeCheckerMock.Verify(
									checker => checker.CheckForUpgrade(It.IsAny<bool>(), It.IsAny<TimeSpan>()),
									Times.AtLeast(3)));

							AssertNoExceptionThrown(() => logoffHandlerMock.VerifyNoOtherCalls());

							var timers = GetTimers(heartbeat);
							AssertNotNull(nameof(timers.threadTimer), timers.threadTimer);
							AssertNotNull(nameof(timers.windowsTimer), timers.windowsTimer);
						});
					}
				}

				(object threadTimer, object windowsTimer) GetTimers(Heartbeat heartbeat)
				{
					var threadTimer = typeof(Heartbeat)
						.GetField("threadTimer", BindingFlags.NonPublic | BindingFlags.Instance)
						.GetValue(heartbeat);
					var windowsTimer = typeof(Heartbeat)
						.GetField("windowsTimer", BindingFlags.NonPublic | BindingFlags.Instance)
						.GetValue(heartbeat);

					return (threadTimer, windowsTimer);
				}
			}

			static Mock<IDbEnvironment> CreateAndConfigureDbEnvironmentMock()
			{
				var dbEnvironmentMock = Mock.Of<IDbEnvironment>(environment =>
						environment.ConnectionPooling == DbEnv.Instance.ConnectionPooling
						&& environment.ConnectionTimeout == DbEnv.Instance.ConnectionTimeout
						&& environment.IsServingWebBasedApp == DbEnv.Instance.IsServingWebBasedApp
						&& environment.DeadlockPriority == DbEnv.Instance.DeadlockPriority
						&& environment.IsTimerDisabledDuringDbUpgrade == DbEnv.Instance.IsTimerDisabledDuringDbUpgrade
						&& environment.ConnectionGuiPlugin == DbEnv.Instance.ConnectionGuiPlugin,
					MockBehavior.Strict);
				return Mock.Get(dbEnvironmentMock);
			}

			IHeartbeatInfoFactory heartbeatFactoryMock;
			IHeartbeatInfo heartbeatInfoMock;
			Mock<SemaphoreDbManager> semaphoreDbManagerMock;
			Mock<IUpgradeChecker> upgradeCheckerMock;
			Mock<IHeartBeatRemoteLogoff> logoffHandlerMock;
			Mock<IWindowsTimer> windowsTimerMock;
		}

		class MiscellaneousTest : TestCase
		{
			public void TestWrongParamsCallHeartbeat()
			{
				var heartbeatInfoFactory = Mock.Of<IHeartbeatInfoFactory>();
				var timeSpan = TimeSpan.Zero;
				var logoffHandler = Mock.Of<IHeartBeatRemoteLogoff>();
				var semaphoreDbManager = Mock.Of<ISemaphoreDbManager>();
				var upgradeChecker = Mock.Of<IUpgradeChecker>();
				var windowsTimer = Mock.Of<IWindowsTimer>();

				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => new Heartbeat(
						null,
						timeSpan,
						timeSpan,
						logoffHandler,
						semaphoreDbManager,
						upgradeChecker,
						windowsTimer).Dispose());
					AssertEquals(result.ParamName, "sessionInfoFactory");

					result = AssertExceptionThrown<ArgumentNullException>(() => new Heartbeat(
						heartbeatInfoFactory,
						timeSpan,
						timeSpan,
						null,
						semaphoreDbManager,
						upgradeChecker,
						windowsTimer).Dispose());
					AssertEquals(result.ParamName, "logoffHandler");

					result = AssertExceptionThrown<ArgumentNullException>(() => new Heartbeat(
						heartbeatInfoFactory,
						timeSpan,
						timeSpan,
						logoffHandler,
						null,
						upgradeChecker,
						windowsTimer).Dispose());
					AssertEquals(result.ParamName, "dbManager");

					result = AssertExceptionThrown<ArgumentNullException>(() => new Heartbeat(
						heartbeatInfoFactory,
						timeSpan,
						timeSpan,
						logoffHandler,
						semaphoreDbManager,
						null,
						windowsTimer).Dispose());
					AssertEquals(result.ParamName, "upgradeChecker");
				});
			}

			public void TestWrongParamsCallHeartbeatNew()
			{
				var heartbeatInfoFactory = Mock.Of<IHeartbeatInfoFactory>();
				var heartBeatRemoteLogoff = Mock.Of<IHeartBeatRemoteLogoff>();
				var windowsTimer = Mock.Of<IWindowsTimer>();
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => Heartbeat.New(null, TimeSpan.Zero, heartBeatRemoteLogoff, windowsTimer).Dispose());
					AssertEquals(result.ParamName, "sessionInfo");

					result = AssertExceptionThrown<ArgumentNullException>(() => Heartbeat.New(heartbeatInfoFactory, TimeSpan.Zero, null, windowsTimer).Dispose());
					AssertEquals(result.ParamName, "logoffHandler");

					Mock.Get(heartbeatInfoFactory)
						.Setup(factory => factory.New())
						.Returns(Mock.Of<IHeartbeatInfo>(info =>
							info.HostName == "host"
							&& info.HeartbeatType == "TYP"));
					AssertNoExceptionThrown(() => Heartbeat.New(heartbeatInfoFactory, TimeSpan.Zero, heartBeatRemoteLogoff, null).Dispose());
				});
			}
		}

		[UseSnapshotProtection]
		public void TestGetSystemUpgradeDateTime_WithNoLock()
		{
			using (var extraConnection = Db.NewAdminConnection())
			{
				extraConnection.BeginTransaction();
				var utcNow = DateTime.UtcNow;
				var upgradeTime = utcNow.AddMinutes(10);
				var upgradeRefreshedTime = utcNow;
				var propertyValue = string.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeTime), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedTime));
				// Set the property on another connection in a transaction so it is only accessible with nolock from other connections.
				DataUtils.SaveDbExtendedProperty(extraConnection, DataUtils.DateTimeForthcomingUpgradeExtPty, propertyValue);

				using (Db.Connection.TemporarySetLockTimeout(TimeSpan.Zero))
				{
					AssertDateTimeWithinOneSecond("GetSystemUpgradeDateTime", upgradeTime, Heartbeat.GetSystemUpgradeDateTime());
				}
			}
		}
	}
}
