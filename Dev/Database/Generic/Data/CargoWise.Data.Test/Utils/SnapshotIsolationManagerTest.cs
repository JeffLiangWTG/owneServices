using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class SnapshotIsolationManagerTest : TestCase
	{
		public void TestSetSnapshotIsolationForDatabase()
		{
			string testDb = "EnableSnapshotIsolationTestDb";
			var manager = new SnapshotIsolationManagerForTest();

			try
			{
				RecreateTestDbs(testDb);
				AssertEquals("[PRE-CONDITION] Snapshot Isolation enabled?", false, Db.Connection.IsDbUsingSnapshotIsolation(testDb));

				using (var userConn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, testDb))
				{
					bool hasConnectionStateChanged;
					((IDbConnectionInternals)userConn).ADOConnection.StateChange +=
						new StateChangeEventHandler((object sender, StateChangeEventArgs e) => hasConnectionStateChanged = true);

					userConn.EnsureIsOpen();

					// Enable
					hasConnectionStateChanged = false;
					manager.EnableSnapshotIsolationForDatabase(Db.Connection, testDb);
					AssertEquals("Snapshot Isolation enabled?", true, Db.Connection.IsDbUsingSnapshotIsolation(testDb));
					userConn.EnsureIsOpen();
					AssertEquals("Was other user session disconnected?", true, hasConnectionStateChanged);

					// Call again with option to check if already enabled. Should skip as already enabled. Other connection won;t be closed.
					hasConnectionStateChanged = false;
					manager.EnableSnapshotIsolationForDatabase(Db.Connection, testDb, checkAlreadyEnabled: true);
					AssertEquals("Snapshot Isolation enabled?", true, Db.Connection.IsDbUsingSnapshotIsolation(testDb));
					userConn.EnsureIsOpen();
					AssertEquals("Was other user session disconnected?", false, hasConnectionStateChanged);

					// Disable
					hasConnectionStateChanged = false;
					manager.DisableSnapshotIsolationForDatabase(Db.Connection, testDb);
					AssertEquals("Snapshot Isolation enabled?", false, Db.Connection.IsDbUsingSnapshotIsolation(testDb));
					userConn.EnsureIsOpen();
					AssertEquals("Was other user session disconnected?", true, hasConnectionStateChanged);
				}
			}
			finally
			{
				DropTestDbs(testDb);
			}
		}

		public void TestSetSnapshotIsolationForDatabase_WithRetry()
		{
			string testDb = "EnableSnapshotIsolationTestDb";

			using (AdoTestUtils.CreateDbDropExistingDisposable(testDb, Db.DatabaseName))
			using (Db.Connection.PrepareRetryContextForTest(
				condition: (sqlText, executionCount) => sqlText.Contains(" ALLOW_SNAPSHOT_ISOLATION ") && sqlText.Contains(" READ_COMMITTED_SNAPSHOT "),
				action: (sqlText, executionCount) =>
				{
					if (executionCount < 2)
					{
						throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
					}
				}))
			{
				Db.Connection.ExecuteNonQuery($@"
					ALTER DATABASE [{testDb}]
						SET ALLOW_SNAPSHOT_ISOLATION OFF;
					ALTER DATABASE [{testDb}]
						SET READ_COMMITTED_SNAPSHOT OFF WITH ROLLBACK IMMEDIATE;");

				var manager = new SnapshotIsolationManagerForTest();
				manager.EnableSnapshotIsolationForDatabase(Db.Connection, testDb);

				AssertEquals(2, Db.Connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		void RecreateTestDbs(params string[] dbNames)
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				foreach (string dbName in dbNames)
				{
					AdoTestUtils.CreateDbDropExisting(adminConnection, dbName, Db.DatabaseName);
				}
			}
		}

		void DropTestDbs(params string[] dbNames)
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				foreach (string dbName in dbNames)
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName, Db.DatabaseName);
				}
			}
		}

		class SnapshotIsolationManagerForTest : SnapshotIsolationManager
		{
			public void DisableSnapshotIsolationForDatabase(DbConnection conn, string dbName)
			{
				SetSnapshotIsolationForDatabase(conn, dbName, enable: false);
			}
		}
	}
}
