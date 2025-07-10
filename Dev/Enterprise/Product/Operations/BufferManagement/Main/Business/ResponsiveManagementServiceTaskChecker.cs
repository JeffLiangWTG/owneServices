using CargoWise.Application;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.BufferManagement.Business
{
	public class ResponsiveManagementServiceTaskChecker : IResponsiveManagementServiceTaskChecker
	{
		const string AETServiceTaskCode = "AET";

		public bool AreDependentServiceTasksActive()
		{
			return ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(AETServiceTaskCode) > ServiceTaskStatus.ServiceTaskIsInactive;
		}
	}

	public interface IResponsiveManagementServiceTaskChecker
	{
		bool AreDependentServiceTasksActive();
	}
}
