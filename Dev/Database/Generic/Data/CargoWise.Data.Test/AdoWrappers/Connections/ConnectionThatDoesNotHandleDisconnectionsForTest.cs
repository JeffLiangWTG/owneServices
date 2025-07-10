using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data.Testing
{
	sealed class ConnectionThatDoesNotHandleDisconnectionsForTest : DbConnection<RestrictedWriterLoginCredentials>
	{
		public ConnectionThatDoesNotHandleDisconnectionsForTest()
			: base()
		{
			connectionErrorHandler = new DummyConnectionErrorManager(this);
		}

		public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);
	}

	sealed class IntegratedSecurityConnectionThatDoesNotHandleDisconnectionsForTest : ExtraConnection
	{
		public IntegratedSecurityConnectionThatDoesNotHandleDisconnectionsForTest(string serverName, string databaseName)
		: base(new IntegratedSecuritySqlDataProviderFactory(), serverName, databaseName, userLogin: null, userPassword: null)
		{
			connectionErrorHandler = new DummyConnectionErrorManager(this);
		}
	}
}
