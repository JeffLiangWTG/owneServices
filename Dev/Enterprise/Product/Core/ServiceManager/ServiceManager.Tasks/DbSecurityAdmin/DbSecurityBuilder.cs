using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.SqlSecurity;

namespace Enterprise.ServiceManager.Tasks.DbSecurityAdmin
{
	class DbSecurityBuilder : DbSecurityLockDown
	{
		public void BuildSecurity(AdminConnection connection, bool isClientLicencedAsOpenDbSecurityMode, CancellationToken cancellationToken, ILogger logger)
		{
			Argument.NotNull(connection, "connection");
			Argument.NotNull(cancellationToken, "cancellationToken");
			Argument.NotNull(logger, "logger");

			if (!isClientLicencedAsOpenDbSecurityMode && !IsHostedAtWiseTechGlobal(connection))
			{
				var stringBuilder = new StringBuilder();
				DisableServerDdlTriggers(connection, stringBuilder);
				if (stringBuilder.Length > 0)
				{
					logger.Information("Disabling server DDL triggers:\r\n\r\n" + stringBuilder.ToString());
					stringBuilder.Clear();
				}

				DisableSqlAgentJobs(connection, stringBuilder);
				if (stringBuilder.Length > 0)
				{
					logger.Information("Disabling Sql agent jobs:\r\n\r\n" + stringBuilder.ToString());
					stringBuilder.Clear();
				}

				SetLockModeSqlServerSysadminPwd(connection);
			}

#if DEBUG
			var sqlSecurityManager = new SqlSecurityManager(logger, Db.DatabaseName, allowTransaction: true);
#else
			var sqlSecurityManager = new SqlSecurityManager(logger, Db.DatabaseName);
#endif
			sqlSecurityManager.BuildSecurity(connection, cancellationToken);
		}
	}
}
