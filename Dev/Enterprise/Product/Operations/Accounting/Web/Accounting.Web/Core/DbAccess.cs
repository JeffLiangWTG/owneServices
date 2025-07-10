using System;
using System.Data;
using CargoWise.DataProtection;
using Enterprise.Accounting.Web.Exceptions;

namespace Enterprise.Accounting.Web.Core
{
	public static class DbAccess
	{
		const int ConnectTimeout = 15;
		const int MaxPoolSize = 100;
		const int MinPoolSize = 0;
		const int LoadBalanceTimeout = 0;
		const string ApplicationName = "Accounting.Web";

		public static System.Data.Common.DbConnection NewConnection()
		{
			IDbConnection tempResult = null;
			IDbConnection result = null;
			try
			{
				var sqlDataProviderFactory = new CargoWise.Data.Providers.Common.SqlDataProviderFactory();

				try
				{
					tempResult = sqlDataProviderFactory.OpenNewDbConnection<RestrictedWriterLoginCredentials>(
						serverName: WebConfigManager.EnterpriseDbServer,
						databaseName: WebConfigManager.EnterpriseDbName,
						applicationName: ApplicationName,
						connectTimeout: ConnectTimeout,
						connectionPooling: true,
						loadBalanceTimeout: LoadBalanceTimeout,
						maxPoolSize: MaxPoolSize,
						minPoolSize: MinPoolSize);
				}
				catch (Exception ex)
				{
					throw AccountingWebDbConnectionException.New(ex);
				}

				result = tempResult;
				tempResult = null;
			}
			finally
			{
				if (tempResult != null)
				{
					tempResult.Dispose();
				}
			}

			return result as System.Data.Common.DbConnection;
		}
	}
}
