using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(DbMaintenanceTasks.AlwaysOnManagement, "AlwaysOn Management Service", "DBM", typeof(AlwaysOnManagementServiceTask),
	MinimumPeriod = "15minutes",
	MaximumPeriod = "1day",
	IsReadOnlyForWiseCloudClient = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	public class AlwaysOnManagementServiceTask : ServiceProviderImpl
	{
		public AlwaysOnManagementServiceTask()
			: base()
		{
		}

		public AlwaysOnManagementServiceTask(ILogger serviceLogger)
			: base()
		{
			ServiceLogger = serviceLogger;
		}

		internal virtual ISqlSecurityManager GetSqlSecurityManager()
		{
			return new SqlSecurityManager(ServiceLogger, Db.DatabaseName);
		}

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "AlwaysOn Management task started");
			RunAlwaysOnMaintenanceTasks(token);
			ServiceLogger.Log(LogType.Information, "AlwaysOn Management task finished");
		}

		void RunAlwaysOnMaintenanceTasks(CancellationToken token)
		{
			var useModernSqlSecuritySystem = EnvProxy.Instance.Registry.UseModernSqlSecuritySystem;

			if (AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, Db.DatabaseName))
			{
				PerformMaintenance(token);

				if (useModernSqlSecuritySystem)
				{
					var replicas = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: true);
					var sqlSecurityManager = GetSqlSecurityManager();
					using (var adminConnection = Db.NewAdminConnection())
					{
						sqlSecurityManager.Propagate(adminConnection, replicas, token);
					}
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "CW1 databases are not configured for AlwaysOn. No maintenance tasks were executed.");
			}

			if (!useModernSqlSecuritySystem)
			{
				SynchroniseLoginsAndUsersAndPropagateLogins();
			}
		}

		#region Propagate Databases

		void PerformMaintenance(CancellationToken token)
		{
			using (var primaryConnection = Db.NewAdminConnection())
			{
				ServiceLogger.Log(LogType.Information, "Database propagation");

				var ag = AlwaysOnHelper.GetAvailabilityGroupInfo(primaryConnection, Db.DatabaseName);
				if (ag.GroupId != Guid.Empty)
				{
					var databases = GetDatabases(primaryConnection);

					ServiceLogger.Log(LogType.Information, "Checking primary server");
					var lockResult = AlwaysOnHelper.RunActionWithAlwaysOnLock(primaryConnection, _ =>
					{
						CheckPrimaryServer(primaryConnection, ag, databases, token, ServiceLogger);
						RepairUnhealthyReplication(primaryConnection, ag.GroupName, databases, ServiceLogger);
					});

					if (lockResult != LockedProcessResult.Completed)
					{
						ServiceLogger.Log(LogType.Warning, "Failed to get AppLock. Please try again after other process completes its task.");
					}
				}
			}
		}

		protected void RepairUnhealthyReplication(AdminConnection primaryConnection, string groupName, IEnumerable<string> databases, ILogger serviceLogger)
		{
			var unhealthyDatabases = GetUnhealthyDatabases(primaryConnection, groupName, databases);
			var databasesWherePrimaryIsRecoveryPending = new List<string>();
			var serversConnectedFailed = new List<string>();
			foreach (var db in unhealthyDatabases)
			{
				var dbName = db.dbName;
				if (databasesWherePrimaryIsRecoveryPending.Contains(dbName) || serversConnectedFailed.Contains(db.serverName))
				{
					continue;
				}

				var secondary = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(db.serverName);
				using (var secondaryConnection = Db.NewAdminConnection(secondary, Db.SqlMasterDb))
				{
					try
					{
						var isShared = RefDbTableNameResolver.IsSharedDatabase(dbName);
						if (AlwaysOn.IsDbPartOfAlwaysOn(secondaryConnection, dbName))
						{
							if (GetDatabaseStateDescription(primaryConnection, dbName).Equals("RECOVERY_PENDING"))
							{
								serviceLogger.Log(LogType.Error, $"Database [{dbName}] state is RECOVERY_PENDING on the primary replica.");
								databasesWherePrimaryIsRecoveryPending.Add(dbName);
							}
							else if (GetDatabaseStateDescription(secondaryConnection, dbName).Equals("RECOVERY_PENDING"))
							{
								serviceLogger.Log(LogType.Warning, $"Database [{dbName}] state is RECOVERY_PENDING on secondary replica [{secondary}].");
								RemoveDatabaseFromSecondaryAndAttemptToRejoin(secondaryConnection, dbName, groupName, secondary, serviceLogger, isShared, primaryConnection);
							}
							else
							{
								RepairUnhealthyDb(primaryConnection, secondaryConnection, dbName, groupName, secondary, serviceLogger, isShared);
							}
						}
						else
						{
							ExecuteJoinDatabaseToReplica(primaryConnection, dbName, groupName, serviceLogger, isShared, secondary);
						}
					}
					catch (SqlException sqlException) when (new DbErrorMatch(sqlException).ExceptionType == DbErrorType.GeneralNetworkError)
					{
						serviceLogger.Log(LogType.Warning, $"Failed to connect to secondary replica [{secondary}]. Please double check the replica's availability.\r\nError Number: {sqlException.Number}, Message:{sqlException.Message}");
						serversConnectedFailed.Add(db.serverName);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						serviceLogger.Log(LogType.Error, $"An exception occured during attempted repair of [{secondary}]:\r\n\r\n{ex}");
					}
				}
			}
		}

		virtual protected List<(string dbName, string serverName)> GetUnhealthyDatabases(DbConnection primaryConnection, string groupName, IEnumerable<string> databases)
		{
			var unhealthyDatabases = new List<(string, string)>();
			primaryConnection.ExecuteReader(
				@"SELECT DB_NAME(database_id) dbname, replica_server_name servername
FROM sys.dm_hadr_database_replica_states states
JOIN sys.availability_replicas rep ON rep.replica_id = states.replica_id AND rep.group_id = states.group_id
WHERE synchronization_health = 0", reader =>
				{
					var dbName = (string)reader["dbname"];
					if (databases.Contains(dbName))
					{
						unhealthyDatabases.Add((dbName, (string)reader["servername"]));
					}
				});
			return unhealthyDatabases;
		}

		protected void RepairUnhealthyDb(AdminConnection primaryConnection, AdminConnection secondaryConnection, string dbName, string groupName, string serverName, ILogger serviceLogger, bool isShared)
		{
			try
			{
				serviceLogger.Log(LogType.Information, $"Database [{dbName}] replication appears to be unhealthy on [{serverName}].");

				SuspendAndResumeHADR(secondaryConnection, dbName, serverName, serviceLogger);
			}
			catch (SqlException ex)
			{
				serviceLogger.Log(LogType.Warning, $"Unable to suspend and resume database:\r\n{ex.Message}");
			}
		}

		void SuspendAndResumeHADR(AdminConnection connection, string dbName, string serverName, ILogger serviceLogger)
		{
			try
			{
				serviceLogger.Log(LogType.Information, $"Suspending database [{dbName}] on [{serverName}].");
				ExecuteSql(connection, $"ALTER DATABASE [{dbName}] SET HADR SUSPEND;");
			}
			catch (SqlException ex)
			{
				serviceLogger.Log(LogType.Warning, $"Unable to suspend database:\r\n{ex.Message}");
			}

			serviceLogger.Log(LogType.Information, $"Resuming database [{dbName}].");
			ExecuteSql(connection, $"ALTER DATABASE [{dbName}] SET HADR RESUME;");
		}

		void RemoveDatabaseFromSecondaryAndAttemptToRejoin(AdminConnection secondaryConnection, string dbName, string groupName, string secondary, ILogger serviceLogger, bool isShared, AdminConnection primaryConnection)
		{
			serviceLogger.Log(LogType.Information, $"Removing database [{dbName}] from availability group [{groupName}] on secondary [{secondary}].");
			UnjoinAvailabilityGroup(secondaryConnection, dbName);
			var state = DatabaseIsRestoring(secondaryConnection, dbName);
			if (state.Equals("RESTORING"))
			{
				ExecuteJoinDatabaseToReplica(primaryConnection, dbName, groupName, serviceLogger, isShared, secondary);
			}
			else
			{
				NotifyTimeout(dbName, secondary, serviceLogger);
			}
		}

		void ExecuteJoinDatabaseToReplica(AdminConnection connection, string dbName, string groupName, ILogger serviceLogger, bool isShared, string secondary)
		{
			try
			{
				if (isShared)
				{
					connection.RunLocked(key: dbName, process: (_) =>
					{
						var alwaysOn = ObjectFactory.Get<ISqlAlwaysOnAutomation>(nameof(ISqlAlwaysOnAutomation), serviceLogger);
						alwaysOn.BackupNewDatabase(dbName);
						JoinDatabaseToReplica(connection, secondary, dbName, groupName, serviceLogger);
					}, max_tries: 1, dbName: dbName);
				}
				else
				{
					JoinDatabaseToReplica(connection, secondary, dbName, groupName, serviceLogger);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				serviceLogger.Log(LogType.Warning, FormattableString.Invariant($"Failed to add new database to secondary server [{secondary}]."), ex);
			}
		}

		void UnjoinAvailabilityGroup(AdminConnection connection, string dbName)
		{
			ExecuteSql(connection, $"ALTER DATABASE [{dbName}] SET HADR OFF;");
		}

		static void NotifyTimeout(string dbName, string serverName, ILogger serviceLogger) => serviceLogger.Log(LogType.Error, $"Database [{dbName}] on [{serverName}] did not return to a restoring state after 5 minutes.\r\nA database join will be attempted on the next AON service task run.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		string DatabaseIsRestoring(DbConnection connection, string dbName)
		{
			var startTime = DateTime.UtcNow;
			do
			{
				var state = GetDatabaseStateDescription(connection, dbName);
				if (state.Equals("RESTORING"))
				{
					return state;
				}
				if (!Globals.IsTest)
				{
					Thread.Sleep(TimeSpan.FromSeconds(10));
				}
			}
			while (DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromMinutes(5));

			return string.Empty;
		}

		protected virtual void ExecuteSql(DbConnection connection, string sql)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(sql);
			}
		}

		void JoinDatabaseToReplica(AdminConnection primaryConnection, string serverName, string dbName, string groupName, ILogger serviceLogger)
		{
			serviceLogger.Log(LogType.Information, $"Joining database [{dbName}] to availability group [{groupName}] on secondary replica [{serverName}]");
			var sqlSecondaryWorker = new SqlAlwaysOnSecondaryServerWorker(groupName, dbName);
			sqlSecondaryWorker.AddDbToGivenSecondaryServer(primaryConnection, serverName);

			foreach (var secServerException in sqlSecondaryWorker.SecondaryServerExceptions)
			{
				serviceLogger.Warning($"Failed to join database to secondary server [{secServerException.Key}].\r\n{secServerException.Value.Message}");
			}
		}

		void CheckPrimaryServer(DbConnection primaryConnection, AvailabilityGroupInfo availabilityGroup, IEnumerable<string> operationalDbs, CancellationToken token, ILogger serviceLogger)
		{
			var databasesNotJoinedtoAG = GetDatabasesNotJoinedToReplica(primaryConnection, availabilityGroup.PrimaryReplicaId, operationalDbs);
			if (databasesNotJoinedtoAG.Count > 0)
			{
				var sqlAlwaysOn = ObjectFactory.Get<ISqlAlwaysOnAutomation>(nameof(ISqlAlwaysOnAutomation), serviceLogger);
				foreach (var db in databasesNotJoinedtoAG)
				{
					token.ThrowIfCancellationRequested();

					serviceLogger.Log(LogType.Information, $"Joining database [{db}] to availability group [{availabilityGroup.GroupName}] on primary replica [{primaryConnection.ServerName}]");

					if (RefDbTableNameResolver.IsSharedDatabase(db))
					{
						primaryConnection.RunLocked(key: db, process: (_) =>
						{
							sqlAlwaysOn.BackupNewDatabase(db);
							sqlAlwaysOn.AddDatabaseToAlwaysOnGroup(db);
						}, max_tries: 1, dbName: db);
					}
					else
					{
						sqlAlwaysOn.AddDatabaseToAlwaysOnGroup(db);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Auto generated baseline suppressions - WI00545660")]
		internal List<string> GetDatabasesNotJoinedToReplica(DbConnection connection, Guid replicaId, IEnumerable<string> operationalDbs)
		{
			var result = new List<string>();
			string dbCsvList = string.Join(",", operationalDbs.Select(db => string.Format(CultureInfo.InvariantCulture, "'{0}'", db)));

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT name FROM sys.databases
				WHERE name in ({0})
				AND (replica_id is null OR replica_id != '{1}')",
				dbCsvList, replicaId.ToString());

			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader[0].ToString());
				}
			}

			return result;
		}

		protected virtual string GetDatabaseStateDescription(DbConnection connection, string dbName) => connection.DatabaseStateDescription(dbName);

		protected static IEnumerable<string> GetDatabases(DbConnection connection)
		{
			return (RefDbTableNameResolver.ShouldUseSharedAvailabilityGroupDatabases(connection))
				? connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef)
				: connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.BI);
		}

		#endregion

		#region Propagate Logins

		void SynchroniseLoginsAndUsersAndPropagateLogins()
		{
			using (var securityConnection = Db.NewAdminConnection())
			{
				ServiceLogger.Log(LogType.Information, "User Login Synchronisation");
				var userManager = new DbUserManager(ServiceLogger);
				userManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(securityConnection);

				ServiceLogger.Log(LogType.Information, "Application Login Rights");
				EnsureApplicationDbLoginRightsForAllDatabases(securityConnection);
			}
		}

		/// <summary>
		/// All Application logins:
		///	  - Ensure login and database mappings.
		/// </summary>
		protected void EnsureApplicationDbLoginRightsForAllDatabases(AdminConnection securityConnection)
		{
			ServiceLogger.Log(LogType.Information, "Ensure login and database mappings");
			((IDbLoginRepair)securityConnection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => { ServiceLogger.Log(LogType.Information, msg); });

			var auditServerName = GetBiServerName(securityConnection, Db.AuditDatabaseName);
			var edwServerName = GetBiServerName(securityConnection, Db.EdwDatabaseName);

			auditServerName = string.IsNullOrEmpty(auditServerName) ? string.Empty : auditServerName;
			edwServerName = string.IsNullOrEmpty(edwServerName) ? string.Empty : edwServerName;

			var isMainDbOnSameServerAsAuditDb = Db.ServerName.Equals(auditServerName, StringComparison.OrdinalIgnoreCase);
			var isMainDbOnSameServerAsEdwDb = Db.ServerName.Equals(edwServerName, StringComparison.OrdinalIgnoreCase);
			var isEdwOnSameServerAsAuditDb = auditServerName.Equals(edwServerName, StringComparison.OrdinalIgnoreCase) && auditServerName.Length > 0;

			if (!isMainDbOnSameServerAsAuditDb || !isMainDbOnSameServerAsEdwDb)
			{
				if (isEdwOnSameServerAsAuditDb)
				{
					//edw is on the same server as audit - and they are not on the same server as the main db.
					EnsureApplicationDbLoginRightsForDatabasesOnServer(auditServerName, Db.AuditDatabaseName);
					EnsureApplicationDbLoginRightsForDatabasesOnServer(edwServerName, Db.EdwDatabaseName);
				}
				else
				{
					if (auditServerName.Length > 0 && !isMainDbOnSameServerAsAuditDb)
					{
						//audit db is on a separate server to edw - and they are not on the same server as the main db.
						EnsureApplicationDbLoginRightsForDatabasesOnServer(auditServerName, Db.AuditDatabaseName);
					}

					if (edwServerName.Length > 0 && !isMainDbOnSameServerAsEdwDb)
					{
						//edw db is on a separate server to audit - and they are not on the same server as the main db.
						EnsureApplicationDbLoginRightsForDatabasesOnServer(edwServerName, Db.EdwDatabaseName);
					}
				}
			}
		}

		void EnsureApplicationDbLoginRightsForDatabasesOnServer(string serverName, string databaseName)
		{
			using (var connection = Db.NewAdminConnection(serverName, databaseName))
			{
				((IDbLoginRepair)connection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => { ServiceLogger.Log(LogType.Information, msg); });
			}
		}

		public virtual string GetBiServerName(AdminConnection connection, string dbName)
		{
			var server = string.Empty;
			if (dbName == Db.AuditDatabaseName)
			{
				server = BiServers.LoadAuditServerUsingCacheIfPossible(connection);
			}
			else if (dbName == Db.EdwDatabaseName)
			{
				server = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection);
			}

			return server;
		}
		#endregion

	}
}
