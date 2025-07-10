using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using Moq;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	[UseSnapshotProtection]
	sealed class WinFormsDbEnvironmentTest : TestCase
	{
#if !WINZOR

		[GuiTest]
		[SnailTest]
		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "Form redundant cast required")]
		public void TestHandleDatabaseUpgradeInProgressException()
		{
			// Arrange
			var databaseUpgradeInProgressExceptionIsHandledEvent = new ManualResetEvent(false);

			WinFormsAppDbConnectionGuiPlugin.GetOverridableUpgradeInProgressHandled_ForTest.Value = () => databaseUpgradeInProgressExceptionIsHandledEvent.Set();
			var errorReporterMock = new Mock<IErrorReporter>();
			var isWarningShowing = false;
			var upgradingWarningManagerMock = new Mock<IDbUpgradingWarningManager>();
			upgradingWarningManagerMock
				.Setup(x => x.ShowWarningAndWait())
				.Callback(() =>
				{
					isWarningShowing = true;
					new DbUpgradingWarningManager()
						.ShowWarningAndWait();
				});
			var envMock = new Mock<BaseDbEnvironment>() { CallBase = true };
			envMock
				.Setup(x => x.ConnectionGuiPlugin)
				.Returns(new WinFormsAppDbConnectionGuiPlugin(Mock.Of<IDatabaseUpgradedExceptionHandler>(), Mock.Of<IVersionUpgradedHandler>(), Mock.Of<IUpgradeCheckHelper>(), upgradingWarningManagerMock.Object));

			using (DbEnv.SetTemporaryDbEnvironment(envMock.Object))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var lockoutStartSignal = new ManualResetEvent(false))
			using (var upgradeFormShowingSignal = new ManualResetEvent(false))
			using (Form testForm = new Form())
			{
				bool isTimerDisabledDuringDbUpgrade = false;
				try
				{
					var lockOutTask = Task.Run(() =>
					{
						using (var adminConnection = Db.NewAdminConnection())
						using (new DisposableAction(() => adminConnection.ResetLockout()))
						{
							AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
							Assert(DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName));
							lockoutStartSignal.Set();
							upgradeFormShowingSignal.WaitOne();
						}
					});
					lockoutStartSignal.WaitOne();
					// Act
					testForm.Shown += (object sender, EventArgs e) =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							Db.Connection.ExecuteScalar("select getdate()");
						}
					}; // Testing basic db connectivitiy
					testForm.Show();

					var monitorUpgradeFormThread = new Thread(() =>
					{
						var count = 0;
						do
						{
							Thread.Sleep(50);
							if (testForm.IsHandleCreated)
							{
								if (isWarningShowing)
								{
									testForm.Invoke(new Action(() => isTimerDisabledDuringDbUpgrade = DbEnv.Instance.IsTimerDisabledDuringDbUpgrade));
								}
							}
							count++;
						}
						while (!isWarningShowing && count < 1000);

						upgradeFormShowingSignal.Set();
					});
					monitorUpgradeFormThread.Start();

					while (!monitorUpgradeFormThread.Join(TimeSpan.FromMilliseconds(50)))
					{
						Application.DoEvents();
					}
				}
				finally
				{
					Db.Connection.EnsureIsOpen();
					databaseUpgradeInProgressExceptionIsHandledEvent.WaitOne();
					testForm.Invoke(new Action(testForm.Dispose));
				}
				errorReporterMock.Verify(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				Assert(isWarningShowing);
				AssertEquals(true, isTimerDisabledDuringDbUpgrade);
			}
		}

#endif

		[GuiTest]
		[SnailTest]
		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "Form redundant cast required")]
		public void TestHandleDatabaseUpgradeInProgressExceptionWithTimerCallback()
		{
			var timerEvent = new AutoResetEvent(false);
			var date = DateTime.MinValue;
			var isWarningShowing = false;
			var upgradingWarningManagerMock = new Mock<IDbUpgradingWarningManager>();
			upgradingWarningManagerMock
				.Setup(x => x.ShowWarningAndWait())
				.Callback(() =>
				{
					isWarningShowing = true;
					new DbUpgradingWarningManager()
						.ShowWarningAndWait();
				});
			var envMock = new Mock<BaseDbEnvironment>() { CallBase = true };
			envMock
				.Setup(x => x.ConnectionGuiPlugin)
				.Returns(new WinFormsAppDbConnectionGuiPlugin(Mock.Of<IDatabaseUpgradedExceptionHandler>(), Mock.Of<IVersionUpgradedHandler>(), Mock.Of<IUpgradeCheckHelper>(), upgradingWarningManagerMock.Object));

			using (DbEnv.SetTemporaryDbEnvironment(envMock.Object))
			using (var lockoutStartSignal = new ManualResetEvent(false))
			using (var upgradeFormShowingSignal = new ManualResetEvent(false))
			using (Form testForm = new Form())
			{
				var lockOutTask = Task.Run(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					using (new DisposableAction(() => adminConnection.ResetLockout()))
					{
						AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
						Assert(DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName));
						lockoutStartSignal.Set();
						upgradeFormShowingSignal.WaitOne();
					}
				});
				testForm.Show();
				lockoutStartSignal.WaitOne();

				using (var timer = new System.Windows.Forms.Timer())
				{
					timer.Tick += (object sender, EventArgs e) =>
					{
						timerEvent.Set();
						using (Db.DisposableActionForDbConnection())
						{
							date = (DateTime)Db.Connection.ExecuteScalar("select getdate()"); // Testing basic db connectivitiy
						}
					};
					timer.Interval = 1000;
					timer.Start();

					var monitorUpgradeFormThread = new Thread(() =>
					{
						timerEvent.WaitOne();
						var count = 0;
						do
						{
							Thread.Sleep(50);
							count++;
						}
						while (!isWarningShowing && count < 1000);
						upgradeFormShowingSignal.Set();
					});
					monitorUpgradeFormThread.Start();

					while (!monitorUpgradeFormThread.Join(TimeSpan.FromMilliseconds(50)))
					{
						Application.DoEvents();
					}

					Assert(isWarningShowing);

					while (!timerEvent.WaitOne(TimeSpan.FromMilliseconds(50)))
					{
						Application.DoEvents();
					}
					AssertNotEquals(date, DateTime.MinValue);
				}
			}

			Db.Connection.EnsureIsOpen();
		}

#if WINZOR

		[UseSnapshotProtection]
		public void TestHandleDatabaseUpgradeInProgressExceptionFromNonGuiThread()
		{
			var date = DateTime.MinValue;
			var isWarningShowing = false;
			var taskCompleted = false;
			var upgradingWarningManagerMock = new Mock<IDbUpgradingWarningManager>();
			upgradingWarningManagerMock
				.Setup(x => x.ShowWarningAndWait())
				.Callback(() =>
				{
					isWarningShowing = true;
					new DbUpgradingWarningManager()
						.ShowWarningAndWait();
				});
			var envMock = new Mock<BaseDbEnvironment>() { CallBase = true };
			envMock
				.Setup(x => x.ConnectionGuiPlugin)
				.Returns(new WinFormsAppDbConnectionGuiPlugin(Mock.Of<IDatabaseUpgradedExceptionHandler>(), Mock.Of<IVersionUpgradedHandler>(), Mock.Of<IUpgradeCheckHelper>(), upgradingWarningManagerMock.Object));

			using (DbEnv.SetTemporaryDbEnvironment(envMock.Object))
			using (var lockoutStartSignal = new ManualResetEvent(false))
			using (var upgradeFormShowingSignal = new ManualResetEvent(false))
			using (var testForm = new Form())
			{
				testForm.Show();
				var lockOutTask = Task.Run(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					using (new DisposableAction(() => adminConnection.ResetLockout()))
					{
						AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
						Assert(DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName));
						lockoutStartSignal.Set();
						upgradeFormShowingSignal.WaitOne();
					}
				});
				lockoutStartSignal.WaitOne();
				var task = Task.Run(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							date = (DateTime)Db.Connection.ExecuteScalar("select getdate()"); // Testing basic db connectivitiy
						}
					}
					catch (DatabaseUpgradeException ex)
					{
						DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(ex);
					}
					finally
					{
						taskCompleted = true;
					}
				});

				var monitorUpgradeFormThread = new Thread(() =>
				{
					var count = 0;
					do
					{
						Thread.Sleep(50);
						count++;
					}
					while (!isWarningShowing && count < 1000);

					upgradeFormShowingSignal.Set();
				});
				monitorUpgradeFormThread.Start();

				while (!monitorUpgradeFormThread.Join(TimeSpan.FromMilliseconds(50)) || !taskCompleted)
				{
					Application.DoEvents();
					Thread.Sleep(50);
				}

				Assert(isWarningShowing);

				task.GetAwaiter().GetResult();
			}

			Db.Connection.EnsureIsOpen();
		}
#endif

		public void TestNoMoreDbUpgradeExceptionThrownWhenOneWasBeingHandled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery("kill " + Db.Connection.SPID);
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
			}

			plugin.HandleDatabaseUpgradeException(new DatabaseUpgradedException());

			AssertEquals(false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestNoDatabaseUpgradedExceptionThrownWhenDatabaseUpgradeInProgressExceptionWasBeingHandled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var databaseMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
				try
				{
					AssertNoExceptionThrown(() => plugin.HandleDatabaseUpgradeException(new DatabaseUpgradeInProgressException()));
				}
				finally
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(databaseMajorSchemaVersion, adminConnection);
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}
		}

		public void TestHandleDatabaseUpgradeException_RestartsWithDisabledSchemaVersionCheck()
		{
			bool isDisabled = false;
			mockDatabaseUpgradedExceptionHandler.Setup(x => x.Restart()).Callback(() =>
			{
				isDisabled = Db.IsThreadSchemaVersionCheckDisabled;
			});

			AssertNoExceptionThrown(() => plugin.HandleDatabaseUpgradeException(new DatabaseUpgradedException()));

#if !WINZOR
			AssertEquals("thread schema check is disabled before calling Restart", true, isDisabled);
			AssertEquals("thread schema check is disabled permanently", true, Db.IsThreadSchemaVersionCheckDisabled);
			mockDatabaseUpgradedExceptionHandler.Verify(x => x.Restart(), Times.Once);
			mockDatabaseUpgradedExceptionHandler.Verify(x => x.Exit(ExitCodes.DatabaseUpgraded), Times.Never);
#else
			mockDatabaseUpgradedExceptionHandler.Verify(x => x.Restart(), Times.Never);
			mockDatabaseUpgradedExceptionHandler.Verify(x => x.Exit(ExitCodes.DatabaseUpgraded), Times.Once);
#endif
		}

		public void TestCheckVersionUpgraded_DoesNotCallExitIfVersionNotUpgraded()
		{
			mockUpgradeCheckHelper.Setup(x => x.HasBeenUpgraded()).Returns(false);

			AssertNoExceptionThrown(() => plugin.CheckVersionUpgraded());

#if !WINZOR
			mockVersionUpgradedHandler.Verify(x => x.Exit(ExitCodes.VersionUpgraded), Times.Never);
#else
			mockVersionUpgradedHandler.Verify(x => x.Exit(ExitCodes.VersionUpgraded), Times.Never);
#endif
		}

		public void TestCheckVersionUpgraded_CallsExitIfVersionUpgraded()
		{
			mockUpgradeCheckHelper.Setup(x => x.HasBeenUpgraded()).Returns(true);

			AssertNoExceptionThrown(() => plugin.CheckVersionUpgraded());

#if !WINZOR
			mockVersionUpgradedHandler.Verify(x => x.Exit(ExitCodes.VersionUpgraded), Times.Never);
#else
			mockVersionUpgradedHandler.Verify(x => x.Exit(ExitCodes.VersionUpgraded), Times.Once);
#endif
		}

		[UseSnapshotProtection]
		public void TestHandleDatabaseUpgradeExceptionOnlyDisableSchemaCheckOnCurrentThread()
		{
			// Arrange
			var schemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			using (new DisposableAction(() => DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaVersion, adminConnection)))
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
				adminConnection.ExecuteNonQuery($"INSERT INTO [StmUpgrade] (SZ_PK, SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch, SZ_Status, SZ_SystemCreateUser, SZ_SystemLastEditUser, SZ_SystemCreateTimeUtc, SZ_SystemLastEditTimeUtc) VALUES (NEWID(), 1, 2, 3, 4, 'CUR', 'TES', 'TES', GETDATE(),GETDATE())");
				Exception extraThreadConnectionEx = null;

				mockDbUpgradingWarningManager
					.Setup(x => x.ShowWarningAndWait())
					.Callback(() =>
					{
						var extraConnectionTask = Task.Run(() =>
						{
							try
							{
								using var disposeAction = Db.DisposableActionForDbConnection();
								Db.Connection.EnsureIsOpen();
							}
							catch (Exception ex)
							{
								extraThreadConnectionEx = ex;
							}
						});
						extraConnectionTask.Wait();
					});

				var mockDbEnvironment = new Mock<BaseDbEnvironment>() { CallBase = true };
				mockDbEnvironment
					.Setup(x => x.ConnectionGuiPlugin)
					.Returns(plugin);
				
				using (DbEnv.SetTemporaryDbEnvironment(mockDbEnvironment.Object))
				{
					// Act
					plugin.HandleDatabaseUpgradeException(new DatabaseUpgradeInProgressException());

					// Assert
					AssertNotNull(extraThreadConnectionEx);
					AssertType(typeof(DatabaseUpgradedException), extraThreadConnectionEx);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockDatabaseUpgradedExceptionHandler = new Mock<IDatabaseUpgradedExceptionHandler>();
			mockVersionUpgradedHandler = new Mock<IVersionUpgradedHandler>();
			mockUpgradeCheckHelper = new Mock<IUpgradeCheckHelper>();
			mockDbUpgradingWarningManager = new Mock<IDbUpgradingWarningManager>();
			plugin = new WinFormsAppDbConnectionGuiPlugin(mockDatabaseUpgradedExceptionHandler.Object, mockVersionUpgradedHandler.Object, mockUpgradeCheckHelper.Object, mockDbUpgradingWarningManager.Object);
			ResetCurrentVersion();
		}

		protected override void TearDown()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			base.TearDown();
		}

		void ResetCurrentVersion()
		{
			Db.Connection.ExecuteNonQuery(
				"UPDATE dbo.StmUpgrade SET SZ_Status = 'APL' WHERE SZ_Status = @status",
				cmd => cmd.AddParameter("@status", System.Data.SqlDbType.VarChar, 3, "CUR"));
		}

		WinFormsAppDbConnectionGuiPlugin plugin;
		Mock<IDatabaseUpgradedExceptionHandler> mockDatabaseUpgradedExceptionHandler;
		Mock<IVersionUpgradedHandler> mockVersionUpgradedHandler;
		Mock<IUpgradeCheckHelper> mockUpgradeCheckHelper;
		Mock<IDbUpgradingWarningManager> mockDbUpgradingWarningManager;
	}
}
