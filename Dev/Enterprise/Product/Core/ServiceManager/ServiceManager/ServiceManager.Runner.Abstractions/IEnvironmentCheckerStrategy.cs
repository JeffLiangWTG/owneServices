namespace ServiceManager.Runner.Abstractions
{
	public interface IEnvironmentCheckerStrategy
	{
		void Initialize(IRunCommandInfo runCommandInfo);

		void ExecuteOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler);

		void ExecuteOnServiceTaskException(IServiceTaskHandler serviceTaskHandler);
	}
}
