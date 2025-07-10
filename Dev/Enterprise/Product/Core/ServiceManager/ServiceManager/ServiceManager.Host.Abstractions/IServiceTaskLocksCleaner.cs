namespace ServiceManager.Host.Abstractions
{
	public interface IServiceTaskLocksCleaner
	{
		void ReleaseLocksFromServiceTask(int processId, string code);

		void ReleaseLocksFromHost();
	}
}
