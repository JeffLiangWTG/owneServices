namespace Enterprise.ZArchitecture.GUI
{
	public interface IModuleGridTasksStatusChangerFactory
	{
		IModuleFilterTaskStatusChanger Create(ZGrid grid);
	}

	public interface IModuleFilterTaskStatusChanger
	{
		void Initialise();
	}
}
