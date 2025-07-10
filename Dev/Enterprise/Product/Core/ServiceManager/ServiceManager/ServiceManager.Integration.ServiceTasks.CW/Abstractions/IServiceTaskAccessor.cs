using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	public interface IServiceTaskAccessor
	{
		IServiceTaskSchedule GetServiceTask(object businessObject);
	}
}
