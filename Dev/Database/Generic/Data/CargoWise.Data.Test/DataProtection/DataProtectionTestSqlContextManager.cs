using CargoWise.DataProtection.Administration.SqlServer;

namespace CargoWise.Data.Test.DataProtection
{
	public class DataProtectionTestSqlContextManager : ProtectedDataAdministrationSqlExecutionContextManagerBase
	{
		protected override ISqlExecutionContext CreateProtectedDataAdministrationSqlContext(string serverName, string databaseName)
		{
			IDbConnectionInternals connection = Db.Connection;
			return new SqlExecutionContext(connection.ADOConnection, connection.ADOTransaction);
		}
	}
}
