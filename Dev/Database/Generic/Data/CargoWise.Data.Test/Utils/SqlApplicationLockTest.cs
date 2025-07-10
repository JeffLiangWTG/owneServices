using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data.Testing;
using Moq;
using NUnit.Framework;
using static CargoWise.Data.Testing.DbConnectionTest;

namespace CargoWise.Data.Utils.Testing
{
	sealed class SqlApplicationLockTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDatabaseUpgradedExceptionDuringDispose_LoginsDisabled()
			=> AssertDatabaseUpgradedExceptionDuringDispose(disableLogins: true);

		[UseSnapshotProtection]
		public void TestDatabaseUpgradedExceptionDuringDispose_LoginsNotDisabled()
			=> AssertDatabaseUpgradedExceptionDuringDispose(disableLogins: false);

		void AssertDatabaseUpgradedExceptionDuringDispose(bool disableLogins)
		{
			// Arrange
			using (var externConnection = Db.NewExtraConnectionToMainDb())
			using (var result = new SqlApplicationLock("Test", externConnection, externConnection.CurrentDatabase))
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout(disableLogins: disableLogins));
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					try
					{
						AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => externConnection.BeginTransaction());
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}

					try
					{
						try
						{
							externConnection.EnsureIsOpen();
							throw new AssertionFailedError("An Exception should have been thrown before we get here");
						}
						// Assert
						catch (DatabaseUpgradedException)
						{
						}
						finally
						{
							// Act
							result.Dispose();
						}
					}
					finally
					{
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) - 1, adminConnection);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDisposeLockMultipleTimes()
		{
			var initialDbEnv = DbEnv.Instance;
			try
			{
				var mockGuiPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>() { CallBase = true };
				mockDbEnv.Setup(x => x.ConnectionGuiPlugin).Returns(mockGuiPlugin.Object);
				mockGuiPlugin.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>())).Verifiable();
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				var mainSpid = Db.Connection.SPID;
				int resultCode = 0;

				var task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var spid = Db.Connection.SPID;
						AssertNotEquals(mainSpid, spid);

						resultCode = RunInternal(spid);

						Db.Connection.EnsureIsOpen();
					}
				});

				Task.WaitAll(task);

				AssertEquals(-3, resultCode);

				mockGuiPlugin.Verify(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once());
				AssertEquals("DatabaseUpgradeException has been thrown multiple times.\r\nCheck the call stack for any exception handling that swallows a DatabaseUpgradeException.", ErrorReporter.LastExceptionReported.Message);
			}
			finally
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection) - 1, Db.Connection);
				ErrorReporter.Clear();
				DbEnv.SetDbEnvironment(initialDbEnv);
			}
		}

		int RunInternal(int spid)
		{
			try
			{
				if (Db.Connection.TryGetLock("Upgrade lock", out var sqlLock))
				{
					try
					{
						RunServiceTask(spid);
					}
					finally
					{
						sqlLock.Dispose();
					}
				}
			}
			catch (DatabaseUpgradeException)
			{
				return -3;
			}
			catch (SqlLockLostException)
			{
				if (!Db.Connection.DatabaseUpgradedExceptionHasBeenThrown)
				{
					return -2;
				}
				else
				{
					return -200;
				}
			}

			return 0;
		}

		void RunServiceTask(int spid)
		{
			using (var upgConnection = Db.NewAdminConnection())
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(upgConnection) + 1, upgConnection);
				upgConnection.ExecuteNonQuery($"KILL {spid}");
			}
		}

		public void TestDatabaseUpgradedExceptionBeforeDispose()
		{
			using (var conn = new UpgradedDbConnectionForTest(Db.ServerName, Db.DatabaseName))
			{
				conn.shouldSayDbSchemaHasChanged = false;

				SqlApplicationLock appLock;
				Assert(conn.TryGetLock("TestDatabaseUpgradedExceptionBeforeDispose", out appLock));

				var mockGuiPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>() { CallBase = true };
				mockDbEnv.Setup(x => x.ConnectionGuiPlugin).Returns(mockGuiPlugin.Object);
				mockGuiPlugin.Setup(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>())).Verifiable();

				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				conn.shouldSayDbSchemaHasChanged = true;
				AdoTestUtils.KillConnection(conn);

				using (conn.SetDatabaseUpgradedExceptionHasBeenThrown_ForTest())
				{
					appLock.Dispose();
					mockGuiPlugin.Verify(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Never());
				}
			}
		}

		public void TestDisposeReleasesLock()
		{
			SqlApplicationLock @lock;
			Assert("Precondition: Lock is acquired", Db.Connection.TryGetLock("TestDisposeReleasesLock_LockKey", out @lock));

			using (@lock)
			{
				Assert("Precondition: Lock must be acquired first", @lock.IsHoldingLock());

				@lock.Dispose();

				using (var command = Db.Connection.Command("SELECT APPLOCK_MODE('public', @Key, @LockOwner)"))
				{
					command.AddParameter("@Key", SqlDbType.NVarChar, 255, "TestDisposeReleasesLock_LockKey");
					command.AddParameter("@LockOwner", SqlDbType.VarChar, 32, SqlApplicationLock.LockOwner);
					Assert("Lock should no longer exist", command.ExecuteScalar().ToString() == "NoLock");
				}
			}
		}

		#region Existing Lock Session Information Tests

		public void TestLockOnTwoDatabases()
		{
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				const string Key = "WowItWorks";
				using (var lock1 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, Key, TimeSpan.Zero))
				using (var lock2 = SqlApplicationLock.Acquire(connection2, Db.DatabaseName + "_SD001", Key, TimeSpan.Zero))
				{
					Assert(lock1.IsHoldingLock());
					Assert(lock2.IsHoldingLock());
				}
			}
		}

		#endregion

		#region Acquire Lock Tests

		public void TestAcquireTwiceInSameConnection()
		{
			const string testKey = "~TestAcquireTwiceInSameConnection~";

			using (var mutex1 = SqlApplicationLock.Acquire(Db.Connection, Db.Connection.CurrentDatabase, testKey, TimeSpan.Zero))
			using (var mutex2 = SqlApplicationLock.Acquire(Db.Connection, Db.Connection.CurrentDatabase, testKey, TimeSpan.Zero))
			{
				Assert(mutex1.IsHoldingLock());
				Assert(mutex2.IsHoldingLock());
			}
		}

		public void TestMutexOnSecondConnection()
		{
			const string testKey = "~TestMutexOnSecondConnection~";

			using (var mutex1 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero))
			{
				Assert(mutex1.IsHoldingLock());
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					var mutex2 = SqlApplicationLock.Acquire(connection2, Db.DatabaseName, testKey, TimeSpan.Zero);
					AssertNull(mutex2);

					mutex1.Dispose();

					using (var mutex3 = SqlApplicationLock.Acquire(connection2, Db.DatabaseName, testKey, TimeSpan.Zero))
					{
						Assert(mutex3.IsHoldingLock());
					}
				}
			}
		}

		public void TestAcquireLockUsesPassedLockMode()
		{
			const string testKey = "~TestMutexOnSecondConnectionSharedLock~";

			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				using var mutex1 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero, SqlApplicationLockMode.Shared);
				using var mutex2 = SqlApplicationLock.Acquire(connection2, Db.DatabaseName, testKey, TimeSpan.Zero, SqlApplicationLockMode.Shared);

				Assert(mutex1.IsHoldingLock());
				Assert(mutex2.IsHoldingLock());
			}
		}

		public void TestIsHoldingLockChecksStrongerLocks()
		{
			const string testKey = "~AppLockUnionLockKey~";

			using var lock1 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero, SqlApplicationLockMode.Shared);

			AssertEquals(true, lock1.IsHoldingLock());

			using var lock2 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero, SqlApplicationLockMode.Exclusive);

			AssertEquals(true, lock2.IsHoldingLock());
			AssertEquals(true, lock1.IsHoldingLock());
		}

		public void TestMutexReleasedOnConnectionDrop()
		{
			const string testKey = "~TestMutexReleasedOnConnectionDrop~";

			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var mutex1 = SqlApplicationLock.Acquire(connection, Db.DatabaseName, testKey, TimeSpan.Zero))
			{
				Assert(mutex1.IsHoldingLock());
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery("kill " + connection.SPID);
				}
				Assert(!mutex1.IsHoldingLock());

				using (var mutex2 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero))
				{
					Assert(mutex2.IsHoldingLock());
					Assert(!mutex1.IsHoldingLock());
				}
			}
		}

		public void TestMutexReleasedOnConnectionClose()
		{
			const string testKey = "~TestMutexReleasedOnConnectionClose~";

			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var mutex1 = SqlApplicationLock.Acquire(connection, Db.DatabaseName, testKey, TimeSpan.Zero))
			{
				Assert(mutex1.IsHoldingLock());
				connection.CloseConnection();
				Assert(!mutex1.IsHoldingLock());

				using (var mutex2 = SqlApplicationLock.Acquire(Db.Connection, Db.DatabaseName, testKey, TimeSpan.Zero))
				{
					Assert(mutex2.IsHoldingLock());
				}
			}
		}

		public void TestLockReleasedProperlyWhenCurrentDatabaseChanges()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var mutex1 = SqlApplicationLock.Acquire(connection, Db.DatabaseName, "~TestLockReleasedProperlyWhenCurrentDatabaseChanges~", TimeSpan.Zero))
			{
				Assert(mutex1.IsHoldingLock());

				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					mutex1.Dispose();
				}

				Assert("Lock should have been released but it wasn't.", !mutex1.IsHoldingLock());
			}
		}

		public void TestIsHoldingLockWhenCurrentDatabaseChanges()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var mutex1 = SqlApplicationLock.Acquire(connection, Db.DatabaseName, "~TestIsHoldingLockWhenCurrentDatabaseChanges~", TimeSpan.Zero))
			{
				AssertEquals("IsHoldingLock result when current database is the same", true, mutex1.IsHoldingLock());

				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					AssertEquals("IsHoldingLock result when current database has changed since acquiring it", true, mutex1.IsHoldingLock());
				}
			}
		}

		public void TestAcquireDoesNotThrowWhenLockTimeoutLongerThanCommandDefault()
		{
			var testMutexName = "~TestAcquireLockTimeoutDoesNotThrowWhenTimeoutLongerThanCommandDefault~";
			using (Db.Connection.TemporarySetDefaultCommandTimeOut(1))
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var mutex1 = SqlApplicationLock.Acquire(connection1, Db.DatabaseName, testMutexName, TimeSpan.Zero))
			{
				Assert("Lock not held in test setup", mutex1.IsHoldingLock());
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					AssertNull("Acquire did not return null on timeout", SqlApplicationLock.Acquire(connection2, Db.DatabaseName, testMutexName, TimeSpan.FromSeconds(3)));
				}
			}
		}

		#endregion

		#region Strategy

		public void TestDifferentLockStrategy()
		{
			var strategy = new ComboSqlApplicationLockStrategyForTest(Db.Connection, Db.DatabaseName);
			var key = Guid.NewGuid().ToString();
			using (var appLock = new SqlApplicationLock(key, Db.Connection, Db.DatabaseName, strategy))
			{
				Assert(appLock.IsHoldingLock());
				appLock.Dispose();
				Assert(!appLock.IsHoldingLock());
			}
		}

		class ComboSqlApplicationLockStrategyForTest : ISqlApplicationLockStrategy
		{
			public ComboSqlApplicationLockStrategyForTest(DbConnection connection, string lockDatabase)
			{
				appLocks = GetAppLocks(connection, lockDatabase, new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }).ToArray();
			}

			public bool IsHoldingLock(DbConnection connection, string lockDatabase, string key, string lockOwner, SqlApplicationLockMode lockMode)
			{
				return appLocks != null && appLocks.Any();
			}

			public void Cleanup(DbConnection connection, string lockDatabase, string key, string lockOwner)
			{
				ReleaseAppLocks(connection, lockDatabase);
			}

			IEnumerable<string> GetAppLocks(DbConnection connection, string dbName, IEnumerable<string> keys)
			{
				var cmdText = string.Format(CultureInfo.InvariantCulture, "[{0}]..sp_getapplock", dbName);

				using (var cmd = connection.Command(cmdText))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, string.Empty);
					cmd.AddParameter("@LockMode", SqlDbType.VarChar, 32, "Exclusive");
					cmd.AddParameter("@LockOwner", SqlDbType.VarChar, 32, "Session");
					cmd.AddParameter("@LockTimeout", SqlDbType.Int, 5000);
					cmd.Prepare();
					foreach (var key in keys)
					{
						cmd.GetParameter("@Resource").Value = key;
						if (cmd.ExecuteProcedureWithReturnValue() >= 0)
						{
							yield return key;
						}
					}
				}
			}

			void ReleaseAppLocks(DbConnection connection, string dbName)
			{
				var cmdText = string.Format(CultureInfo.InvariantCulture, "[{0}]..sp_releaseAppLock", dbName);

				using (var cmd = connection.Command(cmdText))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, string.Empty);
					cmd.AddParameter("@LockOwner", SqlDbType.VarChar, 32, "Session");
					cmd.Prepare();
					var releasedLocks = new List<string>();
					foreach (var key in appLocks)
					{
						cmd.GetParameter("@Resource").Value = key;
						cmd.ExecuteNonQuery();
						releasedLocks.Add(key);
					}
					appLocks = appLocks.Except(releasedLocks);
				}
			}

			IEnumerable<string> appLocks;
		}

		#endregion
	}
}
