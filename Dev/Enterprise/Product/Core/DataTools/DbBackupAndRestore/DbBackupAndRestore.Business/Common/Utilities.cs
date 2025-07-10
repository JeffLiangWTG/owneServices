using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public delegate void StartTaskDelegate();
	public delegate void InformationEvent(string textMessage);
	public delegate void ProgressEvent(string textMessage, int numValue);
	public delegate void ReleaseKeyDelegate(SessionInfo sessionInfo);

	public enum FileValidity
	{
		ValidFBK,
		ValidDBK,
		ValidStandard,
		NotValid,
		ValidDepFBK,
		ValidDepDBK,
		ValidDepStandard
	}

	// Copied from Microsoft.SqlServer.Management.Smo.DatabaseStatus
	public enum DatabaseStatus
	{
		Online,
		Offline,
		Restoring,
		RecoveryPending,
		Recovering,
		Suspect,
		Inaccessible,
		Standby,
		Shutdown,
		EmergencyMode,
		AutoClosed,
		DoesNotExist
	}

	public static class Utilities
	{
		public static bool DatabaseExists(DbConnection connection, string databaseName)
		{
			using (var command = connection.Command(@"IF EXISTS (SELECT null FROM sys.databases WHERE name = @dbname) BEGIN SELECT 1 RETURN END SELECT 0;"))
			{
				command.AddParameter("@dbname", SqlDbType.NVarChar, 128, databaseName);
				var queryResult = command.ExecuteScalar();
				return Convert.ToInt32(queryResult) == 1;
			}
		}

		public static bool IsProductionDb(DbConnection conn, string dbName)
		{
			var sqlText = string.Format(
				@"DECLARE @cmd nvarchar(1000) = 'SELECT 0';

				IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
				BEGIN
						IF EXISTS (SELECT null FROM [{0}].sys.tables t INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id WHERE t.name = 'GlbStaff' AND s.name = 'dbo')
						BEGIN
							SET @cmd = 'IF EXISTS (SELECT null FROM [{0}]..GlbStaff WHERE convert(char(1), GS_IsSystemAccount) in (''0'', ''N'')';

							IF EXISTS (SELECT null FROM [{0}].sys.columns WHERE name = 'GS_IsOperational')
							BEGIN
									SET @cmd = @cmd  + ' AND GS_IsOperational = 1';
							END

							SET @cmd = @cmd  + ') SELECT 1 ELSE SELECT 0';
						END
				END

				EXEC (@cmd);",
				dbName);

			var doesDbHaveNonSystemUsers = Convert.ToBoolean(conn.ExecuteScalar(sqlText));
			bool isProductionDb;
			try
			{
				isProductionDb = (GetDbType(conn, dbName) == DatabaseTypes.Codes.Production);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidColumnName)
			{
				isProductionDb = false;
			}
			return doesDbHaveNonSystemUsers && isProductionDb;
		}

		public static bool IsTestOrTrainingDb(DbConnection conn, string dbName)
		{
			var dbType = GetDbType(conn, dbName);
			return dbType == DatabaseTypes.Codes.Test || dbType == DatabaseTypes.Codes.Training;
		}

		internal static string GetDbType(DbConnection conn, string dbName)
		{
			var regoKey = LicenceBuilder.GetProductRegistrationKey(conn, dbName);
			if (regoKey != null)
			{
				return regoKey.DbType;
			}
			var systemKey = LicenceBuilder.GetSystemRegistrationKey(conn, dbName);
			return (systemKey == null) ? "" : systemKey.DatabaseType;
		}

		public static DatabaseStatus GetDatabaseStatus(DbConnection connection, string databaseName)
		{
			if (!connection.DatabaseExists(databaseName))
			{
				return DatabaseStatus.DoesNotExist;
			}

			var getStatus = string.Format(CultureInfo.InvariantCulture, @"SELECT DATABASEPROPERTYEX ('{0}', 'Status')", databaseName);
			var statusResult = connection.ExecuteScalar(getStatus).ToString();
			DatabaseStatus dbStatus;
			if (!Enum.TryParse(statusResult, true, out dbStatus))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Couldn't process status of database: \"{0}\"", statusResult));
			}

			return dbStatus;
		}

		public static FileValidity DetermineFileValidity(string backupFilePath)
		{
			FileInfo backupFile;

			try
			{
				backupFile = new FileInfo(backupFilePath);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return FileValidity.NotValid;
			}

			var standardFileNameMatch = StandardBackupFileRegex.Match(backupFile.Name);
			if (standardFileNameMatch.Success)
			{
				if (DocManagerOrRefFileDbRegex.IsMatch(standardFileNameMatch.Groups[4].Value))
				{
					return FileValidity.ValidDepStandard;
				}
				return FileValidity.ValidStandard;
			}

			if (StandardFBKBackupFileRegex.Match(backupFile.Name).Success)
			{
				if (DocManagerOrRefFileDbRegexForFBK.IsMatch(backupFile.Name))
				{
					return FileValidity.ValidDepFBK;
				}
				return FileValidity.ValidFBK;
			}

			if (StandardDBKBackupFileRegex.Match(backupFile.Name).Success)
			{
				if (DocManagerOrRefFileDbRegexForDBK.IsMatch(backupFile.Name))
				{
					return FileValidity.ValidDepDBK;
				}
				return FileValidity.ValidDBK;
			}

			return FileValidity.NotValid;
		}

		public static string GetLocalMachineNameForLocalhost(string serverName)
		{
			return Db.GetMachineNameIfLocal(serverName);
		}

		public static void UpdateRegistry(DbConnection connection, string mainDbName, string registryName, string registryValue, bool preserveTestValue)
		{
			var sqlText = $@"
IF EXISTS (SELECT 1 FROM [{mainDbName}].[dbo].[StmData] WHERE SD_Name = @Name)
	UPDATE [{mainDbName}].[dbo].[StmData] SET SD_BinaryValue = @BinaryValue, SD_PreserveTestValue = @PreserveTestValue WHERE SD_Name = @Name
ELSE
	INSERT INTO [{mainDbName}].[dbo].[StmData](SD_PK, SD_Name, SD_BinaryValue, SD_Type, SD_PreserveTestValue, SD_IsLogged, SD_IsCancelled) VALUES(NEWID(), @Name, @BinaryValue, 'BIN', @PreserveTestValue, 0, 0)
";
			try
			{
				using (var cmd = connection.Command(sqlText))
				{
					var bytes = System.Text.Encoding.Unicode.GetBytes(registryValue);
					cmd.AddParameter("@BinaryValue", SqlDbType.Binary, bytes);
					cmd.AddParameter("@Name", SqlDbType.VarChar, registryName);
					cmd.AddParameter("@PreserveTestValue", SqlDbType.Bit, preserveTestValue);
					cmd.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				var type = new DbErrorMatch(ex).ExceptionType;
				if (type != DbErrorType.InvalidColumnName && type != DbErrorType.InvalidObjectName)
				{
					throw;
				}
			}
		}

		public static string GetAuditServer(AdminConnection mainServerConnection, string mainDbName)
		{
			return GetRegistryValue(mainServerConnection, mainDbName, "BiAuditServer");
		}

		public static string GetDataWarehouseServer(AdminConnection mainServerConnection, string mainDbName)
		{
			return GetRegistryValue(mainServerConnection, mainDbName, "BiDataWarehouseServer");
		}

		public static string GetRegistryValue(AdminConnection mainServerConnection, string mainDbName, string registryName)
		{
			string registryValue = null;
			try
			{
				using (((ICurrentDbControl)mainServerConnection).UseDatabase(mainDbName))
				{
					var sqlText = $"SELECT CONVERT(NVARCHAR(MAX), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{registryName}'";
					var value = mainServerConnection.ExecuteScalar(sqlText);
					if (value != DBNull.Value && !string.IsNullOrWhiteSpace(value?.ToString()))
					{
						registryValue = value.ToString();
					}
				}
			}
			catch (SqlException ex)
			{
				var errorType = new DbErrorMatch(ex).ExceptionType;

				switch (errorType)
				{
					case DbErrorType.InvalidColumnName:
						break;
					case DbErrorType.InvalidObjectName:
						break;
					case DbErrorType.SynonymRefersToAnInvalidObject:
						break;
					case DbErrorType.DatabaseOffline:
						throw new DbBackupAndRestoreException($"Could not get {registryName} registry item from offline database. Untick 'Include Business Intelligence Databases' and try again.");
					case DbErrorType.DatabaseIsInTheMiddleOfRestore:
						// we'll ignore this because "RESTORING" is a valid state that we should handle
						break;
					case DbErrorType.ExplicitConversionNotAllowed:
						// we'll ignore this because binaryvalue can be an invalid data type
						break;
					default:
						throw;
				}
			}

			return registryValue;
		}

		public static void DeleteRegistry(DbConnection connection, string mainDbName, string registryName)
		{
			var sqlText = $@"DELETE FROM [{mainDbName}].[dbo].[StmData] WHERE SD_Name = @Name";
			try
			{
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@Name", SqlDbType.VarChar, registryName);
					cmd.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				var type = new DbErrorMatch(ex).ExceptionType;
				if (type != DbErrorType.InvalidColumnName && type != DbErrorType.InvalidObjectName)
				{
					throw;
				}
			}
		}

		public static DateTime GetDbServerDateTime(DbConnection connection)
		{
			var sqlText = "SELECT GETDATE() AS CurrentDateTime";

			return Convert.ToDateTime(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// EdiBkp_T-yyyyMMdd-HHmmss_S-[server]-[instance]_D-[DbName].bak
		/// </summary>
		public static readonly Regex StandardBackupFileRegex = new Regex(
			"^(" + BackupFilePrefix + @"|" + OldBackupFilePrefix + @")_T-([0-9]{8}-[0-9]{6})_S-(\w+(?:-\w+)*)_D-(\w+|(\w+(?:(-\w+)+)))" + FullBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public static readonly Regex StandardFBKBackupFileRegex = new Regex(
			@"^\w+" + FullBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public static readonly Regex StandardDBKBackupFileRegex = new Regex(
			@"^\w+" + DifferentialBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static readonly Regex DocManagerOrRefFileDbRegex = new Regex(
			@"^(\w+)_(SD[0-9]{3}|RefDb_\w{3}_\w{2})$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public static readonly Regex DocManagerOrRefFileDbRegexForFBK = new Regex(
			@"^(\w+)_(SD[0-9]{3}|RefDb_\w{3}_\w{2})" + FullBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public static readonly Regex DocManagerOrRefFileDbRegexForDBK = new Regex(
			@"^(\w+)_(SD[0-9]{3}|RefDb_\w{3}_\w{2})" + DifferentialBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static readonly Regex TransactionLogBackupFileRegex = new Regex(
			@"^(\w+)_([0-9]{14})" + TransactionLogBackupFileExtension.Replace(".", @"\.") + "$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public const string BackupFilePrefix = "CW1Bkp";
		public const string OldBackupFilePrefix = "EdiBkp";
		public const string FullBackupFileExtension = ".bak";
		public const string DifferentialBackupFileExtension = Db.BackupDiffFileExtension;
		public const string DifferentialBackupFileSuffix = Db.BackupDiffFileSuffix;
		public const string TransactionLogBackupFileExtension = ".trn";
		public const int BackupTimeoutInSeconds = 0;
	}
}
