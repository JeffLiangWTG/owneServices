using CargoWise.Data;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class DbConnectionDisposerChecker : IEnvironmentChecker
{
	public void Initialize(IRunCommandInfo runCommandInfo) { }

	public void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
	{
		if (Db.IsDbConnectionDisposerMissing())
		{
			throw new DbConnectionDisposerCorruptedException(serviceTaskHandler.HostedServiceAttribute);
		}
	}

	public void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler) { }
}
