using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public class AlwaysOnReplica : IAlwaysOnReplica
	{
		public AlwaysOnReplica(ReplicaRole roleCode, SyncronisationHealth healthCode, JoinState joinStateCode, IReplicaOptions options, Guid replicaId, IAvailabilityGroup parentGroup, SqlServerInfo serverInfo, string endpointAddress)
		{
			this.serverInfo = serverInfo;
			this.role = roleCode;
			this.health = healthCode;
			this.joinState = joinStateCode;
			this.options = options;
			this.replicaId = replicaId;
			this.parentGroup = parentGroup;
			this.endpointUrl = endpointAddress;
		}

		#region IAlwaysOnReplica Members

		readonly SqlServerInfo serverInfo;
		public SqlServerInfo ServerInfo => serverInfo;

		ReplicaRole IAlwaysOnReplica.Role
		{
			get => role;
		}

		readonly ReplicaRole role;

		public bool IsPrimary => role == ReplicaRole.Primary;

		public event EventHandler HealthStatusChanged;

		void NotifyReplicaHealthStatusChanged()
		{
			HealthStatusChanged?.Invoke(this, EventArgs.Empty);
		}

		SyncronisationHealth IAlwaysOnReplica.Health
		{
			get { return health; }
		}
		SyncronisationHealth health;

		JoinState IAlwaysOnReplica.JoinState
		{
			get { return joinState; }
		}

		readonly JoinState joinState;

		IReplicaOptions IAlwaysOnReplica.Options
		{
			get { return options; }
		}
		IReplicaOptions options;

		Guid IAlwaysOnReplica.ReplicaId
		{
			get { return replicaId; }
		}
		readonly Guid replicaId;

		IAvailabilityGroup IAlwaysOnReplica.ParentGroup
		{
			get { return parentGroup; }
		}
		readonly IAvailabilityGroup parentGroup;

		string IAlwaysOnReplica.ServerAddressFromEndpointUrl
		{
			get { return EndPointServerAddress; }
		}
		readonly string endpointUrl;

		public DbLoginInfo OdysseyAdminLogin { get; private set; }

		public bool IsOdysseyAdminLoginSidDifferentFromPrimary
		{
			get
			{
				if (IsPrimary)
				{
					return false;
				}

				return OdysseyAdminLogin != parentGroup.PrimaryReplica.OdysseyAdminLogin;
			}
		}

		public bool IsOdysseyAdminDbOwner { get; private set; }

		public bool RequiresManualFailOver => !IsOdysseyAdminDbOwner;
		public bool IsAvailableForManualFailOver => health == SyncronisationHealth.HEALTHY && !IsOdysseyAdminLoginSidDifferentFromPrimary;
		string IValidationStatus.LastErrorMessage
		{
			get { return lastErrorMessage; }
		}
		string lastErrorMessage;

		bool IValidationStatus.IsLoaded
		{
			get { return true; }
		}

		bool IValidationStatus.HasErrors
		{
			get { return !string.IsNullOrWhiteSpace(lastErrorMessage); }
		}

		IDictionary<string, DbFileAndTransactionLogInfo> IAlwaysOnReplica.GetGroupDatabases()
		{
			IDictionary<string, DbFileAndTransactionLogInfo> result;

			try
			{
				lastErrorMessage = null;

				var context = GetSqlExecutionContextForReplicaServer();
				result = GetGroupDatabaseList(context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = null;
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}

			return result;
		}

		IEnumerable<DbLoginInfo> IAlwaysOnReplica.GetGroupDatabaseLogins()
		{
			try
			{
				lastErrorMessage = null;

				return ((IAlwaysOnReplica)this).GetGroupDatabaseLoginsWithoutErrorHandling();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
				return Enumerable.Empty<DbLoginInfo>();
			}
		}

		IEnumerable<DbLoginInfo> IAlwaysOnReplica.GetGroupDatabaseLoginsWithoutErrorHandling()
		{
			var context = GetSqlExecutionContextForReplicaServer();

			return GetGroupDatabaseLoginList(context);
		}

		public void CheckOdysseyAdminIsDbOwner(IEnumerable<string> groupDatabases)
		{
			try
			{
				lastErrorMessage = null;
				var context = GetSqlExecutionContextForReplicaServer();

				if (IsPrimary)
				{
					groupDatabases.ForEach(x => DbSecurity.ChangeDatabaseOwnerToLogin(context, x, OdysseyAdminLogin.LoginName));
				}

				var databaseOwners = DbSecurity.QueryDatabaseOwnerLogin(context, groupDatabases);
				IsOdysseyAdminDbOwner = databaseOwners.All(x => x.OwnerLogin == OdysseyAdminLogin.LoginName);

				NotifyReplicaHealthStatusChanged();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = ex.Message;
			}
		}

		public DbLoginInfo GetOdysseyAdminLoginIfExists()
		{
			try
			{
				lastErrorMessage = null;

				return ((IAlwaysOnReplica)this).GetOdysseyAdminLoginIfExistsWithoutErrorHandling();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
				return OdysseyAdminLogin;
			}
		}

		DbLoginInfo IAlwaysOnReplica.GetOdysseyAdminLoginIfExistsWithoutErrorHandling()
		{
			var sqlContext = GetSqlExecutionContextForReplicaServer();
			OdysseyAdminLogin = GetOdysseyAdminLogin(sqlContext);

			NotifyReplicaHealthStatusChanged();

			return OdysseyAdminLogin;
		}

		static DbLoginInfo GetOdysseyAdminLogin(ISqlExecutionContext sqlContext)
		{
			return GetDbLoginInfo(sqlContext, OdysseyAdminCredentials.AdminUserName, isOdysseyAdminLogin: true);
		}

		internal static DbLoginInfo GetDbLoginInfo(ISqlExecutionContext sqlContext, string loginName, bool isOdysseyAdminLogin = false)
		{
			DbLoginInfo dbLoginInfo = default;

			if (!string.IsNullOrEmpty(loginName))
			{
				var sqlText = Invariant($@"
SELECT top 1
	type,
	default_database_name,
	sys.fn_varbintohexsubstring(1, sid, 1, 0) AS SidStr,
	sys.fn_varbintohexsubstring(1, convert(varbinary(256), password_hash), 1, 0) AS HashedPassword
FROM
	sys.sql_logins
WHERE
	name = N'{loginName.QuoteEscapedName('\'')}'
");

				using (var reader = sqlContext.ExecuteReader(sqlText))
				{
					if (reader.Read())
					{
						var loginType = reader.Read(0);
						var defaultDatabase = reader.Read(1);
						var loginSid = reader.Read(2);
						var pwdHash = reader.Read(3);

						dbLoginInfo = new DbLoginInfo(loginName, loginType, defaultDatabase, loginSid, pwdHash, isOdysseyAdminLogin);
					}
				}
			}

			return dbLoginInfo;
		}

		void IAlwaysOnReplica.CopyDatabaseLoginsFromPrimaryReplica()
		{
			try
			{
				lastErrorMessage = null;

				if (parentGroup.PrimaryReplica == null)
				{
					throw new InvalidOperationException("PrimaryReplica cannot be null");
				}

				// Step 1 of 2: copy all other logins
				var dbLogins = parentGroup.PrimaryReplica.GetGroupDatabaseLoginsWithoutErrorHandling();
				var context = GetSqlExecutionContextForReplicaServer();

				DbManager.PropagateDatabaseLogins(context, dbLogins);

				// Step 2 of 2: copy OdysseyAdmin login
				var odysseyAdminLoginFromPrimary = parentGroup.PrimaryReplica.GetOdysseyAdminLoginIfExistsWithoutErrorHandling();
				var odysseyAdminFromSecondary = ((IAlwaysOnReplica)this).GetOdysseyAdminLoginIfExistsWithoutErrorHandling();

				if (!odysseyAdminFromSecondary.Equals(odysseyAdminLoginFromPrimary))
				{
					DbSecurity.PropagateOdysseyAdminLogin(serverInfo, odysseyAdminLoginFromPrimary);
					context = GetSqlExecutionContextForReplicaServer();

					// check again ensure same
					odysseyAdminFromSecondary = ((IAlwaysOnReplica)this).GetOdysseyAdminLoginIfExistsWithoutErrorHandling();
					if (!odysseyAdminFromSecondary.Equals(odysseyAdminLoginFromPrimary))
					{
						throw new AlwaysOnException(Invariant($"Failed to copy admin login from primary replica [{parentGroup.PrimaryReplica.ServerInfo.ServerAlias}] to secondary replica [{serverInfo.ServerAlias}]."));
					}

					// check again if OdysseyAdmin is dbOwner
					if (!IsPrimary)
					{
						CheckOdysseyAdminIsDbOwner(parentGroup.Databases);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		void IAlwaysOnReplica.PerformPlannedManualFailover()
		{
			lastErrorMessage = null;

			if (parentGroup.PrimaryReplica == null)
			{
				throw new InvalidOperationException("PrimaryReplica cannot be null");
			}

			if (options.CommitMode == AvailabilityMode.ASYNCHRONOUS_COMMIT || parentGroup.PrimaryReplica.Options.CommitMode == AvailabilityMode.ASYNCHRONOUS_COMMIT)
			{
				lastErrorMessage = "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.";
				return;
			}

			if (health != SyncronisationHealth.HEALTHY)
			{
				lastErrorMessage = "A planned manual failover is supported only if the secondary replica is in a healthy state.";
				return;
			}

			try
			{
				var sqlContext = GetSqlExecutionContextForReplicaServer();
				ExecuteFailover(sqlContext);
				if (CheckDatabaseIsOnlineWithWait(sqlContext))
				{
					AlterAuthorization(sqlContext);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		protected virtual bool CheckDatabaseIsOnlineWithWait(ISqlExecutionContext sqlContext)
		{
			var isDbOnline = false;
			var mainDbName = parentGroup.MainDatabaseName;
			var sqlScript = GetCheckDatabaseIsOnlineScript(mainDbName);
			while (!isDbOnline && waitingLoops > 0)
			{
				isDbOnline = (bool)sqlContext.ExecuteScalar(sqlScript);
				waitingLoops--;
				if (!isDbOnline)
				{
					Thread.Sleep(sleepTimeSpan);
				}
			}

			return isDbOnline;
		}

		protected string GetCheckDatabaseIsOnlineScript(string mainDbName)
		{
			return Invariant($@"
Declare @isDbOnline bit = 0
If EXISTS(
			Select null
			From
				sys.databases dbs INNER JOIN
				sys.dm_hadr_availability_replica_states replicas on dbs.replica_id = replicas.replica_id
			Where 1=1
				AND dbs.database_id = DB_ID('{mainDbName}')
				AND dbs.state = 0  
				AND replicas.is_local = 1
				AND replicas.role = 1
				AND replicas.operational_state = 2
		)
	Set @isDbOnline = 1
Select @isDbOnline
");
		}

		void AlterAuthorization(ISqlExecutionContext sqlContext)
		{
			var dbName = parentGroup.MainDatabaseName;
			var loginName = OdysseyAdminCredentials.AdminUserName;

			var sqlText = Invariant($@"
DECLARE @SqlCmd nvarchar(1000) = (
	SELECT TOP (1)
		'EXEC [{dbName}]..sp_executesql N''
			IF exists(SELECT null FROM sys.database_principals WHERE name = ''''{loginName}'''') DROP USER [{loginName}];
			ALTER AUTHORIZATION ON DATABASE::[{dbName}] TO [{loginName}];
		'';'
	FROM
		(SELECT [sid] FROM sys.server_principals WHERE name = '{loginName}') AdminLogin
		LEFT JOIN (SELECT [sid] FROM [{dbName}].sys.database_principals WHERE name = 'dbo') DboUser
			ON DboUser.sid = AdminLogin.sid
		LEFT JOIN (SELECT owner_sid FROM sys.databases WHERE name = '{dbName}') UserDatabase
			ON UserDatabase.owner_sid = AdminLogin.sid
	WHERE
		(DboUser.sid is null OR UserDatabase.owner_sid is null)
);
IF (@SqlCmd is not null) EXEC (@SqlCmd);
");

			sqlContext.ExecuteNonQuery(sqlText);
		}

		void IAlwaysOnReplica.ChangeSettings(IReplicaOptions newSettings)
		{
			options = newSettings;
		}

		#endregion // IAlwaysOnReplica Members

		void ExecuteFailover(ISqlExecutionContext sqlContext)
		{
			var sqlText = Invariant($"ALTER AVAILABILITY GROUP [{parentGroup.GroupName}] FAILOVER");
			sqlContext.ExecuteNonQuery(sqlText);
		}

		#region Load Data

		IDictionary<string, DbFileAndTransactionLogInfo> GetGroupDatabaseList(ISqlExecutionContext sqlContext)
		{
			var sqlText = Invariant($@"
SELECT
	groupDbBackup.database_name,
	groupDbBackup.last_lsn,
	bf.file_type,
	bf.logical_name,
	bf.physical_name
FROM
	(
		SELECT
			groupDb.database_name,
			bs.backup_set_id,
			bs.last_lsn,
			row_number() over (partition by bs.database_name order by bs.backup_start_date desc) RowNumber
		FROM
			sys.availability_databases_cluster groupDb
			LEFT JOIN msdb..backupset bs ON groupDb.database_name = bs.database_name COLLATE DATABASE_DEFAULT
		WHERE
			groupDb.group_id = '{parentGroup.GroupId}'
			AND bs.[type] in ('D', 'L')
			AND bs.recovery_model = 'FULL'
	) groupDbBackup
	LEFT JOIN msdb..backupfile bf ON bf.backup_set_id = groupDbBackup.backup_set_id
WHERE
	groupDbBackup.RowNumber = 1
ORDER BY
	groupDbBackup.database_name, bf.logical_name
");

			var groupDbList = new Dictionary<string, DbFileAndTransactionLogInfo>();

			using (var reader = sqlContext.ExecuteReader(sqlText))
			{
				string dbName = null;
				decimal lastBackupLsn = 0;
				var dbFiles = new DatabaseFileCollection();

				while (reader.Read())
				{
					var dbNameObj = reader[0] ?? throw new InvalidOperationException("database_name returned a null value");

					if (dbName != dbNameObj.ToString())
					{
						if (dbName != null)
						{
							groupDbList.Add(dbName, new DbFileAndTransactionLogInfo(dbName, lastBackupLsn, dbFiles));
						}

						dbName = reader[0].ToString();
						var lastBackupLsnObj = reader[1];
						lastBackupLsn = (lastBackupLsnObj == DBNull.Value) ? -1m : Convert.ToDecimal(lastBackupLsnObj);
						dbFiles = new DatabaseFileCollection();
					}

					if (lastBackupLsn > 0)
					{
						var fileTypeObj = reader[2];
						var fileLogicalNameObj = reader[3];
						var filePhysicalPathObj = reader[4];

						if (fileTypeObj == null || fileLogicalNameObj == null || filePhysicalPathObj == null)
						{
							throw new InvalidOperationException("file_type or logical_name or physical_name returned a null value");
						}

						var fileType = fileTypeObj.ToString();
						var fileLogicalName = fileLogicalNameObj.ToString();
						var filePhysicalPath = filePhysicalPathObj.ToString();
						dbFiles.Add(new DatabaseFile(fileLogicalName, filePhysicalPath, fileType));
					}
				}

				if (dbName != null)
				{
					groupDbList.Add(dbName, new DbFileAndTransactionLogInfo(dbName, lastBackupLsn, dbFiles));
				}
			}

			return groupDbList;
		}

		IEnumerable<DbLoginInfo> GetGroupDatabaseLoginList(ISqlExecutionContext sqlContext)
		{
			var groupDbLoginList = new List<DbLoginInfo>();

			var mainDbName = parentGroup.MainDatabaseName;

			if (!string.IsNullOrWhiteSpace(mainDbName))
			{
				var sqlText = Invariant($@"
SELECT
	l.name,
	l.type,
	CASE WHEN l.default_database_name like '{mainDbName}%' THEN '{mainDbName}' ELSE '' END AS DefaultDbName,
	sys.fn_varbintohexsubstring(1, l.sid, 1, 0) AS SidStr,
	sys.fn_varbintohexsubstring(1, convert(varbinary(256), LOGINPROPERTY(l.name, 'PasswordHash')), 1, 0) AS HashedPwdStr
FROM
	sys.server_principals l
	INNER JOIN [{mainDbName}].sys.database_principals u ON u.sid = l.sid AND u.name = l.name COLLATE database_default
WHERE
	l.type in ('U', 'S')
");

				using (var reader = sqlContext.ExecuteReader(sqlText))
				{
					while (reader.Read())
					{
						var loginNameObj = reader[0];
						var loginTypeObj = reader[1];
						var defaultDatabaseObj = reader[2];

						if (loginNameObj == null || loginTypeObj == null || defaultDatabaseObj == null)
						{
							throw new InvalidOperationException("name column or type column or DefaultDbName column returned a null value");
						}

						var loginName = loginNameObj.ToString();
						var loginType = loginTypeObj.ToString();
						var defaultDatabase = (defaultDatabaseObj == DBNull.Value) ? null : defaultDatabaseObj.ToString();

						var loginSidObj = reader[3];
						var pwdHashObj = reader[4];

						if (loginSidObj == null || pwdHashObj == null)
						{
							throw new InvalidOperationException("SidStr column or HashedPwdStr column returned a null value");
						}

						var loginSid = loginSidObj.ToString();
						var pwdHash = pwdHashObj.ToString();
						groupDbLoginList.Add(new DbLoginInfo(loginName, loginType, defaultDatabase, loginSid, pwdHash));
					}
				}
			}
			return groupDbLoginList;
		}

		#endregion //Load Data

		#region Replica Server Connection

		protected virtual ISqlExecutionContext GetSqlExecutionContextForReplicaServer()
		{
			try
			{
				return Program.SqlContextManager.GetSqlExecutionContext(ServerInfo);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if ((ex is SqlException || ex.Source.StartsWith("System.Data")) && !string.IsNullOrEmpty(EndPointServerAddress))
				{
					return Program.SqlContextManager.GetSqlExecutionContext(EndPointServerAddress, DbManager.MasterDbName);
				}
				else
				{
					throw;
				}
			}
		}

		void IAlwaysOnReplica.SuspendReplication(string dbName)
		{
			AlterHADRReplication(dbName, "SUSPEND");
		}

		void IAlwaysOnReplica.ResumeReplication(string dbName)
		{
			AlterHADRReplication(dbName, "RESUME");
		}

		void IAlwaysOnReplica.RefreshHealthState()
		{
			var query = @"
SELECT
	ars.synchronization_health
FROM
	sys.availability_replicas ar
	INNER JOIN sys.dm_hadr_availability_replica_states ars ON ars.replica_id = ar.replica_id
WHERE
	ar.replica_id = @replicaId
";

			try
			{
				var sqlContext = GetSqlExecutionContextForReplicaServer();
				using var reader = sqlContext.ExecuteReader(query, CommandType.Text, cmd => cmd.AddParameter("@replicaId", DbType.Guid, replicaId));
				{
					if (reader.Read())
					{
						health = (SyncronisationHealth)Convert.ToInt32(reader[0], CultureInfo.InvariantCulture);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				health = SyncronisationHealth.NOT_HEALTHY;
			}

			NotifyReplicaHealthStatusChanged();
		}

		protected virtual IDataReader GetReader(SqlCommand cmd)
		{
			return cmd.ExecuteReader();
		}

		void AlterHADRReplication(string dbName, string suspendOrResume)
		{
			var sqlText = Invariant($@"
If EXISTS (Select null From sys.databases Where name = '{dbName}' and replica_id IS NOT NULL)
ALTER DATABASE [{dbName}] SET HADR {suspendOrResume}
");

			var sqlContext = GetSqlExecutionContextForReplicaServer();
			sqlContext.ExecuteNonQuery(sqlText);
		}

		string EndPointServerAddress
		{
			get
			{
				if (endPointServerAddressDoNotUseDirectly == null)
				{
					endPointServerAddressDoNotUseDirectly = "";

					if (!string.IsNullOrWhiteSpace(endpointUrl))
					{
						var serverAddressMatch = ServerAddrFromEndpointRegex.Match(endpointUrl);

						if (serverAddressMatch.Success)
						{
							var group = serverAddressMatch.Groups["ADDRESS"] ?? throw new InvalidOperationException("group cannot be null");

							endPointServerAddressDoNotUseDirectly = group.Value;
						}
					}
				}

				return endPointServerAddressDoNotUseDirectly;
			}
		}

		string endPointServerAddressDoNotUseDirectly;
		protected int waitingLoops = 18;
		protected TimeSpan sleepTimeSpan = TimeSpan.FromSeconds(10);
		static readonly Regex ServerAddrFromEndpointRegex = new Regex(@"^TCP://(?<ADDRESS>[^:]+):[0-9]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		#endregion //Replica Server Connection
	}
}
