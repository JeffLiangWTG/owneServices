namespace Enterprise.ServiceManager.Host
{
	interface IServiceTasksInitializer
	{
		bool InitializeServiceTasks();
		IDSATaskRunner CreateDSARunner();
	}
}
