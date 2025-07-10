namespace Enterprise.ServiceManager.Host
{
	public interface ISchedulerDispatcher
	{
		void Schedule(ITaskQueue taskQueue);
		void Dispatch(ITaskQueue taskQueue);
	}
}