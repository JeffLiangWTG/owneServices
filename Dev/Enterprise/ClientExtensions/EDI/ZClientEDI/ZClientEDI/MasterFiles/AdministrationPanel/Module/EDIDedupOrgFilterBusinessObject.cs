using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIDedupOrgFilterBusinessObject : DeduplicationOrganisationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters =  base.GetModuleFiltersCore();
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, EDIOrganisationFilterBusinessObject.GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, EDIOrganisationFilterBusinessObject.GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory));
			return filters;
		}
	}
}
