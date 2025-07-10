using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbHealth.Check;
using Enterprise.Integration;

namespace CargoWise.Bi.Maintenance
{
	public class BiDatabaseChecker
	{
		public BiDatabaseChecker(string serverName, string databaseName, ILogger logger)
		{
			this.serverName = serverName;
			this.databaseName = databaseName;
			this.logger = logger;
		}
		readonly string serverName;
		readonly string databaseName;
		readonly ILogger logger;

		public DbHealthWarningList Check()
		{
			if (DatabaseExists())
			{
				var dbCheckRunner = new DbCheckRunner();
				var healthWarnings = dbCheckRunner.CheckDatabaseConsistency(serverName, databaseName, logger);

				if (healthWarnings != null && healthWarnings.Count > 0)
				{
					foreach (var warning in healthWarnings)
					{
						logger?.Log(LogType.Warning, warning.Description);
					}
				}

				return healthWarnings;
			}
			else
			{
				return new DbHealthWarningList();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		bool DatabaseExists()
		{
			try
			{
				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(serverName, Db.SqlMasterDb))
				{
					return connection.DatabaseExists(databaseName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger?.Log(LogType.Warning, "Checking of database existence failed.", ex);
				return false;
			}
		}
	}
}
