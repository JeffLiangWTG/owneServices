using CargoWise.Data;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class UndisposedSqlLockChecker : IEnvironmentChecker
{
	public void Initialize(IRunCommandInfo runCommandInfo)
	{
	}

	public void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
	{
		DoCheck(serviceTaskHandler);
	}

	public void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler)
	{
		DoCheck(serviceTaskHandler);
	}

	void DoCheck(IServiceTaskHandler serviceTaskHandler)
	{
		if (Db.Connection.HasSqlLocks)
		{
			var lockKeys = new List<string>();
			foreach (var sqlLock in Db.Connection.UndisposedSqlLocks)
			{
				lockKeys.Add(sqlLock.Key);
				sqlLock.Dispose();
			}
			throw new UndisposedSqlLockException(serviceTaskHandler.HostedServiceAttribute, lockKeys);
		}
	}
}
