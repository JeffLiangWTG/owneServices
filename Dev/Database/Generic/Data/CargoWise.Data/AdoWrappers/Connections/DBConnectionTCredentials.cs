using System.Data;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	public abstract class DbConnection<TCredentials> : DbConnection where TCredentials : DBCredentials
	{
		protected DbConnection() : this(null)
		{
		}

		protected DbConnection(string applicationNameSuffix) : this(Db.DefaultDataProviderFactory, Db.ServerName, Db.DatabaseName, applicationNameSuffix, null)
		{
		}

		protected DbConnection(IDataProviderFactory dataProviderFactory, string serverName, string databaseName) : this(dataProviderFactory, serverName, databaseName, null, null)
		{
		}

		protected DbConnection(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string applicationNameSuffix, IErrorReporter errorReporter) : base(dataProviderFactory, serverName, databaseName, applicationNameSuffix, errorReporter)
		{
			Argument.NotNull(dataProviderFactory, nameof(dataProviderFactory));

			DataProviderFactory = dataProviderFactory;
		}

		protected override IDbConnection OpenNewDbConnection()
		{
			var connectionPooling = ConnectionPoolingValue;
			return DataProviderFactory.OpenNewDbConnection<TCredentials>(
						fServerName,
						fInitialDatabaseName,
						SuffixedApplicationName,
						ConnectionStringTimeoutValue,
						connectionPooling.IsPooling,
						connectionPooling.LoadBalanceTimeout,
						connectionPooling.MaxPoolSize,
						connectionPooling.MinPoolSize);
		}
	}
}
