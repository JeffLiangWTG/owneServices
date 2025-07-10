using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskRequirementsChecker : IServiceTaskRequirementsChecker
	{
		public bool AttributeSatisfiesRequirements(IHostedServiceAttribute hostedServiceAttribute, out string branchCode)
		{
			return new HostedServiceConfiguration(hostedServiceAttribute).SatisfiesRequirements(out branchCode);
		}
	}
}
