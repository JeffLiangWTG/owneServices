using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Startup;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	class UpgradeInProgressFormTest : TestCase
	{
		public void TestFinalizeFromWhenTimerIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				var testForm = new UpgradeInProgressFormForTest();
				testForm.Dispose_Exposed(true);
				testForm.ForegroundTimer_Exposed = null;
				testForm.Dispose_Exposed(false);
			});
		}

		[UseSnapshotProtection]
		public void TestDoBackgroundCheck_LoginsEnabled()
			=> AssertDoBackgroundCheck(schemaLockOnly: false);

		[UseSnapshotProtection]
		// Test the case of the connection managed to open without checking for a DbLockout.
		public void TestDoBackgroundCheck_LoginsEnabled_SchemaLockOnly()
			=> AssertDoBackgroundCheck(schemaLockOnly: true);

		void AssertDoBackgroundCheck(bool schemaLockOnly)
		{
			// Disabling ALL connection schema checks,
			// just like the code that launches this form in WinFormsAppDbConnectionGuiPlugin.HandleDatabaseUpgradeException.
			using (Db.DisableSchemaVersionCheck())
			using (var adminConnection = Db.NewAdminConnection())
			using (var testForm = new UpgradeInProgressFormForTest(startTimers: false))
			{
				if (!schemaLockOnly)
				{
					DbLockout.AcquireLockout(adminConnection, disableLogins: false);
				}
				adminConnection.BeginTransaction();
				new DatabasePadlock().LockDatabaseResources(adminConnection, new[] { Db.DatabaseName });

				for (int i = 0; i < 3; ++i)
				{
					Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (Db.Connection.TemporarySetDefaultCommandTimeOut(1))
						using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(0)))
						{
							testForm.DoBackgroundCheck_Exposed();
						}
					}).Wait();
					AssertEquals("i=" + i.ToString(), false, testForm.close_Exposed);
					AssertEquals("errors reported", 0, ErrorReporter.TotalErrorCount);
					AssertNull(nameof(testForm.UnhandledException), testForm.UnhandledException);
				}

				adminConnection.RollbackTransaction();
				if (!schemaLockOnly)
				{
					DbLockout.ResetLockout(adminConnection);
				}

				Task.Run(() =>
				{
					testForm.DoBackgroundCheck_Exposed();
				}).Wait();
				AssertEquals(true, testForm.close_Exposed);
				AssertEquals("errors reported", 0, ErrorReporter.TotalErrorCount);
				AssertNull(nameof(testForm.UnhandledException), testForm.UnhandledException);
			}
		}

		class UpgradeInProgressFormForTest : UpgradeInProgressForm
		{
			public UpgradeInProgressFormForTest(bool startTimers = true)
				: base(startTimers)
			{
			}

			public System.Windows.Forms.Timer ForegroundTimer_Exposed
			{
				get { return foregroundTimer; }
				set { foregroundTimer = value; }
			}

			public void Dispose_Exposed(bool disposing)
			{
				Dispose(disposing);
			}

			public void ForegroundTimer_Tick_Exposed()
			{
				ForegroundTimer_Tick(null, null);
			}

			public void DoBackgroundCheck_Exposed()
				=> DoBackgroundCheck();

			public bool close_Exposed
				=> CanClose;

			public void SetupUpgradedException()
			{
				upgradedException = new DatabaseUpgradedException();
			}

			protected override void HandleUnhandledException(Exception ex)
			{
				UnhandledException = ex;
				base.HandleUnhandledException(ex);
			}

			public Exception UnhandledException { get; set; }
		}

		[ExpectNoExceptions]
		public void TestForegroundTimer()
		{
			var baseEnv = DbEnv.Instance;
			try
			{
				var mock = new DbEnvMock();
				DbEnv.SetDbEnvironment(mock);
				mock.Mock.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>())).Callback(() =>
				{
					var poke = Db.Connection;
				});
				using (var form = new UpgradeInProgressFormForTest())
				{
					form.SetupUpgradedException();
					form.ForegroundTimer_Tick_Exposed();
				}
			}
			finally
			{
				DbEnv.SetDbEnvironment(baseEnv);
			}
		}

		[ExpectNoExceptions]
		public void TestUpgradeInProgressFormRestartsCW1WhilstUpgradeCompletedDuringForegroundTimerExecuting()
		{
			// Arrange
			var dbEnvMock = new DbEnvMock();
			var upgradeFormMock = new Mock<UpgradeInProgressForm> { CallBase = true };
			var upgradeCompletedSignal = new ManualResetEvent(false);

			using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock))
			{
				upgradeFormMock
					.Protected()
					.Setup<IUpgradeCheckHelper>("upgradeChecker")
					.Callback(() =>
					{
						upgradeCompletedSignal.Set();
					}).Returns(Mock.Of<IUpgradeCheckHelper>(y => y.HasBeenUpgraded()));

				upgradeFormMock
					.Protected()
					.SetupGet<bool>("CanClose")
					.Callback(() =>
					{
						upgradeCompletedSignal.WaitOne();
						Thread.Sleep(1000);
					});

				// Act
				using (_ = upgradeFormMock.Object)
				{
					upgradeFormMock.Object.ShowDialog();
				}

				// Assert
				dbEnvMock.Mock.Verify(m =>
					m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>()),
					Times.Once,
					"upgradedException should not be null when upgrade form is closing."
				);
			}
		}

		[UseSnapshotProtection]
		public void TestBackgroundTimerCallbackNotThrowDbUpgradeInProgressException()
		{
			// Arrange
			var mockGuiPlugin = new Mock<IDbConnectionGuiPlugin>();
			mockGuiPlugin
				.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
				.Callback(() => Db.Connection.EnsureIsOpen());
			var dbEnvMock = new Mock<BaseDbEnvironment>() { CallBase = true };
			dbEnvMock
				.Setup(x => x.ConnectionGuiPlugin)
				.Returns(mockGuiPlugin.Object);

			using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock.Object))
			using (var lockoutAcquiredSignal = new ManualResetEvent(false))
			using (var backgroundCheckDoneSignal = new ManualResetEvent(false))
			{
				var dbLockoutTask = Task.Run(() =>
				{
					using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
					using (new DisposableAction(() => adminConnection.ResetLockout()))
					{
						if (DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade) != DbLockoutState.AquiredLockout)
						{
							Assert("Failed to lockout DB", false);
						}
						lockoutAcquiredSignal.Set();
						backgroundCheckDoneSignal.WaitOne();
					}
				});
				lockoutAcquiredSignal.WaitOne();

				// Act
				using (var form = new UpgradeInProgressFormForTest())
				{
					AssertNoExceptionThrown(() => form.DoBackgroundCheck_Exposed());
				}

				// Assert
				backgroundCheckDoneSignal.Set();
				AssertNoExceptionThrown(() => dbLockoutTask.Wait(TimeSpan.FromSeconds(5)));
				Assert("Task should complete in 5 secs", dbLockoutTask.IsCompleted);
			}
		}

		[UseSnapshotProtection]
		public void TestForegroundTimerTickNotThrowDbUpgradeInProgressException()
		{
			// Arrange
			using (var lockoutAcquiredSignal = new ManualResetEvent(false))
			using (var foregroundDoneSignal = new ManualResetEvent(false))
			{
				var dbLockoutTask = Task.Run(() =>
				{
					using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
					using (new DisposableAction(() => adminConnection.ResetLockout()))
					{
						if (DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade) != DbLockoutState.AquiredLockout)
						{
							Assert("Failed to lockout DB", false);
						}
						lockoutAcquiredSignal.Set();
						foregroundDoneSignal.WaitOne();
					}
				});
				lockoutAcquiredSignal.WaitOne();

				// Act
				using (var form = new UpgradeInProgressFormForTest())
				{
					AssertNoExceptionThrown(() => form.ForegroundTimer_Tick_Exposed());
				}

				// Assert
				foregroundDoneSignal.Set();
				AssertNoExceptionThrown(() => dbLockoutTask.Wait(TimeSpan.FromSeconds(5)));
				Assert("Task should complete in 5 secs", dbLockoutTask.IsCompleted);
			}
		}

		class DbEnvMock : BaseDbEnvironment
		{
			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => Mock.Object;
			public Mock<IDbConnectionGuiPlugin> Mock { get; } = new Mock<IDbConnectionGuiPlugin>();
		}
	}
}
