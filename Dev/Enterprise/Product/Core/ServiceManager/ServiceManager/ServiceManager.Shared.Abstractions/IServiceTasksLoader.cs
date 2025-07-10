namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTasksLoader
	{
		IServiceTaskCollectionGovernor Load();
	}
}
