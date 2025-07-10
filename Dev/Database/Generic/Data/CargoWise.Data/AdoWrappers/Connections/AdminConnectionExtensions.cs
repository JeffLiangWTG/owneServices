using System;
using CargoWise.Data.Services;
using static CargoWise.Data.DbConnection;

namespace CargoWise.Data
{
	public static class DatabaseCreationServiceOldAliasExtensions
	{
		public static void CreateDatabase(this AdminConnection connection, string databaseName, string dataPath = null, string logPath = null,
int? dataInitialSizeMb = null, int? logInitialSizeMb = null, int? dataGrowthMb = null, int? logGrowthMb = null, bool mapDbLogins = true, Action<string> actionToPerformAfterCreatingDb = null, TimeSpan? sqlLockTimeout = null, string physicalFileSuffix = "")
		{
			Db.Service<IDatabaseCreationService>().CreateDatabase(connection, databaseName, dataPath, logPath,
				dataInitialSizeMb, logInitialSizeMb, dataGrowthMb, logGrowthMb, mapDbLogins, actionToPerformAfterCreatingDb, sqlLockTimeout, physicalFileSuffix);
		}

#if DEBUG
		internal static string GetFilePathFromMasterIfRamDrive(this AdminConnection connection, string databaseName, string path, DatabaseFileTypes fileType)
		{
			return ((DatabaseCreationService)Db.Service<IDatabaseCreationService>()).GetFilePathFromMasterIfRamDrive(connection, databaseName, path, fileType);
		}

		internal static IDisposable AcquireSqlAppLock_Exposed(this AdminConnection connection, string databaseName, TimeSpan? sqlLockTimeout = null)
		{
			return ((DatabaseCreationService)Db.Service<IDatabaseCreationService>()).AcquireSqlAppLock_Exposed(connection, databaseName, sqlLockTimeout);
		}
#endif
	}
}
