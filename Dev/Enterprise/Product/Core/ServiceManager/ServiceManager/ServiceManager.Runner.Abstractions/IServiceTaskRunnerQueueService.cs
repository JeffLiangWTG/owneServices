namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskRunnerQueueService
	{
		IServiceTaskRunnerQueue Start(string grpcEventHandleNameBase);
	}
}
