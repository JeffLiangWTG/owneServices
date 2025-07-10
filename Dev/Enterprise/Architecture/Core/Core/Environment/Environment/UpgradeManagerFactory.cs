using CargoWise.Data;
using Enterprise.Upgrades;

namespace Enterprise.ZArchitecture.Core
{
	public static class UpgradeManagerFactory
	{
		public static UpgradeManager NewUpgradeManager()
		{
			return NewUpgradeManager(Db.Connection);
		}

		public static UpgradeManager NewUpgradeManager(DbConnection connection)
		{
			connection.EnsureIsOpen();
			var sqlConnection = (SqlConnection)((IDbConnectionInternals)connection).InternalDbConnection;
			var sqlTransaction = (SqlTransaction)((IDbConnectionInternals)connection).InternalDbTransaction;
			var sqlContext = new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction);
			return new SqlUpgradeManager(sqlContext);
		}
	}
}
