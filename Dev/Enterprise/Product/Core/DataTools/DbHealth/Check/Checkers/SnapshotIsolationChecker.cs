using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	class SnapshotIsolationChecker : IChecker
	{
		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (!connection.IsCurrentDbUsingSnapshotIsolation)
			{
				warningList.Add(new DatabaseWarning(connection.ServerNameReportedByDatabase, DatabaseWarning.SnapshotIsolationWarning, "Snapshot Isolation is not enabled on your database, this may cause performance and locking problems", "Contact " + Constants.ProductSupportName + " to enable Snapshot Isolation on your database"));
			}
		}

		string IChecker.Description
		{
			get { return "Snapshot Isolation Configuration Checker"; }
		}
	}
}
