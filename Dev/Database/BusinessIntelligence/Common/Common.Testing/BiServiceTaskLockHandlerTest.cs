#region Test
#if DEBUG

namespace CargoWise.Bi.Common.Testing
{
	using System;
	using System.Globalization;
	using CargoWise.Data;
	using CargoWise.Data.Utils;
	using NUnit.Framework;

	class BiServiceTaskLockHandlerTest : TestCase
	{
		public void TestAcquireLock()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var lockHandler1 = new BiServiceTaskLockHandlerForTest(connection1);
				using (var lockOnConnection1DisposableAction = lockHandler1.AcquireLock(Db.AuditDatabaseSuffix, TimeSpan.Zero))
				{
					AssertNotNull(lockOnConnection1DisposableAction);

					string expectedErrorMessage = string.Format(CultureInfo.InvariantCulture,
						"Could not acquire BI service task lock. It is currently held by another session.",
						Environment.MachineName,
						DbConnectionConstants.ApplicationNames.CargoWiseOne,
						((IDbReconnectionHandling)connection1).LoginName,
						SqlFormatInfo.ToSqlDateTimeString(connection1.LoginTime)
					);

					var lockHandler2 = new BiServiceTaskLockHandlerForTest(connection2);
					AssertExceptionThrown(
						"Attempting to acquire lock already held by another session...",
						typeof(BiServiceLockException),
						expectedErrorMessage,
						() => lockHandler2.AcquireLock(Db.AuditDatabaseSuffix, TimeSpan.Zero)
					);
				}
			}
		}
		public void TestTryGetLock()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var lockHandler1 = new BiServiceTaskLockHandlerForTest(connection1);
				bool isLock1Acquired, isLock2Acquired;

				using (var lockOnConnection1DisposableAction = lockHandler1.TryGetLock(Db.AuditDatabaseSuffix, TimeSpan.Zero, out isLock1Acquired))
				{
					AssertEquals("Is Lock1 acquired?", true, isLock1Acquired);
					AssertNotNull(lockOnConnection1DisposableAction);

					var lockHandler2 = new BiServiceTaskLockHandlerForTest(connection2);
					using (var lockOnConnection2DisposableAction = lockHandler2.TryGetLock(Db.AuditDatabaseSuffix, TimeSpan.Zero, out isLock2Acquired))
					{
						AssertEquals("Is Lock2 acquired?", false, isLock2Acquired);
						AssertNull(lockOnConnection2DisposableAction);
					}
				}
			}
		}

		public void TestDisconnectingEtlConnectionThrowsExceptionToPreventContinuingWithoutAnEtlLock()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var lockHandler1 = new BiServiceTaskLockHandlerForTest(connection);
				using (lockHandler1.AcquireLock(Db.AuditDatabaseSuffix, TimeSpan.Zero))
				{
					AdoTestUtils.KillConnection(connection);

					AssertExceptionThrown(
						"Attempting to re-open connection with a lost lock (undisposed on client side but gone with the server side disconnection)...",
						typeof(SqlLockLostException),
						"A db reconnect was attempted while undisposed SqlLocks existed",
						() => connection.EnsureIsOpen(), true
					);

					// Lost Lock is up for grabs
					using (var anotherConnection = Db.NewExtraConnectionToMainDb())
					{
						var lockHandler2 = new BiServiceTaskLockHandlerForTest(anotherConnection);
						AssertNoExceptionThrown(
							"Acquiring lost lock in another connection",
							() => { using (lockHandler2.AcquireLock(Db.AuditDatabaseSuffix, TimeSpan.Zero)) { } }
						);
					}
				}
			}
		}

		public void TestLockKey()
		{
			// In order to obtain information about a potential existing lock, we need to query metadata for the lock key.
			// SQL Server documentation:
			// "After an application lock has been acquired, only the first 32 characters can be retrieved in plain text; the remainder will be hashed."
			Assert(
				"LockKey length should be <= 32 but it is " + BiServiceTaskLockHandler.LockKey.Length.ToString(),
				BiServiceTaskLockHandler.LockKey.Length <= 32);

			AssertEquals(
				"LockKey (if changed, must be cut back to all rings)",
				"BI-SERVICE-TASK-LOCK",
				BiServiceTaskLockHandler.LockKey);
		}

		class BiServiceTaskLockHandlerForTest : BiServiceTaskLockHandler
		{
			public BiServiceTaskLockHandlerForTest(DbConnection mainDbConnection)
			{
				this.mainDbConnection = mainDbConnection;
			}
			readonly DbConnection mainDbConnection;

			protected override DbConnection MainDbConnection => mainDbConnection;
		}
	}
}

#endif
#endregion
