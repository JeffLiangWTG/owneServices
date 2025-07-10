namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskRunnerStrategy
	{
		ServiceTaskRunResult Run(IRunCommandInfo runCommandInfo);
	}
}
