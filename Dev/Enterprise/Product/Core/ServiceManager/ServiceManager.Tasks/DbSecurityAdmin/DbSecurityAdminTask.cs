using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ScheduleTypeConstants))]
[assembly:
	HostedService(
		ScheduleTypeConstants.DbSecurityAdminCode,
		DbSecurityAdminTask.ServiceTaskDescription,
		"DBM",
		typeof(DbSecurityAdminTask),
		IsMandatory = true,
		MinimumPeriod = "1hour",
		MaximumPeriod = "1day",
		IsScheduleReadOnly = true,
		IsReadOnlyForWiseCloudClient = true,
		AlwaysRunAtStartup = true,
		CanRunInAnyBranch = true,
		DefaultScheduleRunEvery = "12hours",
		ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.DbSecurityAdmin
{
	public class DbSecurityAdminTask : ServiceProviderImpl
	{
		public const string ServiceTaskDescription = "Database Security Admin Task";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Security check started");

			using (var securityConnection = Db.NewAdminConnection())
			{
				ServiceLogger.Log(LogType.Information, "Security rights");

				var dbSecurityGuard = new DatabaseSecurityGuard();
				if (EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
				{
					dbSecurityGuard.BuildSecurity(securityConnection, token, ServiceLogger);
				}
				else
				{
					dbSecurityGuard.CheckAndLockDownOpenDatabaseSecurity(securityConnection, ServiceLogger);
				}

				dbSecurityGuard.CheckAndLockDownMsdbAccess(securityConnection, ServiceLogger);

				ServiceLogger.Log(LogType.Information, "Database Settings");
				EnsureDatabaseSettings(securityConnection, token);
			}

			ServiceLogger.Log(LogType.Information, "Security check completed");
		}

		#region EnsureDatabaseSettings

		protected void EnsureDatabaseSettings(AdminConnection connection, CancellationToken token)
		{
			foreach (var dbName in connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.Audit))
			{
				token.ThrowIfCancellationRequested();

				try
				{
					DataUtils.AlterDbAuthorisation(connection, dbName);
				}
				catch (SqlException ex)
				{
					ServiceLogger.Log(LogType.Warning, $"EnsureDatabaseSettings encountered an error with database: {dbName}, Error: {ex}");
				}
			}

			token.ThrowIfCancellationRequested();

			DataUtils.EnsureClrEnabledAndTrustworthyOn(connection, Db.DatabaseName);

			EnsureBiDbSettings(connection);

			return;
		}

		void EnsureBiDbSettings(AdminConnection connection)
		{
			var auditServerName = GetBiServerName(connection, Db.AuditDatabaseName);
			EnableBiDbTrustworthinessIfExists(auditServerName, Db.AuditDatabaseName);

			var edwServerName = GetBiServerName(connection, Db.EdwDatabaseName);
			EnableBiDbTrustworthinessIfExists(edwServerName, Db.EdwDatabaseName);
		}

		void EnableBiDbTrustworthinessIfExists(string biServerName, string dbName)
		{
			if (!string.IsNullOrEmpty(biServerName))
			{
				using (var biConnection = Db.NewAdminConnection(biServerName, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(dbName))
					{
						if (BiDbIsOnLinkedServer(biServerName))
						{
							DataUtils.SetServerConfigOption(biConnection, "clr enabled", "1");
						}

						DataUtils.EnsureClrEnabledAndTrustworthyOn(biConnection, dbName);
					}
				}
			}
		}

		bool BiDbIsOnLinkedServer(string biServerName)
		{
			return !Db.ServerName.Equals(biServerName);
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
