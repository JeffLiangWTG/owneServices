using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data.HttpClient
{
	public class HttpDataProviderFactory : IDataProviderFactory
	{
		public IDbConnection OpenNewDbConnection(
			string serverName, string databaseName, string userName, string userPwd,
			string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize)
		{
			return new HttpConnection(serverName, databaseName, userName, userPwd, applicationName, connectTimeout);
		}

		public IDbConnection OpenNewDbConnection<TCredentials>(string serverName, string databaseName, string applicationName, int connectTimeout, bool connectionPooling, int loadBalanceTimeout, int maxPoolSize, int minPoolSize) where TCredentials : DBCredentials
		{
			var credential = GetCredentials<TCredentials>(serverName, databaseName);
			return new HttpConnection(serverName, databaseName, credential.UserName, credential.Password, applicationName, connectTimeout);
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public IDbCommand NewDbCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
		{
			var command = ((HttpConnection)connection).CreateCommand(); // The only place to use "HttpConnection.CreateCommand"
			command.CommandText = commandText;
			return command;
		}

		public DbDataAdapter NewDataAdapter(IDbCommand selectCommand)
		{
			return HttpDataAdapter.New(selectCommand);
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public DbDataAdapter NewDataAdapter(string commandText, IDbConnection connection)
		{
			var command = ((HttpConnection)connection).CreateCommand(); // The only place to use "HttpConnection.CreateCommand"
			command.CommandText = commandText;
			return NewDataAdapter(command);
		}

		public IDbDataParameter NewDbParameter(string name, SqlDbType sqlDbType, int size)
		{
			return new SqlParameter(name, sqlDbType, size);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses")]
		public static TCredentials GetCredentials<TCredentials>(string serverName, string databaseName) where TCredentials : DBCredentials
		{
			var credentials = ProtectedDataService.SystemProtectedDataService.ListCredentialsOfType<TCredentials>();
			if (!credentials.Any())
			{
				throw new NotSupportedException($"No credentials found for type: {typeof(TCredentials).Name}");
			}

			if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(databaseName))
			{
				return credentials.First();
			}

			foreach (var credential in credentials)
			{
				try
				{
					using var sqlConnection = new SqlConnection((GetSqlConnectionString(credential.UserName, credential.Password)));
					sqlConnection.Open();

					return credential;
				}
				catch (SqlException sqlException) when (sqlException.Number == 18456)
				{
					continue;
				}
			}

			return credentials.First();

			string GetSqlConnectionString(string userName, string passWord)
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder
				{
					PersistSecurityInfo = false,
					Pooling = false,
					ConnectTimeout = 120,
					ApplicationName = nameof(HttpDataProviderFactory),
					DataSource = serverName,
					InitialCatalog = databaseName,
					UserID = userName,
					Password = passWord,
				};

				var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(connectionStringBuilder.DataSource);
				connectionStringBuilder.Encrypt = encrypt;
				connectionStringBuilder.TrustServerCertificate = !encrypt;
				connectionStringBuilder.IntegratedSecurity = true;

				return connectionStringBuilder.ToString();
			}
		}

		public bool ConnectionShouldTimeOut => false;
	}
}
