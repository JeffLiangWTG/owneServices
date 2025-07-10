#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public class PrimaryServerInstance : DbServerInstance, IPrimaryServerInstance
	{
		public PrimaryServerInstance(SqlServerInfo serverInfo)
			: base(serverInfo)
		{
		}

		protected override void LoadUnsafe(ISqlExecutionContext sqlContext)
		{
			base.LoadUnsafe(sqlContext);
			sqlServiceAccount = GetSqlServiceAccount(sqlContext);
			eligibleTopLevelDatabases = GetEligiblePrimaryTopLevelDatabases(sqlContext);
		}

		protected override void InvokeAdditionalTasksAfterLoad(ISqlExecutionContext sqlContext)
		{
			try
			{
				DbSecurity.AlterAuthorizationsAndDropAlwaysOnOldLoginIfExists(sqlContext, odysseyAdminLogin.LoginName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// if failed drop old login because old login used to own many other databases, the oldLogin can be left there no harm
			}
		}

		#region IPrimaryServerInstance Members

		string IPrimaryServerInstance.SqlServiceAccount
		{
			get { return sqlServiceAccount; }
		}
		string sqlServiceAccount;

		IEnumerable<IAlwaysOnDatabase> IPrimaryServerInstance.EligibleTopLevelDatabases
		{
			get { return eligibleTopLevelDatabases; }
		}
		IEnumerable<IAlwaysOnDatabase> eligibleTopLevelDatabases;

		Guid IPrimaryServerInstance.CreateAvailabilityGroup(string mainDbName, string newGroupName, int endpointPort)
		{
			var groupId = Guid.Empty;

			try
			{
				lastErrorMessage = null;

				var sqlContext = GetSqlExecutionContextForServer();

				if (alwaysOnEndpointPort <= 0)
				{
					CreateMirroringEndpoint(sqlContext, endpointPort);
					alwaysOnEndpointPort = endpointPort;
				}

				EnsureEndpointCommunication(sqlContext, sqlServiceAccount);
				groupId = CreateGroupAndPrimaryReplica(sqlContext, mainDbName, newGroupName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}

			return groupId;
		}

		void IPrimaryServerInstance.RemoveAvailabilityGroup(string groupName)
		{
			try
			{
				lastErrorMessage = null;
				RemoveGroup(serverInfo, groupName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		public void SuspendReplication(string dbName)
		{
			var sqlContext = GetSqlExecutionContextForServer();
			AlterHADRReplication(dbName, "SUSPEND");
		}

		public void ResumeReplication(string dbName)
		{
			var sqlContext = GetSqlExecutionContextForServer();
			AlterHADRReplication(dbName, "RESUME");
		}

		void AlterHADRReplication(string dbName, string suspendOrResume)
		{
			var sqlText = Invariant($"ALTER DATABASE [{dbName}] SET HADR {suspendOrResume}");

			var sqlContext = GetSqlExecutionContextForServer();
			sqlContext.ExecuteNonQuery(sqlText);
		}

		IEnumerable<DatabaseAlwaysOnStatus> IPrimaryServerInstance.GetAlwaysOnDatabaseSet(string mainDbName, bool alterDbSettingToMeetRequiremtns)
		{
			IEnumerable<DatabaseAlwaysOnStatus> result = null;

			try
			{
				lastErrorMessage = null;

				var sqlContext = GetSqlExecutionContextForServer();
				result = GetAlwaysOnSetOfDatabases(sqlContext, mainDbName, alterDbSettingToMeetRequiremtns);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}

			return result;
		}

		#endregion // IPrimaryServerInstance Members

		#region Load Data

		string GetSqlServiceAccount(ISqlExecutionContext sqlContext)
		{
			string result;

			var registryPath = Invariant($"SYSTEM\\CurrentControlSet\\Services\\{((serverInfo.InstanceName == SqlServerInfo.DefaultSqlServerInstanceName) ? "" : "MSSQL$")}{serverInfo.InstanceName}");

			const string sqlText = @"
SELECT service_account FROM sys.dm_server_services
WHERE servicename like 'SQL Server (%)';
";

			var commandObj = sqlContext.ExecuteScalar(sqlText) ?? throw new InvalidOperationException("command cannot be null");
			result = commandObj.ToString();

			if (result == DBNull.Value.ToString() || string.IsNullOrEmpty(result))
			{
				throw new Exception(Invariant($"Unable to get domain account running the SQL Server windows service from view: sys.dm_server_services\\{registryPath}"));// no translation needed
			}

			if (result.Contains('@'))
			{
				var atSplit = result.Split('@');

				var dotSplit = atSplit[1].Split('.');
				result = dotSplit[0] + "\\" + atSplit[0];
			}

			return result;
		}

		/// <summary>
		/// Get list of main CargoWise databases which:
		///   * are not part of an availability group
		///   OR
		///   * are in the primary replica of its group
		/// </summary>
		protected IEnumerable<IAlwaysOnDatabase> GetEligiblePrimaryTopLevelDatabases(ISqlExecutionContext sqlContext)
		{
			var eligibleDbs = new List<IAlwaysOnDatabase>();

			const string getAlwaysOnTopLevelDbListSql = @"
DECLARE @PrimaryReplica TABLE (replica_id uniqueidentifier, group_id uniqueidentifier);
DECLARE @Grp TABLE (group_id uniqueidentifier, name varchar(150), resource_id uniqueidentifier);

INSERT INTO @PrimaryReplica
	SELECT replica_id, group_id
	FROM sys.dm_hadr_availability_replica_states
	WHERE is_local = 1
	AND [role] = 1;

INSERT INTO @Grp 
	SELECT group_id, name, resource_id
	FROM sys.availability_groups;

SELECT
	   db.name as DatabaseName,
	   isnull(ag.name, '') as GroupName,
	   isnull(ag.group_id, convert(uniqueidentifier, '00000000-0000-0000-0000-000000000000')) as GroupId
FROM
	   sys.databases db
	   LEFT JOIN @PrimaryReplica pr ON pr.replica_id = db.replica_id
	   LEFT JOIN @Grp ag ON ag.group_id = pr.group_id
WHERE
	   db.name not in ('master', 'tempdb', 'model', 'msdb')
	   AND db.name not like '%[''.,_-]%'
	   AND db.[state] = 0
	   AND db.is_in_standby = 0
	   AND db.is_read_only = 0
	   AND db.recovery_model = 1
	   AND (db.replica_id is null OR pr.replica_id is not null)
ORDER BY
	   db.[name];
";

			using (var reader = sqlContext.ExecuteReader(getAlwaysOnTopLevelDbListSql, System.Data.CommandType.Text, cmd => cmd.CommandTimeout = TimeSpan.FromMinutes(5).Seconds))
			{
				while (reader.Read())
				{
					var dbNameObj = reader[0];
					var groupNameObj = reader[1];

					if (dbNameObj == null || groupNameObj == null)
					{
						throw new InvalidOperationException("DatabaseName or GroupName column returned a null value");
					}

					var dbName = dbNameObj.ToString();
					var groupName = groupNameObj.ToString();
					var groupId = reader.GetGuid(2);
					eligibleDbs.Add(AlwaysOnDatabaseFactory.New(dbName, groupName, groupId));
				}
			}

			return eligibleDbs;
		}

		protected IEnumerable<DatabaseAlwaysOnStatus> GetAlwaysOnSetOfDatabases(ISqlExecutionContext sqlContext, string mainDbName, bool alterDbSettingToMeetRequirements)
		{
			var getDbListSql = Invariant($@"
SELECT
	db.[name],
	db.recovery_model,
	db.is_auto_close_on,
	bs.last_backup_date,
	gdb.group_id,
	ISNULL(gdb.is_suspended, 0) is_suspended
FROM
	sys.databases db
	LEFT JOIN (
		SELECT database_name, max(backup_finish_date) last_backup_date
		FROM msdb..backupset
		WHERE database_name like '{mainDbName}%' AND [type] = 'D' AND recovery_model = 'FULL'
		GROUP BY database_name
	) bs ON bs.database_name = db.[name]
	LEFT JOIN sys.dm_hadr_database_replica_states gdb ON gdb.group_database_id = db.group_database_id
WHERE
	db.[state] = 0
	AND db.is_in_standby = 0
	AND db.is_read_only = 0
	AND db.user_access = 0
	AND (gdb.is_local = 1 OR gdb.is_local is null)
	AND (
		db.[name] = '{mainDbName}'
		OR db.[name] like '{mainDbName}[_]SD[0-9][0-9][0-9]'
		OR db.[name] like '{mainDbName}[_]RefDb[_]___[_]__'
		OR db.[name] like '{mainDbName}[_]UserRepository'
	)
ORDER BY
	db.[name]
");

			var databaseList = new List<DatabaseAlwaysOnStatus>();
			var fixDbSettingsCmdBuilder = new StringBuilder();

			using (var reader = sqlContext.ExecuteReader(getDbListSql))
			{
				while (reader.Read())
				{
					var dbNameObj = reader[0];
					var isFullRecovery = (Convert.ToInt32(reader[1]) == 1);
					var isAutoCloseOn = (Convert.ToInt32(reader[2]) != 0);
					var hasFullBackup = (reader[3] != DBNull.Value);

					var groupIdObj = reader[4];
					var isDataMovementSuspended = (Convert.ToInt32(reader[5]) == 1);

					if (dbNameObj == null || groupIdObj == null)
					{
						throw new InvalidOperationException("[name] or group_id column returned a null value");
					}

					var dbName = dbNameObj.ToString();
					var groupId = (groupIdObj == DBNull.Value) ? Guid.Empty : (Guid)groupIdObj;

					var dbAlwaysOnStatus = new DatabaseAlwaysOnStatus(dbName, groupId, hasFullBackup, isDataMovementSuspended);
					if (groupId == Guid.Empty && alterDbSettingToMeetRequirements)
					{
						if (!isFullRecovery)
						{
							fixDbSettingsCmdBuilder.AppendFormat("ALTER DATABASE [{0}] SET RECOVERY FULL;", dbName);// This is SQL Script
							dbAlwaysOnStatus.RecoveryModelChanged = true;
						}

						if (isAutoCloseOn)
						{
							fixDbSettingsCmdBuilder.AppendFormat("ALTER DATABASE [{0}] SET AUTO_CLOSE OFF;", dbName);// This is SQL script
						}
					}

					databaseList.Add(dbAlwaysOnStatus);
				}
			}

			if (alterDbSettingToMeetRequirements && fixDbSettingsCmdBuilder.Length > 0)
			{
				sqlContext.ExecuteNonQuery(fixDbSettingsCmdBuilder.ToString());
			}

			return databaseList;
		}

		#endregion

		#region Create New Availability Group

		Guid CreateGroupAndPrimaryReplica(ISqlExecutionContext sqlContext, string mainDbName, string newGroupName)
		{
			EnsureNtAuthoritySystemHasAlterAnyAvailabilityGroupRights(sqlContext);
			var databaseList = GetDatabaseListEnforcingAlwaysOnRequirements(sqlContext, mainDbName);

			var createAvailabilityGroupSql = Invariant($@"
CREATE AVAILABILITY GROUP [{newGroupName}]
	WITH (AUTOMATED_BACKUP_PREFERENCE = SECONDARY)
	FOR
	DATABASE {string.Join(",", databaseList.Select(db => "[" + db + "]"))}
	REPLICA ON N'{serverInfo.ServerAlias}' WITH (
		ENDPOINT_URL = N'TCP://{serverInfo.ServerFQDN}:{alwaysOnEndpointPort.ToString()}',
		FAILOVER_MODE = MANUAL,
		AVAILABILITY_MODE = SYNCHRONOUS_COMMIT,
		PRIMARY_ROLE(ALLOW_CONNECTIONS = ALL),
		SECONDARY_ROLE(ALLOW_CONNECTIONS = ALL)
					)
");

			sqlContext.ExecuteNonQuery(createAvailabilityGroupSql);
			var commandObj = sqlContext.ExecuteScalar($"SELECT group_id FROM sys.availability_groups WHERE [name] = '{newGroupName}'")
				?? throw new InvalidOperationException("command cannot be null");
			return new Guid(commandObj.ToString());
		}

		void EnsureNtAuthoritySystemHasAlterAnyAvailabilityGroupRights(ISqlExecutionContext sqlContext)
		{
			const string ensureAlterAnyGroupSql = @"
IF not exists (SELECT null FROM sys.server_principals WHERE name = 'NT AUTHORITY\SYSTEM') CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;
GRANT ALTER ANY AVAILABILITY GROUP TO [NT AUTHORITY\SYSTEM];
";
			sqlContext.ExecuteNonQuery(ensureAlterAnyGroupSql);
		}

		/// <summary>
		/// Checks and fixes following requirements:
		///  - RECOVERY = FULL
		///  - AUTO_CLOSE = OFF
		///  - RECENT FULL BACKUP
		/// </summary>
		protected IEnumerable<string> GetDatabaseListEnforcingAlwaysOnRequirements(ISqlExecutionContext sqlContext, string mainDbName)
		{
			var alwaysOnDbSet = GetAlwaysOnSetOfDatabases(sqlContext, mainDbName, alterDbSettingToMeetRequirements: true).ToArray();

			var databasesAlreadyInAnotherGroup = alwaysOnDbSet.Where(db => db.GroupId != Guid.Empty).ToArray();
			if (databasesAlreadyInAnotherGroup.Any())
			{
				throw new AlwaysOnException(
					"The following databases already belong to an availability group:\r\n\r\n" +
					string.Join("\r\n", databasesAlreadyInAnotherGroup)
				);
			}

			var databasesWithChangedRecovery = alwaysOnDbSet.Where(db => db.RecoveryModelChanged).ToArray();
			if (databasesWithChangedRecovery.Any())
			{
				throw new AlwaysOnException(
					"The RECOVERY model for the following databases has been changed to FULL according to the system requirements. Please ensure all databases have a recent full backup.\r\n\r\n" +
					string.Join("\r\n", databasesWithChangedRecovery)
				);
			}

			var databasesWithNoFullBackup = alwaysOnDbSet.Where(db => !db.HasFullBackup).ToArray();
			if (databasesWithNoFullBackup.Any())
			{
				throw new AlwaysOnException(
					"The following databases have no full backup. Please ensure all databases have a recent full backup.\r\n\r\n" +
					string.Join("\r\n", databasesWithNoFullBackup)
				);
			}

			return alwaysOnDbSet.Select(db => db.Name);
		}

		#endregion

		#region Remove Availability Group

		void RemoveGroup(SqlServerInfo serverInfo, string groupName)
		{
			var contextManager = Program.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
			var contex = contextManager.GetSqlExecutionContext(serverInfo.ServerAlias, DbManager.MasterDbName);

			contex.ExecuteNonQuery(Invariant($"DROP AVAILABILITY GROUP [{groupName}]"));
		}

		#endregion
	}
}
#endregion
