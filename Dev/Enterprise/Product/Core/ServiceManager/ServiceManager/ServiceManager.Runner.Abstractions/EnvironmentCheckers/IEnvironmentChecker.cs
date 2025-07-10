namespace ServiceManager.Runner.Abstractions
{
	public interface IEnvironmentChecker
	{
		void Initialize(IRunCommandInfo runCommandInfo);

		void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler);

		void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler);
	}
}
