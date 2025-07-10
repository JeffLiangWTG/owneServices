using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	internal class DbUpgradingWarningManagerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestShowWarningAndWaitOnMainUIThread()
		{
			// Arrange
			var dbUpgradingWarningManager = new Mock<DbUpgradingWarningManager>() { CallBase = true };

			// Act
			dbUpgradingWarningManager.Object.ShowWarningAndWait();

			// Assert
			dbUpgradingWarningManager
				.Protected()
				.Verify("ShowDialog", Times.Exactly(1), ItExpr.IsAny<UpgradeInProgressForm>());
		}

#if !WINZOR
		public void TestShowWarningAndWaitBlockNonUIThread()
		{
			// Arrange
			var upgradeDuration = TimeSpan.FromSeconds(10);
			using (var warningShowingSignal = new ManualResetEvent(false))
			{
				var dbUpgradingWarningManager = new Mock<DbUpgradingWarningManager>() { CallBase = true };
				dbUpgradingWarningManager
					.Protected()
					.Setup("ShowDialog", ItExpr.IsAny<UpgradeInProgressForm>())
					.Callback(() =>
					{
						warningShowingSignal.Set();
						var applicationEventStopWatch = Stopwatch.StartNew();
						while (applicationEventStopWatch.Elapsed <= upgradeDuration)
						{
							Application.DoEvents();
						}
					});
				var nonUIThreadMonitor = new Stopwatch();
				var nonUIThread = Task.Run(() =>
				{
					warningShowingSignal.WaitOne();
					nonUIThreadMonitor.Start();
					dbUpgradingWarningManager.Object.ShowWarningAndWait();
					nonUIThreadMonitor.Stop();
				});

				// Act
				dbUpgradingWarningManager.Object.ShowWarningAndWait();

				// Assert
				var gracePeriod = TimeSpan.FromMilliseconds(1000);
				var expectedBlockTime = upgradeDuration - gracePeriod;
				Assert(nonUIThread.Wait(TimeSpan.FromSeconds(30)));
				AssertGreaterThanOrEqualTo(nonUIThreadMonitor.Elapsed, expectedBlockTime);
				dbUpgradingWarningManager
					.Protected()
					.Verify("ShowDialog", Times.Exactly(1), ItExpr.IsAny<UpgradeInProgressForm>());
			}
		}
#endif

		public void TestShowWarningAndWaitRunWarningWhenNonUIThreadCall()
		{
			// Arrange
			var warningFormThreadId = 0;
			var dbUpgradingWarningManager = new Mock<DbUpgradingWarningManager>() { CallBase = true };
			dbUpgradingWarningManager
				.Protected()
				.Setup("ShowDialog", ItExpr.IsAny<UpgradeInProgressForm>())
				.Callback(() =>
				{
					warningFormThreadId = System.Environment.CurrentManagedThreadId;
				});

			// Act
			var nonUIThread = Task.Run(() =>
			{
				dbUpgradingWarningManager.Object.ShowWarningAndWait();
			});

			// Assert
			while (warningFormThreadId == 0)
			{
				Application.DoEvents();
			}
			Assert(nonUIThread.Wait(TimeSpan.FromSeconds(60)));
			AssertEquals(System.Environment.CurrentManagedThreadId, warningFormThreadId);
			dbUpgradingWarningManager
				.Protected()
				.Verify("ShowDialog", Times.Exactly(1), ItExpr.IsAny<UpgradeInProgressForm>());
		}

		[UseSnapshotProtection]
		public void TestShowWarningAndWaitDoesNotDisableSchemaCheck() 
		{
			// Arrange
			Exception uiThreadException = null;
			var dbUpgradingWarningManager = new Mock<DbUpgradingWarningManager>() { CallBase = true };
			dbUpgradingWarningManager
				.Protected()
				.Setup("ShowDialog", ItExpr.IsAny<UpgradeInProgressForm>())
				.Callback(() =>
				{
					try
					{
						using (var extraConnection = Db.NewExtraConnectionToMainDb())
						{
							extraConnection.EnsureIsOpen();
						}
					}
					catch (Exception ex)
					{
						uiThreadException = ex;
					}
				});

			using (var adminConnection = Db.NewAdminConnection())
			using (new DisposableAction(() => adminConnection.ResetLockout()))
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				Assert(DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName));
				
				// Act
				dbUpgradingWarningManager.Object.ShowWarningAndWait();

				// Assert
				AssertNotNull(uiThreadException);
				AssertType(typeof(DatabaseUpgradeInProgressException), uiThreadException);
				dbUpgradingWarningManager
					.Protected()
					.Verify("ShowDialog", Times.Exactly(1), ItExpr.IsAny<UpgradeInProgressForm>());
			}
		}
	}
}
