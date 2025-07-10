using System.Threading;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskRunnerWithNextRunTimeCheck
	{
		ServiceTaskRunResult RunServiceTask(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource);
	}
}
