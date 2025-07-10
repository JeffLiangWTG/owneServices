#if DEBUG

using System;
using CargoWise.Common;
using CargoWise.IO;

namespace CargoWise.Data.Testing
{
	public static class SnapshotCreator
	{
		public static IDisposable CreateSnapshot(AdminConnection adminConnection, Action commandToRunBeforeRestore, string dbName = null, string snapshotName = null, int lockTimeout = 30, string filename_ext = "", string mainDbName = null)
		{
			Argument.NotNull(adminConnection, nameof(adminConnection)); // Suggested By ReviewBot 
			Argument.NotNull(commandToRunBeforeRestore, nameof(commandToRunBeforeRestore));

			var connection = ((IDbConnectionInternals)adminConnection).ADOConnection
				?? throw new InvalidOperationException("SqlConnection is required for this operation.");

			var databaseName = !string.IsNullOrEmpty(dbName) ? dbName : connection.Database;
			var snapshotDbName = !string.IsNullOrEmpty(snapshotName) ? snapshotName : databaseName + "-SS";

			AdoTestUtils.DropDbIfExists(adminConnection, snapshotDbName, mainDbName);
			ExecuteSnapshotCreation(connection, adminConnection, databaseName, snapshotDbName, filename_ext);

			return new Disposable(() => RestoreFromSnapshot(adminConnection, commandToRunBeforeRestore, databaseName, snapshotDbName, mainDbName, lockTimeout));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Low level access to run backup for tests")]
		public static void RestoreFromSnapshot(AdminConnection adminConnection, Action commandToRunBeforeRestore, string databaseName, string snapshotDbName, int lockTimeout = 30)
			=> RestoreFromSnapshot(adminConnection, commandToRunBeforeRestore, databaseName, snapshotDbName, string.Empty, lockTimeout);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Low level access to run backup for tests")]
		public static void RestoreFromSnapshot(AdminConnection adminConnection, Action commandToRunBeforeRestore, string databaseName, string snapshotDbName, string mainDbName, int lockTimeout = 30)
		{
			var adoConnection = ((IDbConnectionInternals)adminConnection).ADOConnection ?? throw new InvalidOperationException("SqlConnection is required for this operation.");

			adminConnection.EnsureIsOpen();
			using (var dropCommand = adoConnection.CreateCommand()) // Low level access to run backup for tests
			{
				adoConnection.ChangeDatabase(Db.SqlMasterDb);
				commandToRunBeforeRestore();
				Db.DbStateLock.AcquireWriterLock(TimeSpan.FromSeconds(lockTimeout));
				try
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
					DbConnectionKiller.KillOtherConnections(adminConnection, databaseName);
					dropCommand.CommandText = $"RESTORE DATABASE {databaseName.QuoteName()} FROM database_snapshot = {snapshotDbName.QuoteName('\'')}; DROP DATABASE {snapshotDbName.QuoteName()};";
					dropCommand.ExecuteNonQuery();

#if DEBUG
					if (databaseName == Db.DatabaseName
					|| databaseName == Db.AuditDatabaseName
					|| databaseName == Db.EdwDatabaseName
					|| RefDbTableNameResolver.IsSharedDatabase(databaseName)
					|| RefDbTableNameResolver.IsExclusiveDatabase(mainDbName, databaseName))
					{
						DbCommitTracker.Reset(databaseName);
					}
#endif
				}
				finally
				{
					Db.DbStateLock.ReleaseWriterLock();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Low level access to run backup for tests")]
		static void ExecuteSnapshotCreation(System.Data.Common.DbConnection connection, AdminConnection adminConnection, string databaseName, string snapshotDbName, string filenameExt)
		{
			using var command = connection.CreateCommand(); // Low level access to run backup for tests
			command.CommandText = GenerateSnapshotCreationSql(
				Temp.TempPath,
				databaseName,
				snapshotDbName,
				filenameExt
				);

			try
			{
				command.ExecuteNonQuery();
			}
			catch (SqlException ex) when (ex.Number == -2)
			{
				DbConnectionKiller.KillOtherConnections(adminConnection, databaseName);
				command.ExecuteNonQuery();
			}
		}

		static string GenerateSnapshotCreationSql(string tempPath, string databaseName, string snapshotName, string fileExtension = "")
		{
			var quotedDatabaseName = databaseName.QuoteName();
			var quotedSnapshotName = snapshotName.QuoteName();

			return $@"
SET NOCOUNT ON;

DECLARE @tempPath varchar(255) = '{tempPath}';
DECLARE @fileExtension varchar(50) = '{fileExtension}';
DECLARE @deleteFileSql varchar(max) = '';
DECLARE @declareFileSql varchar(max) = '';

SELECT
	@deleteFileSql = @deleteFileSql + 'EXEC CLRDeleteFile ''' + @tempPath + 
					RIGHT(physical_name, CHARINDEX('\', REVERSE(physical_name))) + 
					CASE WHEN @fileExtension <> '' THEN @fileExtension ELSE '' END + '.ss'', ''1'';',
	@declareFileSql = CASE WHEN @declareFileSql = '' THEN '' ELSE @declareFileSql + ',' END + 
					'(name=''' + name + ''', filename = ''' + @tempPath + 
					RIGHT(physical_name, CHARINDEX('\', REVERSE(physical_name))) + 
					CASE WHEN @fileExtension <> '' THEN @fileExtension ELSE '' END + '.ss'')'
FROM {quotedDatabaseName}.sys.database_files
WHERE [type] = 0;

DECLARE @dynamicsql varchar(max) =
	@deleteFileSql +
	'CREATE DATABASE {quotedSnapshotName} ON ' + @declareFileSql + ' AS SNAPSHOT OF {quotedDatabaseName}';

EXEC (@dynamicsql);";
		}

		class Disposable : IDisposable
		{
			public Disposable(Action action)
			{
				Argument.NotNull(action, nameof(action));
				this.action = action;
			}

			readonly Action action;

			public void Dispose()
			{
				action();
			}
		}
	}
}

#endif
