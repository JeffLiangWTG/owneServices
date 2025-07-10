#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Dat.Integration;
using Enterprise.DbUpgrader.Startup;
using Enterprise.Initialisation;
using WTG.DeploymentUtils.FileSystem;
using static System.FormattableString;

namespace Enterprise.Dat.Implementation
{
	class DatBackupFileGenerator : IDeploymentProcess
	{
		public DatBackupFileGenerator(DeploymentConfiguration configuration)
		{
			Configuration = configuration;
		}

		public DeploymentConfiguration Configuration { get; }

		internal string CurrentDATDbBackupPrefixFileLocation
		{
			get { return Path.Combine(Configuration.SourcePath, "Common", "Tools", "BuildTools", "BuildTools", "CurrentDatDbBackupPrefix.txt"); }
		}

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public void Deploy(ITaskLogger logger)
		{
			Db.DisableSchemaVersionCheckPermanently();
			Db.InitializeDatabaseDetails(System.Environment.MachineName, DatbaseName);

			try
			{
				using (var connection = Db.NewAdminConnection(System.Environment.MachineName, "master"))
				{
					using (logger.RecordTask("Restore"))
					{
						new DbRestorer(((IDbConnectionInternals)connection).ADOConnection, logger, DatbaseName).RestoreCurrentDbs();
					}
				}

				using (var connection = Db.NewAdminConnection(System.Environment.MachineName, DatbaseName))
				{
					using (logger.RecordTask("Creating application login"))
					{
						((IDbLoginRepair)connection).EnsureRestrictedWriterDbLogin();
					}

					using (logger.RecordTask("Disable CDC"))
					{
						connection.ExecuteNonQuery("EXEC sys.sp_cdc_disable_db");
					}

					using (logger.RecordTask("Disable change tracking on enabled tables"))
					{
						DisableChangeTrackingOnEnabledTables(connection);
					}

					using (logger.RecordTask("CHECKPOINT"))
					{
						connection.ExecuteNonQuery("CHECKPOINT");
					}
				}

				using (logger.RecordTask("Initalizing"))
				{
					Initialiser.InitialiseWinForms();
				}

				using (logger.RecordTask("Checking SQL Server Name"))
				{
					CheckSqlServerName(logger);
				}

				using (logger.RecordTask("Upgrade"))
				{
					using (ConfigureEnvironmentForUpgrade())
					{
						if (!new DeployDbUpgraderDirector(logger).CheckAndUpgradeDb().Successful)
						{
							throw new InvalidOperationException("Upgrade failed");
						}
					}
				}

				using (var connection = Db.NewAdminConnection(System.Environment.MachineName, DatbaseName))
				{
					using (logger.RecordTask("Deincrement schema version"))
					{
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(connection) - 1, connection);
					}
				}

				using (var temp = new TempDirectory())
				{
					var filePrefix = "CW" + DateTime.Now.ToString("yyyyMMdd");

					PrepareBackup(DatbaseName, Path.Combine(temp, filePrefix + ".bak"), logger);
					PrepareBackup(DatbaseName + "_Audit", Path.Combine(temp, filePrefix + "_Audit.bak"), logger);
					PrepareBackup(DatbaseName + "_EDW", Path.Combine(temp, filePrefix + "_EDW.bak"), logger);
					PrepareBackup(DatbaseName + "_SD001", Path.Combine(temp, filePrefix + "_StorageDocs.bak"), logger);

					using (ConfigureDatabaseBackupsPath())
					using (logger.RecordTask("Copying files to " + DbRestorer.OverridableDatabaseBackupsPath.Value))
					{
						foreach (var file in Directory.GetFiles(temp))
						{
							var targetFile = Path.Combine(DbRestorer.OverridableDatabaseBackupsPath.Value, Path.GetFileName(file));
							var oldFile = targetFile + ".old";
							if (File.Exists(oldFile))
							{
								File.Delete(oldFile);
							}
							if (File.Exists(targetFile))
							{
								File.Move(targetFile, oldFile);
							}
							File.Copy(file, targetFile);
						}
					}

					if (!IsUnitTest())
					{
						using (logger.RecordTask("Submitting update to CurrentDatDbBackupPrefix.txt"))
						{
							var currentDatDbBackupPrefixFile = CurrentDATDbBackupPrefixFileLocation;
							File.WriteAllText(currentDatDbBackupPrefixFile, filePrefix);
							using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(Configuration.SourcePath))
							{
								((GitSourceControl)sourceControl).UseIdentity("DAT Service", "dat@wisetechglobal.com");
								sourceControl.SubmitChanges("dat", "Update Db Backup / " + DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, new[] { currentDatDbBackupPrefixFile });
							}
						}
					}
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection(System.Environment.MachineName, "master"))
				{
					foreach (var db in BuildDeployer.GetDatabasesToDrop(connection, DatbaseName, null, string.Empty))
					{
						BuildDeployer.DropDatabase(connection, "DATREF-" + db, logger);
						BuildDeployer.DropDatabase(connection, db, logger);
					}
					BuildDeployer.DropLogins(connection, DatbaseName);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Starting and stopping mssqlserver not to open a file or url")]
		static void CheckSqlServerName(ITaskLogger logger)
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					new SqlServerNameChecker().CheckServerName(connection);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					using (logger.RecordTask("Restarting mssqlserver"))
					{
						var process = Process.Start("net", "stop mssqlserver");
						process.WaitForExit();
						if (process.ExitCode != 0)
						{
							logger.RecordTask("could not stop mssqlserver (" + process.ExitCode + ")");
						}
						Process.Start("net", "start mssqlserver").WaitForExit();
					}
				}
			}
		}

		static void PrepareBackup(string databaseName, string backupFile, ITaskLogger logger)
		{
			using (var connection = Db.NewAdminConnection(System.Environment.MachineName, databaseName))
			{
				connection.DefaultCommandTimeOutInSeconds = 300;

				using (logger.RecordTask(databaseName + " rebuild all tables"))
				{
					RebuildAllTables(connection);
				}

				using (logger.RecordTask(databaseName + " shrink all files"))
				{
					ShrinkFiles(connection);
				}

				using (logger.RecordTask(databaseName + " bakup"))
				{
					new DbRestorer(((IDbConnectionInternals)connection).ADOConnection, logger).BackupDatabase(databaseName, backupFile);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void RebuildAllTables(DbConnection connection)
		{
			var tables = new List<string>();
			using (var reader = connection.Command("select name from sys.tables order by name").ExecuteReader())
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}

			foreach (var table in tables)
			{
				var cmd = "alter table " + table + " rebuild";
				try
				{
					connection.ExecuteNonQuery(cmd);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void ShrinkFiles(DbConnection connection)
		{
			var files = new List<string>();
			using (var reader = connection.Command("select name from sys.master_files where database_id = DB_ID(N'" + connection.CurrentDatabase + "')").ExecuteReader())
			{
				while (reader.Read())
				{
					files.Add((string)reader[0]);
				}
			}

			foreach (var file in files)
			{
				var cmd = "DBCC SHRINKFILE (N'" + file + "', 1)";
				try
				{
					connection.ExecuteNonQuery(cmd);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		static void DisableChangeTrackingOnEnabledTables(DbConnection connection)
		{
			var sql = Invariant($@"
BEGIN
	USE [{connection.CurrentDatabase}]
;

	DECLARE @SQL NVARCHAR(MAX) = '';
	SELECT @SQL = @SQL + 'ALTER TABLE ' + s.name + '.' + t.name + ' Disable Change_tracking;' + CHAR(10)
	FROM sys.change_tracking_tables ct
	JOIN sys.tables t ON ct.object_id = t.object_id
	JOIN sys.schemas s ON t.schema_id = s.schema_id
;

	EXEC sp_executesql @SQL
;
END
");
			connection.ExecuteNonQuery(sql);
		}

		const string DatbaseName = "DatBackupFileGenerator";

		#region Helpers

		IDisposable ConfigureEnvironmentForUpgrade()
		{
			WTG.TestHelpers.TestingState.IsRunningOnDAT = true;

			return new DisposableAction(() =>
			{
				WTG.TestHelpers.TestingState.IsRunningOnDAT = false;
			});
		}

		IDisposable ConfigureDatabaseBackupsPath()
		{
			if (IsUnitTest())
			{
				var backupDir = Configuration.Parameter.Substring(9);
				if (!Directory.Exists(backupDir))
				{
					Directory.CreateDirectory(backupDir);
				}

				DbRestorer.OverridableDatabaseBackupsPath.Value = backupDir;
			}

			return new DisposableAction(() =>
			{
				DbRestorer.OverridableDatabaseBackupsPath.ResetValue();
			});
		}

		bool IsUnitTest()
		{
			return Configuration.Parameter != null && Configuration.Parameter.StartsWith("UnitTest:", StringComparison.Ordinal);
		}
		#endregion
	}
}
#endif
