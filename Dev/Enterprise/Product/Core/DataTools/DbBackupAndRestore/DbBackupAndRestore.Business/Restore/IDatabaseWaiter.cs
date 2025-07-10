using System.Threading;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public interface IDatabaseWaiter
	{
		void WaitUntilDatabaseRemovedFromAvailabilityGroup(DbConnection connection, string dbName, Logger logger, CancellationToken token, IAlwaysOnHelper alwaysOnHelper);
		void WaitUntilDatabaseReadyToRestore(DbConnection connection, string dbName, Logger logger, CancellationToken token);
		void WaitUntilDatabaseIsOnline(DbConnection connection, string dbName, Logger logger, CancellationToken token);
	}
}
