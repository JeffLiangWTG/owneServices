namespace Enterprise.StlAnalysis.Load
{
	using System;
	using CargoWise.Data.Providers.Common;
	using CargoWise.DataProtection;
	using Microsoft.Extensions.DependencyInjection;

	class DbManager
	{
		#region Connection

		public static SqlConnection NewEdiProdReadonlyConnection()
		{
			return NewConnectionFromServerInfo(SqlServerInfo.NewEdiProdSqlServerInfo());
		}

		public static SqlConnection OpenNewStlAnalysisDataWarehouseConnection()
		{
			return OpenNewConnection("stlanalysis.db.corporate.cargowise.com", "StlAnalysis", "StlAnalysis", "drefAnaJ56eP", pooling: true);
		}

		public static SqlConnection NewConnectionFromServerInfo(SqlServerInfo connectionInfo, bool pooling = false)
		{
			if (connectionInfo.UseRestrictedReaderLoginCredentials)
			{
				var sqlConnectionProvider = Program.ServiceProvider.GetRequiredService<ISqlConnectionProvider>();
				var pdsFactory = Program.ServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
				var pds = pdsFactory.CreateSystemService(connectionInfo.ServerName, connectionInfo.DatabaseName);

				return sqlConnectionProvider.OpenNewSqlConnection<RestrictedReaderLoginCredentials>(pds,
					builder => ConfigureConnectionString(builder,
					connectionInfo.ServerName,
					connectionInfo.DatabaseName,
					userName: null, // will be filled with the value for RestrictedReaderLoginCredentials from pds
					userPwd: null,
					pooling));
			}
			else
			{
				return OpenNewConnection(
					connectionInfo.ServerName,
					connectionInfo.DatabaseName,
					connectionInfo.LoginName,
					connectionInfo.LoginPwd,
					pooling);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		static SqlConnection OpenNewConnection(string serverName, string dbName, string userName, string userPwd, bool pooling)
		{
			var builder = new SqlConnectionStringBuilder();
			ConfigureConnectionString(builder, serverName, dbName, userName, userPwd, pooling);
			var connection = new SqlConnection(builder.ConnectionString);
			connection.Open();
			return connection;
		}

		internal static void ConfigureConnectionString(SqlConnectionStringBuilder builder, string serverName, string dbName, string userName, string userPwd, bool pooling)
		{
			builder.DataSource = serverName;
			builder.InitialCatalog = dbName;
			builder.ApplicationName = ApplicationName;
			builder.Pooling = pooling;
			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
			builder.Encrypt = encrypt;
			builder.TrustServerCertificate = !encrypt;

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(serverName))
			{
				builder.MultiSubnetFailover = true;
			}

			if (String.IsNullOrWhiteSpace(userName))
			{
				builder.IntegratedSecurity = true;
			}
			else
			{
				builder.IntegratedSecurity = false;
				builder.Password = userPwd;
				builder.UserID = userName;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public static SqlCommand NewSqlCommand(string cmdText, SqlConnection connection, int? timeoutInSeconds = null)
		{
			var cmd = new SqlCommand(cmdText, connection); // Cannot use CargoWise.Data because external tool

			if (timeoutInSeconds.HasValue)
			{
				cmd.CommandTimeout = timeoutInSeconds.Value;
			}

			return cmd;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public static SqlCommand NewSqlCommand(string cmdText, SqlTransaction transaction, int? timeoutInSeconds = null)
		{
			var cmd = new SqlCommand(cmdText, transaction.Connection, transaction); // Cannot use CargoWise.Data because external tool

			if (timeoutInSeconds.HasValue)
			{
				cmd.CommandTimeout = timeoutInSeconds.Value;
			}

			return cmd;
		}

		#endregion

		public const string ApplicationName = "cwStlAnalysis";
	}
}
