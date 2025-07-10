using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskRequirementsChecker
	{
		bool AttributeSatisfiesRequirements(IHostedServiceAttribute hostedServiceAttribute, out string branchCode);
	}
}
