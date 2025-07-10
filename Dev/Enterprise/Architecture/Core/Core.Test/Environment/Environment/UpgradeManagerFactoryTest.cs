using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class UpgradeManagerFactoryTest : TestCase
	{
		public void TestConnectionClosed()
		{
			var connection = Db.NewAdminConnection();
			connection.EnsureIsOpen();
			connection.CloseConnection();
			AssertEquals(ConnectionState.Closed, ((IDbConnectionInternals)connection).InternalDbConnection.State);

			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager(connection);
			AssertEquals(Db.ServerName, upgradeManager.dbServerName);
			AssertEquals(Db.DatabaseName, upgradeManager.databaseName);
			AssertEquals(ConnectionState.Open, ((IDbConnectionInternals)connection).InternalDbConnection.State);
		}

		public void TestSqlUpgradeManagerDbParamters()
		{
			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			AssertEquals(Db.ServerName, upgradeManager.dbServerName);
			AssertEquals(Db.DatabaseName, upgradeManager.databaseName);
		}
	}
}
