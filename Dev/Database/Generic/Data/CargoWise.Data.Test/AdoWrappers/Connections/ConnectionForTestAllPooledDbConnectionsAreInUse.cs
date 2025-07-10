using CargoWise.Data.Providers.Common;

namespace CargoWise.Data.Testing
{
	sealed class ConnectionForTestAllPooledDbConnectionsAreInUse : ConnectionWithTinyTimeoutForTest
	{
		public ConnectionForTestAllPooledDbConnectionsAreInUse(string serverName, string databaseName)
			: base(serverName, databaseName)
		{
		}

		protected override IConnectionPooling ConnectionPoolingValue
		{
			get { return new DefaultConnectionPooling(); }
		}

		public void ClearThePool()
		{
			if (InternalConnectionUnsafe is SqlConnection)
			{
				SqlConnection.ClearPool(InternalConnectionUnsafe as SqlConnection);
			}
		}
	}

	sealed class IntegratedSecurityConnectionForTestAllPooledDbConnectionsAreInUse : ExtraConnectionWithTinyTimeoutForTest
	{
		public IntegratedSecurityConnectionForTestAllPooledDbConnectionsAreInUse(string serverName, string databaseName) : base(new IntegratedSecuritySqlDataProviderFactory(), serverName, databaseName, string.Empty, string.Empty)
		{
		}

		protected override IConnectionPooling ConnectionPoolingValue
		{
			get { return new DefaultConnectionPooling(); }
		}

		public void ClearThePool()
		{
			if (InternalConnectionUnsafe is SqlConnection)
			{
				SqlConnection.ClearPool(InternalConnectionUnsafe as SqlConnection);
			}
		}
	}
}
