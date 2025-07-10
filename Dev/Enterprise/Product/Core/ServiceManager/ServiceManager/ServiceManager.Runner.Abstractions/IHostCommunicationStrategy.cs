namespace ServiceManager.Runner.Abstractions
{
	public interface IHostCommunicationStrategy
	{
		void Completed(IServiceTaskRunnerQueue queueService);
		void ServiceTaskLockNotAcquired(IServiceTaskRunnerQueue queueService, ICommandInfo commandInfo);
		void GroupLockNotAcquired(IServiceTaskRunnerQueue queueService, ICommandInfo commandInfo);
		void GrpcPortOpened(int port);
	}
}
