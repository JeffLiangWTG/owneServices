using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Semaphores;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Semaphores.Testing
{
	[TestedType(typeof(InsertSemaphoreHandle))]
	class InsertSemaphoreHandleTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			int maxConcurrentHandles = 1;
			string semaphoreLockInfo = "TestLockKey";
			string category = "TST";
			Guid heartbeatPk = Guid.NewGuid();
			Guid handleId = Guid.Empty;
			InsertHeartbeat(heartbeatPk);

			AssertEquals("No semaphore handles with current lock info (key)", 0, GetSemaphoreHandlesCount(semaphoreLockInfo));
			InsertSemaphoreHandle(maxConcurrentHandles, semaphoreLockInfo, category, heartbeatPk, out handleId);

			AssertEquals("Semaphore handle was inserted", 1, GetSemaphoreHandlesCount(semaphoreLockInfo));
			AssertEquals("UseCount", 1, GetSemaphoreUseCount(semaphoreLockInfo));
			AssertNotEquals("HandleId", Guid.Empty, handleId);

			try
			{
				InsertSemaphoreHandle(maxConcurrentHandles, semaphoreLockInfo, category, heartbeatPk, out handleId);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				AssertEquals("Exception message", "Handle not created, semaphore has reached max quantity of concurrent handles.", ex.Message);
			}
		}

		public void TestUseCountIncrement()
		{
			int maxConcurrentHandles = 3;
			string semaphoreLockInfo = "TestLockKey";
			string category = "TST";
			Guid heartbeatPk = Guid.NewGuid();
			Guid handleId = Guid.Empty;
			Guid secondHandleId = Guid.Empty;
			InsertHeartbeat(heartbeatPk);

			AssertEquals("No semaphore handles with current lock info (key)", 0, GetSemaphoreHandlesCount(semaphoreLockInfo));
			InsertSemaphoreHandle(maxConcurrentHandles, semaphoreLockInfo, category, heartbeatPk, out handleId);
			AssertEquals("Semaphore handle was inserted", 1, GetSemaphoreHandlesCount(semaphoreLockInfo));
			AssertEquals("UseCount", 1, GetSemaphoreUseCount(semaphoreLockInfo));
			AssertNotEquals("HandleId", Guid.Empty, handleId);

			InsertSemaphoreHandle(maxConcurrentHandles, semaphoreLockInfo, category, heartbeatPk, out secondHandleId);
			AssertEquals("No more handles were inserted", 1, GetSemaphoreHandlesCount(semaphoreLockInfo));
			AssertEquals("UseCount was incremented", 2, GetSemaphoreUseCount(semaphoreLockInfo));
			AssertEquals("HandleId is the same", handleId, secondHandleId);
		}

		void InsertSemaphoreHandle(int maxConcurrentHandles, string semaphoreLockInfo, string category, Guid heartbeatPk, out Guid handleId)
		{
			using (DbCommand command = TestConnection.Command(ScriptToTest.Name))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@currentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow);
				command.AddParameter("@maxConcurrentHandles", SqlDbType.Int, maxConcurrentHandles);
				command.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatPk);
				command.AddParameter("@lockInfo", SqlDbType.VarChar, semaphoreLockInfo);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@newHandleId", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddOutputParameter("@actualHandleId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.ExecuteNonQuery();
				handleId = (Guid)command.GetParameterValue("@actualHandleId");
			}
		}

		void InsertHeartbeat(Guid heartbeatPk)
		{
			string sqlText = string.Format(@"
				INSERT dbo.StmServiceHeartBeat (SV_PK, SV_ParentTableCode, SV_ParentId, SV_ExpiresAtUtc)
				VALUES('{0}', 'GS', NEWID(), DateAdd(minute, 5, GetUtcDate()))",
				heartbeatPk);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		int GetSemaphoreHandlesCount(string semaphoreLockInfo)
		{
			string sqlText = string.Format(@"
				SELECT count(*)
				FROM dbo.StmServiceSemaphore
				WHERE SS_LockInfo = '{0}'",
				semaphoreLockInfo);
			int count = (int)TestConnection.ExecuteScalar(sqlText);
			return count;
		}

		int GetSemaphoreUseCount(string semaphoreLockInfo)
		{
			string sqlText = string.Format(@"
				SELECT sum(SS_UseCount)
				FROM dbo.StmServiceSemaphore
				WHERE SS_LockInfo = '{0}'",
				semaphoreLockInfo);
			int count = (int)TestConnection.ExecuteScalar(sqlText);
			return count;
		}
	}
}

