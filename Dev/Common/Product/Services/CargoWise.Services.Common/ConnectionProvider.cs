using System.Configuration;
#if NETFRAMEWORK
using System.Data.EntityClient;
#else
using System.Data.Entity.Core.EntityClient;
#endif
using CargoWise.Data.Providers.Common;

namespace CargoWise.Services.Common
{
	public static class ConnectionProvider
	{
		public static string GetCommonConnectionString()
		{
			return GetConnectionString("eHubTransactions");
		}

		public static string GetCommonProviderConnectionString()
		{
			return GetProviderConnectionString("eHubTransactions");
		}

		public static string GetConnectionString(string configItemsSuffix)
		{
			return GetConnectionStringBuilder(configItemsSuffix).ConnectionString;
		}

		public static string GetProviderConnectionString(string configItemsSuffix)
		{
			return GetConnectionStringBuilder(configItemsSuffix).ProviderConnectionString;
		}

		static EntityConnectionStringBuilder GetConnectionStringBuilder(string configItemsSuffix)
		{
			var serverName = System.Environment.MachineName;
			var dbName = configItemsSuffix;
			var userName = string.Empty;
			var password = string.Empty;

			var serverNameConfig = ConfigurationManager.AppSettings["DbServerName_" + configItemsSuffix];
			if (!string.IsNullOrEmpty(serverNameConfig))
			{
				serverName = serverNameConfig;
			}

			var dbNameConfig = ConfigurationManager.AppSettings["DbName_" + configItemsSuffix];
			if (!string.IsNullOrEmpty(dbNameConfig))
			{
				dbName = dbNameConfig;
			}

			var userNameConfig = ConfigurationManager.AppSettings["DbUserName_" + configItemsSuffix];
			if (!string.IsNullOrEmpty(userNameConfig))
			{
				userName = userNameConfig;
			}

			var passwordConfig = ConfigurationManager.AppSettings["DbPassword_" + configItemsSuffix];
			if (!string.IsNullOrEmpty(passwordConfig))
			{
				password = passwordConfig;
			}

			var sqlBuilder = new SqlConnectionStringBuilder();
			sqlBuilder.MultipleActiveResultSets = true;
			sqlBuilder.DataSource = serverName;
			sqlBuilder.InitialCatalog = dbName;
			sqlBuilder.ApplicationName = "eServices";

			if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
			{
				sqlBuilder.IntegratedSecurity = true;
			}
			else
			{
				sqlBuilder.UserID = userName;
				sqlBuilder.Password = password;
			}

			sqlBuilder.ConnectTimeout = 60;

			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
			sqlBuilder.Encrypt = encrypt;
			sqlBuilder.TrustServerCertificate = !encrypt;

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(serverName))
			{
				sqlBuilder.MultiSubnetFailover = true;
			}

			var entityBuilder = new EntityConnectionStringBuilder();
			entityBuilder.ProviderConnectionString = sqlBuilder.ToString();
			entityBuilder.Metadata = GetMetadataForEntityBuilder(configItemsSuffix);
			entityBuilder.Provider = "System.Data.SqlClient";

			return entityBuilder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Internal purpose string")]
		static string GetMetadataForEntityBuilder(string configItemsSuffix)
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			string metadataDetails = string.Empty;

			switch (configItemsSuffix)
			{
				case "eHubTransactions":
					metadataDetails = "CommonModel";
					break;
				case "DeniedPartyTransactions":
					metadataDetails = "TransactionModel";
					break;
				case "DeniedPartyContent":
					metadataDetails = "ContentModel";
					break;
				default:
					break;
			}

			if (string.IsNullOrEmpty(metadataDetails))
			{
				result.Append("res://*/");
			}
			else
			{
				result.AppendFormat("res://*/Model.{0}.csdl|res://*/Model.{0}.ssdl|res://*/Model.{0}.msl", metadataDetails);
			}

			return result.ToString();
		}
	}
}
