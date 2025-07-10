using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.DataProtection.TestFramework;
using CargoWise.Shared;
using Dat.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework.Dat;

namespace Enterprise.Dat.Implementation
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public class DbRestorer
	{
		public string MainDbName { get; }
		public string ServerName { get; }
		string StorageDocsDbNamePrefix { get { return MainDbName + "_SD"; } }
		public static string DatDatabaseBackups => Path.Combine(DatServerConnection.DatFileSharePath, @"DatabaseBackups");
		public readonly static Overridable<string> OverridableDatabaseBackupsPath = new Overridable<string>(DatDatabaseBackups);

		static string LocalDatabaseCachePath
		{
			get { return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "CargoWise edi", "DAT", "DBBackupCache"); }
		}

		readonly ITaskLogger logger;
		readonly System.Data.Common.DbConnection sqlConnection;
		readonly DataProtectionTestBed dataProtectionTestBed;

		public DbRestorer(System.Data.Common.DbConnection sqlConnection, ITaskLogger logger, string mainDbName = DatConfiguration.OdysseyTestDatabase)
			: this(sqlConnection, logger, mainDbName, DataProtectionTestBed.Current)
		{
		}

		public DbRestorer(System.Data.Common.DbConnection sqlConnection, ITaskLogger logger, string mainDbName, DataProtectionTestBed dataProtectionTestBed)
		{
			this.logger = logger;

			this.sqlConnection = sqlConnection;

			ServerName = sqlConnection.DataSource;
			MainDbName = mainDbName;
			this.dataProtectionTestBed = dataProtectionTestBed;
		}

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		public void RestoreCurrentDbs()
		{
			if (MatchesCurrentCycle())
			{
				logger.RecordInfo("Current database matches executing cycle, skipping restore");
				return;
			}

			using (Db.DisableSchemaVersionCheck())
			{
				int attempts = 0;
				List<Exception> exceptions = null;
				while (attempts < 10)
				{
					try
					{
						DropLogins();
						dataProtectionTestBed.CleanUpAllProtectedDataRepositories();
						dataProtectionTestBed.SetupAndActivateTestEnvironmentEnterpriseSecrets(ServerName);
						EnsureTSQLEndpointPermission();

						RestoreDbCore(CurrentDatDbBackupPrefix.GetValue());
						exceptions = null;
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						attempts++;
						logger.RecordInfo("DB Restore problem. Attempt #" + attempts + " failure: " + ex.Message);
						if (exceptions == null)
						{
							exceptions = new List<Exception>();
						}
						if (!exceptions.Any(e => e.Message == ex.Message))
						{
							exceptions.Add(ex);
						}
						if (DbFileDeleter.HandleDbFileExistsException(logger, ex.Message))
						{
							attempts--;
						}
					}
				}

				if (exceptions != null && exceptions.Count > 0)
				{
					var firstException = exceptions[0];

					var innerException = exceptions.Count > 1 ? new AggregateException(exceptions) : firstException;

					throw new IOException($"Could not restore DB after 10 attempts. 1/{exceptions.Count} inner exception = [{firstException.GetType().FullName}] {firstException.Message}", innerException);
				}
				else
				{
					SaveCycleInfo();
				}
			}

			CleaupOldCachedFiles();
		}

		void EnsureTSQLEndpointPermission()
		{
			using var cmd = sqlConnection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $@"GRANT CONNECT ON ENDPOINT::[TSQL Default TCP] to [public]";
			cmd.ExecuteNonQuery();
		}

		void DropLogins()
		{
			var logins = new List<string>();
			using var cmd = sqlConnection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $@"SELECT name FROM sys.server_principals WHERE name LIKE @loginName";
			var param = cmd.CreateParameter();
			param.ParameterName = "@loginName";
			param.DbType = DbType.String;
			param.Value = $"{MainDbName}[_]%";
			cmd.Parameters.Add(param);

			var lst = new List<string>();
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var login = (string)reader["name"];
					lst.Add(login);
				}
			}
			foreach (var login in lst)
			{
				var cmd2 = sqlConnection.CreateCommand();
				cmd2.CommandText = $"DROP LOGIN [{login}]";
				cmd2.ExecuteNonQuery();
			}
		}

		public bool IsRestoringReferenceFiles
		{
			get { return restoringReferenceFiles; }
		}

		bool restoringReferenceFiles;

		public void BackupDatabase(string db, string localPath)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandTimeout = sqlConnection.ConnectionTimeout;
				cmd.CommandText = string.Format("BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT, COMPRESSION", db, localPath);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format("RESTORE VERIFYONLY FROM DISK = '{0}'", localPath);
				cmd.ExecuteNonQuery();
			}
		}

		void RestoreDbCore(string filePrefix)
		{
			if (sqlConnection.State == ConnectionState.Closed)
			{
				sqlConnection.Open();
			}

			DropExtraDbs();

			RestoreDbCore(GetDatabaseName(MainDbName), filePrefix + ".bak", useRamDrive: true);

			ExecuteConfigureServerFromMainDatabase(sqlConnection, MainDbName);

			dataProtectionTestBed.SetupAndActivateTestEnvironmentSystemSecrets(ServerName, MainDbName);

			RestoreDbCore(GetDatabaseName("Audit", true), filePrefix + "_Audit.bak", useRamDrive: true);
			RestoreDbCore(GetDatabaseName("EDW", true), filePrefix + "_EDW.bak", useRamDrive: true);

			RestoreAllReferenceFilesDbs();

			RestoreDbCore(GetDatabaseName(StorageDocsDbNamePrefix + "001"), filePrefix + "_StorageDocs.bak");

			SetDatabaseCompatibilityLevel("model");
		}

		IEnumerable<string> ReferenceFileDbs
		{
			get
			{
				if (referenceFileDbs == null)
				{
					referenceFileDbs = new string[]
					{
						"RefDb_Cmr_AU",
						"RefDb_Ent_AU",
						"RefDb_Ent_CA",
						"RefDb_Ent_GB",
						"RefDb_Ent_NZ",
						"RefDb_Ent_SG",
						"RefDb_Ent_US",
						"RefDb_Ent_ZA",
						"RefDb_Ent_ZZ",
						"RefDb_Trf_AU",
						"RefDb_Trf_CA",
						"RefDb_Trf_GB",
						"RefDb_Trf_NZ",
					};
				}
				return referenceFileDbs;
			}
		}

		IEnumerable<string> referenceFileDbs;

		void RestoreAllReferenceFilesDbs()
		{
			try
			{
				restoringReferenceFiles = true;

				foreach (var referenceDbName in ReferenceFileDbs)
				{
					RestoreReferenceFilesDb(referenceDbName);
				}
			}
			finally
			{
				restoringReferenceFiles = false;
			}
		}

		void RestoreReferenceFilesDb(string databaseName)
		{
			var backupFileName = databaseName;

			if (!File.Exists(GetDatabaseBackupPath(backupFileName)))
			{
				logger.RecordInfo("Skipping " + databaseName + " no backups found");
				return;
			}

			databaseName = GetDatabaseName(databaseName);
			RestoreFromSnapshotOrBackup(databaseName, backupFileName);
		}

		void RestoreFromSnapshotOrBackup(string databaseName, string backupFileName, bool useRamDrive = false)
		{
			var snapshotDbName = "DATREF-" + databaseName;
			bool restoredFromSnapshot = false;
			if (DatabaseExists(snapshotDbName))
			{
				if (DatabaseCreateDate(snapshotDbName) < File.GetLastWriteTime(GetDatabaseBackupPath(backupFileName)))
				{
					logger.RecordInfo("Dropping " + snapshotDbName + " new backup file detected");
					DropDatabase(snapshotDbName);
				}
				else
				{
					try
					{
						using (var cmd = sqlConnection.CreateCommand())
						{
							cmd.CommandTimeout = sqlConnection.ConnectionTimeout;
							cmd.CommandText = $@"
ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [{databaseName}] FROM database_snapshot = '{snapshotDbName}';
ALTER DATABASE [{databaseName}] SET MULTI_USER;";
							cmd.ExecuteNonQuery();
							restoredFromSnapshot = true;
						}
					}
					catch (SqlException)
					{
						logger.RecordInfo("Failed to restore " + databaseName + " from snapshot");
						DropDatabase(snapshotDbName);
					}
				}
			}
			if (!restoredFromSnapshot)
			{
				RestoreDbCore(databaseName, backupFileName);

				using (var cmd = sqlConnection.CreateCommand())
				{
					string file = Path.Combine(GetDatabaseFolder(databaseName, useRamDrive), databaseName + "_DATAREF.ss");
					if (File.Exists(file))
					{
						File.Delete(file);
					}
					cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
					DECLARE @declareFileSql varchar(max);

					SELECT
						@declareFileSql = isnull(@declareFileSql + ',', '') + '(name=' + name + ', filename = ''' + physical_name + '_DATAREF.ss'')'
					FROM [{0}].sys.database_files
					WHERE [type] = 0;

					DECLARE @dynamicsql varchar(max) =
						'CREATE DATABASE [{1}] ON ' + @declareFileSql + ' AS SNAPSHOT OF [{0}]';

					EXEC (@dynamicsql);", databaseName, snapshotDbName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		string GetDatabaseName(string databaseName, bool forceRefD = false)
		{
			var localDatabaseName = databaseName;
			if (databaseName.StartsWith("RefDb_", StringComparison.OrdinalIgnoreCase) || forceRefD)
			{
				localDatabaseName = MainDbName + "_" + localDatabaseName;
			}
			return localDatabaseName;
		}

		void RestoreDbCore(string databaseName, string backupFileName, bool useRamDrive = false)
		{
			var databaseFolder = GetDatabaseFolder(databaseName, useRamDrive);

			FormatRAMDriveIfRequired(databaseFolder[0]);

			using (logger.RecordTask("Restoring database " + databaseName))
			{
				DropDatabase(databaseName);

				var backupFilePath = CacheDbLocally(backupFileName);

				var info = new DbBackupInfo();

				info.LoadBackupInfo(sqlConnection, backupFilePath);

				var restoreCmdBuilder = new StringBuilder();
				restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH REPLACE", databaseName, backupFilePath);

				foreach (var dbFile in info.DatabaseFiles)
				{
					var phyicalPath = Path.Combine(databaseFolder, databaseName + dbFile.PhysicalFileSuffix);
					FileIO.DeleteFile(phyicalPath);
					restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, ", MOVE '{0}' TO '{1}'", dbFile.LogicalName, phyicalPath);
				}

				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandTimeout = sqlConnection.ConnectionTimeout;
					cmd.CommandText = restoreCmdBuilder.ToString();
					cmd.ExecuteNonQuery();
				}

				SetDatabaseAuthorizationAndTrustworthy(databaseName);

				SetDatabaseCompatibilityLevel(databaseName);

				ClearRegisteredLoginServerName(databaseName);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Baseline")]
		string GetDatabaseFolder(string database, bool useRamDrive)
		{
			var sqlText = "SELECT TOP 1 physical_name FROM sys.master_files WHERE database_id = DB_ID(@database);";
			string databaseFilePath;

			try
			{
				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandText = sqlText;
					cmd.AddParameterWithValue("@database", database);
					databaseFilePath = (string)cmd.ExecuteScalar();
				}
			}
			catch (SqlException)
			{
				databaseFilePath = null;
			}

			if (string.IsNullOrEmpty(databaseFilePath))
			{
				if (useRamDrive && InstalledSoftwareDetection.IsVm && MainDbName.Equals(DatConfiguration.OdysseyTestDatabase, StringComparison.OrdinalIgnoreCase))
				{
					return @"D:\";
				}

				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandText = sqlText;
					cmd.AddParameterWithValue("@database", "master");
					databaseFilePath = cmd.ExecuteScalar().ToString();
				}
			}

			return Path.GetDirectoryName(databaseFilePath);
		}

		internal void DropDatabase(string databaseName)
		{
			if (DatabaseExists(databaseName))
			{
				DropSnapshots(databaseName);

				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandTimeout = sqlConnection.ConnectionTimeout;
					cmd.CommandText = $@"
						IF (EXISTS (SELECT null FROM sys.databases WHERE state = 0 AND source_database_id IS null AND name = '{databaseName}'))
						BEGIN 
							ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
						END

						DROP DATABASE [{databaseName}]";

					cmd.ExecuteNonQuery();
				}
			}
		}

		void DropSnapshots(string databaseName)
		{
			var snapshots = new List<string>();
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandTimeout = sqlConnection.ConnectionTimeout;
				cmd.CommandText = "select name from sys.databases where source_database_id = (select database_id from sys.databases where name = @databaseName)";
				cmd.AddParameterWithValue("databaseName", databaseName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						snapshots.Add(reader.GetValue<string>("name"));
					}
				}
			}

			foreach (var snapshot in snapshots)
			{
				DropDatabase(snapshot);
			}
		}

		bool DatabaseExists(string databaseName)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = string.Format("SELECT COUNT(*) FROM sys.databases WHERE name = '{0}'", databaseName);
				return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
			}
		}

		DateTime DatabaseCreateDate(string databaseName)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = "SELECT create_date FROM sys.databases WHERE name = @databaseName";
				cmd.AddParameterWithValue("databaseName", databaseName);
				return (DateTime)cmd.ExecuteScalar();
			}
		}

		#region Backup File Caching

		string CacheDbLocally(string backupFileName)
		{
			string backupPath = GetDatabaseBackupPath(backupFileName);
			string localPath = GetLocallyCachedDbBackupPath(backupFileName);

			try
			{
				using (logger.RecordTask("Considering Caching " + backupFileName))
				{
					NetworkFileCacher.CacheFileLocally(backupPath, localPath);
				}
			}
			catch (IOException)
			{
				bool fileServerOnline = CheckIfFileServerIsUp();
				if (fileServerOnline)
				{
					throw;
				}
				else if (!File.Exists(localPath))
				{
					throw new IOException("Local file does not exist : " + localPath);
				}
			}

			File.SetLastAccessTimeUtc(localPath, DateTime.UtcNow);

			return localPath;
		}

		static bool CheckIfFileServerIsUp()
		{
			bool result = true;
			try
			{
				Directory.GetFiles(OverridableDatabaseBackupsPath.Value);
			}
			catch (IOException)
			{
				result = false;
			}
			return result;
		}

		static string GetDatabaseBackupPath(string backupFileName)
		{
			return Path.Combine(DatDatabaseBackups, backupFileName);
		}

		static string GetLocallyCachedDbBackupPath(string backupFileName)
		{
			return Path.Combine(LocalDatabaseCachePath, backupFileName);
		}

		#endregion

		#region Database Compatibility Level

		void SetDatabaseCompatibilityLevel(string databaseName)
		{
			var compatibilityLevel = sqlConnection.ServerVersion.Split('.')[0] + "0";
			using (logger.RecordTask(string.Format("Setting Compatibility Level of Database {0} to {1}", databaseName, compatibilityLevel)))
			{
				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandText = string.Format("ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = {1}", databaseName, compatibilityLevel);
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Drop Extra Databases

		void DropExtraDbs()
		{
			for (int i = AllDatabases.Count - 1; i >= 0; i--)
			{
				if (IsExtraDbForDelete(AllDatabases[i]))
				{
					DropDatabase(AllDatabases[i]);
				}
			}
		}

		bool IsExtraDbForDelete(string dbName)
		{
			var result = false;

			if (dbName.StartsWith(MainDbName, StringComparison.OrdinalIgnoreCase) && dbName.EndsWith("-#NEW#", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.StartsWith("#NEW#-", StringComparison.OrdinalIgnoreCase) && dbName.IndexOf(MainDbName, StringComparison.OrdinalIgnoreCase) > -1)
			{
				result = true;
			}
			else if (dbName.StartsWith(StorageDocsDbNamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				var numberPart = dbName.Substring(StorageDocsDbNamePrefix.Length);

				if ((numberPart.Length == 3) && numberPart.All(c => char.IsDigit(c)))
				{
					result = true;
				}
			}
			else if (dbName.Equals(MainDbName + "_RB", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.Equals(MainDbName + "_UserRepository", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.StartsWith("DBUPG_") && dbName.IndexOf("_" + MainDbName, StringComparison.OrdinalIgnoreCase) > 0)
			{
				result = true;
			}
			else if (dbName.Equals(MainDbName + "_Audit_New", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.Equals(MainDbName + "_EDW_New", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.StartsWith(MainDbName + "_AutoTester_UserTests_ForTest", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.StartsWith(MainDbName, StringComparison.OrdinalIgnoreCase) && !dbName.Equals(MainDbName, StringComparison.OrdinalIgnoreCase) && IsRecoveryPending(dbName))
			{
				result = true;
			}
			else if (dbName.StartsWith(MainDbName, StringComparison.OrdinalIgnoreCase) && dbName.EndsWith("-TestMock", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			else if (dbName.StartsWith("CW-RefDb", StringComparison.OrdinalIgnoreCase) && InstalledSoftwareDetection.IsVm)
			{
				result = true;
			}
			else if (dbName.StartsWith("CW-AG-RefDb", StringComparison.OrdinalIgnoreCase) && InstalledSoftwareDetection.IsVm)
			{
				result = true;
			}
			// Unit test should not delete CW-RefDatabase.
			else if (!Globals.IsTest && dbName.Equals("CW-RefDatabase", StringComparison.OrdinalIgnoreCase) && InstalledSoftwareDetection.IsVm)
			{
				result = true;
			}
			else if (dbName.StartsWith("NewDb_", StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}
			return result;
		}

		bool IsRecoveryPending(string dbName)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = "select state from sys.databases where name = @name";
				cmd.AddParameterWithValue("@name", dbName);
				return (byte)cmd.ExecuteScalar() == 3;
			}
		}

		#endregion

		void ClearRegisteredLoginServerName(string databaseName)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = string.Format(@"
					IF EXISTS(SELECT NULL FROM [{0}].sys.extended_properties WHERE class = 0 AND name = 'RegisteredLoginServerName')
						EXEC [{0}].sys.sp_dropextendedproperty 'RegisteredLoginServerName';", databaseName);
				cmd.ExecuteNonQuery();
			}
		}

		void SetDatabaseAuthorizationAndTrustworthy(string dbName)
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = FormattableString.Invariant($@"
					ALTER AUTHORIZATION ON DATABASE::[{dbName}] TO [OdysseyAdmin]
					ALTER DATABASE [{dbName}] SET TRUSTWORTHY ON");
				cmd.ExecuteNonQuery();
			}
		}

		List<string> AllDatabases
		{
			get
			{
				if (allDatabases == null)
				{
					var tempList = new List<string>();
					var sqlText = "SELECT name FROM sys.databases";

					using (var cmd = sqlConnection.CreateCommand())
					{
						cmd.CommandText = sqlText;
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								tempList.Add(reader.GetString(0).ToUpperInvariant());
							}
						}
					}

					allDatabases = tempList;
				}

				return allDatabases;
			}
		}

		List<string> allDatabases;

		void FormatRAMDriveIfRequired(char driveLetter)
		{
			if ((driveLetter == 'D' || driveLetter == 'd') && InstalledSoftwareDetection.IsVm && !formatChecked)
			{
				var query = new SelectQuery("select * from Win32_Volume where DriveLetter = '" + driveLetter + ":'");
				using (var searcher = new ManagementObjectSearcher(query))
				{
					using (var volume = searcher.Get().Cast<ManagementObject>().Single())
					{
						if (!((string)volume["FileSystem"]).Equals("NTFS", StringComparison.OrdinalIgnoreCase))
						{
							using (logger.RecordTask("Formatting " + driveLetter))
							{
								var inParams = volume.GetMethodParameters("Format");
								inParams["FileSystem"] = "NTFS";
								inParams["QuickFormat"] = true;
								volume.InvokeMethod("Format", inParams, null);
							}
						}
					}
				}
				formatChecked = true;
			}
		}
		bool formatChecked;

		void SaveCycleInfo()
		{
			var cycleId = Guid.NewGuid().ToString("N");
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = $@"if exists (select null from [{MainDbName}].sys.extended_properties where name = 'DatDbCycleId')
										EXEC [{MainDbName}].sys.sp_dropextendedproperty @name = 'DatDbCycleId'
									EXEC [{MainDbName}].sys.sp_addextendedproperty @name = 'DatDbCycleId', @value = @cycleId";
				cmd.AddParameterWithValue("cycleId", cycleId);
				cmd.ExecuteNonQuery();
			}

			File.WriteAllText(CycleIdFilePath, GetCycleIdAndVersion());
		}

		bool MatchesCurrentCycle()
		{
			try
			{
				if (File.Exists(CycleIdFilePath))
				{
					var cycleId = File.ReadAllText(CycleIdFilePath);
					if (!string.IsNullOrEmpty(cycleId) && DatabaseExists(MainDbName) && !DatabaseExists("DATREF-" + MainDbName))
					{
						return cycleId == GetCycleIdAndVersion();
					}
				}
			}
			catch (Exception ex)
			{
				logger.RecordInfo("MatchesCurrentCycle failed: " + ex.ToString());
			}
			return false;
		}

		string GetCycleIdAndVersion()
		{
			using (var cmd = sqlConnection.CreateCommand())
			{
				cmd.CommandText = $@"Select CONCAT(CONVERT(char(32), value), '_', Trim(CONVERT(char(20), SERVERPROPERTY('ProductVersion')))) from [{MainDbName}].sys.extended_properties where name = 'DatDbCycleId'";
				return (string)cmd.ExecuteScalar();
			}
		}

		void CleaupOldCachedFiles()
		{
			try
			{
				NetworkFileCacher.CleanupOldCachedFiles(LocalDatabaseCachePath, ReleaseInfo.Instance.ReleaseRing);
			}
			catch (Exception ex)
			{
				logger.RecordInfo("Error cleaning up old cached files: " + ex.ToString());
			}
		}

		void ExecuteConfigureServerFromMainDatabase(System.Data.Common.DbConnection connection, string mainDatabaseName)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = $@"
IF OBJECT_ID(N'{mainDatabaseName}.dbo.ConfigureServer') IS NOT NULL
BEGIN
    EXEC {mainDatabaseName}.dbo.ConfigureServer
END;";
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery();
		}

		// CycleID is calculated when the first restore is done, it is saved to the DB and also to a file in the bin directory.
		// If the DAT client tries to runs the same cycle, it will see if the cycle id in the db matches the one in the bin directory, if it does then no need to restore.
		// Different db should have different path so as to avoid file being overridden
		// e.g. tests of DbRestorer could also write such file
		string CycleIdFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"DatDbCycleId-{MainDbName}");
	}
}
