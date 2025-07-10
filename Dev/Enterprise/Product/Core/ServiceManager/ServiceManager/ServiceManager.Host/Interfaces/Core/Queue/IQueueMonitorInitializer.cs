namespace Enterprise.ServiceManager.Host.Queue
{
	interface IQueueMonitorInitializer
	{
		void ConfigureQueueMonitor(ITaskStatusProvider taskStatusProvider);
	}
}
