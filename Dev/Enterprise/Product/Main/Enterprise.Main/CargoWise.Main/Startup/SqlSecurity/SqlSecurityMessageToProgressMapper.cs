using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.SqlSecurity
{
	class SqlSecurityMessageToProgressMapper
	{
		int totalNumberOfSteps;
		int stepsExecuted = -1;

		public bool Progress(string message)
		{
			if (totalNumberOfSteps == 0)
			{
				using (Db.DisposableActionForDbConnection())
				{
					totalNumberOfSteps = 1 + Db.Connection.GetDatabases(DatabaseType.All).Count();
					var auditServerConnectionString = DbRegistry.BiAuditServer.LoadValue(Db.Connection);
					if (!string.IsNullOrWhiteSpace(auditServerConnectionString) && !auditServerConnectionString.Equals(Db.ServerName, System.StringComparison.OrdinalIgnoreCase))
					{
						var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(auditServerConnectionString, Db.SqlMasterDb);
						totalNumberOfSteps++;
						if (auditConnection.DatabaseExists(Db.AuditDatabaseName))
						{
							totalNumberOfSteps++;
						}
					}

					var edwServerConnectionString = DbRegistry.BiDataWarehouseServer.LoadValue(Db.Connection);
					if (!string.IsNullOrWhiteSpace(edwServerConnectionString)
						&& !edwServerConnectionString.Equals(Db.ServerName, System.StringComparison.OrdinalIgnoreCase)
						&& !edwServerConnectionString.Equals(auditServerConnectionString, System.StringComparison.OrdinalIgnoreCase)
						)
					{
						var dataWareHouseConnection = Db.NewExtraConnectionWithMainDbCredentials(edwServerConnectionString, Db.SqlMasterDb);
						totalNumberOfSteps++;
						if (dataWareHouseConnection.DatabaseExists(Db.EdwDatabaseName))
						{
							totalNumberOfSteps++;
						}
					}
				}
			}

			if (message.StartsWith((NoResString)"Building Sql security took"))
			{
				stepsExecuted = totalNumberOfSteps;
				CurrentStatus = message;
				CurrentProgress = 100;
				return true;
			}

			if (message.StartsWith((NoResString)"Building Sql security") && !message.Contains((NoResString)"took"))
			{
				stepsExecuted++;
				CurrentStatus = message;
				CurrentProgress = stepsExecuted * 100 / totalNumberOfSteps;
				return true;
			}

			return false;
		}

		public string CurrentStatus { get; private set; }
		public int CurrentProgress { get; private set; }
	}
}
