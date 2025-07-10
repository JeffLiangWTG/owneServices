using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Semaphores.Common
{
	/// <summary>
	/// Controls database access to the Semaphore tables.
	/// Manages opening and disposing connections and transactions for each DB command.
	/// 
	/// NOTE (DEBUG MODE): If running transactional tests on the main connection,
	/// it uses the main connection instead of creating a new one for each command.
	/// </summary>
	public class SemaphoreDbManager : ISemaphoreDbManager
	{
		public SemaphoreDbManager()
		{
			heartbeatPulsed = new Stopwatch();
		}

		DateTime UtcNow
		{
			get { return ZDateTime.UtcNow.ToDateTime(); }
		}

		#region Connection/Command

		DbConnection GetNewConnection()
		{
			return Db.NewExtraConnectionToMainDb();
		}

		/// <summary>
		/// Returns a database command object on the given connection.
		/// NOTE (DEBUG MODE):
		///		If running transactional tests on the main connection, 
		///		it creates the command on the main connection.
		/// </summary>
		/// <param name="connection">Connection used to create command (see debug mode note above)</param>
		/// <param name="sqlText">Command Text</param>
		DbCommand GetDbCommand(DbConnection connection, string sqlText)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(sqlText, nameof(sqlText));

#if DEBUG
			const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

			object shouldUseMainConnectionValue = null;
			var shouldUseMainConnection = typeof(Db).GetProperty("ShouldUseMainConnection", flags);

			if (shouldUseMainConnection != null)
			{
				shouldUseMainConnectionValue = shouldUseMainConnection.GetValue(Db.Instance, null);
			}

			if (shouldUseMainConnectionValue != null && (bool)shouldUseMainConnectionValue && WTG.TestHelpers.TestingState.InTransactionedTestCase && Db.Connection.IsInTransaction)
			{
				return Db.Connection.Command(sqlText);
			}
#endif

			return connection.Command(sqlText);
		}

		#endregion

		#region Semaphore DB Layer Commands

		#region Heartbeat Commands

		public void CreateHeartbeatInDatabase(Guid heartbeatUniqueId, string hostName, int sessionProcessId, Guid userPk, string userParentTableCode, int pulseInSeconds, string heartbeatType, string clientIdentifier)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			Argument.NotNull(hostName, nameof(hostName));
			using (var connection = GetNewConnection())
			{
				CleanUpOldHeartbeatFromDatabase(connection, userPk, heartbeatType);

				using (var cmd = GetDbCommand(connection, CreateHeartbeatScript))
				{
					cmd.AddParameterBasedOnDbColumn("@heartbeatPk", heartbeatUniqueId, StmServiceHeartBeatSchema.PK);
					cmd.AddParameterBasedOnDbColumn("@hostName", hostName, StmServiceHeartBeatSchema.SV_WorkstationName);
					cmd.AddParameterBasedOnDbColumn("@clientIdentifier", clientIdentifier ?? string.Empty, StmServiceHeartBeatSchema.SV_ClientIdentifier);
					cmd.AddParameterBasedOnDbColumn("@processId", sessionProcessId, StmServiceHeartBeatSchema.SV_ProcessID);
					cmd.AddParameterBasedOnDbColumn("@parentTableCode", userParentTableCode, StmServiceHeartBeatSchema.SV_ParentTableCode);
					cmd.AddParameterBasedOnDbColumn("@userPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
					cmd.AddParameter("@pulseInSeconds", SqlDbType.Int, pulseInSeconds);
					cmd.AddParameter("@utcNow", SqlDbType.DateTime, UtcNow);
					cmd.AddParameterBasedOnDbColumn("@heartbeatType", heartbeatType, StmServiceHeartBeatSchema.SV_HeartbeatType);
					cmd.ExecuteNonQuery();
				}
			}

			heartbeatPulsed.Start();
		}

		public const string CreateHeartbeatScript = @"
			INSERT dbo.StmServiceHeartBeat (SV_PK, SV_WorkstationName, SV_ClientIdentifier, SV_ProcessID, SV_ParentTableCode, SV_ParentId, SV_ExpiresAtUtc, SV_HeartbeatType, SV_CreateTimeUtc) VALUES
				(@heartbeatPk, @hostName, @clientIdentifier, @processId, @parentTableCode, @userPk, DateAdd(second, @pulseInSeconds, @utcNow), @heartbeatType, @utcNow)";

		/// <summary>
		/// Delete from the database Heartbeat records which:
		///	  - have the same HostName+UserId as this process and are older than [MaxDaysToKeepSameUserAndHostExpiredHeartbeat]
		///	  - OR are expired and older than [MaxDaysToKeepExpiredHeartbeat]
		/// </summary>
		void CleanUpOldHeartbeatFromDatabase(DbConnection connection, Guid userPk, string heartbeatType)
		{
			Argument.NotNull(connection, nameof(connection));

			using (var cmd = GetDbCommand(connection, CleanUpSameUserOldHeartbeatScript + CleanUpOldHeartbeatScript))
			{
				cmd.AddParameterBasedOnDbColumn("@userPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
				cmd.AddParameter("@maxDaysToKeepSameUserExpiredHeartbeat", SqlDbType.Int, -MaxDaysToKeepSameUserExpiredHeartbeat);
				cmd.AddParameter("@maxDaysToKeepAnyExpiredHeartbeat", SqlDbType.Int, -MaxDaysToKeepExpiredHeartbeat);
				cmd.AddParameter("@utcNow", SqlDbType.DateTime, UtcNow);
				cmd.AddParameterBasedOnDbColumn("@heartbeatType", heartbeatType, StmServiceHeartBeatSchema.SV_HeartbeatType);
				cmd.ExecuteNonQuery();
			}
		}

		public const string CleanUpSameUserOldHeartbeatScript = @"
			DELETE dbo.StmServiceHeartBeat WITH (READPAST, READCOMMITTEDLOCK)
				WHERE SV_ParentId = @userPk
				AND SV_HeartbeatType = @heartbeatType
				AND SV_ExpiresAtUtc <= dateadd(day, @maxDaysToKeepSameUserExpiredHeartbeat, @utcNow) AND SV_CreateTimeUtc <= dateadd(day, @maxDaysToKeepSameUserExpiredHeartbeat, @utcNow);";

		public const string CleanUpOldHeartbeatScript = @"
			DELETE TOP (50) dbo.StmServiceHeartBeat WITH (READPAST, READCOMMITTEDLOCK)
				WHERE SV_ExpiresAtUtc <= dateadd(day, @maxDaysToKeepAnyExpiredHeartbeat, @utcNow) AND SV_CreateTimeUtc <= dateadd(day, @maxDaysToKeepAnyExpiredHeartbeat, @utcNow);";

		public const int MaxDaysToKeepExpiredHeartbeat = 2;
		public const int MaxDaysToKeepSameUserExpiredHeartbeat = 1;

		/// <summary>
		/// The refreshing of the heartbeat is triggered by a Timer, hence it runs in a separate thread.
		/// </summary>
		/// <param name="heartbeatUniqueId"></param>
		/// <param name="pulseInSeconds"></param>
		/// <param name="upgradeDateTimeUtc"></param>
		/// <param name="isForegroundThead"></param>
		/// <returns>true if refreshed, false if it has been forced expired by RemoteLogoff</returns>
		public virtual bool RefreshHeartbeatInDatabase(Guid heartbeatUniqueId, TimeSpan heartbeatDuration, bool isNewConnection)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			if (!IsItTimeToRefreshHeatbeat(heartbeatDuration))
			{
				return true;
			}

			var refreshed = true;

			if (!isNewConnection)
			{
				return ExecuteHeartbeatRefresh(heartbeatUniqueId, heartbeatDuration, Db.Connection, isNewConnection);
			}

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
#if DEBUG
					Db.DbStateLock.AcquireReaderLock(TimeSpan.FromSeconds(30));
#endif
					if (!((IDbReconnectionHandling)connection).HasDbSchemaOrScriptOrTransformationVersionChanged())
					{
						refreshed = ExecuteHeartbeatRefresh(heartbeatUniqueId, heartbeatDuration, connection, isNewConnection);
					}
				}
				catch (SqlException ex)
				{
					if (!Db.IsUpgradeLockoutError(ex))
					{
#if DEBUG
						var errorType = new DbErrorMatch(ex).ExceptionType;
						if (!(WTG.TestHelpers.TestingState.IsTest && errorType == DbErrorType.CouldNotFindStoredProcedure))
#endif
						{
							throw;
						}
					}
				}
#if DEBUG
				finally
				{
					Db.DbStateLock.ReleaseReaderLock();
				}
#endif
			}
			return refreshed;
		}

		public bool UserPkInDatabase(Guid userPk)
		{
			using (var connection = GetNewConnection())
			using (var cmd = GetDbCommand(connection, UserPkInDatabaseScript))
			{
				cmd.AddParameterBasedOnDbColumn("@userPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
				var result = cmd.ExecuteScalar();
				var count = (int)result;
				if (count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public const string UserPkInDatabaseScript = @"
			SELECT count(*) from dbo.StmServiceHeartBeat WHERE SV_ParentId = @userPk;";

		public virtual bool CheckHeartbeatExpiredTimeIsExpired(Guid heartbeatUniqueId)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}
			using (var connection = GetNewConnection())
			using (var cmd = GetDbCommand(connection, CheckHeartbeatExpiredTimeIsExpiredScript))
			{
				cmd.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatUniqueId);
				cmd.AddParameter("@utcNow", SqlDbType.DateTime, UtcNow);
				var result = cmd.ExecuteScalar();
				var count = (int)result;
				if (count > 0)
				{
					return false;
				}
			}
			return true;
		}

		public const string CheckHeartbeatExpiredTimeIsExpiredScript = @"
			SELECT count(*) from dbo.StmServiceHeartBeat WHERE SV_PK = @heartbeatPk AND SV_ExpiresAtUtc > @utcNow;";

		protected virtual bool ExecuteHeartbeatRefresh(Guid heartbeatUniqueId, TimeSpan hearbeatDuration, DbConnection connection, bool isNewConnection)
		{
			Argument.NotNull(connection, nameof(connection));

			heartbeatPulsed.Restart();

			var refreshed = false;

			using (var command = GetDbCommand(connection, KeepHeartbeatAliveProcedure))
			{
				command.AddParameter("@isNewConnection", SqlDbType.Bit, isNewConnection);
				command.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatUniqueId);
				command.AddParameter("@pulseInSeconds", SqlDbType.Int, (int)hearbeatDuration.TotalSeconds);
				command.AddOutputParameter("@currentUTC", SqlDbType.DateTime, 0, 0, 0, DBNull.Value);

				var rowsAffected = command.ExecuteProcedureWithReturnValue();
				refreshed = (rowsAffected > 0);
			}

			return refreshed;
		}

		protected virtual bool IsItTimeToRefreshHeatbeat(TimeSpan hearbeatDuration)
		{
			return heartbeatPulsed.Elapsed >= GetStopwatchThreshold(hearbeatDuration);
		}

		readonly Stopwatch heartbeatPulsed;

		public static TimeSpan GetStopwatchThreshold(TimeSpan hearbeatDuration)
		{
			return TimeSpan.FromMilliseconds(hearbeatDuration.TotalMilliseconds * Heartbeat.DURATION_GUARANTEE_PERCENTAGE * 0.75);
		}

		public const string KeepHeartbeatAliveProcedure = "dbo.SemaphoreKeepSessionAlive";

		/// <summary>
		/// Updates Heartbeat Session user context (after login).
		/// Also deletes from the database Heartbeat records which have the same UserId as this process
		/// and are older than [MaxDaysToKeepSameUserExpiredHeartbeat]
		/// </summary>
		public void UpdateUserContext(Guid heartbeatUniqueId, string hostName, Guid userPk, string userParentTableCode, string heartbeatType)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			Argument.NotNull(hostName, nameof(hostName));

			using (var connection = GetNewConnection())
			{
				using (var cmd = GetDbCommand(connection, CleanUpSameUserOldHeartbeatScript + UpdateUserContextScript))
				{
					cmd.AddParameterBasedOnDbColumn("@userPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
					cmd.AddParameterBasedOnDbColumn("@tableCode", userParentTableCode, StmServiceHeartBeatSchema.SV_ParentTableCode);
					cmd.AddParameterBasedOnDbColumn("@heartbeatPk", heartbeatUniqueId, StmServiceHeartBeatSchema.PK);
					cmd.AddParameterBasedOnDbColumn("@hostName", hostName, StmServiceHeartBeatSchema.SV_WorkstationName);
					cmd.AddParameter("@maxDaysToKeepSameUserExpiredHeartbeat", SqlDbType.Int, -MaxDaysToKeepSameUserExpiredHeartbeat);
					cmd.AddParameter("@utcNow", SqlDbType.DateTime, UtcNow);
					cmd.AddParameterBasedOnDbColumn("@heartbeatType", heartbeatType, StmServiceHeartBeatSchema.SV_HeartbeatType);
					cmd.ExecuteNonQuery();
				}
			}
		}

		public const string UpdateUserContextScript = @"
			UPDATE dbo.StmServiceHeartBeat SET
				SV_ParentId        = @userPk,
				SV_ParentTableCode = @tableCode,
				SV_SystemLastEditTimeUtc = GetUtcDate(),
				SV_SystemLastEditUser = SV_SystemLastEditUser
			WHERE
				SV_PK = @heartbeatPk";

		public void DeleteHeartbeatFromDatabase(Guid heartbeatUniqueId)
		{
			using (var connection = GetNewConnection())
			using (var cmd = GetDbCommand(connection, DeleteHeartbeatScript))
			{
				cmd.AddParameterBasedOnDbColumn("@heartbeatPk", heartbeatUniqueId, StmServiceHeartBeatSchema.PK);
				cmd.ExecuteNonQuery();
			}
		}

		public const string DeleteHeartbeatScript = @"DELETE dbo.StmServiceHeartBeat WHERE SV_PK = @heartbeatPk";

		#endregion

		#region Semaphore Commands

		public Guid CreateSemaphoreHandleInTransaction(Guid heartbeatUniqueId, string lockInfo, string category, int maxConcurrentHandles)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));

			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return CreateSemaphoreHandleInTransactionInternal(connection, heartbeatUniqueId, lockInfo,
							category, maxConcurrentHandles);
					}
				}
				else
				{
					return CreateSemaphoreHandleInTransactionInternal(Db.Connection, heartbeatUniqueId, lockInfo,
						category, maxConcurrentHandles);
				}
			}
		}

		public Guid LoadSemaphoreHandle(Guid heartbeatUniqueId, string lockInfo, string category)
		{
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));

			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return LoadSemaphoreHandleInternal(connection, heartbeatUniqueId, lockInfo, category);
					}
				}
				else
				{
					return LoadSemaphoreHandleInternal(Db.Connection, heartbeatUniqueId, lockInfo, category);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		Guid CreateSemaphoreHandleInTransactionInternal(DbConnection connection, Guid heartbeatUniqueId, string lockInfo, string category, int maxConcurrentHandles)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));

			var handleId = Guid.Empty;

			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				var value = GetParameterValueFromScript(connection, heartbeatUniqueId, lockInfo, category, maxConcurrentHandles);
				handleId = value == null || DBNull.Value.Equals(value) ? default : (Guid)value;

				transactionManager.CommitTransaction();
			}

			return handleId;
		}

		protected virtual object GetParameterValueFromScript(DbConnection connection, Guid heartbeatUniqueId, string lockInfo, string category, int maxConcurrentHandles)
		{
			Argument.NotNull(connection, nameof(connection));
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));

			using (var command = connection.Command(CreateSemaphoreHandleScript))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandTimeout = 1;
				command.AddParameter("@currentTimeUtc", SqlDbType.DateTime, UtcNow);
				command.AddParameter("@maxConcurrentHandles", SqlDbType.Int, maxConcurrentHandles);
				command.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatUniqueId);
				command.AddParameter("@lockInfo", SqlDbType.VarChar, 128, lockInfo);
				command.AddParameter("@category", SqlDbType.VarChar, 3, category);
				command.AddParameter("@newHandleId", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddOutputParameter("@actualHandleId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.ExecuteNonQuery();

				return command.GetParameterValue("@actualHandleId");
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql statement command, internal purpose string")]
		Guid LoadSemaphoreHandleInternal(DbConnection connection, Guid heartbeatUniqueId, string lockInfo, string category)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));

			var handleId = Guid.Empty;

			using (var cmd = connection.Command("select SS_PK from dbo.StmServiceSemaphore where SS_LockInfo = @lockinfo and SS_ServiceClass = @category and SS_SV = @heartbeatPk "))
			{
				cmd.AddParameterBasedOnDbColumn("@heartbeatPk", heartbeatUniqueId, StmServiceSemaphoreSchema.SS_SV);
				cmd.AddParameterBasedOnDbColumn("@lockInfo", lockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
				cmd.AddParameterBasedOnDbColumn("@category", category, StmServiceSemaphoreSchema.SS_ServiceClass);

				var result = cmd.ExecuteScalar();
				if (result is Guid)
				{
					handleId = (Guid)result;
				}
			}
			return handleId;
		}

		public const string CreateSemaphoreHandleScript = "InsertSemaphoreHandle";

		#region GetActiveSemaphoreHandles for Semaphore Type

		public ISemaphoreInfo[] GetActiveSemaphoreHandles(ISemaphoreType semaphore)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return GetActiveSemaphoreHandlesInternal(connection, semaphore);
					}
				}
				else
				{
					return GetActiveSemaphoreHandlesInternal(Db.Connection, semaphore);
				}
			}
		}

		ISemaphoreInfo[] GetActiveSemaphoreHandlesInternal(DbConnection connection, ISemaphoreType semaphore)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(semaphore, nameof(semaphore));

			var result = new List<ISemaphoreInfo>();

			using (var cmd = connection.Command(ActiveSemaphoreHandlesScript))
			{
				cmd.AddParameterBasedOnDbColumn("@lockInfo", semaphore.LockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						IHeartbeatInfo sessionInfo = new HeartbeatInfo(
							(Guid)reader["HeartbeatId"],
							reader["HostName"].ToString(),
							(Guid)reader["UserPk"],
							reader["FullUserName"] != null ? reader["FullUserName"].ToString() : string.Empty,
							reader["Code"] != null ? reader["Code"].ToString() : string.Empty,
							reader["EmailAddress"] != null ? reader["EmailAddress"].ToString() : string.Empty,
							GetLogonType(reader["UserType"].ToString()),
							(int)reader["ProcessId"],
							reader["HeartbeatType"].ToString(),
							reader["ClientIdentifier"].ToString());

						var createTimeUtc = (reader["CreateTimeUtc"] == null || reader["CreateTimeUtc"] == DBNull.Value) ?
							 DateTime.MinValue : (DateTime)reader["CreateTimeUtc"];

						ISemaphoreInfo semaphoreInfo = new SemaphoreInfo(sessionInfo, semaphore, createTimeUtc);
						result.Add(semaphoreInfo);
					}
				}
			}

			return result.ToArray();
		}

		public HashSet<ZGuid> GetActiveSemaphoreLockInfo(string lockInfoPrefix)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return GetActiveSemaphoreLockInfoInternal(connection, lockInfoPrefix);
					}
				}
				else
				{
					return GetActiveSemaphoreLockInfoInternal(Db.Connection, lockInfoPrefix);
				}
			}
		}

		HashSet<ZGuid> GetActiveSemaphoreLockInfoInternal(DbConnection connection, string lockInfoPrefix)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new HashSet<ZGuid>();
			var sqlText = string.Format(CultureInfo.InvariantCulture, ActiveSemaphoreLockInfoScript, lockInfoPrefix);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						if (ZGuid.TryParse(reader[0], out var zguid))
						{
							result.Add(zguid);
						}
					}
				}
			}

			return result;
		}

		const string ActiveSemaphoreLockInfoScript = @"
			SELECT
					RIGHT(SS_LockInfo, LEN(SS_LockInfo) - LEN('{0}'))
			FROM
					dbo.StmServiceSemaphore WITH (READPAST, READCOMMITTEDLOCK)
					INNER JOIN dbo.StmServiceHeartBeat WITH (READPAST, READCOMMITTEDLOCK) ON SS_SV = SV_PK
			WHERE
					LEFT(SS_LockInfo, LEN('{0}')) = '{0}'
					AND SV_ExpiresAtUtc > @utcNow;
		";

		const string ActiveSemaphoreHandlesScript = @"
			SELECT
					SV_PK HeartbeatId,
					SV_WorkstationName HostName,
					coalesce(GS_Code, OH_Code) Code,
					coalesce(GS_FullName, OC_ContactName) FullUserName,
					coalesce(GS_EmailAddress, OC_Email) EmailAddress,
					SV_ParentTableCode UserType,
					SV_ParentId UserPk,
					SV_ProcessID ProcessId,
					SV_HeartbeatType HeartbeatType,
					SS_AcquiredTimeUtc CreateTimeUtc,
					SV_ClientIdentifier ClientIdentifier
				FROM
					dbo.StmServiceSemaphore WITH (READPAST, READCOMMITTEDLOCK)
					INNER JOIN dbo.StmServiceHeartBeat WITH (READPAST, READCOMMITTEDLOCK) ON SS_SV = SV_PK
					LEFT JOIN dbo.GlbStaff ON GS_PK = SV_ParentId
					LEFT JOIN dbo.OrgContact ON OC_PK = SV_ParentId
					LEFT JOIN dbo.OrgHeader ON OH_PK = OC_OH
					WHERE
						SS_LockInfo = @lockInfo
						AND SV_ExpiresAtUtc > @utcNow;";

		#endregion

		#region GetActiveSemaphoreHandles for User

		public ISemaphoreInfo[] GetActiveSemaphoreHandles(Guid heartbeatId)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return GetActiveSemaphoreHandlesInternal(connection, heartbeatId);
					}
				}
				else
				{
					return GetActiveSemaphoreHandlesInternal(Db.Connection, heartbeatId);
				}
			}
		}

		LogonType GetLogonType(string code)
		{
			switch (code)
			{
				case GlbStaffSchema.Constants.Prefix:
					return LogonType.Staff;
				case OrgContactSchema.Constants.Prefix:
					return LogonType.Contact;

				default:
					return LogonType.Unknown;
			}
		}

		ISemaphoreInfo[] GetActiveSemaphoreHandlesInternal(DbConnection connection, Guid heartbeatId)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new List<ISemaphoreInfo>();

			using (var cmd = connection.Command(ActiveSemaphoreHandlesForUserScript))
			{
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);
				cmd.AddParameterBasedOnDbColumn("@heartbeatId", heartbeatId, StmServiceHeartBeatSchema.PK);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						IHeartbeatInfo sessionInfo = new HeartbeatInfo(
							heartbeatId,
							reader["HostName"].ToString(),
							(Guid)reader["UserPk"],
							reader["FullUserName"] != null ? reader["FullUserName"].ToString() : string.Empty,
							reader["Code"] != null ? reader["Code"].ToString() : string.Empty,
							reader["EmailAddress"] != null ? reader["EmailAddress"].ToString() : string.Empty,
							GetLogonType(reader["UserType"].ToString()),
							(int)reader["ProcessId"],
							reader["HeartbeatType"].ToString(),
							reader["ClientIdentifier"].ToString());

						var createTimeUtc =
							(reader["CreateTimeUtc"] == null || reader["CreateTimeUtc"] == DBNull.Value)
								? DateTime.MinValue
								: (DateTime)reader["CreateTimeUtc"];

						ISemaphoreType semaphoreType = new CommonSemaphoreType(
							reader["LockInfo"].ToString(),
							reader["Category"].ToString(),
							(int)reader["UseCount"]);

						ISemaphoreInfo semaphoreInfo = new SemaphoreInfo(sessionInfo, semaphoreType, createTimeUtc);
						result.Add(semaphoreInfo);
					}
				}
			}

			return result.ToArray();
		}

		const string ActiveSemaphoreHandlesForUserScript = @"
			SELECT
				SV_WorkstationName HostName,
				SV_ParentId UserPk,
				coalesce(GS_FullName, OC_ContactName) FullUserName,
				coalesce(GS_Code, OH_Code) Code,
				coalesce(GS_EmailAddress, OC_Email) EmailAddress,
				SV_ParentTableCode UserType,
				SV_ProcessID ProcessId,
				SV_HeartbeatType HeartbeatType,
				SS_AcquiredTimeUtc CreateTimeUtc,
				SS_ServiceClass Category,
				SS_LockInfo LockInfo,
				SS_UseCount UseCount,
				SV_ClientIdentifier ClientIdentifier
			FROM
				dbo.StmServiceSemaphore WITH (READPAST, READCOMMITTEDLOCK)
				INNER JOIN dbo.StmServiceHeartBeat WITH (READPAST, READCOMMITTEDLOCK) ON SS_SV = SV_PK
				LEFT JOIN dbo.GlbStaff ON GS_PK = SV_ParentId
				LEFT JOIN dbo.OrgContact ON OC_PK = SV_ParentId
				LEFT JOIN dbo.OrgHeader ON OH_PK = OC_OH
			WHERE
				SV_PK = @heartbeatId
				AND SV_ExpiresAtUtc > @utcNow;";

		#endregion

		#region GetRemoteActiveSemaphoreHandles

		public ISemaphoreInfo[] GetRemoteActiveSemaphoreHandles(ISemaphoreType semaphore, Guid userPk, string hostName, string clientIdentifier)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						return GetRemoteActiveSemaphoreHandlesInternal(connection, semaphore, userPk, hostName, clientIdentifier);
					}
				}
				else
				{
					return GetRemoteActiveSemaphoreHandlesInternal(Db.Connection, semaphore, userPk, hostName, clientIdentifier);
				}
			}
		}

		ISemaphoreInfo[] GetRemoteActiveSemaphoreHandlesInternal(DbConnection connection, ISemaphoreType semaphore, Guid userPk, string hostName, string clientIdentifier)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(semaphore, nameof(semaphore));

			if (hostName != null)
			{
				hostName = hostName.Split('/').LastOrDefault();
			}

			var result = new List<ISemaphoreInfo>();

			using (var cmd = connection.Command(ActiveSemaphoreHandlesOneUserScript))
			{
				cmd.AddParameterBasedOnDbColumn("@LockInfo", semaphore.LockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
				cmd.AddParameterBasedOnDbColumn("@UserPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
				cmd.AddParameterBasedOnDbColumn("@HostName", hostName, StmServiceHeartBeatSchema.SV_WorkstationName);
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);
				cmd.AddParameterBasedOnDbColumn("@clientIdentifier", clientIdentifier ?? string.Empty, StmServiceHeartBeatSchema.SV_ClientIdentifier);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						IHeartbeatInfo sessionInfo = new HeartbeatInfo(
							(Guid)reader["HeartbeatId"],
							reader["HostName"].ToString(),
							userPk,
							reader["FullUserName"] != null ? reader["FullUserName"].ToString() : string.Empty,
							reader["Code"] != null ? reader["Code"].ToString() : string.Empty,
							reader["EmailAddress"] != null ? reader["EmailAddress"].ToString() : string.Empty,
							GetLogonType(reader["UserType"].ToString()),
							(int)reader["ProcessId"],
							reader["HeartbeatType"].ToString(),
							reader["ClientIdentifier"].ToString());

						var createTimeUtc = (reader["CreateTimeUtc"] == null || reader["CreateTimeUtc"] == DBNull.Value) ?
							 DateTime.MinValue : (DateTime)reader["CreateTimeUtc"];

						ISemaphoreInfo semaphoreInfo = new SemaphoreInfo(sessionInfo, semaphore, createTimeUtc);
						result.Add(semaphoreInfo);
					}
				}
			}

			return result.ToArray();
		}

		const string ActiveSemaphoreHandlesOneUserScript = @"
			SELECT
				SV_PK HeartbeatId,
				SV_WorkstationName HostName,
				coalesce(GS_FullName, OC_ContactName) FullUserName,
				coalesce(GS_Code, OH_Code) Code,
				coalesce(GS_EmailAddress, OC_Email) EmailAddress,
				SV_ParentTableCode UserType,
				SV_ProcessID ProcessId,
				SV_HeartbeatType HeartbeatType,
				SS_AcquiredTimeUtc CreateTimeUtc,
				SV_ClientIdentifier ClientIdentifier
			FROM
				dbo.StmServiceSemaphore WITH(READPAST, READCOMMITTEDLOCK)
				INNER JOIN dbo.StmServiceHeartBeat WITH(READPAST, READCOMMITTEDLOCK) ON SS_SV = SV_PK
				LEFT JOIN dbo.GlbStaff ON GS_PK = SV_ParentId
				LEFT JOIN dbo.OrgContact ON OC_PK = SV_ParentId
				LEFT JOIN dbo.OrgHeader ON OH_PK = OC_OH
			WHERE
				SS_LockInfo = @LockInfo
				AND SV_ExpiresAtUtc > @utcNow
				AND SV_ParentId = @UserPk
				AND (
						-- RDP check for matching workstation name
						(@clientIdentifier = '' AND SV_ClientIdentifier = '' AND SV_WorkstationName <> @HostName AND SV_WorkstationName NOT LIKE '%/' + @HostName)
						OR
						-- Winzor check for matching client identifier
						(@clientIdentifier <> '' AND SV_ClientIdentifier <> '' AND SV_ClientIdentifier <> @clientIdentifier)
				);";

		#endregion

		#region RemoteLogoff

		public void RemoteLogoff(Guid userPk, string hostName, string heartbeatType, string clientIdentifier)
		{
			Argument.NotNull(hostName, nameof(hostName));

			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						RemoteLogoffInternal(connection, userPk, hostName, heartbeatType, clientIdentifier);
					}
				}
				else
				{
					RemoteLogoffInternal(Db.Connection, userPk, hostName, heartbeatType, clientIdentifier);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql parameter name")]
		void RemoteLogoffInternal(DbConnection connection, Guid userPk, string hostName, string heartbeatType, string clientIdentifier)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(hostName, nameof(hostName));

			var sql = @"
			UPDATE dbo.StmServiceHeartBeat SET
				SV_ExpiresAtUtc = @utcNow,
				SV_SystemLastEditTimeUtc = @utcNow,
				SV_SystemLastEditUser = @user
			WHERE SV_ParentId = @UserPK
				AND (
						-- RDP check for matching workstation name
						(@clientIdentifier = '' AND SV_ClientIdentifier = '' AND SV_WorkstationName <> @HostName)
						OR
						-- Winzor check for matching client identifier
						(@clientIdentifier <> '' AND SV_ClientIdentifier <> '' AND SV_ClientIdentifier <> @clientIdentifier)
				)
				AND SV_HeartbeatType = @HeartbeatType
				AND SV_ExpiresAtUtc > @utcNow;";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@UserPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
				cmd.AddParameterBasedOnDbColumn("@HostName", hostName, StmServiceHeartBeatSchema.SV_WorkstationName);
				cmd.AddParameterBasedOnDbColumn("@HeartbeatType", heartbeatType, StmServiceHeartBeatSchema.SV_HeartbeatType);
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);
				cmd.AddParameterBasedOnDbColumn("@user", Db.GetCurrentUserOrDefault(), StmServiceHeartBeatSchema.SV_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@clientIdentifier", clientIdentifier ?? string.Empty, StmServiceHeartBeatSchema.SV_ClientIdentifier);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Release Locks

		public void ReleaseLocks(string lockInfo, string heartbeatType, Guid userPk)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						ReleaseLocksInternal(connection, lockInfo, heartbeatType, userPk);
					}
				}
				else
				{
					ReleaseLocksInternal(Db.Connection, lockInfo, heartbeatType, userPk);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql parameter name")]
		void ReleaseLocksInternal(DbConnection connection, string lockInfo, string heartbeatType, Guid userPk)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = @"
			UPDATE dbo.StmServiceHeartBeat SET
				SV_ExpiresAtUtc = @utcNow,
				SV_SystemLastEditTimeUtc = @utcNow,
				SV_SystemLastEditUser = @user
			WHERE
				SV_ParentId = @UserPK
				AND SV_PK IN (SELECT SS_SV FROM dbo.StmServiceSemaphore WHERE SS_LockInfo = @LockInfo)
				AND SV_HeartbeatType = @HeartbeatType
				AND SV_ExpiresAtUtc > @utcNow";

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@UserPk", userPk, StmServiceHeartBeatSchema.SV_ParentId);
				cmd.AddParameterBasedOnDbColumn("@LockInfo", lockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
				cmd.AddParameterBasedOnDbColumn("@HeartbeatType", heartbeatType, StmServiceHeartBeatSchema.SV_HeartbeatType);
				cmd.AddParameterBasedOnDbColumn("@utcNow", UtcNow, StmServiceHeartBeatSchema.SV_ExpiresAtUtc);
				cmd.AddParameterBasedOnDbColumn("@user", Db.GetCurrentUserOrDefault(), StmServiceHeartBeatSchema.SV_SystemLastEditUser);
				cmd.ExecuteNonQuery();
			}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
		}

		#endregion

		public void RemoveSemaphore(ISemaphoreType semaphore)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using var connection = GetNewConnection();
					RemoveSemaphoreInternal(connection, semaphore);
				}
				else
				{
					RemoveSemaphoreInternal(Db.Connection, semaphore);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql parameter name")]
		void RemoveSemaphoreInternal(DbConnection connection, ISemaphoreType semaphore)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(semaphore, nameof(semaphore));

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using var cmd = connection.Command("DELETE FROM dbo.StmServiceSemaphore WHERE SS_ServiceClass = @category AND SS_LockInfo = @lockinfo");
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
			cmd.AddParameterBasedOnDbColumn("@category", semaphore.Category, StmServiceSemaphoreSchema.SS_ServiceClass);
			cmd.AddParameterBasedOnDbColumn("@lockInfo", semaphore.LockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
			cmd.ExecuteNonQuery();
		}

		public void DeleteSemaphoreHandleFromDatabase(Guid handleUniqueId)
		{
			if (handleUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(handleUniqueId));
			}

			using (Db.DisposableActionForDbConnection())
			{
				if (UseNewDbConnection)
				{
					using (var connection = GetNewConnection())
					{
						DeleteSemaphoreHandleFromDatabaseInternal(connection, handleUniqueId);
					}
				}
				else
				{
					DeleteSemaphoreHandleFromDatabaseInternal(Db.Connection, handleUniqueId);
				}
			}
		}

		void DeleteSemaphoreHandleFromDatabaseInternal(DbConnection connection, Guid handleUniqueId)
		{
			Argument.NotNull(connection, nameof(connection));

			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				using (var cmd = connection.Command(DeleteSemaphoreHandleScript))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@semaphorePk", SqlDbType.UniqueIdentifier, handleUniqueId);
					cmd.ExecuteNonQuery();
				}
				transactionManager.CommitTransaction();
			}
		}

		public const string DeleteSemaphoreHandleScript = "RemoveSemaphoreHandle";

		bool UseNewDbConnection
		{
			get
			{
				var useNewDbConnection = DbEnv.Instance.ConnectionPooling.IsPooling || Db.Connection.IsInTransactionOtherThanTransactionedTestCase || Db.Connection.IsDelayedTransaction;
#if DEBUG
				useNewDbConnection = useNewDbConnection && !shouldUseTheSameDbConnection_ForTestOnly;
#endif
				return useNewDbConnection;
			}
		}

#if DEBUG
		[ThreadStatic]
		static bool shouldUseTheSameDbConnection_ForTestOnly;

		public static IDisposable ForceToUseTheSameDbConnection_ForTestOnly()
		{
			shouldUseTheSameDbConnection_ForTestOnly = true;
			return new DisposableAction(() =>
			{
				shouldUseTheSameDbConnection_ForTestOnly = false;
			});
		}
#endif

		#endregion

		#endregion
	}
}
