using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.DbSecurityAdmin
{
	class DatabaseSecurityGuard
	{
		readonly DbSecurityBuilder dbSecurity = new DbSecurityBuilder();

		public void BuildSecurity(AdminConnection connection, CancellationToken cancellationToken, ILogger logger)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cancellationToken, nameof(cancellationToken));
			Argument.NotNull(logger, nameof(logger));

			dbSecurity.BuildSecurity(connection, IsClientLicencedAsOpenDbSecurityMode(), cancellationToken, logger);
		}

		public void CheckAndLockDownOpenDatabaseSecurity(AdminConnection connection, ILogger logger)
		{
			if (!IsClientLicencedAsOpenDbSecurityMode())
			{
				CleanupDbLevelRights(connection, logger);

				if (!dbSecurity.IsHostedAtWiseTechGlobal(connection))
				{
					CleanupServerLevelRights(connection, logger);
				}
			}
		}

		#region MSDB

		public void CheckAndLockDownMsdbAccess(AdminConnection connection, ILogger logger)
		{
			if (EnvProxy.IsHostedWithCargowise)
			{
				logger.Information("Check and lock down MSDB access");

				LockDownMsdbAccess(connection, logger);
			}
		}

		void LockDownMsdbAccess(AdminConnection connection, ILogger logger)
		{
			var mainDb = ((ICurrentDbControl)connection).InitialDatabase;

			var serverName = connection.ServerName;

			var loginsToCheck = new DbUserManager().GetStaffDbLogins(mainDbConnection: connection);
			loginsToCheck.AddRange(connection.Logins.Select(login => login.LoginName));

			// check primary server
			logger.Information($"Checking database [{Db.SqlMsdb}] on the server [{serverName}]");
			if (LockDownMsdbAccessOnServer(connection, serverName, loginsToCheck))
			{
				logger.Information("Modifications were made to security");
			}

			// propagate changes to each secondary replica
			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, mainDb))
			{
				foreach (var replicaName in AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(connection, mainDb, useCache: false))
				{
					try
					{
						using (var secondary = Db.NewAdminConnection(replicaName, Db.SqlMasterDb))
						{
							// check secondary server
							logger.Information($"Checking database [{Db.SqlMsdb}] on the server [{replicaName}]");
							if (LockDownMsdbAccessOnServer(secondary, replicaName, loginsToCheck))
							{
								logger.Information("Modifications were made to security");
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						logger.Information($"Connection problem\r\n{ex}");
					}
				}
			}
		}

		protected virtual bool LockDownMsdbAccessOnServer(AdminConnection connection, string serverName, IEnumerable<string> loginsToCheck)
		{
			return DbSecurity.LockDownMsdbAccessOnServer(connection, serverName, loginsToCheck);
		}

		#endregion // MSDB

		/// <summary>
		/// DATABASE LEVEL CLEAN UP (applies to LOCKED security mode only)
		/// </summary>
		void CleanupDbLevelRights(AdminConnection connection, ILogger logger)
		{
			string outputLog = dbSecurity.LockdownDatabaseLevelSecurity(connection);

			if (!String.IsNullOrWhiteSpace(outputLog))
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					outputLog = "Modifications were made to security";
				}
				logger.Log(LogType.Information, "Database level access\r\n\r\n" + outputLog);
			}
		}

		/// <summary>
		/// SERVER LEVEL CLEAN UP (applies to LOCKED security mode only)
		/// </summary>
		void CleanupServerLevelRights(AdminConnection connection, ILogger logger)
		{
			string outputLog = dbSecurity.LockdownServerLevelSecurity(connection);

			if (!String.IsNullOrWhiteSpace(outputLog))
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					outputLog = "Modifications were made to security";
				}
				logger.Log(LogType.Information, "Server level access\r\n\r\n" + outputLog);
			}
		}

		static bool IsClientLicencedAsOpenDbSecurityMode()
		{
#if DEBUG
			if (!Globals.IsTest)
			{
				return true;
			}
#endif
			return new EnterpriseInformationRetriever().IsDatabaseSecurityModeOpenAccordingToRegistrationKey();
		}
	}
}
