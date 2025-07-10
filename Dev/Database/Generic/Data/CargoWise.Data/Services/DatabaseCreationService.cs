using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Data;
using CargoWise.Data.Utils;
using static System.FormattableString;
using static CargoWise.Data.DbConnection;

namespace CargoWise.Data.Services
{
	public interface IDatabaseCreationService
	{
		void CreateDatabase(AdminConnection connection, string databaseName, string dataPath = null, string logPath = null,
			int? dataInitialSizeMb = null, int? logInitialSizeMb = null, int? dataGrowthMb = null, int? logGrowthMb = null, bool mapDbLogins = true, Action<string> actionToPerformAfterCreatingDb = null, TimeSpan? sqlLockTimeout = null, string physicalFileSuffix = "");
	}

	public partial class DatabaseCreationService : IDatabaseCreationService
	{
		internal const string TempCreateDbNamePrefix = "#NEW#-"; // Suffix For Temporary Create DB Name

		public void CreateDatabase(AdminConnection connection, string databaseName, string dataPath = null, string logPath = null,
	int? dataInitialSizeMb = null, int? logInitialSizeMb = null, int? dataGrowthMb = null, int? logGrowthMb = null, bool mapDbLogins = true, Action<string> actionToPerformAfterCreatingDb = null, TimeSpan? sqlLockTimeout = null, string physicalFileSuffix = "")
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			Action<string> action = dbName =>
			{
				DbRecoveryModelManager.AdjustDatabase(connection, Db.InitialDatabaseName, databaseName, tempDbNameWhileCreating: dbName);

				var sql = Invariant($@"
ALTER DATABASE [{dbName}] SET AUTO_CLOSE OFF WITH ROLLBACK IMMEDIATE;
IF (DatabasePropertyEx('{dbName}','Collation') != '{Db.DatabaseCollation}') ALTER DATABASE [{dbName}] COLLATE {Db.DatabaseCollation};
");
				connection.ExecuteNonQuery(sql);
				actionToPerformAfterCreatingDb?.Invoke(dbName);
				new SnapshotIsolationManager().EnableSnapshotIsolationForDatabase(connection, dbName);
				if (!mapDbLogins)
				{ return; }

				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					((IDbLoginRepair)connection).EnsureDbLoginsHaveRightsToCurrentDatabase();
				}
			};

			try
			{
				CreateDatabaseUnsafe(connection, databaseName, dataPath, logPath, dataInitialSizeMb, logInitialSizeMb, dataGrowthMb, logGrowthMb, action, sqlLockTimeout, physicalFileSuffix);
			}
			catch (SqlException ex)
			{
				var errorMatch = new DbErrorMatch(ex);

				switch (errorMatch.ExceptionType)
				{
					case DbErrorType.CreateDbFailedSizeCannotAccommodateCopyOfModelDb:
						CreateDatabaseUnsafe(connection, databaseName, dataPath, logPath, null, null, dataGrowthMb, logGrowthMb, action, sqlLockTimeout, physicalFileSuffix);
						return;

					case DbErrorType.CannotCreateFileBecauseItAlreadyExists:
						var cmd = Invariant($"select db_ID('{databaseName}')");
						if (connection.ExecuteScalar(cmd) == DBNull.Value)
						{
							// true file collision only, db name still available
							throw;
						}
						return;

					case DbErrorType.DatabaseAlreadyExists:
						return;

					default:
						throw;
				}
			}
		}

		IDisposable AcquireSqlAppLockForDbCreation(AdminConnection connection, string databaseName, TimeSpan? sqlLockTimeout = null)
		{
			var failureMessage = Invariant($"Failed to acquire a lock on {Db.SqlMasterDb} db to create database {databaseName}"); // internal error message when failed to acquire a SqlAppLock

			try
			{
				if (!connection.TryGetLock(databaseName, sqlLockTimeout ?? TimeSpan.FromMinutes(5), out var sqlApplicationLock, Db.SqlMasterDb))
				{
					throw new CreateDatabaseException(failureMessage);
				}

				return new DisposableAction(() => { sqlApplicationLock?.Dispose(); });
			}
			catch (Exception exception)
			{
				throw new CreateDatabaseException(failureMessage, exception);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CreateDatabaseUnsafe(AdminConnection connection, string databaseName, string dataPath, string logPath,
			int? dataInitialSizeMb, int? logInitialSizeMb, int? dataGrowthMb, int? logGrowthMb, Action<string> actionToPerformAfterCreatingDb,
			TimeSpan? sqlLockTimeout = null, string physicalFileSuffix = "")
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName)); // Suggested By ReviewBot
			if (connection.DatabaseExists(databaseName))
			{
				return;
			}

			using (AcquireSqlAppLockForDbCreation(connection, databaseName, sqlLockTimeout))
			{
				if (connection.DatabaseExists(databaseName))
				{
					return;
				}

#if DEBUG
				// Minimize database size for tests
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					dataInitialSizeMb ??= 1;
					logInitialSizeMb ??= 1;
					dataGrowthMb ??= 1;
					logGrowthMb ??= 1;
				}
#endif

				if (logInitialSizeMb == null || logGrowthMb == null)
				{
					var isFullModel = DbRecoveryModelManager.GetDesired(connection, Db.DatabaseName, databaseName) == DbRecoveryModel.Full;
					logInitialSizeMb = logInitialSizeMb ?? (isFullModel ? DatabaseConstants.LogFileIntialSizeMb : null);
					logGrowthMb = logGrowthMb ?? (isFullModel ? DatabaseConstants.LogFileGrowthMb : null);
				}

				var tempDbName = TempCreateDbNamePrefix + databaseName;
				void DropDb()
				{
					var sqlTextDropDb = string.Format(CultureInfo.InvariantCulture, @"IF (DB_ID('{0}') IS NOT NULL) DROP DATABASE [{0}];", tempDbName); // May be a part of SQL expression.
					connection.ExecuteNonQuery(sqlTextDropDb);
				}
				try
				{
					DropDb();
				}
				catch (Exception ex) when (ex.GetInnermostException() is SqlException sqex && new DbErrorMatch(sqex).ExceptionType is DbErrorType error && (error == DbErrorType.CouldNotObtainExclusiveLock || error == DbErrorType.CannotDropDatabaseInUse))
				{
					DbConnectionKiller.KillOtherConnections(connection, tempDbName);
					DropDb();
				}

				var dataPathForSql = (string.IsNullOrEmpty(dataPath)) ? GetFilePathFromPreviousEDocsOrMainDatabase(connection, databaseName, DatabaseFileTypes.Data) : dataPath.Trim();
				var logPathForSql = (string.IsNullOrEmpty(logPath)) ? GetFilePathFromPreviousEDocsOrMainDatabase(connection, databaseName, DatabaseFileTypes.Log) : logPath.Trim();
				var dataInitialSizeMbForSql = (dataInitialSizeMb == null) ? "" : Invariant($", SIZE={dataInitialSizeMb.Value}MB"); // May be a part of SQL expression.
				var dataGrowthMbForSql = (dataGrowthMb == null) ? "" : Invariant($", FILEGROWTH={dataGrowthMb.Value}MB"); // May be a part of SQL expression.
				var logInitialSizeMbForSql = (logInitialSizeMb == null) ? "" : Invariant($", SIZE={logInitialSizeMb.Value}MB"); // May be a part of SQL expression.
				var logGrowthMbForSql = (logGrowthMb == null) ? "" : Invariant($", FILEGROWTH={logGrowthMb.Value}MB"); // May be a part of SQL expression.

				string sqlText = FormattableString.Invariant($@"
				DECLARE @DataPath varchar(max) = '{dataPathForSql}';
				DECLARE @LogPath varchar(max) = '{logPathForSql}';

				IF (right(@DataPath, 1) != '\') SET @DataPath = @DataPath + '\';
				IF (right(@LogPath, 1) != '\') SET @LogPath = @LogPath + '\';

				DECLARE @CreateDbCmd nvarchar(max) = '
					IF (DB_ID(''{tempDbName}'') IS NULL)
					BEGIN
						CREATE DATABASE [{tempDbName}]
							ON     (NAME = [{databaseName}_Data], FILENAME = ''' + @DataPath + '{databaseName}{physicalFileSuffix}_Data.mdf'' {dataInitialSizeMbForSql}{dataGrowthMbForSql})
							LOG ON (NAME = [{databaseName}_Log] , FILENAME = ''' + @LogPath  + '{databaseName}{physicalFileSuffix}_Log.ldf''  {logInitialSizeMbForSql}{logGrowthMbForSql})
							COLLATE {Db.DatabaseCollation};
					END';
				EXEC (@CreateDbCmd);"); // May be a part of SQL expression.

				var i = 0;
				var dbCreated = false;
				while (!dbCreated)
				{
					try
					{
						connection.ExecuteNonQuery(sqlText);
						dbCreated = true;
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CouldNotObtainExclusiveLock)
					{
						if (++i > 10)
						{
							throw;
						}

						Thread.Sleep(TimeSpan.FromSeconds(1));
					}
				}

				try
				{
					actionToPerformAfterCreatingDb?.Invoke(tempDbName);

					connection.ExecuteNonQueryWithRetry(Invariant($"ALTER DATABASE [{tempDbName}] MODIFY NAME = {databaseName.QuoteName()}"),
						stopRetryCondition: (Exception ex) =>
						{
							if (ex is SqlException sqlException && new DbErrorMatch(sqlException).ExceptionType == DbErrorType.LockTimeoutExpired)
							{
								string[] dbErrorMessages = ["Lock request time out period exceeded", "The database name", "has been set", "The statement has been terminated"];

								if (dbErrorMessages.All(errorMessage => ex.Message.Contains(errorMessage)) &&
									!connection.DatabaseExists(tempDbName) &&
									connection.DatabaseExists(databaseName))
								{
									return true;
								}
							}

							return false;
						});
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						connection.ExecuteNonQuery(Invariant($"DROP DATABASE [{tempDbName}]")); // Is a part of an SQL expression.
					}
					catch (Exception) { } //we'd rather the original exception than the new one be what's propagated
					throw;
				}
				catch (SqlLockLostException)
				{
					//this feels like a weird thing to do, but we can't do database creation/alteration in one transaction.
					//And it's either 'make a new connection' or 'figure out how to forget that we had a lock on this one' which seems intentionally hard to do since you SHOULDN'T do it.
					try
					{
						using (DbConnection dropDbConnection = Db.NewAdminConnection())
						{
							dropDbConnection.RunLocked(key: databaseName, process: (_) =>
							{
								dropDbConnection.ExecuteNonQuery(Invariant($"DROP DATABASE [{tempDbName}]")); // Is a part of an SQL expression.
							});
						}
					}
					catch (Exception) { } //we'd rather the original exception than the new one be what's propagated
					throw;
				}
			}
		}

		string GetFilePathFromPreviousEDocsOrMainDatabase(AdminConnection connection, string databaseName, DatabaseFileTypes fileType)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

#if DEBUG
			var mainDbName = mainDatabaseNameOverrideForTest ?? Db.DatabaseName;
#else
			var mainDbName = Db.DatabaseName;
#endif

			if (connection.DatabaseExists(mainDbName))
			{
				var databaseNameToGetFilePathsFrom = mainDbName;

				if (databaseName.Length > 6 && Regex.IsMatch(databaseName, Db.StorageDocDbSuffixSqlPattern + "$", RegexOptions.IgnoreCase))
				{
					int newEDocsDbNumber = 0;
					if (int.TryParse(databaseName.Substring(databaseName.Length - 3), out newEDocsDbNumber))
					{
						if (newEDocsDbNumber >= 1)
						{
							var oldValue = string.Format(CultureInfo.InvariantCulture, "{0:000}", newEDocsDbNumber);
							databaseNameToGetFilePathsFrom = databaseName.Replace(oldValue, string.Format(CultureInfo.InvariantCulture, "{0:000}", newEDocsDbNumber - 1));

							if (!connection.DatabaseExists(databaseNameToGetFilePathsFrom))
							{
								databaseNameToGetFilePathsFrom = mainDbName;
							}
						}
					}
				}

				string path = GetDbFilePathFromMasterFiles(connection, databaseNameToGetFilePathsFrom, fileType);
#if DEBUG
				path = GetFilePathFromMasterIfRamDrive(connection, databaseName, path, fileType);
#endif
				return path;
			}
			else if (!Db.IsSystemDatabase(connection.CurrentDatabase))
			{
				return GetDbFilePathFromMasterFiles(connection, connection.CurrentDatabase, fileType);
			}
			else
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"SELECT SERVERPROPERTY ('{0}')", // Is a part of an SQL expression.
						fileType == DatabaseFileTypes.Data ? "InstanceDefaultDataPath" : "InstanceDefaultLogPath");

				return connection.ExecuteScalar(sqlText).ToString();
			}
		}

		string GetDbFilePathFromMasterFiles(AdminConnection connection, string dbName, DatabaseFileTypes fileType)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			string sqlText = @"
				SELECT physical_name
				FROM master.sys.master_files
				WHERE database_id = DB_ID(@dbName)
					AND type = @fileType
				ORDER BY file_id"; // Sql queries do not need translating

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, dbName);
				cmd.AddParameter("@fileType", SqlDbType.TinyInt, fileType == DatabaseFileTypes.Data ? 0 : 1);

				var dbFile = DataUtils.GetListOfValuesFromCommand(cmd).FirstOrDefault();

				if (dbFile != null)
				{
					return Path.GetDirectoryName(dbFile);
				}
				else
				{
					return null;
				}
			}
		}
	}

	#region Test
#if DEBUG

	public partial class DatabaseCreationService : IDatabaseCreationService
	{
		string mainDatabaseNameOverrideForTest;
		internal IDisposable OverrideMainDatabaseNameTemporarily(string databaseName)
		{
			var previous = mainDatabaseNameOverrideForTest;
			mainDatabaseNameOverrideForTest = databaseName;
			return new DisposableAction(() => mainDatabaseNameOverrideForTest = previous);
		}

		internal IDisposable AcquireSqlAppLock_Exposed(AdminConnection connection, string databaseName, TimeSpan? sqlLockTimeout = null)
		{
			return AcquireSqlAppLockForDbCreation(connection, databaseName, sqlLockTimeout);
		}

		internal string GetFilePathFromMasterIfRamDrive(AdminConnection connection, string databaseName, string path, DatabaseFileTypes fileType)
		{
			try
			{
				if (!databaseName.StartsWith("DBUPG", StringComparison.OrdinalIgnoreCase) && new DriveInfo(path.Substring(0, 1)).TotalSize < (long)3 * 1024 * 1024 * 1024)
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture,
						@"SELECT TOP 1 LEFT(physical_name, len(physical_name) - CHARINDEX('\', REVERSE(physical_name)) + 1)
						FROM master.sys.database_files
						WHERE type = {0}
						ORDER BY file_id", fileType == DatabaseFileTypes.Data ? "0" : "1");
					path = (string)connection.ExecuteScalar(sqlText);
				}
			}
			catch (DriveNotFoundException)
			{
			}
			return path;
		}
	}

#endif
	#endregion
}
