using CargoWise.Data.Utils;

namespace CargoWise.Integration
{
	public interface ITransactionLockManager
	{
		void AddSqlLock(ISqlApplicationLock sqlLock);
		void ReleaseSqlLocks();
	}
}
