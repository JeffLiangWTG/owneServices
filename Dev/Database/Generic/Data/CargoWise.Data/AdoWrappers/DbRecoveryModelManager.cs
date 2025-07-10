using System;

namespace CargoWise.Data
{
	public static class DbRecoveryModelManager
	{
		public static DbRecoveryModel GetActual(DbConnection connection, string databaseName)
		{
			return Instance.GetActual(connection, databaseName);
		}

		public static DbRecoveryModel? GetDesired(DbConnection connection,string mainDbName, string databaseName)
		{
			return Instance.GetDesired(connection, mainDbName, databaseName);
		}

		public static void AdjustDatabase(DbConnection connection, string mainDbName, string databaseName)
		{
			Instance.AdjustDatabase(connection, mainDbName, databaseName);
		}

		internal static void AdjustDatabase(AdminConnection connection, string mainDbName, string databaseName, string tempDbNameWhileCreating)
		{
			Instance.AdjustDatabase(connection, mainDbName, databaseName, tempDbNameWhileCreating);
		}

		public static string[] AdjustAllDatabases(DbConnection connection, string mainDbName)
		{
			return Instance.AdjustAllDatabases(connection, mainDbName);
		}

		static IDbRecoveryModelManagerInternals Instance => (IDbRecoveryModelManagerInternals)DbEnv.Instance.DbRecoveryModelManager;
	}

	public interface IDbRecoveryModelManagerInternals : IDbRecoveryModelManager
	{
		DbRecoveryModel GetActual(DbConnection connection, string databaseName);

		DbRecoveryModel? GetDesired(DbConnection connection, string mainDbName, string databaseName);

		void AdjustDatabase(DbConnection connection, string mainDbName, string databaseName, string tempDbNameWhileCreating = null);

		string[] AdjustAllDatabases(DbConnection connection, string mainDbName);

		IDisposable LoadAndCacheRequiredDbValues(DbConnection mainDbConnection);
	}
}
