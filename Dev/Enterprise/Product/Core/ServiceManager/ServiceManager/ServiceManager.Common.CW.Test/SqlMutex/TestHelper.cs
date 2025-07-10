using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Semaphores.Common.Test
{
	static class TestHelper
	{
		public static DateTime? UpdateLockAsExpiredToOtherHostProcess(DbConnection connection, string lockInfo, string category, int? processId = null, string workStation = null)
		{
			var script = @"
UPDATE [StmServiceMutex]
SET
	SMX_ExpiresAtUtc = DATEADD(second, -10, GETUTCDATE())
	, SMX_WorkstationName = @workStationName
	, SMX_ProcessId = @processId
WHERE
	SMX_ServiceClass = @category
	AND SMX_LockInfo = @lockInfo
";
			connection.ExecuteNonQuery(script, cmd =>
			{
				cmd.AddParameter("@category", System.Data.SqlDbType.VarChar, 3, category);
				cmd.AddParameter("@lockInfo", System.Data.SqlDbType.VarChar, 128, lockInfo);
				cmd.AddParameter("@workStationName", System.Data.SqlDbType.VarChar, 128, workStation ?? System.Environment.MachineName);
				cmd.AddParameter("@processId", System.Data.SqlDbType.Int, processId ?? Process.GetCurrentProcess().Id);
			});

			return GetLockExpiryTimeUtc(connection, lockInfo, category, processId, workStation);
		}

		public static DateTime? UpdateLockAsAcquiredToOtherHostProcess(DbConnection connection, string lockInfo, string category, int? processId = null, string workStation = null)
		{
			var script = @"
UPDATE [StmServiceMutex]
SET
	SMX_ExpiresAtUtc = DATEADD(second, 900, GETUTCDATE())
	, SMX_WorkstationName = @workStationName
	, SMX_ProcessId = @processId
WHERE
	SMX_ServiceClass = @category
	AND SMX_LockInfo = @lockInfo
";
			connection.ExecuteNonQuery(script, cmd =>
			{
				cmd.AddParameter("@category", System.Data.SqlDbType.VarChar, 3, category);
				cmd.AddParameter("@lockInfo", System.Data.SqlDbType.VarChar, 128, lockInfo);
				cmd.AddParameter("@workStationName", System.Data.SqlDbType.VarChar, 128, workStation ?? System.Environment.MachineName);
				cmd.AddParameter("@processId", System.Data.SqlDbType.Int, processId ?? Process.GetCurrentProcess().Id);
			});

			return GetLockExpiryTimeUtc(connection, lockInfo, category, processId, workStation);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1183:DATEADD with non-integer value", Justification = "Rule was implemented incorrectly")]
		public static DateTime? UpdateLockExpiresAfterTimeoutInSeconds(
			DbConnection connection,
			string lockInfo,
			string category,
			int timeoutInSeconds = -10,
			int? processId = null,
			string workStation = null)
		{
			var script = @"
UPDATE [StmServiceMutex]
SET
	SMX_ExpiresAtUtc = DATEADD(second, @timeoutInSeconds, GETUTCDATE())
WHERE
	SMX_ServiceClass = @category
	AND SMX_LockInfo = @lockInfo
	AND SMX_WorkstationName = @workStationName
	AND SMX_ProcessId = @processId
";
			connection.ExecuteNonQuery(script, cmd =>
			{
				cmd.AddParameter("@timeoutInSeconds", System.Data.SqlDbType.Int, timeoutInSeconds);
				cmd.AddParameter("@category", System.Data.SqlDbType.VarChar, 3, category);
				cmd.AddParameter("@lockInfo", System.Data.SqlDbType.VarChar, 128, lockInfo);
				cmd.AddParameter("@workStationName", System.Data.SqlDbType.VarChar, 128, workStation ?? System.Environment.MachineName);
				cmd.AddParameter("@processId", System.Data.SqlDbType.Int, processId ?? Process.GetCurrentProcess().Id);
			});

			return GetLockExpiryTimeUtc(connection, lockInfo, category, processId, workStation);
		}

		public static DateTime? GetLockExpiryTimeUtc(string lockInfo, string category, int? processId = null, string workStation = null)
			=> GetLockExpiryTimeUtc(Db.Connection, lockInfo, category, processId, workStation);

		public static DateTime? GetLockExpiryTimeUtc(
			DbConnection connection,
			string lockInfo,
			string category,
			int? processId = null,
			string workStation = null)
		{
			var script = @"
SELECT SMX_ExpiresAtUtc
FROM [StmServiceMutex] WITH (nolock, INDEX([NR_UC__SMX_ServiceClass_SMX_LockInfo]))
WHERE SMX_ServiceClass = @category
	AND SMX_LockInfo = @lockInfo
	AND SMX_WorkstationName = @workStationName
	AND SMX_ProcessId = @processId
";
			var result = connection.ExecuteScalar(script, cmd =>
			{
				cmd.AddParameter("@category", System.Data.SqlDbType.VarChar, 3, category);
				cmd.AddParameter("@lockInfo", System.Data.SqlDbType.VarChar, 128, lockInfo);
				cmd.AddParameter("@workStationName", System.Data.SqlDbType.VarChar, 128, workStation ?? System.Environment.MachineName);
				cmd.AddParameter("@processId", System.Data.SqlDbType.Int, processId ?? Process.GetCurrentProcess().Id);
			});

			if (result == null || result == DBNull.Value)
			{
				return null;
			}

			return (DateTime)result;
		}

		/// <summary>
		/// Get update locks on all rows in StmServiceMutex in a transaction.
		/// </summary>
		/// <returns></returns>
		public static IDisposable AcquireStmServiceMutexRowLocks(DbConnection connection, bool commit)
		{
			connection.BeginTransaction();
			using (var cmd = connection.Command("update dbo.StmServiceMutex set SMX_SystemLastEditTimeUtc = GETUTCDATE()"))
			{
				cmd.CommandTimeout = 10; // seconds
				cmd.ExecuteNonQuery();
			}

			return new DisposableAction(() =>
			{
				if (commit)
				{
					connection.CommitTransaction();
				}
				else
				{
					connection.RollbackTransaction();
				}
			});
		}

		public static void CleanupSqlMutexLocks()
		{
			Db.Connection.ExecuteNonQuery("DELETE [StmServiceMutex]");
		}
	}
}
