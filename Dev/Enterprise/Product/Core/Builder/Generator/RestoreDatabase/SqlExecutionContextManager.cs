using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection.Administration.SqlServer;

namespace Enterprise.Builder.Generator.RestoreDatabase
{
	public class SqlExecutionContextManager : IProtectedDataAdministrationSqlExecutionContextManager
	{
		public IDbConnection Connection;

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "Should continue to use SqlConnection rather than CargoWise.Data.Db.Connection here.'")]
		public static IDbConnection OpenNewIntegratedSecurityConnection(string serverName)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder
			{
				PersistSecurityInfo = false,
				Pooling = false,
				ConnectTimeout = 120,
				ApplicationName = "CargoWiseOneBuilder",
				DataSource = serverName,
				InitialCatalog = "master",
			};

			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(connectionStringBuilder.DataSource);
			connectionStringBuilder.Encrypt = encrypt;
			connectionStringBuilder.TrustServerCertificate = !encrypt;
			connectionStringBuilder.IntegratedSecurity = true;

			var connection = new SqlConnection(connectionStringBuilder.ToString());
			connection.Open();
			return connection;
		}

		public void Close()
		{
			Connection?.Dispose();
			Connection = null;
		}

		public ISqlExecutionContext GetSqlExecutionContext(string serverName, string databaseName)
		{
			if (Connection is null)
			{
				Connection = OpenNewIntegratedSecurityConnection(serverName);
			}

			return new SqlExecutionContext(Connection, null);
		}
	}
}
