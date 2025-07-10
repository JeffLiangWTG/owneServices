using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using ServiceManager.Common.Abstractions;
using static System.FormattableString;

namespace ServiceManager.Common.CW
{
	class SqlMutexLockDbManager : ISqlMutexLockDbManager
	{
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static SqlMutexLockResult AcquireOrUpdateSqlMutexCore(
			DbConnection connection,
			string category,
			string lockInfo,
			TimeSpan timeout,
			string workStationName,
			int processId,
			string userCode,
			bool releaseLock)
		{
			using (var command = connection.Command(OverridableAcquireOrUpdateSqlMutexProc.Value))
			{
				command.CommandType = CommandType.StoredProcedure;

				#region SuppressResourceStringsCheckRegion

				command.AddParameter("@category", SqlDbType.VarChar, 3, category);
				command.AddParameter("@lockInfo", SqlDbType.VarChar, 128, lockInfo);
				command.AddParameter("@timeoutInSeconds", SqlDbType.Int, releaseLock ? TimeOutInSecondsToReleaseLock : timeout.TotalSeconds);
				command.AddParameter("@workStationName", SqlDbType.VarChar, 128, workStationName);
				command.AddParameter("@processId", SqlDbType.Int, processId);
				command.AddParameter("@userCode", SqlDbType.VarChar, 3, userCode);
				command.AddOutputParameter("@currentTimeUtc", SqlDbType.DateTime, 0, 0, 0, null);
				command.AddOutputParameter("@mutexExpiresAtTimeUtc", SqlDbType.DateTime, 0, 0, 0, null);
				command.AddOutputParameter("@returnMessage", SqlDbType.VarChar, 256, 0, 0, string.Empty);
				command.AddReturnValueParameter();

				#endregion

				var returnValue = command.ExecuteProcedureWithReturnValue();
				var expiresAtSqlTimeUtc = DateTime.SpecifyKind((DateTime)command.GetParameterValue("@mutexExpiresAtTimeUtc"), DateTimeKind.Utc);
				var acquiredTimeUtc = DateTime.SpecifyKind((DateTime)command.GetParameterValue("@currentTimeUtc"), DateTimeKind.Utc);
				var returnMessage = (string)command.GetParameterValue("@returnMessage");

				return new SqlMutexLockResult(returnValue, expiresAtSqlTimeUtc, acquiredTimeUtc, returnMessage);
			}
		}

		static SqlMutexLockResult AcquireOrUpdateSqlMutexLock(
			string category,
			string lockInfo,
			TimeSpan timeout,
			string workStationName,
			int processId,
			string userCode,
			bool releaseLock)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return AcquireOrUpdateSqlMutexCore(
					Db.Connection,
					category,
					lockInfo,
					timeout,
					workStationName,
					processId,
					userCode,
					releaseLock);
			}
		}

		public SqlMutexLockResult ReleaseSqlMutexLock(SqlMutexLock mutexLock)
		{
			Argument.NotNull(mutexLock, nameof(mutexLock));
			return AcquireOrUpdateSqlMutexLock(
				mutexLock.Category,
				mutexLock.LockInfo,
				mutexLock.LockTimeout,
				mutexLock.WorkStationName,
				mutexLock.ProcessId,
				mutexLock.UserCode,
				true);
		}

		public SqlMutexLockResult AcquireOrUpdateSqlMutexLock(SqlMutexLock mutexLock)
		{
			Argument.NotNull(mutexLock, nameof(mutexLock));
			return AcquireOrUpdateSqlMutexLock(
				mutexLock.Category,
				mutexLock.LockInfo,
				mutexLock.LockTimeout,
				mutexLock.WorkStationName,
				mutexLock.ProcessId,
				mutexLock.UserCode,
				false);
		}

		public SqlMutexLockResult AcquireOrUpdateSqlMutexLock(
			string category,
			string lockInfo,
			TimeSpan timeout,
			string workStationName,
			int processId,
			string userCode)
		{
			return AcquireOrUpdateSqlMutexLock(
				category,
				lockInfo,
				timeout,
				workStationName,
				processId,
				userCode,
				false);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "no use of Business Object and Business Object Factory in Process Controller")]
		public void ReleaseLocks(SqlMutexLockInfo sqlMutexLockInfo)
		{
			using (var command = Db.Connection.Command(string.Empty))
			{
				var scriptBuilder = new StringBuilder(Invariant($@"
UPDATE [dbo].[StmServiceMutex]
SET
	SMX_TimeoutInSeconds = {TimeOutInSecondsToReleaseLock},
	SMX_ExpiresAtUtc = DATEADD(SECOND, {TimeOutInSecondsToReleaseLock}, GETUTCDATE())
WHERE 1=1
"));
				scriptBuilder.AppendLine();

				#region SuppressResourceStringsCheckRegion

				if (!string.IsNullOrEmpty(sqlMutexLockInfo.Category))
				{
					scriptBuilder.AppendLine("    AND SMX_ServiceClass = @category");
					command.AddParameter("@category", SqlDbType.VarChar, 3, sqlMutexLockInfo.Category);
				}

				if (!string.IsNullOrEmpty(sqlMutexLockInfo.LockInfo))
				{
					scriptBuilder.AppendLine("    AND SMX_LockInfo = @lockInfo");
					command.AddParameter("@lockInfo", SqlDbType.VarChar, 128, sqlMutexLockInfo.LockInfo);
				}

				if (!string.IsNullOrEmpty(sqlMutexLockInfo.WorkStationName))
				{
					scriptBuilder.AppendLine("    AND SMX_WorkStationName = @workStationName");
					command.AddParameter("@workStationName", SqlDbType.NVarChar, 128, sqlMutexLockInfo.WorkStationName);
				}

				if (sqlMutexLockInfo.ProcessId.HasValue)
				{
					scriptBuilder.AppendLine("    AND SMX_ProcessId = @processId");
					command.AddParameter("@processId", SqlDbType.Int, sqlMutexLockInfo.ProcessId.Value);
				}

				#endregion

				command.CommandText = scriptBuilder.ToString();
				command.ExecuteNonQuery();
			}
		}

		internal static readonly Overridable<string> OverridableAcquireOrUpdateSqlMutexProc = new Overridable<string>(AcquireOrUpdateSqlMutexProc);

		internal const string AcquireOrUpdateSqlMutexProc = "AcquireOrUpdateSqlMutex";
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int TimeOutInSecondsToReleaseLock = -10;
	}
}
