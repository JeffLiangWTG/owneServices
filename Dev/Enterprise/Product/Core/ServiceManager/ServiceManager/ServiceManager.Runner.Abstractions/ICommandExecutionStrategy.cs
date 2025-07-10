namespace ServiceManager.Runner.Abstractions
{
	public interface ICommandExecutionStrategy
	{
		ServiceTaskRunResult Execute(ICommandInfo commandInfo);
	}
}
