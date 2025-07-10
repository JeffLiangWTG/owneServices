using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Host
{
	class DbConnectionSetup : IDbConnectionSetup
	{
		public DbConnectionSetup(IEventLogger eventLogger, ICancellationTokenProvider cancellationTokenProvider)
		{
			this.eventLogger = eventLogger;
			this.cancellationTokenProvider = cancellationTokenProvider;
		}

		readonly ILogger eventLogger;
		readonly ICancellationTokenProvider cancellationTokenProvider;

		public void TryConnectAndHandleErrors()
		{
			try
			{
				Db.Connection.EnsureIsOpen();
			}
			catch (SqlException sqlEx)
			{
				var shouldUseModernSqlSecurity = false;
				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					shouldUseModernSqlSecurity = EnvProxy.Instance.Registry.UseModernSqlSecuritySystem;
				}

				if (shouldUseModernSqlSecurity)
				{
					if (new DbErrorMatch(sqlEx).ExceptionType.In(DbErrorType.LoginFailedForUser, DbErrorType.CannotOpenDbRequestedInLogin))
					{
						var sqlSecurityManager = new SqlSecurityManager(eventLogger.ToCW1Logger(), Db.DatabaseName);
						using (var adminConnection = Db.NewAdminConnection())
						{
							sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenProvider.Token);
						}
					}
					else
					{
						throw;
					}
				}
				else
				{
					if (!Db.HandleDbConnectionSetupErrors<RestrictedWriterLoginCredentials>(sqlEx, Db.ServerName, Db.DatabaseName, Db.ServerName))
					{
						throw;
					}
				}
			}
			catch (DatabaseUpgradeException)
			{
			}
		}
	}
}
