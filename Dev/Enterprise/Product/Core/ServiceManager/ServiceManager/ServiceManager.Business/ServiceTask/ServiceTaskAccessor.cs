using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskAccessor : IServiceTaskAccessor
	{
		public IServiceTaskSchedule GetServiceTask(object businessObject)
		{
			if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				return ((IStmServiceTaskConfigControlDataProvider)businessObject).ConfigAdapter;
			}

			return (IServiceTaskSchedule)businessObject;
		}
	}
}
