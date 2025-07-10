using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Common.Data;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.Loader.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Server.Setup
{
	public class DatabaseInstaller : InstallationItem
	{
		public DatabaseInstaller(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.instanceName = installationSettings.SelectedDatabase.InstanceName;
			this.installationSettings = installationSettings;
			this.sqlContextManager = (CargowiseSetupSqlContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
			this.sqlInitializationService = Application.ServiceProvider.GetRequiredService<ISqlServerSecurityInitializationService>();
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			var sb = new StringBuilder();

			ChangeCurrentTaskDescription("Initializing database");

			sb.AppendLine("LOG");
			sb.AppendLine("OpenConnection() called");

			sqlContextManager.OpenConnection(installationSettings.ServerName);

			sb.AppendLine("OpenConnection() exited");

			bool serverInitialized = sqlInitializationService.IsServerInitialized(installationSettings.ServerName, installationSettings.DbName);
			bool serverAlreadyConfigured = serverInitialized && !DoesServerHaveNonCargowiseSysadmins();

			if (!serverAlreadyConfigured)
			{
				var otherApplicationDatabases = GetOtherApplicationDatabases();
				if (otherApplicationDatabases.Length > 0)
				{
					return InstallationResult.Error("This instance of SQL Server has databases for other applications on it: " + string.Join(",", otherApplicationDatabases) + "\r\n\r\n" +
						BrandingFactory.Instance.ProductName + " may cause these applications to stop working, so as a precaution, installation has been stopped. You should run setup again and choose an instance of SQL Server without any other databases. If you need to run " + BrandingFactory.Instance.ProductName + " on the same instance as other applications, contact WiseTech Global to discuss your requirements.");
				}

				sb.AppendLine("EnableMixedModeAuthentication() called");
				EnableMixedModeAuthentication();
				sb.AppendLine("EnableMixedModeAuthentication() exited");

				sb.AppendLine("ApplyConfigurationSettings() called");
				ApplyConfigurationSettings();
				sb.AppendLine("ApplyConfigurationSettings() exited");
				if (installationSettings.UseDefaultSQLServerMachineName)
				{
					sb.AppendLine("SetupBrowserService() called");
					SetupBrowserService();
					sb.AppendLine("SetupBrowserService() exited");
				}

				if (installationSettings.UseDefaultSQLServerMachineName)
				{
					sb.AppendLine("OpenWindowsFirewallForSqlService() called");
					OpenWindowsFirewallForSqlService();
					sb.AppendLine("OpenWindowsFirewallForSqlService() exited");
					sb.AppendLine("RestartService() called");
					RestartService();
					sb.AppendLine("RestartService() exited");
				}

				if (!serverInitialized)
				{
					sb.AppendLine("InitializeServer() called");
					sqlInitializationService.InitializeServer(installationSettings.ServerName);
					sb.AppendLine("InitializeServer() exited");
				}

				sb.AppendLine("SwitchToSysAdminAccount() called");
				sqlContextManager.SwitchToSysAdminAccount(installationSettings.ServerName);
				sb.AppendLine("SwitchToSysAdminAccount() exited");

				sb.AppendLine("RevokeSysadminFromOtherLogins() called");
				RevokeSysadminFromOtherLogins();
				sb.AppendLine("RevokeSysadminFromOtherLogins() exited");
			}

			sb.AppendLine("ConfigureModelDatabase() called");
			ConfigureModelDatabase();
			sb.AppendLine("ConfigureModelDatabase() exited");

			sb.AppendLine("CreateMainDatabase() called");
			CreateMainDatabase();
			sb.AppendLine("CreateMainDatabase() exited");

			sb.AppendLine("RestoreDatabases() called");
			RestoreDatabases();
			sb.AppendLine("RestoreDatabases() exited");

			sb.AppendLine("CreateApplicationLogin() called");
			sqlInitializationService.ReviveApplicationLoginsOnTargetServer(installationSettings.ServerName, installationSettings.DbName, installationSettings.ServerName, installationSettings.DbName);
			sb.AppendLine("CreateApplicationLogin() exited");

			return InstallationResult.OK();
		}

		void SetupBrowserService()
		{
			using (Service service = new Service("SQLBrowser"))
			{
				service.ChangeStartMode("Automatic");
				CommandLine sqlBrowserCommandLine = new CommandLine(service.PathName);
				try
				{
					WindowsFirewallWrapper.AddAuthorizedApplication("SQL Browser for " + BrandingFactory.Instance.ProductName, sqlBrowserCommandLine.ExecutableName);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					// Windows Firewall not installed - doesn't matter.
				}
				service.StartService();
			}
		}

		void OpenWindowsFirewallForSqlService()
		{
			using (Service sqlService = GetNewSqlServerService())
			{
				CommandLine sqlCommandLine = new CommandLine(sqlService.PathName);
				try
				{
					WindowsFirewallWrapper.AddAuthorizedApplication("SQL Server for " + BrandingFactory.Instance.ProductName, sqlCommandLine.ExecutableName);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					// Windows Firewall not installed - doesn't matter.
				}
			}
		}

		public InstallationSettings installationSettings;
		readonly CargowiseSetupSqlContextManager sqlContextManager;
		readonly ISqlServerSecurityInitializationService sqlInitializationService;

		readonly string instanceName;

		protected virtual string[] GetOtherApplicationDatabases()
		{
			var dbs = new List<string>();

			using (var reader = sqlContextManager.CurrentContext.ExecuteReader(@"
				DECLARE @SqlCmd nvarchar(max) = '';

				SELECT
					@SqlCmd = @SqlCmd  + '
						IF NOT exists(SELECT null FROM [' + db.name + '].sys.tables WHERE name = ''StmData'')
						BEGIN
							SELECT ''' + db.name + ''';
							RETURN;
						END;
					'
				FROM
					sys.databases db
				WHERE
					db.name NOT IN ('model', 'master', 'msdb', 'tempdb', 'Northwind', 'pubs', 'AdventureWorks', 'AdventureWorksDW', 'AdventureWorksAS') 
					AND db.name NOT LIKE 'ReportServer%'
					AND db.name not like '%[_]SD[0-9][0-9][0-9]'
					AND db.name not like '%[_]RefDb[_]___[_]__'
					AND db.name not like 'CW-RefDb-%'
					AND db.name not like 'CW-AG-RefDb-%'
					AND db.name not like 'CMR[_]ReferenceFiles[_]__'
					AND db.name not like 'EdiTariff[_]ReferenceFiles[_]__'
					AND db.name not like 'Enterprise[_]ReferenceFiles[_]__'

				SET @SqlCmd = @SqlCmd + 'SELECT 0;';
				EXEC (@SqlCmd);"))
			{
				while (reader.Read())
				{
					if (reader[0] is string)
					{
						dbs.Add((string)reader[0]);
					}
				}
			}
			return dbs.ToArray();
		}

		void EnableMixedModeAuthentication()
		{
			// This command was reverse engineered by profiling while changing the option in Management Studio
			sqlContextManager.CurrentContext.ExecuteNonQuery(@"xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2");
		}

		void ApplyConfigurationSettings()
		{
			sqlContextManager.CurrentContext.ExecuteNonQuery(@"
				EXEC sp_configure 'clr enabled', 1;
				EXEC sp_configure 'max text repl size (B)', -1;
				reconfigure;
			");
		}

		IList<ServiceController> dependentServicesToStart;
		IList<ServiceController> DependentServicesToStart
		{
			get
			{
				if (dependentServicesToStart == null)
				{
					dependentServicesToStart = new List<ServiceController>();
				}
				return dependentServicesToStart;
			}
		}

		void StopDependentServices()
		{
			ServiceController serviceController;
			serviceController = new ServiceController(GetSqlServerServiceName());

			DependentServicesToStart.Clear();

			//stop all the Dependent services that are not stopped
			foreach (ServiceController dependentService in serviceController.DependentServices)
			{
				switch (dependentService.Status)
				{
					case ServiceControllerStatus.Stopped:
						//already stopped...nothing to do
						break;

					case ServiceControllerStatus.StopPending:
						dependentService.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 20));
						break;

					default:
						dependentService.Stop();
						dependentService.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 20));
						DependentServicesToStart.Add(dependentService);
						break;
				}
			}
		}

		void StartDependentServices()
		{
			foreach (ServiceController dependentService in DependentServicesToStart)
			{
				switch (dependentService.Status)
				{
					case ServiceControllerStatus.StartPending:
						//just wait for it to start
						dependentService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 20));
						break;

					case ServiceControllerStatus.Running:
						//already running.nothing to do
						break;

					default:
						dependentService.Start();
						dependentService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 20));
						break;
				}
			}
		}

		void RestartService()
		{
			// These WMI objects are not refreshable so we have to make a new one every time to detect property changes.

			const int millisecondsToWait = 40 * 1000;
			int startingTicks = Environment.TickCount;
			using (Service sqlService = GetNewSqlServerService())
			{
				uint errorCode = sqlService.StopService();
				if (errorCode == 3 /*Dependent services are running*/)
				{
					StopDependentServices();
					sqlService.StopService();
				}
			}

			string state = "";

			while (state != "Stopped" && Environment.TickCount - startingTicks < millisecondsToWait)
			{
				using (Service sqlService = GetNewSqlServerService())
				{
					state = sqlService.State;
				}
				Thread.Sleep(500);
			}
			if (state != "Stopped")
			{
				throw new Exception("Unable to stop SQL Server service");
			}

			startingTicks = Environment.TickCount;
			using (Service sqlService = GetNewSqlServerService())
			{
				sqlService.StartService();
			}

			while (state != "Running" && Environment.TickCount - startingTicks < millisecondsToWait)
			{
				using (Service sqlService = GetNewSqlServerService())
				{
					state = sqlService.State;
				}

				Thread.Sleep(500);
			}
			if (state != "Running")
			{
				throw new Exception("Unable to start SQL Server service");
			}
			else if (DependentServicesToStart.Count > 0)
			{
				StartDependentServices();
			}
		}

		string GetTargetDatabaseFilePath(string path, string dbName, string extension)
		{
			int number = 0;
			string result;
			string prefix = dbName;
			string suffix = (extension == ".ldf") ? "_Log" : "_Data";
			do
			{
				string fileName = prefix;
				if (number > 0)
				{
					fileName += number;
				}
				result = Path.Combine(path, fileName + suffix + extension);
				number++;
			}
			while (File.Exists(result));
			return result;
		}

		bool DoesServerHaveNonCargowiseSysadmins()
		{
			return GetOtherSysAdminLoginNames().Any();
		}

		protected virtual void RevokeSysadminFromOtherLogins()
		{
			var sql = @"
				EXEC sp_dropsrvrolemember @LoginName, 'sysadmin';
				EXEC sp_dropsrvrolemember @LoginName, 'securityadmin';
				EXEC('GRANT VIEW SERVER STATE TO [' + @LoginName + ']');";

			foreach (var loginName in GetOtherSysAdminLoginNames())
			{
				sqlContextManager.CurrentContext.ExecuteNonQuery(sql, CommandType.Text,
					command =>
					{
						command.AddParameter("@LoginName", DbType.String, loginName);
					});
			}
		}

		internal IEnumerable<string> GetOtherSysAdminLoginNames()
		{
			const string commandText = $@"SELECT name
				FROM sys.server_principals
				WHERE (IS_SRVROLEMEMBER('sysadmin', name) = 1 OR IS_SRVROLEMEMBER('securityadmin', name) = 1)
				AND name NOT IN ('sa', @OALoginName)";

			var list = new List<string>();

			using (var reader = sqlContextManager.CurrentContext.ExecuteReader(
				commandText,
				CommandType.Text,
				cmd => cmd.AddParameter("@OALoginName", DbType.String, OdysseyAdminCredentials.AdminUserName)))
			{
				while (reader.Read())
				{
					list.Add((string)reader[0]);
				}
			}

			return list;
		}

		protected virtual void RestoreDatabases()
		{
			var backupDirectory = installationSettings.GetCDInstallPath("Databases");

			foreach (var backupFile in Directory.GetFiles(backupDirectory, "*.bak"))
			{
				var dbName = installationSettings.DbName + "_" + Path.GetFileNameWithoutExtension(backupFile);

				ChangeCurrentTaskDescription("Restoring databases");

				var isMainDb = dbName.Equals("Odyssey", StringComparison.OrdinalIgnoreCase);

				if (!isMainDb && sqlContextManager.CurrentContext.ExecuteScalar($"SELECT DB_ID('{dbName}')") == DBNull.Value)
				{
					var restoreCommand = string.Format("RESTORE DATABASE [{0}] FROM DISK='{1}' WITH ", dbName, backupFile);

					using (var reader = sqlContextManager.CurrentContext.ExecuteReader("RESTORE FILELISTONLY FROM DISK=@BackupFile", CommandType.Text,
						fileList => fileList.AddParameter("@BackupFile", DbType.String, backupFile)))
					{
						while (reader.Read())
						{
							var logicalName = reader[0].ToString();
							var type = reader[2].ToString();

							var path = type == "L" ? installationSettings.LogPath : installationSettings.DataPath;
							var extension = type == "L" ? ".ldf" : ".mdf";
							restoreCommand += string.Format("MOVE '{0}' TO '{1}',", logicalName, GetTargetDatabaseFilePath(path, dbName, extension));
						}
					}

					restoreCommand = restoreCommand.TrimEnd(',');

					sqlContextManager.CurrentContext.ExecuteNonQuery(restoreCommand, CommandType.Text, cmd => cmd.CommandTimeout = 0);
				}
			}
		}

		void ConfigureModelDatabase()
		{
			sqlContextManager.CurrentContext.ExecuteNonQuery(@"
ALTER DATABASE model SET AUTO_CREATE_STATISTICS       ON WITH NO_WAIT;
ALTER DATABASE model SET AUTO_UPDATE_STATISTICS       ON WITH NO_WAIT;
ALTER DATABASE model SET AUTO_UPDATE_STATISTICS_ASYNC ON WITH NO_WAIT;
");
		}

		void CreateMainDatabase()
		{
			if (installationSettings.UseDefaultSQLServerMachineName)
			{
				CreateDataAndLogPathsWithAppropriatePermissions();
			}
			CreateMainDBWithRequiredTables();
		}

		void CreateMainDBWithRequiredTables()
		{
			sqlContextManager.CurrentContext.ExecuteNonQuery(CreateDBSQL(), CommandType.Text, cmd => cmd.CommandTimeout = 0);

			sqlContextManager.CurrentContext.ExecuteNonQuery($"[{installationSettings.DbName}]..sp_executesql", CommandType.StoredProcedure, cmd =>
			{
				cmd.AddParameter("@stmt", DbType.String, DataUtils.SQL_InitialTablesForEmptyDatabase());
				cmd.CommandTimeout = 0;
			});
		}

		protected virtual string CreateDBSQL()
		{
			return string.Format(
	@"USE master

CREATE DATABASE [{0}]
ON ( NAME = [{0}_data], FILENAME = '{1}' )
LOG ON ( NAME = [{0}_log], FILENAME = '{2}' , SIZE={3}MB, FILEGROWTH={4}MB )
ALTER DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS
", installationSettings.DbName, Path.Combine(installationSettings.DataPath, installationSettings.DbName + ".mdf"), Path.Combine(installationSettings.LogPath, installationSettings.DbName + "_log.ldf")
	, DatabaseConstants.LogFileIntialSizeMb, DatabaseConstants.LogFileGrowthMb);
		}

		void CreateDataAndLogPathsWithAppropriatePermissions()
		{
			string serviceAccountName;
			IdentityReference serviceAccount;

			using (Service service = GetNewSqlServerService())
			{
				serviceAccountName = service.StartName;
			}

			// LocalSystem is a special value used by the Service Control Manager. It does not correspond to an account,
			// but it does include SYSTEM's SID in its token.
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dllproc/base/localsystem_account.asp

			if (serviceAccountName.EndsWith("\\LocalSystem", StringComparison.OrdinalIgnoreCase)
				|| serviceAccountName.Equals("LocalSystem", StringComparison.OrdinalIgnoreCase))
			{
				serviceAccount = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
			}
			else
			{
				serviceAccount = new NTAccount(serviceAccountName);
			}

			FileSystemAccessRule sqlServerCanModify = new FileSystemAccessRule(serviceAccount, FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);

			foreach (string path in new string[] { installationSettings.DataPath, installationSettings.LogPath })
			{
				Directory.CreateDirectory(path);
				var directoryInfo = new DirectoryInfo(path);
				DirectorySecurity descriptor = directoryInfo.GetAccessControl();
				descriptor.AddAccessRule(sqlServerCanModify);
				directoryInfo.SetAccessControl(descriptor);
			}
		}

		Service GetNewSqlServerService()
		{
			string serviceName = GetSqlServerServiceName();
			return new Service(serviceName);
		}

		string GetSqlServerServiceName()
		{
			return (string.IsNullOrEmpty(instanceName) ? "MSSQLSERVER" : "MSSQL$" + instanceName);
		}

#if DEBUG
		public InstallationResult InstallExcludingDependenciesForTest()
		{
			return InstallExcludingDependencies();
		}
#endif

	}
}
