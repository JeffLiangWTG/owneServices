using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.PAVE.Common.Cache;
using Enterprise.ZArchitecture.Core;
using Polly;

namespace Enterprise.BufferManagement.Service.Cache
{
	class DBConnectionFactory : IDBConnectionFactory
	{
		static int MaxPoolSize { get; set; } = 500;
		public static IDbConnection OpenNewConnection()
		{
			var sqlDataProviderFactory = new CargoWise.Data.Providers.Common.SqlDataProviderFactory();
			return sqlDataProviderFactory.OpenNewDbConnection<RestrictedWriterLoginCredentials>(
				serverName: Db.ServerName,
				databaseName: Db.DatabaseName,
				applicationName: "DistributedCache",
				connectTimeout: 15,
				connectionPooling: true,
				loadBalanceTimeout: 0,
				minPoolSize: 0,
				maxPoolSize: MaxPoolSize
				);
		}

#if DEBUG
		internal static IDisposable SetMaxPoolSizeForTest(int maxPoolSizeForTest)
		{
			var current = MaxPoolSize;
			MaxPoolSize = maxPoolSizeForTest;
			return new DisposableAction(() => MaxPoolSize = current);
		}
#endif

		readonly bool reuseConnection;

		public DBConnectionFactory(bool reuseConnection = false)
		{
			this.reuseConnection = reuseConnection;
		}

		ConnectionWrapper connection;

		public IDbConnection CreateOpenedConnection()
		{
			var retry = Policy
				.Handle<InvalidOperationException>((ex) => ex.Message.Contains((NoResString)"The timeout period elapsed prior to obtaining a connection from the pool"))
				.WaitAndRetry(6, retryAttempt => TimeSpan.FromSeconds(15)); // max 1.5 minutes to wait!

			return retry.Execute(() => CreateOpenedConnectionCore());
		}

		IDbConnection CreateOpenedConnectionCore()
		{
			if (reuseConnection)
			{
				if (connection == null || connection.State != ConnectionState.Open)
				{
					connection = new ConnectionWrapper();
				}

				return connection;
			}

			return OpenNewConnection();
		}

		public void Dispose() => connection?.DisposeInnerConnection();
	}

	class ConnectionWrapper : IDbConnection
	{
		readonly IDbConnection connection;

		public ConnectionWrapper()
		{
			connection = DBConnectionFactory.OpenNewConnection();
		}

		public string ConnectionString { get => connection.ConnectionString; set => connection.ConnectionString = value; }

		public int ConnectionTimeout => connection.ConnectionTimeout;

		public string Database => connection.Database;

		public ConnectionState State => connection.State;

		public IDbTransaction BeginTransaction() => connection.BeginTransaction();

		public IDbTransaction BeginTransaction(IsolationLevel il) => connection.BeginTransaction(il);

		public void ChangeDatabase(string databaseName) => connection.ChangeDatabase(databaseName);

		public IDbCommand CreateCommand() => connection.CreateCommand();

		public void Open() => connection.Open();

		public void Close() => connection.Close();

		public void Dispose() { }

		public void DisposeInnerConnection() => connection.Dispose();
	}
}

