using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LocalForwarderOrganisationWrapper : ExportAgentOrganisationWrapper
	{
		public LocalForwarderOrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, organisation, mainContactType, factory)
		{
		}

		public LocalForwarderOrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, address, mainContactType, factory)
		{
		}

		public LocalForwarderOrganisationWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(usageType, docAddress, factory)
		{
		}

		internal LocalForwarderOrganisationWrapper(OrganisationUsageType usageType, OrganisationWrapper wrapperBeingCloned)
			: base(usageType, wrapperBeingCloned)
		{
		}
	}
}
