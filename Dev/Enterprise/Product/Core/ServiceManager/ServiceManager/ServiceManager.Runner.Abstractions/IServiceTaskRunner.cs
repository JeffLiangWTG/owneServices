using System.Threading;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskRunner
	{
		ServiceTaskRunResult RunServiceTask(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource);
	}
}
