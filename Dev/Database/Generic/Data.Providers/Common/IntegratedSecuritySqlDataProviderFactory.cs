using System;
using System.Data;

namespace CargoWise.Data.Providers.Common
{
	public class IntegratedSecuritySqlDataProviderFactory : SqlDataProviderFactory
	{
		public IntegratedSecuritySqlDataProviderFactory() : base(null, null)
		{
		}

		public override IDbConnection OpenNewDbConnection(string serverName, string databaseName, string userName, string userPwd, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
			var connectionString = GetConnectionString(serverName, databaseName, null, null, true, applicationName, connectTimeout, connectionPooling, loadBalanceTimeout, maxPoolSize, minPoolSize);
			return OpenConnection(connectionString);
		}

		public override IDbConnection OpenNewDbConnection<TCredentials>(string serverName, string databaseName, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
			throw new InvalidOperationException($"{nameof(OpenNewDbConnection)} with generic type argument {typeof(TCredentials)} is not supported by {nameof(IntegratedSecuritySqlDataProviderFactory)}");
		}
	}
}
