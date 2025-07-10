using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	class DbLockoutTest : TestCase
	{
		public void TestAcquireUpgradeTransactionLockout()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.BeginTransaction();
				AssertEquals(true, DbLockout.AcquireTransactionLockout(adminConnection));
				AssertEquals("Upgrade", DataUtils.LoadDbExtendedProperty(adminConnection, DbLockout.DbIsLockedOutForProperty));
				AssertEquals(true, adminConnection.IsInTransaction);

				adminConnection.RollbackTransaction();
				AssertNull(DataUtils.LoadDbExtendedProperty(adminConnection, DbLockout.DbIsLockedOutForProperty));
			}
		}

		public void TestAcquireUpgradeTransactionLockout_ThrowWhenOpeningAnotherAdminConnection()
		{
			using (var lockoutConnection = Db.NewAdminConnection())
			using (var adminConnection = Db.NewAdminConnection())
			{
				lockoutConnection.BeginTransaction();
				AssertEquals(true, DbLockout.AcquireTransactionLockout(lockoutConnection));

				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => adminConnection.EnsureIsOpen());
			}
		}

		public void TestAcquireUpgradeTransactionLockout_WithUpgradeCheckDisabled_WhenOtherConnectionHasLockout()
		{
			using (var lockoutConnection = Db.NewAdminConnection())
			using (var adminConnection = Db.NewAdminConnection())
			{
				lockoutConnection.BeginTransaction();
				AssertEquals(true, DbLockout.AcquireTransactionLockout(lockoutConnection));

				adminConnection.IsUpgradeCheckDisabled = true;
				adminConnection.BeginTransaction();
				AssertEquals(false, DbLockout.AcquireTransactionLockout(adminConnection));
			}
		}

		public void TestAcquireUpgradeTransactionLockout_ThrowsIfNotInTransaction()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertExceptionThrown<InvalidOperationException>(() => DbLockout.AcquireTransactionLockout(adminConnection));
			}
		}

		public void TestTryLoadLockoutReasonFromMainDb_TransactionLockout()
		{
			using (var connectionWithLockout = Db.NewAdminConnection())
			using (var loadConnection = Db.NewExtraConnectionToMainDb())
			{
				loadConnection.IsUpgradeCheckDisabled = true;
				loadConnection.EnsureIsOpen();
				((ICurrentDbControl)loadConnection).UseDatabase(Db.SqlMasterDb);

				var actualBefore = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				connectionWithLockout.BeginTransaction();
				DbLockout.AcquireTransactionLockout(connectionWithLockout);

				var actualDuring = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				connectionWithLockout.RollbackTransaction();

				var actualAfter = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				AssertEquals("actualBefore.ok", true, actualBefore.ok);
				AssertEquals("actualBefore.reason", LockoutReason.None, actualBefore.reason);
				AssertEquals("actualBefore.isLocked", false, actualBefore.isLocked);

				AssertEquals("actualDuring.ok", true, actualDuring.ok);
				AssertEquals("actualDuring.reason", LockoutReason.Upgrade, actualDuring.reason);
				AssertEquals("actualDuring.isLocked", true, actualDuring.isLocked);

				AssertEquals("actualAfter.ok", true, actualAfter.ok);
				AssertEquals("actualAfter.reason", LockoutReason.None, actualAfter.reason);
				AssertEquals("actualAfter.isLocked", false, actualAfter.isLocked);
			}
		}

		[UseSnapshotProtection]
		public void TestTryLoadLockoutReasonFromMainDb_LegacyLockout()
		{
			using (var connectionWithLockout = Db.NewAdminConnection())
			using (var loadConnection = Db.NewExtraConnectionToMainDb())
			{
				loadConnection.IsUpgradeCheckDisabled = true;
				loadConnection.EnsureIsOpen();
				((ICurrentDbControl)loadConnection).UseDatabase(Db.SqlMasterDb);

				var actualBefore = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				DbLockout.AcquireLockout(connectionWithLockout);

				var actualDuring = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				DbLockout.ResetLockout(connectionWithLockout);

				var actualAfter = DbLockout.TryLoadLockoutReasonFromMainDb(loadConnection);

				AssertEquals("actualBefore.ok", true, actualBefore.ok);
				AssertEquals("actualBefore.reason", LockoutReason.None, actualBefore.reason);
				AssertEquals("actualBefore.isLocked", false, actualBefore.isLocked);

				AssertEquals("actualDuring.ok", true, actualDuring.ok);
				AssertEquals("actualDuring.reason", LockoutReason.Upgrade, actualDuring.reason);
				AssertEquals("actualDuring.isLocked", false, actualDuring.isLocked);

				AssertEquals("actualAfter.ok", true, actualAfter.ok);
				AssertEquals("actualAfter.reason", LockoutReason.None, actualAfter.reason);
				AssertEquals("actualAfter.isLocked", false, actualAfter.isLocked);
			}
		}

		[UseSnapshotProtection]
		public void TestIsUpgradeLockoutError()
		{
			var sqlErrors = new List<(int errorNumber, string errorMessage)>
			{
				(18470, "Login failed for user 'dbUser'. Reason: The account is disabled."),
				(4060, $@"Cannot open database ""{Db.DatabaseName}"" requested by the login. The login failed."),
				(18486, "Login failed for user 'dbUser' because the account is currently locked out. The system administrator can unlock it."),
				(233, "A connection was successfully established with the server, but then an error occurred during the login process. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)"),
			};

			using (var adminConnection = Db.NewAdminConnection())
			{
				// Arrange
				adminConnection.ResetLockout();
				AssertEquals(DbLockoutState.NoLockout, DbLockout.CheckLockoutState(adminConnection));

				try
				{
					// Act
					DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade);
					AssertEquals(DbLockoutState.ValidLockout, adminConnection.CheckLockoutState());

					// Assert
					foreach (var (errorNumber, errorMessage) in sqlErrors)
					{
						Assert(DbLockout.IsUpgradeLockoutError(SqlExceptionBuilder.CreateSqlException(errorNumber, errorMessage)));
					}
				}
				finally
				{
					DbLockout.ResetLockout(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAquireLockout_Upgrade_CausesNewReaderConnectionToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewReaderConnectionToThrowUpgradeInProgress(LockoutReason.Upgrade, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAquireLockout_Purge_CausesNewReaderConnectionToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewReaderConnectionToThrowUpgradeInProgress(LockoutReason.Purge, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAquireTransactionLockout_Upgrade_CausesNewReaderConnectionToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewReaderConnectionToThrowUpgradeInProgress(LockoutReason.Upgrade, inTransaction: true);

		[UseSnapshotProtection]
		public void TestAquireTransactionLockout_Purge_CausesNewReaderConnectionToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewReaderConnectionToThrowUpgradeInProgress(LockoutReason.Purge, inTransaction: true);

		void AssertAcquireLockout_CausesNewReaderConnectionToThrowUpgradeInProgress(LockoutReason reason, bool inTransaction)
		{
			using (var adminConn = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				try
				{
					if (inTransaction)
					{
						adminConn.BeginTransaction();
						DbLockout.AcquireTransactionLockout(adminConn, reason);
					}
					else
					{
						DbLockout.AcquireLockout(adminConn, reason);
					}

					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() =>
					{
						using (var connection = Db.NewExtraRestrictedReaderConnection(Db.ServerName, Db.DatabaseName))
						{
							connection.EnsureIsOpen();
						}
					});
				}
				finally
				{
					if (inTransaction)
					{
						adminConn.RollbackTransaction();
					}
					else
					{
						DbLockout.ResetLockout(adminConn);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAcquireLockout_Upgrade_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress(LockoutReason.Upgrade, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAcquireLockout_Purge_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress(LockoutReason.Purge, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAcquireTransactionLockout_Upgrade_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress(LockoutReason.Upgrade, inTransaction: true);

		[UseSnapshotProtection]
		public void TestAcquireTransactionLockout_Purge_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress(LockoutReason.Purge, inTransaction: true);

		void AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckEnabled_ToThrowUpgradeInProgress(LockoutReason reason, bool inTransaction)
		{
			using (var connectionWithLockout = Db.NewAdminConnection())
			{
				if (inTransaction)
				{
					connectionWithLockout.BeginTransaction();
					DbLockout.AcquireTransactionLockout(connectionWithLockout);
				}
				else
				{
					DbLockout.AcquireLockout(connectionWithLockout, reason);
				}

				if (reason == LockoutReason.Upgrade)
				{
					// Mimic the upgrade DatabasePadlock and take SCH_M lock on the registry table.
					if (!inTransaction)
					{
						connectionWithLockout.BeginTransaction();
					}
					TakeSchemaModificationLockOnTable(connectionWithLockout, "dbo", "StmData");
				}

				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() =>
				{
					using (var testConnection = Db.NewAdminConnection())
					{
						testConnection.EnsureIsOpen();
					}
				});
			}
		}

		[UseSnapshotProtection]
		public void TestAcquireLockout_Upgrade_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow(LockoutReason.Upgrade, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAcquireLockout_Purge_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow(LockoutReason.Purge, inTransaction: false);

		[UseSnapshotProtection]
		public void TestAcquireTransactionLockout_Upgrade_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow(LockoutReason.Upgrade, inTransaction: true);

		[UseSnapshotProtection]
		public void TestAcquireTransactionLockout_Purge_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow()
			=> AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow(LockoutReason.Purge, inTransaction: true);

		void AssertAcquireLockout_CausesNewAdminConnection_WithSchemaCheckDisabled_ToNotThrow(LockoutReason reason, bool inTransaction)
		{
			using (var connectionWithLockout = Db.NewAdminConnection())
			{
				if (inTransaction)
				{
					connectionWithLockout.BeginTransaction();
					DbLockout.AcquireTransactionLockout(connectionWithLockout);
				}
				else
				{
					DbLockout.AcquireLockout(connectionWithLockout, reason);
				}

				if (reason == LockoutReason.Upgrade)
				{
					// Mimic the upgrade DatabasePadlock and take SCH_M lock on the registry table.
					if (!inTransaction)
					{
						connectionWithLockout.BeginTransaction();
					}
					TakeSchemaModificationLockOnTable(connectionWithLockout, "dbo", "StmData");
				}

				AssertNoExceptionThrown(() =>
				{
					using (Db.DisableSchemaVersionCheck())
					using (var testConnection = Db.NewAdminConnection())
					{
						testConnection.EnsureIsOpen();
					}
				});
			}
		}

		[UseSnapshotProtection]
		public void TestGetLockoutReason_Upgrade()
			=> AssertGetLockoutReason(LockoutReason.Upgrade, inTransaction: true);

		[UseSnapshotProtection]
		public void TestGetLockoutReason_Upgrade_LegacyLockout()
			=> AssertGetLockoutReason(LockoutReason.Upgrade, inTransaction: false);

		[UseSnapshotProtection]
		public void TestGetLockoutReason_Purge_LegacyLockout()
			=> AssertGetLockoutReason(LockoutReason.Purge, inTransaction: false);

		void AssertGetLockoutReason(LockoutReason reason, bool inTransaction)
		{
			// Arrange
			using (var adminConn = Db.NewAdminConnection())
			{
				if (inTransaction)
				{
					adminConn.BeginTransaction();
					DbLockout.AcquireTransactionLockout(adminConn);
				}
				else
				{
					DbLockout.AcquireLockout(adminConn, reason);
				}

				// Act
				var actual = DbLockout.GetLockoutReason();

				// Assert
				AssertEquals(reason, actual);
			}
		}

		[UseSnapshotProtection]
		public void TestHasLockout_LoginEnabled()
		{
			var initialResult = DbLockout.HasLockout(Db.Connection);

			// Arrange
			using (var newAdminConnection = Db.NewAdminConnection())
			{
				DbLockout.AcquireLockout(newAdminConnection, LockoutReason.Upgrade, disableLogins: false);

				// Act
				var actual = DbLockout.HasLockout(Db.Connection);

				// Assert
				AssertEquals("PRE", false, initialResult);
				AssertEquals("non admin connection can check for a lockout", true, actual);
			}
		}

		[UseSnapshotProtection]
		public void TestHasLockout_TransactionLockout()
		{
			var initialResult = DbLockout.HasLockout(Db.Connection);

			// Arrange
			using (var newAdminConnection = Db.NewAdminConnection())
			{
				newAdminConnection.BeginTransaction();
				DbLockout.AcquireTransactionLockout(newAdminConnection, LockoutReason.Upgrade);

				// Act
				var actual = DbLockout.HasLockout(Db.Connection);

				// Assert
				AssertEquals("PRE", false, initialResult);
				AssertEquals("non admin connection can check for a lockout", true, actual);
			}
		}

		[ExpectNoExceptions]
		public void TestResetLockout_WhenLoginsDisabled_WithStaffLoginDoesNotDeadlock()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				string staffLoginName = DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName) + Guid.NewGuid().ToString();
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

				adminConnection.BeginTransaction();

				try
				{
					adminConnection.ExecuteNonQuery(string.Format("create login [{0}] with password='{1}'", staffLoginName, Guid.NewGuid().ToString()));
					AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection, disableLogins: true));
					DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, staffLoginName, expected: false);
					DbLockout.ResetLockout(adminConnection);
					DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, staffLoginName, expected: true);
				}
				finally
				{
					try
					{
						DbLockout.ResetLockout(adminConnection);
					}
					finally
					{
						adminConnection.RollbackTransaction();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestConnectionRecoversIfLockoutReset_LoginsDisabled()
		{
			// This test is originally from WI00217716
			// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/commit/61a0de3669a70347b5c0e9edd1aa1bc7084c6c53?refName=refs%2Fheads%2Fmaster&_a=compare
			//
			// It acquires a lockout and then with another connection assumes that the DbConnection.OpenConnectionCore() method
			// will have a SQL Exception (due to login disabled) that will be handled by DbConnection.HandleError.
			// By using a test connection class "DbConnectionWithDelayedHandleError", the error handling is stopped,
			// the lockout is reset, and then error handling is resumed.
			// So by the time the error handling checks for a lockout, there is no lockout and HandleError will call OpenConnectionIfClosed again.
			// The goal is to verify the connection can recover.
			using (var waitForHandleErrorEvent = new AutoResetEvent(false))
			using (var continueHandleErrorEvent = new AutoResetEvent(false))
			using (var adminConnection = Db.NewAdminConnection())
			{
				DbLockout.AcquireLockout(adminConnection, disableLogins: true);
				try
				{
					var timeout = TimeSpan.FromSeconds(5);
					var connectionTask = Task.Run(() =>
					{
						using (var connection = new DbConnectionWithDelayedHandleError(waitForHandleErrorEvent, continueHandleErrorEvent, timeout))
						{
							connection.OpenRetries = 0;
							connection.EnsureIsOpen();
						}
					});

					Assert(waitForHandleErrorEvent.WaitOne(timeout));
					DbLockout.ResetLockout(adminConnection);
					continueHandleErrorEvent.Set();
					Assert(connectionTask.Wait(timeout));
				}
				finally
				{
					DbLockout.ResetLockout(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestConnectionRecoversIfLockoutReset_LoginsEnabled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				DbLockout.AcquireLockout(adminConnection, disableLogins: false);

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => connection.EnsureIsOpen());
					AssertEquals(ConnectionState.Closed, connection.State);

					DbLockout.ResetLockout(adminConnection);

					AssertNoExceptionThrown(() => connection.EnsureIsOpen());
				}
			}
		}

		[UseSnapshotProtection]
		public void TestConnectionRecoversIfTransactionLockoutRolledBack()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.BeginTransaction();
				DbLockout.AcquireTransactionLockout(adminConnection);

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => connection.EnsureIsOpen());
					AssertEquals(ConnectionState.Closed, connection.State);

					adminConnection.RollbackTransaction();

					AssertNoExceptionThrown(() => connection.EnsureIsOpen());
				}
			}
		}

		public void TestLockout_LoginsDisabled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.BeginTransaction();

				AssertNoLockout(adminConnection);
				AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection, disableLogins: true));
				AssertIsValidLockout(adminConnection, disableLogins: true);

				using (var secondConnection = Db.NewAdminConnection())
				{
					secondConnection.IsUpgradeCheckDisabled = true;
					SetLockTimeout(secondConnection, TimeSpan.Zero);

					// Change lockout SPID registry to a second connection
					DbRegistry.LockoutSpid.SaveValue(secondConnection.SPID, adminConnection);
					DbRegistry.LockoutLoginTime.SaveValue(secondConnection.LoginTime, adminConnection);

					AssertEquals(DbLockoutState.ValidLockout, DbLockout.AcquireLockout(adminConnection));
					AssertEquals(DbLockoutState.ValidLockout, DbLockout.CheckLockoutState(adminConnection));
					AssertEquals(DbLockoutState.ValidLockout, DbLockout.ResetLockout(adminConnection));

					DbRegistry.LockoutSpid.SaveValue(adminConnection.SPID, adminConnection);
					DbRegistry.LockoutLoginTime.SaveValue(adminConnection.LoginTime, adminConnection);
				}

				AssertIsValidLockout(adminConnection, disableLogins: true);
				AssertEquals(DbLockoutState.ResetLockout, DbLockout.ResetLockout(adminConnection));
				AssertNoLockout(adminConnection);
			}
		}

		public void TestLockoutInvalidState()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				SetLockTimeout(adminConnection, TimeSpan.Zero);
				adminConnection.BeginTransaction();

				AssertNoLockout(adminConnection);
				AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade));
				DbRegistry.LockoutSpid.SaveValue(DbRegistry.LockoutSpid.LoadValue(adminConnection) - 1, adminConnection);
				AssertEquals(DbLockoutState.InvalidLockout, DbLockout.CheckLockoutState(adminConnection));
				AssertEquals(DbLockoutState.ResetLockout, DbLockout.ResetLockout(adminConnection));
				AssertNoLockout(adminConnection);
			}
		}

		public void TestLockoutResetWhenUsingOtherDatabase()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				SetLockTimeout(adminConnection, TimeSpan.Zero);
				adminConnection.BeginTransaction();

				AssertNoLockout(adminConnection);
				AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection));
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.SqlMasterDb))
				{
					adminConnection.ResetLockout();
				}
				AssertNoLockout(adminConnection);
			}
		}

		public void TestResetLockoutDropExtendedProperty()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				SetLockTimeout(adminConnection, TimeSpan.Zero);
				adminConnection.BeginTransaction();

				AssertNoLockout(adminConnection);
				AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection));
				Assert("Extended Property has been set with LockoutReason = upgrade by default.", DataUtils.LoadDbExtendedProperty(adminConnection, DbLockout.DbIsLockedOutForProperty) == nameof(LockoutReason.Upgrade));
				DbLockout.ResetLockout(adminConnection);

				AssertEquals(DbLockoutState.AquiredLockout, DbLockout.AcquireLockout(adminConnection, LockoutReason.Purge));
				Assert("Extended Property has been set with LockoutReason = Purge .", DataUtils.LoadDbExtendedProperty(adminConnection, DbLockout.DbIsLockedOutForProperty) == nameof(LockoutReason.Purge));
				DbLockout.ResetLockout(adminConnection);

				AssertExceptionThrown<ArgumentException>(() => DbLockout.AcquireLockout(adminConnection, LockoutReason.None));
				Assert("Extended Property has been dropped.", DataUtils.LoadDbExtendedProperty(adminConnection, DbLockout.DbIsLockedOutForProperty).IsNullOrEmpty());

				AssertNoLockout(adminConnection);
			}
		}

		public void TestAcquireLockoutDisablesApplicationDbLogins()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				SetLockTimeout(adminConnection, TimeSpan.Zero);
				adminConnection.BeginTransaction();

				var loginRepair = (IDbLoginRepair)adminConnection;
				loginRepair.EnableApplicationDbLogins();
				DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade, disableLogins: true);

				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.ReaderDbLoginName, expected: false);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.RestrictedReaderDbLoginName, expected: false);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.RestrictedWriterDbLoginName, expected: false);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.UnrestrictedWriterDbLoginName, expected: false);
			}
		}

		public void TestResetLockoutEnableApplicationDbLogins()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				SetLockTimeout(adminConnection, TimeSpan.Zero);
				adminConnection.BeginTransaction();

				var loginRepair = (IDbLoginRepair)adminConnection;
				loginRepair.EnableApplicationDbLogins();
				DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade, disableLogins: true);
				DbLockout.ResetLockout(adminConnection);

				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.ReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.RestrictedReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, loginRepair.UnrestrictedWriterDbLoginName, expected: true);
			}
		}

		static void AssertNoLockout(AdminConnection adminConnection)
		{
			AssertEquals(DbLockoutState.NoLockout, DbLockout.CheckLockoutState(adminConnection));
			AssertEquals(DbLockoutState.NoLockout, DbLockout.ResetLockout(adminConnection));
			DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, ((IDbLoginRepair)adminConnection).RestrictedWriterDbLoginName, expected: true);
		}

		static void AssertIsValidLockout(AdminConnection adminConnection, bool disableLogins)
		{
			AssertEquals(DbLockoutState.ValidLockout, DbLockout.CheckLockoutState(adminConnection));
			DatabaseLoginTest.AssertLoginIsEnabled(adminConnection, ((IDbLoginRepair)adminConnection).RestrictedWriterDbLoginName, expected: !disableLogins);
			Assert(DbLockout.IsLockoutOwner(adminConnection));
		}

		#region Helpers

		static void SetLockTimeout(DbConnection connection, TimeSpan timeSpan)
		{
			connection.ExecuteNonQuery("SET LOCK_TIMEOUT " + ((int)timeSpan.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Used only by <see cref="TestConnectionRecoversIfLockoutReset_LoginsDisabled"/>
		/// </summary>
		internal class DbConnectionWithDelayedHandleError : DbConnection<RestrictedWriterLoginCredentials>
		{
			public DbConnectionWithDelayedHandleError(AutoResetEvent beginHandleError, AutoResetEvent continueHandleError, TimeSpan continueHandleErrorTimeout)
				: base()
			{
				this.beginHandleError = beginHandleError;
				this.continueHandleError = continueHandleError;
				this.continueHandleErrorTimeout = continueHandleErrorTimeout;
			}

			public override bool HandleError(System.Data.Common.DbException e)
			{
				beginHandleError.Set();
				continueHandleError.WaitOne(continueHandleErrorTimeout);
				return base.HandleError(e);
			}

			readonly AutoResetEvent beginHandleError;
			readonly AutoResetEvent continueHandleError;
			readonly TimeSpan continueHandleErrorTimeout;

			public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);
		}

		void TakeSchemaModificationLockOnTable(AdminConnection connection, string schemaName, string tableName)
		{
			var sql = @"EXEC sys.sp_addextendedproperty @PropertyName, 1, 'SCHEMA', @SchemaName, 'TABLE', @TableName;
EXEC sys.sp_dropextendedproperty @PropertyName, 'SCHEMA', @SchemaName, 'TABLE', @TableName;";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PropertyName", System.Data.SqlDbType.NVarChar, 128, "TempForSchemaLock");
				cmd.AddParameter("@SchemaName", System.Data.SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@TableName", System.Data.SqlDbType.NVarChar, 128, tableName);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
