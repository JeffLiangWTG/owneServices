using CargoWise.Application;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common.CW;
public class ServiceTaskGovernorFactory : IServiceTaskGovernorFactory
{
	public ServiceTaskGovernorFactory()
	{
		hostedServiceAttributeProvider = ObjectFactory.Get<IClientHostedServiceAttributeProvider>();
		scheduleStatusProvider = ObjectFactory.Get<IServiceTaskScheduleStatusProvider>();
		businessObjectBindingsProvider = ObjectFactory.Get<IHostedServiceBusinessObjectBindingsProvider>();
		dateTimeProvider = new ServiceManagerDateTimeProvider();
	}

	public IServiceTaskGovernor GetServiceTaskGovernor(IHostedServiceAttribute hostedServiceAttribute)
	{
		var dto = new NativeServiceTaskDTO(hostedServiceAttribute, null);

		var taskGovernor = new NativeServiceTaskGovernor(
			dto,
			hostedServiceAttributeProvider,
			scheduleStatusProvider,
			businessObjectBindingsProvider,
			dateTimeProvider);

		return taskGovernor;
	}

	readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;
	readonly IServiceTaskScheduleStatusProvider scheduleStatusProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider businessObjectBindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
}
