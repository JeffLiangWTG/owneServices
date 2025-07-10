using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.DbHealth.IndexUpdate
{
	public class IndexUpdateRunner
	{
		public void Run(string dbServer, string mainDbName, ILogger logger)
		{
			try
			{
				using (var connection = Db.NewAdminConnection(dbServer, mainDbName))
				{
					var dbNames = connection.GetDatabases(DatabaseType.Operational | DatabaseType.ExclusiveRefOrSharedRef).ToArray();
					new TableRebuilder(logger).Run(connection);
					new IndexRebuilder(logger).Run(connection, dbNames);
					new IndexStatisticsUpdater(logger).Run(connection, dbNames);
					new CachedStatisticsUpdater(logger).Run(connection);
					new IndexReorganiser(logger).Run(connection, dbNames);
					new UserStatisticsCreator(logger).Run(connection, mainDbName);
				}
			}
			catch (BacklogWaiterTimeoutException)
			{
			}
		}
	}
}
