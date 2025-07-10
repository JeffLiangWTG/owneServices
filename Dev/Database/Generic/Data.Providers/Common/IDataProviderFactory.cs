namespace CargoWise.Data.Providers.Common
{
	using System.Data;
	using System.Data.Common;
	using CargoWise.DataProtection;

	public interface IDataProviderFactory
	{
		IDbConnection OpenNewDbConnection(string serverName, string databaseName, string userName, string userPwd, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize);
		IDbConnection OpenNewDbConnection<TCredentials>(string serverName, string databaseName, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize) where TCredentials : DBCredentials;
		IDbCommand NewDbCommand(string commandText, IDbConnection connection, IDbTransaction transaction);
		DbDataAdapter NewDataAdapter(IDbCommand selectCommand);
		DbDataAdapter NewDataAdapter(string commandText, IDbConnection connection);
		IDbDataParameter NewDbParameter(string name, SqlDbType sqlDbType, int size);

		bool ConnectionShouldTimeOut { get; }
	}
}
