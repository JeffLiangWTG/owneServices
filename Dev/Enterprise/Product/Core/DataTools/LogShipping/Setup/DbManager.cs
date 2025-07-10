using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.LogShipping.Setup
{
	public class DbManager
	{
		#region Enterprise Databases

		#region Name Conversion

		public string GetMasterDatabaseNameFromDependentDatabaseName(string databaseName)
		{
			Argument.NotNull(databaseName, nameof(databaseName));

			return Regex.Replace(databaseName, eDocsRegex, string.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string eDocsRegex = @"_SD\d{3}";

		#endregion

		public LogShippingInfo[] GetSecondaryDatabasesInfo(SqlServerInfo server)
		{
			Argument.NotNull(server, nameof(server));

			LastErrorMessage = string.Empty;
			LogShippingInfo[] result = null;
			var secondaryDbList = new List<LogShippingInfo>();

			try
			{
				using var connection = OpenNewConnectionWithOdysseyAdminLogin(server.FullInstanceName);
				using var cmd = NewSqlCommand(SecondaryDatabasesInfoScript, connection);
				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					LogShippingInfo setupInfo;
					DatabaseInfo dbInfo = null;

					string primaryDatabaseName = reader["PrimaryDatabase"] != null ? (string)reader["PrimaryDatabase"] : string.Empty;

					if (reader["IsDependentDatabase"] != null && (int)reader["IsDependentDatabase"] == 0)
					{
						setupInfo = new LogShippingInfo();
						dbInfo = new MainDatabaseInfo(setupInfo, primaryDatabaseName);
						setupInfo.MainDatabase = (MainDatabaseInfo)dbInfo;
						secondaryDbList.Add(setupInfo);

						setupInfo.SecondaryServer = server;
						setupInfo.PrimaryServerFromLSMetadata = (string)reader["PrimaryServer"];

						if (!string.IsNullOrEmpty(setupInfo.PrimaryServerFromLSMetadata))
						{
							setupInfo.PrimaryServer = GetSqlServerInfo(setupInfo.PrimaryServerFromLSMetadata, "0.0.0.0");
						}

						setupInfo.BackupLocalCopyDirectory = setupInfo.OriginalBackupLocalCopyDirectory = (string)reader["BackupDestinationDirectory"];
						setupInfo.BackupSourceDirectory = setupInfo.OriginalBackupSourceDirectory = (string)reader["BackupSourceDirectory"];
					}
					else
					{
						setupInfo = secondaryDbList.Find(dbInfo1 => dbInfo1.MainDatabase.DatabaseName == GetMasterDatabaseNameFromDependentDatabaseName(primaryDatabaseName));

						if (setupInfo != null && setupInfo.MainDatabase != null)
						{
							dbInfo = new DependentDatabaseInfo(setupInfo.MainDatabase, primaryDatabaseName);
							setupInfo.MainDatabase.DependentDatabases.Add((DependentDatabaseInfo)dbInfo);
						}
					}

					if (dbInfo != null)
					{
						if (reader["CopyJob"] != null)
						{
							dbInfo.CopyJobId = (Guid)reader["CopyJob"];
						}

						if (reader["RestoreJob"] != null)
						{
							dbInfo.RestoreJobId = (Guid)reader["RestoreJob"];
						}
					}
				}
				result = secondaryDbList.ToArray();
			}
			catch (System.Data.Common.DbException ex)
			{
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		public bool IsDatabaseInStandByMode(string serverName, string dbName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			LastErrorMessage = string.Empty;
			bool result = false;

			try
			{
				using var connection = OpenNewConnectionWithOdysseyAdminLogin(serverName);
				using var cmd = NewSqlCommand(IsDatabaseInStandByModeScript, connection);

				var parameter = cmd.CreateParameter();
				parameter.ParameterName = "@DbName";
				parameter.Value = dbName;
				cmd.Parameters.Add(parameter);

				var executeScalar = cmd.ExecuteScalar();

				if (executeScalar != null)
				{
					result = (int)executeScalar == 1;
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		public string[] GetPrimaryLinkedEDocsDatabases(LogShippingInfo setupInfo, Action<string> showError = null)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));
			Argument.NotNull(setupInfo.PrimaryServer, nameof(setupInfo.PrimaryServer));
			Argument.NotNull(setupInfo.MainDatabase, nameof(setupInfo.MainDatabase));

			LastErrorMessage = String.Empty;
			string[] result = null;

			try
			{
				using var connection = OpenNewConnectionWithOdysseyAdminLogin(setupInfo.PrimaryServer.FullInstanceName, setupInfo.MainDatabase.DatabaseName);
				using var cmd = NewSqlCommand(GetPrimaryLinkedEDocsDatabasesScript, connection);

				var parameter = cmd.CreateParameter();
				parameter.ParameterName = "@DbName";
				parameter.Value = setupInfo.MainDatabase.DatabaseName;
				cmd.Parameters.Add(parameter);

				using var reader = cmd.ExecuteReader();
				var linkedEDocsDatabases = new List<string>();

				while (reader.Read())
				{
					if (reader.FieldCount > 0)
					{
						linkedEDocsDatabases.Add(reader.GetString(0));
					}
				}

				result = linkedEDocsDatabases.ToArray();
			}
			catch (System.Data.Common.DbException ex)
			{
				result = null;
				LastErrorMessage = ex.Message;
				if (showError != null)
				{
					showError(LastErrorMessage);
				}
			}

			return result;
		}

		public Dictionary<string, List<string>> GetPrimaryDatabaseBackups(LogShippingInfo info)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(info.SecondaryServer, nameof(info.SecondaryServer));

			LastErrorMessage = string.Empty;
			var result = new Dictionary<string, List<string>>();

			try
			{
				string backupSourceDirectory = info.BackupSourceDirectory;

				if (!string.IsNullOrEmpty(backupSourceDirectory))
				{
					using var connection = OpenNewConnectionWithOdysseyAdminLogin(info.SecondaryServer.FullInstanceName);

					var backupFiles = GetBackupFiles(backupSourceDirectory, connection);
					foreach (string backupName in backupFiles)
					{
						string backupInfoScript = string.Format("RESTORE HEADERONLY FROM DISK = '{0}'", Path.Combine(backupSourceDirectory, backupName));

						try
						{
							string backupServerName;
							string backupDbName;
							using (var cmd = NewSqlCommand(backupInfoScript, connection))
							{
								using var reader = cmd.ExecuteReader();
								cmd.CommandTimeout = 600;

								reader.Read();
								backupServerName = reader["ServerName"] != null ? reader["ServerName"].ToString() : string.Empty;
								backupDbName = reader["DatabaseName"] != null ? reader["DatabaseName"].ToString() : string.Empty;
							}

							if (info.PrimaryServer != null && info.PrimaryServer.FullInstanceName.Equals(backupServerName, StringComparison.OrdinalIgnoreCase))
							{
								foreach (DatabaseInfo match in info.GetDatabaseInfoListToProcess())
								{
									if (match != null && match.DatabaseName.Equals(backupDbName, StringComparison.OrdinalIgnoreCase))
									{
										if (!result.ContainsKey(match.DatabaseName))
										{
											result.Add(match.DatabaseName, new List<string>());
										}

										if (result[match.DatabaseName] != null)
										{
											result[match.DatabaseName].Add(backupName);
										}

										break;
									}
								}
							}
						}
						catch (System.Data.Common.DbException ex)
						{
							// Used to swallow all SqlException types with the comment: "IF RESTORE HEADERONLY FAILS DO NOT ADD BACKUP FILE TO THE RESULT".
							// This was hiding a timeout exception, giving the user no clue of what was happening.
							// SHOULD WE NEED TO IGNORE A PARTICULAR EXCEPTION, FIRST WE MUST CHECK FOR THE SPECIFIC ERROR.
							result = null;
							LastErrorMessage = ex.Message;
						}
					}
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				result = null;
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		string[] GetBackupFiles(string sourceDirectory, SqlConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new List<string>();

			string getFilesScript = string.Format("EXEC sys.xp_dirtree '{0}', 1, 1", sourceDirectory);

			using (var cmd = NewSqlCommand(getFilesScript, connection))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader["file"] != null && (int)reader["file"] == 1)
					{
						string backupName = (string)reader["subdirectory"];

						if (backupName != null && backupName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
						{
							result.Add(backupName);
						}
					}
				}
			}

			return result.ToArray();
		}

		public string[] GetEnterpriseDatabases(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			LastErrorMessage = String.Empty;
			string[] result = null;
			var enterpriseDbList = new List<string>();

			try
			{
				using var connection = OpenNewConnectionWithOdysseyAdminLogin(serverName);
				using var cmd = NewSqlCommand(GetSelectEnterpriseDatabasesScript(), connection);
				using var reader = cmd.ExecuteReader();
				{
					while (reader.Read())
					{
						enterpriseDbList.Add((string)reader["DatabaseName"]);
					}
				}
				result = enterpriseDbList.ToArray();
			}
			catch (System.Data.Common.DbException ex)
			{
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string GetSelectEnterpriseDatabasesScript()
		{
			const string selectEnterpriseDatabasesScriptMask = @"
					DECLARE @SelectEnterpriseDbs nvarchar(max);

					SELECT
						@SelectEnterpriseDbs =
							isnull(@SelectEnterpriseDbs + ' UNION ALL ', '') +
							'SELECT ''' + db.[name] + ''' as DatabaseName FROM [' + db.[name] +
							'].sys.tables WHERE [name] in (''GlbStaff'', ''JobShipment'', ''StmData'', ''RefCountry'') HAVING count(*) = 4'
					FROM sys.databases db
					LEFT JOIN sys.dm_hadr_availability_replica_states nonPrimaryReplica
						ON nonPrimaryReplica.replica_id = db.replica_id
						AND nonPrimaryReplica.is_local = 1
						AND nonPrimaryReplica.[role] <> 1
					WHERE db.[name] not in ('master', 'tempdb', 'model', 'msdb')
					AND db.[name] not like '%[''.,_-]%'
					AND db.[state] = 0
					AND db.is_in_standby = 0
					AND db.is_read_only = 0
					AND nonPrimaryReplica.replica_id is null
					{0}
					ORDER BY [name];

					EXEC (@SelectEnterpriseDbs);";

			return String.Format(
				selectEnterpriseDatabasesScriptMask,
				(EnterpriseDbRecoveryMustBeFull) ? " AND recovery_model = 1 " : "");
		}

		bool EnterpriseDbRecoveryMustBeFull
		{
			get
			{
#if DEBUG
				return false;
#else
				return true;
#endif
			}
		}

		const string IsDatabaseInStandByModeScript = @"
SELECT Count(*)
FROM sys.databases
WHERE [name] = @DbName
AND is_in_standby = 1
AND [state] = 0
";

		const string SecondaryDatabasesInfoScript = @"
SELECT
	ls.primary_server PrimaryServer,
	ls.primary_database PrimaryDatabase,
	ls.backup_source_directory BackupSourceDirectory,
	ls.backup_destination_directory BackupDestinationDirectory,
	secdb.secondary_database SecondaryDatabase,
	ls.copy_job_id CopyJob,
	ls.restore_job_id RestoreJob,
	CASE WHEN ls.primary_database LIKE '%[_]SD[0-9][0-9][0-9]' THEN 1 ELSE 0 END AS IsDependentDatabase
FROM
	msdb..log_shipping_secondary ls
	INNER JOIN msdb..log_shipping_secondary_databases secdb
		ON secdb.secondary_id = ls.secondary_id
ORDER BY
	IsDependentDatabase
";

		const string GetPrimaryLinkedEDocsDatabasesScript = @"
SELECT 
	name
FROM
	sys.databases
WHERE
	name LIKE @DbName + '[_]SD[0-9][0-9][0-9]'
ORDER BY
	name";

		#endregion

		#region Exclusive Access

		public static void KillConnections(System.Data.Common.DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));

			using (var cmd = NewSqlCommand(KillConnectionsScript, connection))
			{
				var parameter = cmd.CreateParameter();
				parameter.ParameterName = "@DbName";
				parameter.Value = dbName;
				cmd.Parameters.Add(parameter);
				cmd.ExecuteNonQuery();
			}
		}

		public static void DropDatabase(System.Data.Common.DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));

			using (var cmd = NewSqlCommand(DropDatabaseScript, connection))
			{
				var parameter = cmd.CreateParameter();
				parameter.ParameterName = "@DbName";
				parameter.Value = dbName;
				cmd.Parameters.Add(parameter);
				cmd.ExecuteNonQuery();
			}
		}

		const string KillConnectionsScript = @"
DECLARE @SpId int
DECLARE ProcessesToKillCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT prc.session_id
	FROM sys.dm_exec_sessions as prc
	WHERE
	(
		prc.session_id IN (
				SELECT request_session_id 
				FROM sys.dm_tran_locks 
				WHERE resource_database_id = db_id(@DbName)
		)
		OR prc.database_id = db_id(@DbName)
	)
	AND prc.session_id != @@spid AND prc.session_id >= 51 AND isnull(prc.host_name, '') != '';

OPEN ProcessesToKillCursor;
FETCH NEXT FROM ProcessesToKillCursor INTO @SpId;
WHILE @@FETCH_STATUS = 0
BEGIN
	EXEC('KILL ' + @SpId);
	FETCH NEXT FROM ProcessesToKillCursor INTO @SpId;
END
CLOSE ProcessesToKillCursor;
DEALLOCATE ProcessesToKillCursor;
";
		const string DropDatabaseScript = @"IF DB_ID (@DbName) IS NOT NULL EXEC ('DROP DATABASE [' + @DbName + ']')
";

		#endregion

		#region Get Sql Server Info

		public SqlServerInfo GetSqlServerInfoFromServerName(string serverName)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));

			LastErrorMessage = string.Empty;
			SqlServerInfo result = null;

			try
			{
				result = GetSqlServerInfo(serverName, GetSqlServerVersion(serverName));
			}
			catch (System.Data.Common.DbException ex)
			{
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		SqlServerInfo GetSqlServerInfo(string serverName, string serverVersion)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(serverVersion, nameof(serverVersion));

			var info = serverName.Split('\\');

			return new SqlServerInfo(info[0], info.Length > 1 ? info[1] : string.Empty, serverVersion);
		}

		string GetSqlServerVersion(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));
			using var connection = OpenNewConnectionWithOdysseyAdminLogin(serverName);
			using var cmd = NewSqlCommand(SqlServerPropertiesScript, connection);

			var result = (string)cmd.ExecuteScalar();

			return result;
		}

		#endregion

		#region Check Connection

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		const string LoginFailedForUserError = "Login failed for user '";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		const string LoginAlreadyExistsErrorStart = "The server principal '";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		const string LoginAlreadyExistsErrorEnd = "' already exists.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool CheckConnection(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			LastErrorMessage = String.Empty;
			bool result = TryConnectWithOdysseyAdminLogin(serverName);

			if (
				!result
				&& !String.IsNullOrWhiteSpace(LastErrorMessage)
				&& LastErrorMessage.StartsWith(LoginFailedForUserError, StringComparison.OrdinalIgnoreCase)
			)
			{
				if (TryConnectWithWindowsAuthAndInitializeServerSecurity(serverName))
				{
					// Retry after creating admin login
					result = TryConnectWithOdysseyAdminLogin(serverName);
				}
				else if (
					!String.IsNullOrWhiteSpace(LastErrorMessage)
					&& LastErrorMessage.StartsWith(LoginAlreadyExistsErrorStart, StringComparison.OrdinalIgnoreCase)
					&& LastErrorMessage.EndsWith(LoginAlreadyExistsErrorEnd, StringComparison.OrdinalIgnoreCase)
				)
				{
					LastErrorMessage = "Server should have mixed authentication mode (Windows and SQL Server) to use this tool.";
				}
			}

			return result;
		}

		public bool TryConnectWithOdysseyAdminLogin(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			LastErrorMessage = string.Empty;
			bool result = true;

			try
			{
				var connection = OpenNewConnectionWithOdysseyAdminLogin(serverName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		bool TryConnectWithWindowsAuthAndInitializeServerSecurity(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			var initService = Program.ServiceProvider.GetRequiredService<ISqlServerSecurityInitializationService>();
			var contextManager = (LogShippingSqlAdministrationContextManager)Program.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
			var context = contextManager.GetSqlExecutionContext(serverName, MasterDbName);

			try
			{
				if (!initService.IsServerInitialized(serverName, MasterDbName))
				{
					initService.InitializeServer(serverName, MasterDbName);
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false;
			}

			return false;
		}

		public bool CheckSqlServerName(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			LastErrorMessage = string.Empty;
			bool result = true;

			try
			{
				using var connection = OpenNewConnectionWithOdysseyAdminLogin(serverName);
				using var cmd = NewSqlCommand(CheckSqlServerNameScript, connection);
				cmd.ExecuteNonQuery();
			}
			catch (System.Data.Common.DbException ex)
			{
				result = false;
				LastErrorMessage = ex.Message;
			}

			return result;
		}

		const string SqlServerPropertiesScript = @"
			SELECT SERVERPROPERTY('ProductVersion')";

		const string CheckSqlServerNameScript = @"
DECLARE @OldServerName nvarchar(128);
DECLARE @NewServerName nvarchar(128);
DECLARE @SysServerName nvarchar(128);
DECLARE @Error nvarchar(max)

SELECT
	@OldServerName = @@SERVERNAME,
	@NewServerName = CONVERT(nvarchar(128), SERVERPROPERTY('ServerName'));
      
IF (@OldServerName != @NewServerName)
BEGIN
	SELECT @SysServerName = [name]
		FROM sys.servers
		WHERE server_id = 0;

	IF (@SysServerName != @NewServerName)
	BEGIN
		EXEC ('sp_dropserver ''' +  @SysServerName + '''');
		EXEC ('sp_addserver ''' + @NewServerName+''', ''local''');

		SET @Error =
			'This computer has been renamed after SQL Server was installed, ' +
			'hence the internal SQL Server name does not match the computer name. ' +
			'The problem has been rectified but to become effective the SQL Server Service must be restarted. ' +
			'Please restart the service for [' + @NewServerName + '] and run this setup again.';
		RAISERROR (@Error, 16, 1);
	END;
END;
";

		#endregion

		#region Connection

		internal static SqlConnection OpenNewConnectionWithOdysseyAdminLogin(string serverName)
		{
			return OpenNewConnectionWithOdysseyAdminLogin(serverName, MasterDbName);
		}

		static SqlConnection OpenNewConnectionWithOdysseyAdminLogin(string serverName, string dbName)
		{
			var connectionProvider = Program.ServiceProvider.GetRequiredService<ISqlConnectionProvider>();
			var pdsFactory = Program.ServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var pds = pdsFactory.CreateEnterpriseService(serverName);

			return connectionProvider.OpenNewSqlConnection<OdysseyAdminCredentials>(pds, builder => ConfigureConnectionString(builder, serverName));
		}

		internal static void ConfigureConnectionString(SqlConnectionStringBuilder builder, string serverName)
		{
			builder.DataSource = serverName;
			builder.InitialCatalog = MasterDbName;
			builder.ApplicationName = ApplicationName;
			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
			builder.Encrypt = encrypt;
			builder.TrustServerCertificate = !encrypt;
		}

		internal static System.Data.Common.DbCommand NewSqlCommand(string cmdText, System.Data.Common.DbConnection connection)
		{
			Argument.NotNull(cmdText, nameof(cmdText));
			Argument.NotNull(connection, nameof(connection));

#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one
			var command = connection.CreateCommand();
			command.CommandText = cmdText;
			return command;
#pragma warning restore CW1116
		}

		#endregion

		public string LastErrorMessage { get; private set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string MasterDbName = "master";
		public const string ApplicationName = "CargoWiseOneLogShippingSetup";
	}
}
