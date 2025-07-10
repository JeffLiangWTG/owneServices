using CargoWise.Data.Utils;
using CargoWise.Integration;

namespace CargoWise.Data.Testing
{
	sealed class DummyTransactionLockManager : ITransactionLockManager
	{
		public void AddSqlLock(ISqlApplicationLock sqlLock)
		{
		}

		public void ReleaseSqlLocks()
		{
		}
	}
}
