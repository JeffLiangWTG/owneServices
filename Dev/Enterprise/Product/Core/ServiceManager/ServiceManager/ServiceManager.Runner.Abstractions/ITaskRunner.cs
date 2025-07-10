using ServiceManager.Common.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	public interface ITaskRunner
	{
		RunnerExitCode Run(bool singleRun, IApplicationExceptionHandler exceptionHandler);
	}
}
