using CargoWise.ServiceManager.Next.Shared;

namespace CargoWise.ServiceManager.Next.Launcher;

public class NextProcessRunnerFactory : INextProcessRunnerFactory
{
	delegate INextProcessRunner CreateDelegate(string runnerCode, INextRunnerOptions nextRunnerOptions);

	readonly CreateDelegate createDelegate;

	public NextProcessRunnerFactory(IServiceProvider serviceProvider)
	{
		createDelegate = (runnerCode, nextRunnerOptions) => ActivatorUtilities.CreateInstance<NextProcessRunner>(serviceProvider, runnerCode, nextRunnerOptions);
	}

	public INextProcessRunner Create(string runnerCode, INextRunnerOptions nextRunnerOptions) => createDelegate(runnerCode, nextRunnerOptions);
}
