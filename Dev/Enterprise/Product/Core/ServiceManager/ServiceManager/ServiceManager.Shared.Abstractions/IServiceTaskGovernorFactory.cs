using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskGovernorFactory
	{
		IServiceTaskGovernor GetServiceTaskGovernor(IHostedServiceAttribute serviceTaskAttribute);
	}
}
