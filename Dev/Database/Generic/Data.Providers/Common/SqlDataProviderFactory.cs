namespace CargoWise.Data.Providers.Common
{
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Diagnostics.CodeAnalysis;
	using CargoWise.DataProtection;
	using Microsoft.Extensions.DependencyInjection;

	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public class SqlDataProviderFactory : IDataProviderFactory
	{
		readonly IProtectedDataService protectedDataService;
		readonly ISqlConnectionProvider sqlConnectionProvider;

		public SqlDataProviderFactory() : this(
			ProtectedDataService.SystemProtectedDataService,
			ProtectedDataService.GlobalServiceProvider.GetRequiredService<ISqlConnectionProvider>())
		{
		}

		public SqlDataProviderFactory(IProtectedDataService protectedDataService, ISqlConnectionProvider sqlConnectionProvider)
		{
			this.protectedDataService = protectedDataService;
			this.sqlConnectionProvider = sqlConnectionProvider;
		}

		public virtual IDbConnection OpenNewDbConnection(
			string serverName, string databaseName, string userName, string userPwd,
			string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
			var connectionString = GetConnectionString(
				serverName, databaseName, userName, userPwd, integratedSecuirty: false, applicationName,
				connectTimeout, connectionPooling, loadBalanceTimeout, maxPoolSize, minPoolSize);

			return OpenConnection(connectionString);
		}

		public IDbCommand NewDbCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
		{
			return new SqlCommand(commandText, (SqlConnection)connection, (SqlTransaction)transaction); // The only place to use "new SqlCommand"
		}

		public DbDataAdapter NewDataAdapter(IDbCommand selectCommand)
		{
			return new SqlDataAdapter((SqlCommand)selectCommand); // This is the only place to use "new SqlDataAdapter"
		}

		public DbDataAdapter NewDataAdapter(string commandText, IDbConnection connection)
		{
			commandText += System.Environment.NewLine; //Prevent SqlDataAdapter.FillSchema bug with inline comment at the end of query
			return NewDataAdapter(new SqlCommand(commandText, (SqlConnection)connection)); // This is the only place to use "new SqlDataAdapter"
		}

		public IDbDataParameter NewDbParameter(string name, SqlDbType sqlDbType, int size)
		{
			return new SqlParameter(name, sqlDbType, size);
		}

		public bool ConnectionShouldTimeOut => true;
		protected string GetConnectionString(string serverName, string databaseName, string userName, string userPwd, bool integratedSecuirty, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
#pragma warning disable CW1065 // Encrypt SQL Connection Rule
			var builder = new SqlConnectionStringBuilder();
#pragma warning restore CW1065 // Encrypt SQL Connection Rule

			ConfigureConnectionStringBuilder(builder, serverName, databaseName, applicationName, connectTimeout, connectionPooling, loadBalanceTimeout, maxPoolSize, minPoolSize);

			if (integratedSecuirty)
			{
				builder.IntegratedSecurity = true;
			}
			else
			{
				builder.UserID = userName;
				if (userPwd != null)
				{
					builder.Password = userPwd;
				}
			}

			return builder.ConnectionString;
		}

		void ConfigureConnectionStringBuilder(SqlConnectionStringBuilder builder, string serverName, string databaseName, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
			builder.PersistSecurityInfo = false;
			builder.ApplicationName = applicationName;
			builder.ConnectTimeout = connectTimeout;
			builder.ConnectRetryCount = 0; // Keep no retry (set to 0) ortherwise it will cause the connection that be killed can execute dirctly without reopen, that is short of expectation.
			builder.TypeSystemVersion = "SQL Server 2012"; // SQL Connection option string

			builder.DataSource = serverName;
			builder.InitialCatalog = databaseName;

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(serverName))
			{
				builder.MultiSubnetFailover = true;
			}

			var encryptionStatus = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
			builder.Encrypt = encryptionStatus;
			builder.TrustServerCertificate = !encryptionStatus;

			builder.Pooling = connectionPooling;

			if (builder.Pooling)
			{
				builder.LoadBalanceTimeout = loadBalanceTimeout;
				builder.MaxPoolSize = maxPoolSize;
				builder.MinPoolSize = minPoolSize;
			}
		}

		protected virtual IDbConnection OpenConnection(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
				return connection;
			}
			catch
			{
				connection.Dispose();
				throw;
			}
		}

		public virtual IDbConnection OpenNewDbConnection<TCredentials>(string serverName, string databaseName, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize) where TCredentials : DBCredentials
		{
			return sqlConnectionProvider.OpenNewSqlConnection<TCredentials>(protectedDataService, (builder) => ConfigureConnectionStringBuilder(builder, serverName, databaseName, applicationName, connectTimeout, connectionPooling, loadBalanceTimeout, maxPoolSize, minPoolSize));
		}

		protected virtual IDbConnection AttemptOpenDbConnection(string serverName, string databaseName, DBCredentials credentials, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize, bool encryptionStatus, out Exception exception)
		{
			var connectionString = GetConnectionString(serverName, databaseName, credentials.UserName, credentials.Password, integratedSecuirty: false, applicationName, connectTimeout, connectionPooling, loadBalanceTimeout, maxPoolSize, minPoolSize);

			try
			{
				var connection = OpenConnection(connectionString);
				exception = null;
				return connection;
			}
			catch (SqlException ex) when (ex.Number == 18456 || ex.Number == 233)
			{
				exception = ex;
				return null;
			}
		}
	}
}
