using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	public class SemaphoreDbManagerThatAlwaysRefreshes : SemaphoreDbManager
	{
		protected override bool IsItTimeToRefreshHeatbeat(TimeSpan hearbeatDuration) { return true; }
	}

	public class SemaphoreDbManagerWithDummyHeartbeat : SemaphoreDbManager
	{
		public int ExecuteHeartbeatCount { get; set; }

		protected override bool ExecuteHeartbeatRefresh(Guid heartbeatUniqueId, TimeSpan hearbeatDuration, DbConnection connection, bool isNewConnection)
		{
			++ExecuteHeartbeatCount;
			return true;
		}

		public override bool CheckHeartbeatExpiredTimeIsExpired(Guid heartbeatUniqueId)
		{
			return false;
		}
	}

	public class TestSemaphoreDBManagerWithoutTransaction : TestCase
	{
		/// <summary>
		/// If either the Schema or Script versions has changed it should not refresh the heartbeat
		/// </summary>
		[UseSnapshotProtection]
		public void TestDatabaseUpgradedExceptionIsNotSwollen()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var dbManager = new SemaphoreDbManagerThatAlwaysRefreshes();
				var testPk = Guid.NewGuid();
				var userPk = Guid.NewGuid();

				dbManager.CreateHeartbeatInDatabase(testPk, "testhost", -78, userPk, GlbStaffSchema.Constants.Prefix, 100, HeartbeatTypes.Enterprise, null);
				var heartbeatRow1 = GetHeartbeatRow(testPk);
				var expiresAtUtc1 = (DateTime)heartbeatRow1["SV_ExpiresAtUtc"];

				AssertNoExceptionThrown(() =>
				{
					dbManager.RefreshHeartbeatInDatabase(testPk, TimeSpan.FromSeconds(10000), isNewConnection: true);
				});

				var heartbeatRow2 = GetHeartbeatRow(testPk);
				var expiresAtUtc2 = (DateTime)heartbeatRow2["SV_ExpiresAtUtc"];

				AssertEquals("Expiry Time was refreshed", true, expiresAtUtc2 >= expiresAtUtc1.AddSeconds(4900));

				var sqlText = "UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(8000), N'-1') WHERE SD_Name = 'DATABASE_MINOR_SCHEMA_VERSION';";
				Db.Connection.ExecuteNonQuery(sqlText);

				AssertExceptionThrown<DatabaseUpgradedException>(() =>
				{
					dbManager.RefreshHeartbeatInDatabase(testPk, TimeSpan.FromSeconds(10000), isNewConnection: true);
				});
			}
		}

		DataRow GetHeartbeatRow(Guid heartbeatPk)
		{
			var sqlText = String.Format("SELECT * FROM dbo.StmServiceHeartBeat WITH (READCOMMITTED) WHERE SV_PK = '{0}'", heartbeatPk.ToString());
			var table = DataUtils.GetDataTableFromQuery(Db.Connection, sqlText);
			var result = (table.Rows.Count == 1) ? table.Rows[0] : null;
			return result;
		}
	}

	public class TestSemaphoreDbManager : TransactionedTestCase
	{
		public void TestCreateHeartbeatInDatabase()
		{
			var testStartUtc = (DateTime)TestConnection.ExecuteScalar("SELECT getutcdate()");

			var dbManager = new SemaphoreDbManager();
			var testPk = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			var clientIdentifier = Guid.NewGuid().ToString();

			AssertNull("Heartbeat Row (before creating)", GetHeartbeatRow(testPk));

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.CreateHeartbeatInDatabase(testPk, "testhost", -3213, userPk, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientIdentifier);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 2, commandsAfter - commandsBefore);

			var heartbeatRow = GetHeartbeatRow(testPk);
			AssertEquals("SV_ParentTableCode", "GS", heartbeatRow["SV_ParentTableCode"].ToString());
			AssertEquals("SV_ParentId", userPk, heartbeatRow["SV_ParentId"]);
			AssertEquals("SV_ExpiresAtUtc > intialUtc", true, (DateTime)heartbeatRow["SV_ExpiresAtUtc"] > testStartUtc);
			AssertEquals("SV_WorkstationName", "testhost", heartbeatRow["SV_WorkstationName"].ToString());
			AssertEquals("SV_ProcessID", -3213, (int)heartbeatRow["SV_ProcessID"]);
			AssertEquals("SV_HeartbeatType", HeartbeatTypes.Enterprise, heartbeatRow["SV_HeartbeatType"].ToString());
			AssertEquals("SV_ClientIdentifier", clientIdentifier, heartbeatRow["SV_ClientIdentifier"].ToString());
		}

		public void TestCreateHeartbeatInDatabase_TableCode()
		{
			var dbManager = new SemaphoreDbManager();
			var testPk = Guid.NewGuid();
			var userPk = Guid.NewGuid();

			AssertNull("Heartbeat Row (before creating)", GetHeartbeatRow(testPk));

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.CreateHeartbeatInDatabase(testPk, "testhost", -3213, userPk, OrgContactSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 2, commandsAfter - commandsBefore);

			var heartbeatRow = GetHeartbeatRow(testPk);
			AssertEquals("SV_ParentTableCode", OrgContactSchema.Constants.Prefix, heartbeatRow["SV_ParentTableCode"].ToString());
		}

		public class TestSemaphoreDbManagerForDBUpgrade : SemaphoreDbManager
		{
			public override bool RefreshHeartbeatInDatabase(Guid heartbeatUniqueId, TimeSpan heartbeatDuration, bool isNewConnection)
			{
				if (heartbeatUniqueId == Guid.Empty)
				{
					throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
				}

				throw new DatabaseUpgradedException();
			}
		}

		public void TestRefreshHeartbeatInDatabase()
		{
			var dbManager = new SemaphoreDbManagerThatAlwaysRefreshes();
			var testPk = Guid.NewGuid();
			var userPk = Guid.NewGuid();

			dbManager.CreateHeartbeatInDatabase(testPk, "testhost", -78, userPk, GlbStaffSchema.Constants.Prefix, 100, HeartbeatTypes.Enterprise, null);

			var heartbeatRow1 = GetHeartbeatRow(testPk);
			var initialExpiresAtUtc = (DateTime)heartbeatRow1["SV_ExpiresAtUtc"];

			var hearbeatDuration = TimeSpan.FromSeconds(1000);
			var commandsBefore = TestConnection.ExecutedCommandCount;
			Thread.Sleep(500);
			dbManager.RefreshHeartbeatInDatabase(testPk, hearbeatDuration, false);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			var heartbeatRow2 = GetHeartbeatRow(testPk);
			var newExpiresAtUtc = (DateTime)heartbeatRow2["SV_ExpiresAtUtc"];

			AssertEquals("Expires At Utc was refreshed", true, newExpiresAtUtc >= initialExpiresAtUtc.AddSeconds(900));
		}

		public void TestDeleteHeartbeatFromDatabase()
		{
			var dbManager = new SemaphoreDbManager();
			var testPk = Guid.NewGuid();
			var userPk = Guid.NewGuid();

			dbManager.CreateHeartbeatInDatabase(testPk, "", 0, userPk, GlbStaffSchema.Constants.Prefix, 0, HeartbeatTypes.Enterprise, null);
			AssertNotNull("Heartbeat Row (after creating)", GetHeartbeatRow(testPk));

			TestConnection.ExecuteNonQuery(String.Format("INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV) VALUES (NEWID(), '{0}')", testPk.ToString()));
			dbManager.DeleteHeartbeatFromDatabase(testPk);
			AssertNull("Heartbeat Row (after deleting)", GetHeartbeatRow(testPk));
		}

		#region TestCreateHeartbeatInDatabase

		#region TestCreateHeartbeatInDatabase_CleanUpOldHeartbeats

		public void TestCreateHeartbeatInDatabase_CleanUpOldHeartbeats()
		{
			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();
			var user3 = Guid.NewGuid();
			var user4 = Guid.NewGuid();

			var expiredHeartbeatSameUserPkAndHeartbeatType1 = InsertHeartbeat("ThisPc", 1, user1, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatSameUserPkAndHeartbeatType2 = InsertHeartbeat("ThisPc", 2, user1, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);

			var sameProcessHeartbeatPk = InsertHeartbeat("ThisPc", 3, user2, TimeSpan.FromMinutes(-1), HeartbeatTypes.Enterprise);

			var expiredHeartbeatDifferentUserPk1 = InsertHeartbeat("AnotherPc1", 4, user3, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatDifferentUserPk2 = InsertHeartbeat("AnotherPc2", 5, user4, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);

			var dbManager = new SemaphoreDbManager();

			var newHeartbeatPk = Guid.NewGuid();
			dbManager.CreateHeartbeatInDatabase(newHeartbeatPk, "ThisPc", 3, user1, GlbStaffSchema.Constants.Prefix, 100, HeartbeatTypes.Enterprise, null);
			AssertNotNull("New Heartbeat Row (after creating)", GetHeartbeatRow(newHeartbeatPk));

			// Same host+processID (like a process controller separate app domain) => Should NOT be deleted.
			AssertNotNull("Same Process Heartbeat Row (after clean up)", GetHeartbeatRow(sameProcessHeartbeatPk));

			// Same user, less than MaxDaysToKeepSameUserExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("Recent Same User Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPkAndHeartbeatType1));

			// Same user, older than MaxDaysToKeepSameUserExpiredHeartbeat days => should be DELETED
			AssertNull("Old Same User Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPkAndHeartbeatType2));

			// Different user, less than MaxDaysToKeepExpiredHeartbea days old => should NOT be deleted
			AssertNotNull("Recent Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatDifferentUserPk1));

			// Different user, older than MaxDaysToKeepExpiredHeartbeat days => should be DELETED
			AssertNull("Old Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatDifferentUserPk2));
		}

		#endregion

		#region TestCreateHeartbeatInDatabase_CleanUpSameUserAndHostOldHeartbeats_HeartbeatType

		public void TestCreateHeartbeatInDatabase_CleanUpSameUserAndHostOldHeartbeats_HeartbeatType()
		{
			var user = Guid.NewGuid();
			var expiredHeartbeatPK_SameHeartbeatType1 = InsertHeartbeat("ThisPc", 1, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatPK_SameHeartbeatType2 = InsertHeartbeat("ThisPc", 2, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatPK_DifferentHeartbeatType1 = InsertHeartbeat("ThisPc", 3, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.WarehouseRF);
			var expiredHeartbeatPK_DifferentHeartbeatType2 = InsertHeartbeat("ThisPc", 4, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.WarehouseRF);

			var dbManager = new SemaphoreDbManager();
			var newHeartbeatPk = Guid.NewGuid();
			dbManager.CreateHeartbeatInDatabase(newHeartbeatPk, "ThisPc", 5, user, GlbStaffSchema.Constants.Prefix, 0, HeartbeatTypes.Enterprise, null);
			AssertNotNull("New Heartbeat should be Created.", GetHeartbeatRow(newHeartbeatPk));
			AssertNotNull("Expired Hearbeat of same type less than 3 days old should not be deleted.", GetHeartbeatRow(expiredHeartbeatPK_SameHeartbeatType1));
			AssertNull("Expired Hearbeat of same type older than 3 days should be deleted.", GetHeartbeatRow(expiredHeartbeatPK_SameHeartbeatType2));
			AssertNotNull("Expired Hearbeat of different type less than 15 days old should not be deleted.", GetHeartbeatRow(expiredHeartbeatPK_DifferentHeartbeatType1));
			AssertNull("Expired Hearbeat of different type older than 15 days should be deleted.", GetHeartbeatRow(expiredHeartbeatPK_DifferentHeartbeatType2));
		}

		#endregion

		#endregion

		#region TestUserPkInDatabase

		public void TestUserPkInDatabase()
		{
			var guid = new Guid("217369CF-A3B8-42D1-BC6F-AA4A6437D939");
			var otherGuid = new Guid("f57c146e-7b38-4eff-855d-aa3b8373b2c9");
			Db.Connection.ExecuteNonQuery(@"insert into dbo.StmServiceHeartBeat(SV_PK, SV_ExpiresAtUtc, SV_WorkstationName, SV_ProcessID, SV_HeartbeatType, SV_ParentID, SV_PArentTableCode, SV_CreateTimeUTC)
values (newid(), DateAdd(DAY, -1, GetUTCDate()), 'Hello', 123, 'ENT', '217369CF-A3B8-42D1-BC6F-AA4A6437D939', 'GS', DateAdd(DAY, -1, GetUTCDate()))");
			AssertEquals(true, new SemaphoreDbManager().UserPkInDatabase(guid));
			AssertEquals(false, new SemaphoreDbManager().UserPkInDatabase(otherGuid));
		}

		#endregion

		#region TestUpdateUserContext_CleanUpSameUserAndHostOldHeartbeats

		public void TestUpdateUserContext_CleanUpSameUserAndHostOldHeartbeats()
		{
			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();
			var user3 = Guid.NewGuid();
			var user4 = Guid.NewGuid();
			var expiredHeartbeatSameUserPk1 = InsertHeartbeat("ThisPc", 1, user1, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatSameUserPk2 = InsertHeartbeat("ThisPc", 2, user1, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var sameProcessHeartbeatPk = InsertHeartbeat("ThisPc", 3, user2, TimeSpan.FromMinutes(-1), HeartbeatTypes.Enterprise);
			var expiredHeartbeatDifferentUserPk1 = InsertHeartbeat("AnotherPc1", 4, user3, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatDifferentUserPk2 = InsertHeartbeat("AnotherPc2", 5, user4, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);

			var dbManager = new SemaphoreDbManager();

			var newHeartbeatPk = Guid.NewGuid();
			dbManager.CreateHeartbeatInDatabase(newHeartbeatPk, "ThisPc", 3, Guid.Empty, GlbStaffSchema.Constants.Prefix, 0, HeartbeatTypes.Enterprise, null);
			AssertNotNull("New Heartbeat Row (after creating)", GetHeartbeatRow(newHeartbeatPk));

			// Same host+processID (like a process controller separate app domain) => Should NOT be deleted.
			AssertNotNull("Same Process Heartbeat Row (after clean up)", GetHeartbeatRow(sameProcessHeartbeatPk));

			// Different user, less than MaxDaysToKeepExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("Recent Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPk1));

			// Different user, less than MaxDaysToKeepExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("Recent Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPk2));

			// Different user, less than MaxDaysToKeepExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("Recent Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatDifferentUserPk1));

			// Different user, older than MaxDaysToKeepExpiredHeartbeat days => should be DELETED
			AssertNull("Old Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatDifferentUserPk2));

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.UpdateUserContext(newHeartbeatPk, "ThisPc", user1, GlbStaffSchema.Constants.Prefix, HeartbeatTypes.Enterprise);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			AssertNotNull("New Heartbeat Row (after updating context)", GetHeartbeatRow(newHeartbeatPk));

			// Same PC and same process ID (like a process controller separate app domain) => Should NOT be deleted.
			AssertNotNull("Same Process Heartbeat Row (after clean up)", GetHeartbeatRow(sameProcessHeartbeatPk));

			// Same user+host, less than MaxDaysToKeepSameUserExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("3-day old Same User and Workstation Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPk1));

			// Same user+host, older than MaxDaysToKeepSameUserExpiredHeartbeat days => should be DELETED
			AssertNull("10-day old Same User and Workstation Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatSameUserPk2));

			// Different user, less than MaxDaysToKeepExpiredHeartbeat days old => should NOT be deleted
			AssertNotNull("20-day old Heartbeat Row (after clean up)", GetHeartbeatRow(expiredHeartbeatDifferentUserPk1));
		}

		public void TestUpdateUserContext_CleanUpSameUserAndHostOldHeartbeats_HeartbeatType()
		{
			var user = Guid.NewGuid();
			var expiredHeartbeatPK_SameHeartbeatType1 = InsertHeartbeat("ThisPc", 1, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatPK_SameHeartbeatType2 = InsertHeartbeat("ThisPc", 2, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepSameUserExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.Enterprise);
			var expiredHeartbeatPK_DifferentHeartbeatType1 = InsertHeartbeat("ThisPc", 3, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Subtract(TimeSpan.FromMinutes(1)), HeartbeatTypes.WarehouseRF);
			var expiredHeartbeatPK_DifferentHeartbeatType2 = InsertHeartbeat("ThisPc", 4, user, TimeSpan.FromDays(SemaphoreDbManager.MaxDaysToKeepExpiredHeartbeat).Add(TimeSpan.FromMinutes(1)), HeartbeatTypes.WarehouseRF);

			var dbManager = new SemaphoreDbManager();
			var newHeartbeatPk = Guid.NewGuid();
			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.UpdateUserContext(newHeartbeatPk, "ThisPc", user, GlbStaffSchema.Constants.Prefix, HeartbeatTypes.Enterprise);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			AssertNotNull("Expired Hearbeat of same type less than 3 days old should not be deleted.", GetHeartbeatRow(expiredHeartbeatPK_SameHeartbeatType1));
			AssertNull("Expired Hearbeat of same type older than 3 days should be deleted.", GetHeartbeatRow(expiredHeartbeatPK_SameHeartbeatType2));
			AssertNotNull("Expired Hearbeat of different type less than 30 days old should not be deleted.", GetHeartbeatRow(expiredHeartbeatPK_DifferentHeartbeatType1));
			AssertNotNull("Expired Hearbeat of different type older than 30 days should not be deleted.", GetHeartbeatRow(expiredHeartbeatPK_DifferentHeartbeatType2));
		}

		#endregion

		#region InsertHeartbeat

		Guid InsertHeartbeat(string hostName, int processID, Guid userPK, TimeSpan addTime, string heartbeatType)
		{
			var insertHeartbeatText = @"INSERT dbo.StmServiceHeartbeat
				(SV_PK, SV_WorkstationName, SV_ProcessID, SV_ParentTableCode, SV_ParentId, SV_ExpiresAtUtc, SV_HeartbeatType, SV_CreateTimeUTC)
				VALUES ('{0}', '{1}', {2}, 'GS', '{3}', dateadd(day, {4}, getutcdate()), '{5}', dateadd(MINUTE, {4}, getutcdate()))";
			var result = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(String.Format(insertHeartbeatText, result.ToString(), hostName, processID, userPK, -addTime.TotalMinutes, heartbeatType));
			InsertSemaphore(result);
			return result;
		}

		#endregion

		[TestDate(2018, 02, 07)]
		public void TestCleanupRemovesAllHosts()
		{
			var guid = new Guid("217369CF-A3B8-42D1-BC6F-AA4A6437D939");
			var cmd = Db.Connection.Command(@"insert into dbo.StmServiceHeartBeat(SV_PK, SV_ExpiresAtUtc, SV_WorkstationName, SV_ProcessID, SV_HeartbeatType, SV_ParentID, SV_PArentTableCode, SV_CreateTimeUTC)
values (newid(), DateAdd(DAY, -1, @utcNow), 'Hello', 123, 'ENT', '217369CF-A3B8-42D1-BC6F-AA4A6437D939', 'GS', DateAdd(DAY, -1, @utcNow))");
			cmd.AddParameter("@utcNow", SqlDbType.DateTime, ZDateTime.UtcNow.ToDateTime());
			cmd.ExecuteNonQuery();
			TestDateAttribute.AddSeconds(1);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(guid, "HSL667", 12345, guid, GlbStaffSchema.Constants.Prefix, 100, "ENT", null);
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("select count(*) FROM dbo.StmServiceHeartBeat where SV_WorkstationName = 'Hello'"));
		}

		public void TestGetActiveSemaphoreHandlesDoesNotWaitForLockedRecords()
		{
			var sqlText = @"
				SELECT TOP 1 SV_PK FROM dbo.StmServiceHeartBeat WITH(NOLOCK)
				WHERE (SV_WorkstationName = HOST_NAME() OR SV_WorkstationName LIKE HOST_NAME() + '/%')
				AND SV_ProcessID = HOST_ID()
				ORDER BY SV_ExpiresAtUtc DESC";
			var heartbeatPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			var testLockInfo = "~TestGetActiveSemaphoreHandlesDoesNotWaitForLockedRecords~LockInfo~";
			ISemaphoreType semaphore = new SemaphoreForTesting(testLockInfo);

			var insertSemaphoreText = String.Format(
				"INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_LockInfo, SS_ServiceClass, SS_AcquiredTimeUtc) VALUES (NEWID(), '{0}', '{1}', '~1', '2001-01-11')",
				heartbeatPk.ToString(), semaphore.LockInfo);
			TestConnection.ExecuteNonQuery(insertSemaphoreText);

			var dbManager = new SemaphoreDbManager();
			var activeHandles1 = dbManager.GetActiveSemaphoreHandles(semaphore);
			AssertEquals("Should return created semaphore", 1, activeHandles1.Length);

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				anotherConnection.BeginTransaction();

				try
				{
					insertSemaphoreText = String.Format(
						"INSERT dbo.StmServiceSemaphore WITH(SERIALIZABLE) (SS_PK, SS_SV, SS_LockInfo, SS_ServiceClass, SS_AcquiredTimeUtc) VALUES (NEWID(), '{0}', '{1}', '~2', '2002-02-22')",
						heartbeatPk.ToString(), semaphore.LockInfo);
					anotherConnection.ExecuteNonQuery(insertSemaphoreText);

					var activeHandles2 = dbManager.GetActiveSemaphoreHandles(semaphore);
					AssertEquals("Should only return the first semaphore", 1, activeHandles2.Length);
					AssertEquals("Semaphore Created Time", activeHandles1[0].CreateTimeUtc, activeHandles2[0].CreateTimeUtc);
				}
				finally
				{
					anotherConnection.RollbackTransaction();
				}
			}
		}

		public void TestActiveSemaphoreHandlesScriptWithParameter()
		{
			using (var conn = Db.NewAdminConnection())
			{
				conn.ExecuteNonQuery("DBCC FREEPROCCACHE WITH NO_INFOMSGS");
			}

			RunGetActiveSemaphoreHandles("abc");
			RunGetActiveSemaphoreHandles("def");

			var sqlGetPlanCount = @"
SELECT COUNT(*)
FROM sys.dm_exec_cached_plans
CROSS APPLY sys.dm_exec_sql_text(plan_handle)
CROSS APPLY sys.dm_exec_query_plan(plan_handle)
WHERE TEXT LIKE '%SELECT%' AND TEXT NOT LIKE '%SELECT COUNT(*)%' AND TEXT LIKE '%StmServiceHeartBeat%'
";

			using (var conn = Db.NewAdminConnection())
			{
				var planCount = (int)conn.ExecuteScalar(sqlGetPlanCount);
				AssertEquals(1, planCount);
			}
		}

		void RunGetActiveSemaphoreHandles(string lockInfo)
		{
			var semaphore = new SemaphoreForTesting(lockInfo);
			var dbManager = new SemaphoreDbManager();
			dbManager.GetActiveSemaphoreHandles(semaphore);
		}

		public void TestLoadHandle()
		{
			var sqlText = @"
				SELECT TOP 1 SV_PK FROM dbo.StmServiceHeartBeat WITH(NOLOCK)
				WHERE (SV_WorkstationName = HOST_NAME() OR SV_WorkstationName LIKE HOST_NAME() + '/%')
				AND SV_ProcessID = HOST_ID()
				ORDER BY SV_ExpiresAtUtc DESC";
			var heartbeatPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			var testLockInfo = "~TestGetActiveSemaphoreHandlesDoesNotWaitForLockedRecords~LockInfo~";
			ISemaphoreType semaphore = new SemaphoreForTesting(testLockInfo);

			var ss_pk = Guid.NewGuid();

			var insertSemaphoreText = String.Format(
							 "INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_LockInfo, SS_ServiceClass, SS_AcquiredTimeUtc) VALUES ('{0}', '{1}', '{2}', 'TST', '2001-01-11')",
							 ss_pk.ToString(), heartbeatPk.ToString(), semaphore.LockInfo);
			TestConnection.ExecuteNonQuery(insertSemaphoreText);

			AssertEquals(ss_pk, new SemaphoreDbManager().LoadSemaphoreHandle(heartbeatPk, semaphore.LockInfo, semaphore.Category));
		}

		#region TestGetActiveSemaphoreHandles

		public void TestGetActiveSemaphoreHandles()
		{
			var heartbeat1 = Guid.NewGuid();
			var heartbeat2 = Guid.NewGuid();
			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();

			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat1, "PC1", 1, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat2, "PC2", 2, user2, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.WarehouseRF, null);

			InsertSemaphore(heartbeat1);
			InsertSemaphore(heartbeat1);
			InsertSemaphore(heartbeat2);

			AssertEquals(2, new SemaphoreDbManager().GetActiveSemaphoreHandles(heartbeat1).Length);
			AssertEquals(1, new SemaphoreDbManager().GetActiveSemaphoreHandles(heartbeat2).Length);

			var info = new SemaphoreDbManager().GetActiveSemaphoreHandles(heartbeat2)[0];

			AssertEquals(user2, info.OwnerSession.UserPk);
			AssertEquals("PC2", info.OwnerSession.HostName);
			AssertEquals(2, info.OwnerSession.ProcessId);
			AssertEquals(HeartbeatTypes.WarehouseRF, info.OwnerSession.HeartbeatType);

			AssertEquals("~1", info.Semaphore.Category);
		}

		#endregion

		public void TestGetRemoteActiveSemaphoreHandles()
		{
			var heartbeat1 = Guid.NewGuid();
			var heartbeat2 = Guid.NewGuid();
			var heartbeat3 = Guid.NewGuid();
			var heartbeat4 = Guid.NewGuid();
			var heartbeat5 = Guid.NewGuid();

			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();

			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat1, "PC1", 1, user1, GlbStaffSchema.Constants.Prefix, pulseInSeconds: 1000, HeartbeatTypes.Enterprise, null);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat2, "PC2", 2, user2, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat3, "PC2", 2, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat4, "LocalPC", 2, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat5, "PC3", 3, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);

			InsertSemaphore(heartbeat1, "EnterpriseActiveLogin");
			InsertSemaphore(heartbeat1, "Licence:COR:EDI");
			InsertSemaphore(heartbeat2, "EnterpriseActiveLogin");
			InsertSemaphore(heartbeat3, "EnterpriseActiveLogin");
			InsertSemaphore(heartbeat5, "EnterpriseWinzorActiveLogin");

			ISemaphoreType semaphore = new SemaphoreForTesting("EnterpriseActiveLogin");

			var handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "LocalPC", string.Empty);
			AssertEquals(2, handles1.Length);
			var host1 = handles1[0].OwnerSession.HostName;
			var host2 = handles1[1].OwnerSession.HostName;
			Assert(host1 != host2 && (host1 == "PC1" || host2 == "PC2"));

			var handles2 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user2, "LocalPC", string.Empty);
			AssertEquals(1, handles2.Length);

			var info = handles2[0];

			AssertEquals(user2, info.OwnerSession.UserPk);
			AssertEquals("PC2", info.OwnerSession.HostName);
			AssertEquals(2, info.OwnerSession.ProcessId);

			AssertEquals("TST", info.Semaphore.Category);

			ISemaphoreType winzorSemaphore = new SemaphoreForTesting("EnterpriseWinzorActiveLogin");
			var winzorHandles = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(winzorSemaphore, user1, "LocalPC", string.Empty);
			AssertEquals(1, winzorHandles.Length);
			AssertEquals("PC3", winzorHandles[0].OwnerSession.HostName);
		}

		public void TestGetRemoteActiveSemaphoreHandles_MultipleAppServers()
		{
			var heartbeat1 = Guid.NewGuid();
			var heartbeat2 = Guid.NewGuid();
			var heartbeat3 = Guid.NewGuid();
			var heartbeat4 = Guid.NewGuid();

			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();

			var clientId1 = "1";
			var clientId2 = "2";

			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat1, "BLZ01/PC1", 1, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientId1);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat2, "BLZ02/PC1", 2, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientId1);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat3, "BLZ03/PC1", 2, user1, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientId1);
			new SemaphoreDbManager().CreateHeartbeatInDatabase(heartbeat4, "BLZ01/PC2", 2, user2, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientId2);

			InsertSemaphore(heartbeat1, "EnterpriseWinzorActiveLogin");
			InsertSemaphore(heartbeat2, "EnterpriseWinzorActiveLogin");
			InsertSemaphore(heartbeat3, "EnterpriseWinzorActiveLogin");
			InsertSemaphore(heartbeat4, "EnterpriseWinzorActiveLogin");

			ISemaphoreType semaphore = new SemaphoreForTesting("EnterpriseWinzorActiveLogin");

			var handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "BLZ01/PC1", clientId1);
			AssertEquals(0, handles1.Length);

			handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "BLZ04/PC1", clientId1);
			AssertEquals(0, handles1.Length);

			handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "BLZ01/PC1", clientId2);
			AssertEquals(3, handles1.Length);
			AssertEquals("BLZ01/PC1", handles1[0].OwnerSession.HostName);
			AssertEquals("BLZ02/PC1", handles1[1].OwnerSession.HostName);
			AssertEquals("BLZ03/PC1", handles1[2].OwnerSession.HostName);

			handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "BLZ01/PC3", clientId2);
			AssertEquals(3, handles1.Length);
			AssertEquals("BLZ01/PC1", handles1[0].OwnerSession.HostName);
			AssertEquals("BLZ02/PC1", handles1[1].OwnerSession.HostName);
			AssertEquals("BLZ03/PC1", handles1[2].OwnerSession.HostName);

			handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "BLZ04/PC3", clientId2);
			AssertEquals(3, handles1.Length);
			AssertEquals("BLZ01/PC1", handles1[0].OwnerSession.HostName);
			AssertEquals("BLZ02/PC1", handles1[1].OwnerSession.HostName);
			AssertEquals("BLZ03/PC1", handles1[2].OwnerSession.HostName);

			handles1 = new SemaphoreDbManager().GetRemoteActiveSemaphoreHandles(semaphore, user1, "RDP01/PC2", string.Empty);
			AssertEquals(0, handles1.Length);
		}

		public void TestRemoveSemaphore()
		{
			var heartbeatPk = InsertHeartbeat("ThisPc", 3, Guid.NewGuid(), TimeSpan.FromMinutes(-1), HeartbeatTypes.Enterprise);

			ISemaphoreType semaphore1 = new SemaphoreForTesting("TestSemaphore1");
			ISemaphoreType semaphore2 = new SemaphoreForTesting("TestSemaphore2");

			var insertSemaphoresText = string.Format(
				"INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_LockInfo, SS_ServiceClass, SS_AcquiredTimeUtc) VALUES (NEWID(), '{0}', '{1}', '{2}', '2001-01-11'), (NEWID(), '{0}', '{3}', '{4}', '2001-01-11')",
				heartbeatPk.ToString(), semaphore1.LockInfo, semaphore1.Category, semaphore2.LockInfo, semaphore2.Category);
			TestConnection.ExecuteNonQuery(insertSemaphoresText);

			var dbManager = new SemaphoreDbManager();

			AssertEquals("Pre-requisite: Active handles with: LockInfo = TestSemaphore1", 1, dbManager.GetActiveSemaphoreHandles(semaphore1).Length);
			AssertEquals("Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 1, dbManager.GetActiveSemaphoreHandles(semaphore2).Length);

			dbManager.RemoveSemaphore(semaphore1);

			AssertEquals("After remove TestSemaphore1: Active handles with: LockInfo = TestSemaphore1", 0, dbManager.GetActiveSemaphoreHandles(semaphore1).Length);
			AssertEquals("After remove TestSemaphore1: Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 1, dbManager.GetActiveSemaphoreHandles(semaphore2).Length);

			dbManager.RemoveSemaphore(semaphore2);

			AssertEquals("After remove TestSemaphore2: Active handles with: LockInfo = TestSemaphore1", 0, dbManager.GetActiveSemaphoreHandles(semaphore1).Length);
			AssertEquals("After remove TestSemaphore2: Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 0, dbManager.GetActiveSemaphoreHandles(semaphore2).Length);
		}

		public void TestCheckHeartbeatExpiredTimeIsExpired()
		{
			var heartbeatPk = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			var hostName = "PC1";

			var semaphoreDbManage = new SemaphoreDbManager();
			semaphoreDbManage.CreateHeartbeatInDatabase(heartbeatPk, hostName, 1, userPk,
				GlbStaffSchema.Constants.Prefix, 600, HeartbeatTypes.Enterprise, null);

			InsertSemaphore(heartbeatPk, "EnterpriseActiveLogin");
			AssertEquals(false, semaphoreDbManage.CheckHeartbeatExpiredTimeIsExpired(heartbeatPk));
			semaphoreDbManage.RemoteLogoff(userPk, "PC2", HeartbeatTypes.Enterprise, string.Empty);
			AssertEquals(true, semaphoreDbManage.CheckHeartbeatExpiredTimeIsExpired(heartbeatPk));
		}

		[ExpectNoExceptions, UseSnapshotProtection]
		public void TestAccessToDbConnectionIsWrappedInDisposableAction()
		{
			var semaphoreDbManager = new SemaphoreDbManager();

			// CreateSemaphoreHandleInTransaction
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.CreateSemaphoreHandleInTransaction(heartbeatPK, "Mutex: ThisIsATest:" + heartbeatPK, "MTX", maxConcurrentHandles));

			// LoadSemaphoreHandle
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.LoadSemaphoreHandle(heartbeatPK, semaphore.LockInfo, semaphore.Category));

			// GetActiveSemaphoreHandles
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.GetActiveSemaphoreHandles(heartbeatPK));

			// GetActiveSemaphoreLockInfo
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.GetActiveSemaphoreLockInfo(""));

			// GetRemoteActiveSemaphoreHandles
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.GetRemoteActiveSemaphoreHandles(semaphore, userPK, "LocalPC", string.Empty));

			// RemoteLogoff
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.RemoteLogoff(userPK, "PC1", HeartbeatTypes.Enterprise, string.Empty));

			// DeleteSemaphoreHandleFromDatabase
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.DeleteSemaphoreHandleFromDatabase(heartbeatPK));

			// CheckHeartbeatExpiredTimeIsExpired
			AccessToDbConnectionIsWrappedInDisposableAction_Core((heartbeatPK, userPK, maxConcurrentHandles, semaphore) =>
				semaphoreDbManager.CheckHeartbeatExpiredTimeIsExpired(heartbeatPK));
		}

		void AccessToDbConnectionIsWrappedInDisposableAction_Core(Action<Guid, Guid, int, ISemaphoreType> methodWeWantToRunInAThread)
		{
			var thread = new Thread(() =>
			{
				var heartbeatPK = Guid.NewGuid();
				var userPK = Guid.NewGuid();
				var host = 1;
				var semaphoreDbManager = new SemaphoreDbManager();
				semaphoreDbManager.CreateHeartbeatInDatabase(heartbeatPK, "TEST-HOST-" + host, host, userPK, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);

				var testLockInfo = "~TestGetActiveSemaphoreHandlesDoesNotWaitForLockedRecords~LockInfo~";
				ISemaphoreType semaphore = new SemaphoreForTesting(testLockInfo);

				methodWeWantToRunInAThread(heartbeatPK, userPK, host, semaphore);
			});
			thread.Start();
			thread.Join();
		}

		public void TestRemoteLogoff()
		{
			var heartbeat1 = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			var dbManager = new SemaphoreDbManagerThatAlwaysRefreshes();
			var hearbeatDuration = TimeSpan.FromSeconds(60);

			dbManager.CreateHeartbeatInDatabase(heartbeat1, "PC1", 1, userPk, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			Assert("Precondition - heartbeat should be active.", new SemaphoreDbManager().RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.RemoteLogoff(userPk, "PC1", HeartbeatTypes.Enterprise, string.Empty);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			Thread.Sleep(500);
			Assert("User should not be logged off if same user logged in from same pc and with same heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.Enterprise, "client identifier");
			Thread.Sleep(500);
			Assert("User should not be logged off if same user logged in from different pc with same heartbeat type and client identifier.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.WarehouseRF, string.Empty);
			Thread.Sleep(500);
			Assert("User should not be logged off if same user logs from different pc and with different heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.Enterprise, string.Empty);
			Thread.Sleep(500);
			Assert("User should be logged off if same user logs in from another pc and with same heartbeat type.", !dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));
		}

		public void TestRemoteLogoffWithClientIdentifier()
		{
			var heartbeat1 = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			var clientIdentifier = "client identifier";
			var dbManager = new SemaphoreDbManagerThatAlwaysRefreshes();
			var hearbeatDuration = TimeSpan.FromSeconds(60);

			// Remote logoff from same machine
			dbManager.CreateHeartbeatInDatabase(heartbeat1, "machine1", 1, userPk, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientIdentifier);
			Assert("Precondition - heartbeat should be active.", new SemaphoreDbManager().RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.RemoteLogoff(userPk, "machine1", HeartbeatTypes.Enterprise, clientIdentifier);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			Thread.Sleep(500);
			Assert("User should not be logged off with same client identifier and with same heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "machine1", HeartbeatTypes.Enterprise, string.Empty);
			Thread.Sleep(500);
			Assert("User should not be logged off with same heartbeat type and no client identifier.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "machine1", HeartbeatTypes.WarehouseRF, "some other client identifier");
			Thread.Sleep(500);
			Assert("User should not be logged off with different client identifier and with different heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "machine1", HeartbeatTypes.Enterprise, "some other client identifier");
			Thread.Sleep(500);
			Assert("User should be logged off with different client identifier and with same heartbeat type.", !dbManager.RefreshHeartbeatInDatabase(heartbeat1, hearbeatDuration, isNewConnection: false));

			// Remote logoff from different machine
			var heartbeat2 = Guid.NewGuid();
			dbManager.CreateHeartbeatInDatabase(heartbeat2, "machine1", 1, userPk, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, clientIdentifier);
			Assert("Precondition - heartbeat should be active.", new SemaphoreDbManager().RefreshHeartbeatInDatabase(heartbeat2, hearbeatDuration, isNewConnection: false));

			commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.Enterprise, clientIdentifier);
			commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			Thread.Sleep(500);
			Assert("User should not be logged off with same client identifier and with same heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat2, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.Enterprise, string.Empty);
			Thread.Sleep(500);
			Assert("User should not be logged off with same heartbeat type and no client identifier.", dbManager.RefreshHeartbeatInDatabase(heartbeat2, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.WarehouseRF, "some other client identifier");
			Thread.Sleep(500);
			Assert("User should not be logged off with different client identifier and with different heartbeat type.", dbManager.RefreshHeartbeatInDatabase(heartbeat2, hearbeatDuration, isNewConnection: false));

			dbManager.RemoteLogoff(userPk, "some other machine", HeartbeatTypes.Enterprise, "some other client identifier");
			Thread.Sleep(500);
			Assert("User should be logged off with different client identifier and with same heartbeat type.", !dbManager.RefreshHeartbeatInDatabase(heartbeat2, hearbeatDuration, isNewConnection: false));
		}

		public void TestReleaseLocks()
		{
			var heartbeat1 = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			var dbManager = new SemaphoreDbManagerThatAlwaysRefreshes();

			dbManager.CreateHeartbeatInDatabase(heartbeat1, "PC1", 1, userPk, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);
			InsertSemaphore(heartbeat1, "locksmith");
			AssertEquals("Precondition - semaphore handle should be active", 1, new SemaphoreDbManager().GetActiveSemaphoreHandles(heartbeat1).Length);

			var commandsBefore = TestConnection.ExecutedCommandCount;
			dbManager.ReleaseLocks("locksmith", HeartbeatTypes.Enterprise, userPk);
			var commandsAfter = TestConnection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
			Thread.Sleep(500);
			AssertEquals("Precondition - semaphore handle should be inactive", 0, new SemaphoreDbManager().GetActiveSemaphoreHandles(heartbeat1).Length);
		}

		void InsertSemaphore(Guid heartbeat)
		{
			InsertSemaphore(heartbeat, Guid.NewGuid().ToString());
		}

		void InsertSemaphore(Guid heartbeat, string lockInfo)
		{
			var insertSemaphoreText = String.Format(
				"INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_LockInfo, SS_ServiceClass, SS_AcquiredTimeUtc) VALUES (NEWID(), '{0}', '{1}', '~1', '2001-01-11')",
				heartbeat.ToString(), lockInfo);
			TestConnection.ExecuteNonQuery(insertSemaphoreText);
		}

		DataRow GetHeartbeatRow(Guid heartbeatPk)
		{
			var sqlText = String.Format("SELECT * FROM dbo.StmServiceHeartBeat WITH (READCOMMITTED) WHERE SV_PK = '{0}'", heartbeatPk.ToString());
			var table = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var result = (table.Rows.Count == 1) ? table.Rows[0] : null;
			return result;
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}
	}

	class TestSemaphoreDbManagerWithoutTransaction : TestCase
	{
		[ExpectNoExceptions]
		public void TestNoDeadlocking()
		{
			try
			{
				var clientThreads = new Thread[50];
				for (var i = 0; i < clientThreads.Length; i++)
				{
					clientThreads[i] = new Thread(new ParameterizedThreadStart(RunClient));
					clientThreads[i].Start(i);
					Thread.Sleep(10);
				}
				for (var i = 0; i < clientThreads.Length; i++)
				{
					clientThreads[i].Join();
				}
				if (exceptionThrown != null)
				{
					throw exceptionThrown;
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("delete from dbo.StmServiceSemaphore where SS_SV in (select SV_PK from dbo.StmServiceHeartBeat where SV_WorkstationName like 'TEST%')");
				Db.Connection.ExecuteNonQuery("delete from dbo.StmServiceHeartBeat where SV_WorkstationName like 'TEST%'");
				DbCommitTracker.Reset();
			}
		}

		[UseSnapshotProtection]
		public void TestCreateSemaphoreHandleInTransaction()
		{
			var heartbeatPK = Guid.NewGuid();
			var userPK = Guid.NewGuid();
			var host = 1;
			var semaphoreDbManager = new SemaphoreDbManager();
			semaphoreDbManager.CreateHeartbeatInDatabase(heartbeatPK, "TEST-HOST-" + host, host, userPK, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);

			var objectPk = Guid.NewGuid();

			// Prevent potential reconnection to happen at CreateSemaphoreHandleInTransaction, which introduces extra DB hits
			Db.Connection.EnsureIsOpen();

			var commandsBefore = Db.Connection.ExecutedCommandCount;
			var mutexHandle = semaphoreDbManager.CreateSemaphoreHandleInTransaction(heartbeatPK, "Mutex:ThisIsATest:" + objectPk, "MTX", 1);
			var commandsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Commands count", 1, commandsAfter - commandsBefore);
		}

		[UseSnapshotProtection]
		public void TestCreateSemaphoreHandleInTransactionWithNullQueryResult()
		{
			var heartbeatPK = Guid.NewGuid();
			var userPK = Guid.NewGuid();
			var host = 1;
			var semaphoreDbManager = new SemaphoreDbManagerWithNullQueryResult();
			semaphoreDbManager.CreateHeartbeatInDatabase(heartbeatPK, "TEST-HOST-" + host, host, userPK, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);

			var objectPk = Guid.NewGuid();

			// Prevent potential reconnection to happen at CreateSemaphoreHandleInTransaction, which introduces extra DB hits
			Db.Connection.EnsureIsOpen();

			AssertNoExceptionThrown(() => semaphoreDbManager.CreateSemaphoreHandleInTransaction(heartbeatPK, "Mutex:ThisIsATest:" + objectPk, "MTX", 1));
		}

		class SemaphoreDbManagerWithNullQueryResult : SemaphoreDbManager
		{
			protected override object GetParameterValueFromScript(DbConnection connection, Guid heartbeatUniqueId, string lockInfo, string category, int maxConcurrentHandles)
			{
				if (heartbeatUniqueId == Guid.Empty)
				{
					throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
				}

				return DBNull.Value;
			}
		}

		void RunClient(object hostNumber)
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var heartbeat = Guid.NewGuid();
					var user = Guid.NewGuid();
					var host = (int)hostNumber;
					var semaphoreDbManager = new SemaphoreDbManager();
					semaphoreDbManager.CreateHeartbeatInDatabase(heartbeat, "TEST-HOST-" + host, host, user, GlbStaffSchema.Constants.Prefix, 1000, HeartbeatTypes.Enterprise, null);

					for (var i = 0; i < 50 && exceptionThrown == null; i++)
					{
						Thread.Sleep(100);
						var objectPk = Guid.NewGuid();
						var mutexHandle = semaphoreDbManager.CreateSemaphoreHandleInTransaction(heartbeat, "Mutex:ThisIsATest:" + objectPk, "MTX", 1);
						var actionHandle = semaphoreDbManager.CreateSemaphoreHandleInTransaction(heartbeat, "EnterprisePendingUerAction:" + objectPk, "ACT", 1);
						Thread.Sleep(100);
						semaphoreDbManager.DeleteSemaphoreHandleFromDatabase(actionHandle);
						semaphoreDbManager.DeleteSemaphoreHandleFromDatabase(mutexHandle);
					}

					semaphoreDbManager.DeleteHeartbeatFromDatabase(heartbeat);
				}
				catch (Exception ex)
				{
					if (exceptionThrown == null)
					{
						exceptionThrown = ex;
					}
				}
			}
		}

		Exception exceptionThrown;
	}
}
