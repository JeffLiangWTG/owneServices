using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrganisationFilterBusinessObject : EDIOrganisationFilterBusinessObjectCore
	{
		protected override void AddLicenceUsageFilters(ModuleFilterCollection filters)
		{
			LicenceUsageFilter.AddLicenceUsageFilters(filters, OrgHeaderSchema.Constants.PK, isBilledFilter: true);
			LicenceUsageFilter.AddLicenceUsageFilters(filters, OrgHeaderSchema.Constants.PK, isBilledFilter: false);
		}

		protected override void AddMembershipFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new MembershipFilter());
		}
	}
}
