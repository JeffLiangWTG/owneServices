using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public class SecondaryServerInstance : DbServerInstance, ISecondaryServerInstance
	{
		public SecondaryServerInstance(SqlServerInfo serverInfo, IPrimaryServerInstance primaryServer)
			: base(serverInfo)
		{
			this.primaryServer = primaryServer;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		bool ISecondaryServerInstance.EnsureOdysseyAdminLogin(IEnumerable<IDatabasePreJoinStatus> databases, bool force)
		{
			try
			{
				lastErrorMessage = null;

				if (odysseyAdminLogin == primaryServer.OdysseyAdminLogin)
				{
					return true;
				}

				var dbName = databases.FirstOrDefault(x => !x.IsSecondaryRestoring)?.Name;
				if (databases.Any() && string.IsNullOrEmpty(dbName))
				{
					return false;
				}

				var sqlContext = GetSqlExecutionContextForServer();

				if (!force && !DbManager.IsDbWriteableAndOnline(sqlContext, dbName))
				{
					return false;
				}

				DbSecurity.PropagateOdysseyAdminLogin(serverInfo, primaryServer.OdysseyAdminLogin);
				sqlContext = GetSqlExecutionContextForServer();

				odysseyAdminLogin = DbSecurity.GetDbLoginInfo(sqlContext, OdysseyAdminCredentials.AdminUserName, true);

				return odysseyAdminLogin == primaryServer.OdysseyAdminLogin;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = ex is AlwaysOnException ? ex.Message : ex.ToString();
			}

			return false;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		IEnumerable<IDatabasePreJoinStatus> ISecondaryServerInstance.GetPreJoinDatabaseStatuses(IDictionary<string, DbFileAndTransactionLogInfo> currentGroupDatabases, Guid currentGroupId)
		{
			IEnumerable<IDatabasePreJoinStatus> result;

			try
			{
				lastErrorMessage = null;

				var sqlContext = GetSqlExecutionContextForServer();
				result = GetPreJoinDatabaseStatusList(sqlContext, currentGroupDatabases, currentGroupId);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = null;
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		void ISecondaryServerInstance.EnsureAlwaysOnEndpoint(int primaryServerEndpointPort, string sqlServiceAccount)
		{
			try
			{
				lastErrorMessage = null;
				var sqlContext = GetSqlExecutionContextForServer();

				if (alwaysOnEndpointPort <= 0)
				{
					CreateMirroringEndpoint(sqlContext, primaryServerEndpointPort);
					alwaysOnEndpointPort = primaryServerEndpointPort;
				}

				EnsureEndpointCommunication(sqlContext, sqlServiceAccount);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		void ISecondaryServerInstance.EnsureDatabaseLogins(IEnumerable<DbLoginInfo> primaryDbLogins)
		{
			try
			{
				lastErrorMessage = null;
				var sqlContext = GetSqlExecutionContextForServer();

				DbManager.PropagateDatabaseLogins(sqlContext, primaryDbLogins);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		void ISecondaryServerInstance.JoinGroup(IAvailabilityGroup group)
		{
			try
			{
				lastErrorMessage = null;

				JoinServerToGroup(GetSqlExecutionContextForServer(), group.GroupName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Avoid unhandled exceptions. Errors are then logged in the UI.")]
		void ISecondaryServerInstance.JoinDatabases(IAvailabilityGroup group, IEnumerable<IDatabasePreJoinStatus> databases)
		{
			var sqlContext = GetSqlExecutionContextForServer();

			try
			{
				lastErrorMessage = null;

				JoinSecondaryDatabases(sqlContext, group, databases);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		#region Load Data

		IEnumerable<IDatabasePreJoinStatus> GetPreJoinDatabaseStatusList(ISqlExecutionContext sqlContext, IDictionary<string, DbFileAndTransactionLogInfo> currentGroupDatabases, Guid currentGroupId)
		{
			if (currentGroupDatabases == null || !currentGroupDatabases.Any())
			{
				throw new ArgumentException("Group database list cannot be empty", nameof(currentGroupDatabases));
			}

			var sqlText = Invariant($@"
SELECT
	groupDb.database_name,
	db.[state],
	db.state_desc,
	gdb.group_id,
	mf.redo_start_lsn,
	CASE mf.[type] WHEN 0 THEN 'D' ELSE 'L' END file_type,
	mf.name AS db_file_name,
	mf.physical_name AS db_file_path,
	ISNULL(gdb.is_suspended, 0) is_suspended
FROM
	(VALUES {string.Join(",", currentGroupDatabases.Keys.Select(db => "('" + db + "')"))}) AS groupDb (database_name)
	LEFT JOIN sys.databases db ON groupDb.database_name = db.name
	LEFT JOIN sys.dm_hadr_database_replica_states gdb ON gdb.group_database_id = db.group_database_id
	LEFT JOIN sys.master_files mf ON mf.database_id = db.database_id
ORDER BY
	groupDb.database_name, mf.[type], mf.name
");

			var dbPreJoinStatusList = new List<IDatabasePreJoinStatus>();

			using (var reader = sqlContext.ExecuteReader(sqlText))
			{
				string dbName = null;
				var dbExists = false;
				sbyte state = -1;
				var stateDescription = "-";
				var groupId = Guid.Empty;
				var redoLsn = -1m;
				DatabaseFileCollection dbFiles = null;
				var isDataMovementSuspended = false;

				while (reader.Read())
				{
					var dbNameObj = reader[0] ?? throw new InvalidOperationException("database_name column returned a null value");

					if (dbName != dbNameObj.ToString())
					{
						if (dbName != null)
						{
							var secondaryDbInfo = (dbExists)
								? new DbFileAndTransactionLogInfo(dbName, redoLsn, dbFiles)
								: null;

							dbPreJoinStatusList.Add(new DatabasePreJoinStatus(
								dbName, secondaryDbInfo, currentGroupDatabases[dbName], isDataMovementSuspended, state, stateDescription, groupId, currentGroupId));
						}

						dbName = reader[0].ToString();
						isDataMovementSuspended = (Convert.ToInt32(reader[8]) == 1);
						var stateObj = reader[1];

						dbExists = (stateObj != DBNull.Value);

						if (dbExists)
						{
							state = (sbyte)Convert.ToByte(stateObj, CultureInfo.InvariantCulture);
							stateDescription = reader[2].ToString();

							var groupIdObj = reader[3];

							groupId = (groupIdObj == DBNull.Value) ? Guid.Empty : (Guid)groupIdObj;
							redoLsn = -1m;
							dbFiles = new DatabaseFileCollection();
						}
						else
						{
							groupId = Guid.Empty;
							redoLsn = -1m;
							dbFiles = null;
						}
					}

					if (dbExists)
					{
						var redoLsnObj = reader[4];
						if (redoLsn == -1m && redoLsnObj != DBNull.Value)
						{
							redoLsn = Convert.ToDecimal(redoLsnObj);
						}

						var fileTypeObj = reader[5];
						var fileLogicalNameObj = reader[6];
						var filePhysicalPathObj = reader[7];

						if (fileTypeObj == null || fileLogicalNameObj == null || filePhysicalPathObj == null)
						{
							throw new InvalidOperationException("file_type or db_file_name or db_file_path column returned a null value");
						}

						var fileType = fileTypeObj.ToString();
						var fileLogicalName = fileLogicalNameObj.ToString();
						var filePhysicalPath = filePhysicalPathObj.ToString();

						dbFiles.Add(new DatabaseFile(fileLogicalName, filePhysicalPath, fileType));
					}
				}

				if (dbName != null)
				{
					var secondaryDbInfo = (dbExists)
						? new DbFileAndTransactionLogInfo(dbName, redoLsn, dbFiles)
						: null;

					dbPreJoinStatusList.Add(new DatabasePreJoinStatus(
						dbName, secondaryDbInfo, currentGroupDatabases[dbName], isDataMovementSuspended, state, stateDescription, groupId, currentGroupId));
				}
			}

			return dbPreJoinStatusList;
		}

		void JoinServerToGroup(ISqlExecutionContext sqlContext, string groupName)
		{
			var sqlText = Invariant($"ALTER AVAILABILITY GROUP [{groupName}] JOIN");

			sqlContext.ExecuteNonQuery(sqlText);
		}

		#endregion //Load Data

		#region Restore And Join Databases

		void JoinSecondaryDatabases(ISqlExecutionContext sqlContext, IAvailabilityGroup group, IEnumerable<IDatabasePreJoinStatus> databases)
		{
			foreach (var database in databases)
			{
				if (database.JoinLevel != PreJoinLevel.Already_joined && !IsDatabaseAlreadyJoined(sqlContext, database.Name, group.GroupId))
				{
					switch (database.JoinLevel)
					{
						case PreJoinLevel.Ready_to_be_joined:
							JoinDatabaseWithRetry(sqlContext, group.GroupName, database.Name);
							break;

						case PreJoinLevel.Full_restore_required:
							RestoreAndJoin(sqlContext, group.GroupName, database.Name, requiresFullRestore: true);
							break;

						case PreJoinLevel.Partial_restore_required:
							RestoreAndJoin(sqlContext, group.GroupName, database.Name, requiresFullRestore: false);
							break;
					}
				}
			}
		}

		bool IsDatabaseAlreadyJoined(ISqlExecutionContext sqlContext, string dbName, Guid groupId)
		{
			var sqlText = Invariant($@"
IF exists(SELECT null FROM sys.databases WHERE name = '{dbName}' AND group_database_id is not null)
	SELECT 1
ELSE
	SELECT 0;
");

			return (Convert.ToInt32(sqlContext.ExecuteScalar(sqlText)) == 1);
		}

		void RestoreAndJoin(ISqlExecutionContext sqlContext, string groupName, string dbName, bool requiresFullRestore)
		{
			RestoreDatabase(sqlContext, dbName, requiresFullRestore);
			JoinDatabase(sqlContext, groupName, dbName);
		}

		void RestoreDatabase(ISqlExecutionContext sqlContext, string dbName, bool requiresFullRestore)
		{
			var currentRedoLsn = (requiresFullRestore) ? -1m : GetUpToDateSecondaryRedoLsn(sqlContext, dbName);

			if (currentRedoLsn < 0)
			{
				RestoreFullBackup(sqlContext, dbName);
				currentRedoLsn = GetUpToDateSecondaryRedoLsn(sqlContext, dbName);
			}

			RestoreRemainingLogBackups(sqlContext, dbName, currentRedoLsn);
		}

		decimal GetUpToDateSecondaryRedoLsn(ISqlExecutionContext sqlContext, string dbName)
		{
			var sqlText = Invariant($@"
SELECT TOP (1)
	mf.redo_start_lsn
FROM
	sys.databases db
	LEFT JOIN sys.master_files mf ON mf.database_id = db.database_id
WHERE
	db.name = '{dbName}'
	AND mf.[type] = 0
	AND db.[state] = 1
	AND db.group_database_id is null
");

			var redoLsnObj = sqlContext.ExecuteScalar(sqlText);
			return (redoLsnObj == null || redoLsnObj == DBNull.Value) ? -1m : Convert.ToDecimal(redoLsnObj);
		}

		void RestoreFullBackup(ISqlExecutionContext sqlContext, string dbName)
		{
			var fullBackupPath = GetLatestPrimaryFullBackup(dbName);
			ExecuteRestoreCommand(sqlContext, dbName, fullBackupPath, isFullBackup: true);
		}

		/// <summary>
		/// Connects to PRIMARY server to get the latest full backup path
		/// </summary>
		string GetLatestPrimaryFullBackup(string dbName)
		{
			var sqlText = Invariant($@"
SELECT TOP (1)
	bmf.physical_device_name
FROM
	msdb..backupmediafamily bmf
	INNER JOIN msdb..backupset bs ON bs.media_set_id = bmf.media_set_id
WHERE
	bs.database_name = '{dbName}'
	AND bs.type = 'D'
ORDER BY
	row_number() over (order by bs.backup_start_date) desc
");

			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServer.ServerInfo);
			var commandObj = sqlContext.ExecuteScalar(sqlText) ?? throw new InvalidOperationException("command should not be null");

			return commandObj.ToString();
		}

		void RestoreRemainingLogBackups(ISqlExecutionContext sqlContext, string dbName, decimal redoLsn)
		{
			var logBackupsToRestore = GetLogBackupSequenceToRestore(dbName, redoLsn);

			foreach (var logBackupPath in logBackupsToRestore)
			{
				ExecuteRestoreCommand(sqlContext, dbName, logBackupPath, isFullBackup: false);
			}
		}

		void ExecuteRestoreCommand(ISqlExecutionContext sqlContext, string dbName, string backupPath, bool isFullBackup)
		{
			var bkpType = isFullBackup ? "DATABASE" : "LOG";
			var replaceClause = isFullBackup ? "REPLACE," : "";
			var timeout = isFullBackup ? TimeSpan.FromSeconds(0) : TimeSpan.FromMinutes(15);
			var sqlText = Invariant($"RESTORE {bkpType} [{dbName}] FROM DISK = '{backupPath}' WITH {replaceClause}NORECOVERY");

			try
			{
				sqlContext.ExecuteNonQuery(sqlText);
			}
			catch (SqlException ex)
			{
				if (ex.Number == CannotOpenBackupDeviceErrorNumber)
				{
					throw new AlwaysOnException(Invariant($"{ex.Message}\r\nPlease ensure the primary database was backed up to an UNC path and that the domain account running SQL Server on the secondary replica has read rights to it."));
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Connects to PRIMARY server to list the log backups pending restore in sequence
		/// </summary>
		internal IEnumerable<string> GetLogBackupSequenceToRestore(string dbName, decimal redoLsn)
		{
			var sqlText = Invariant($@"
SELECT bmf.physical_device_name
	FROM msdb..backupmediafamily bmf
	INNER JOIN msdb..backupset bs ON bs.media_set_id = bmf.media_set_id
	WHERE bs.database_name = @DbName AND bs.type = 'L'
	AND bs.last_lsn > @RedoLsn
ORDER BY first_lsn ASC
");

			var trnQueryResult = new List<string>();

			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServer.ServerInfo);
			using var reader = sqlContext.ExecuteReader(sqlText, System.Data.CommandType.Text, cmd =>
			{
				cmd.AddParameter("@DbName", System.Data.DbType.String, dbName);
				cmd.AddParameter("@RedoLsn", System.Data.DbType.Decimal, redoLsn);
			});
			while (reader.Read())
			{
				if (reader[0] == null)
				{
					throw new InvalidOperationException("physical_device_name column returned a null value");
				}

				trnQueryResult.Add((Convert.ToString(reader[0])));
			}
			return trnQueryResult;
		}

		void JoinDatabaseWithRetry(ISqlExecutionContext sqlContext, string groupName, string dbName)
		{
			try
			{
				JoinDatabase(sqlContext, groupName, dbName);
			}
			catch (SqlException ex)
			{
				switch (ex.Number)
				{
					case SecondaryDbHasInsufficientTransactionLogToBeJoinedErrorNumber:
						RestoreAndJoin(sqlContext, groupName, dbName, requiresFullRestore: false);
						break;

					case NonExistentSecondaryDbErrorNumber:
						RestoreAndJoin(sqlContext, groupName, dbName, requiresFullRestore: true);
						break;

					default:
						throw;
				}
			}
		}

		void JoinDatabase(ISqlExecutionContext sqlContext, string groupName, string dbName)
		{
			var sqlText = Invariant($"ALTER DATABASE [{dbName}] SET HADR AVAILABILITY GROUP = [{groupName}]");

			try
			{
				sqlContext.ExecuteNonQuery(sqlText);
			}
			catch (SqlException ex)
			{
				switch (ex.Number)
				{
					case DatabaseAlreadyJoinedToGroupDbErrorNumber:
						// This is an informational message.  No user action is required.
						break;

					case DatabaseDoesNotBelongToGroupDbErrorNumber:
						throw new AlwaysOnException(Invariant($"Cannot join database [{dbName}]. It does not belong to group [{groupName}]"));

					case ConnectionToPrimaryReplicaIsNotActiveErrorNumber:
						throw new AlwaysOnException(Invariant($@"Cannot join database [{dbName}].
Please check the SQL Server service runs under a domain account and it's the same across all replica instances.
Please also check the firewall settings to ensure port [{alwaysOnEndpointPort}] communication is allowed between the servers."));

					default:
						throw;
				}
			}
		}

		/// <summary>
		/// Msg 911, Level 16, State 1, Line 1
		/// Database 'db' does not exist. Make sure that the name is entered correctly.
		/// </summary>
		const int NonExistentSecondaryDbErrorNumber = 911;

		/// <summary>
		/// Msg 1478, Level 16, State 1, Line 1
		/// The mirror database, "db", has insufficient transaction log data to preserve the log backup chain of the principal database.
		/// This may happen if a log backup from the principal database has not been taken or has not been restored on the mirror database.
		/// </summary>
		const int SecondaryDbHasInsufficientTransactionLogToBeJoinedErrorNumber = 1478;

		/// <summary>
		/// Msg 3201, Level 16, State 2, Line 1
		/// Cannot open backup device 'path'. Operating system error ...
		/// </summary>
		const int CannotOpenBackupDeviceErrorNumber = 3201;

		/// <summary>
		/// Msg 35250, Level 16, State 7, Line 1
		/// The connection to the primary replica is not active.  The command cannot be processed.
		/// </summary>
		const int ConnectionToPrimaryReplicaIsNotActiveErrorNumber = 35250;

		/// <summary>
		/// Msg 41132, Level 16, State 0, Line 1
		/// Cannot join database 'db' to availability group 'ag'.
		/// The specified database does not belong to the availability group.
		/// Verify the names of the database and the availability group, and retry the command specifying the correct names.
		/// </summary>
		const int DatabaseDoesNotBelongToGroupDbErrorNumber = 41132;

		/// <summary>
		/// Msg 41145, Level 16, State 1, Line 1
		/// Cannot join database 'db' to availability group 'ag'.
		/// The database has already joined the availability group.
		/// This is an informational message.  No user action is required.
		/// </summary>
		const int DatabaseAlreadyJoinedToGroupDbErrorNumber = 41145;

		#endregion // Restore And Join Databases

		readonly IPrimaryServerInstance primaryServer;
		protected override ISqlExecutionContext GetSqlExecutionContextForServer() => Program.SqlContextManager.GetSqlExecutionContext(serverInfo, primaryServer?.OdysseyAdminLogin ?? default);
	}
}
