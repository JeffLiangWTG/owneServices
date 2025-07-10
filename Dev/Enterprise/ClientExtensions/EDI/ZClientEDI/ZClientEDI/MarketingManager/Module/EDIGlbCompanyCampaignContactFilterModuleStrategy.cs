using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MarketingManager.Module;
using Enterprise.ZArchitecture.Business;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	class EDIGlbCompanyCampaignContactFilterModuleStrategy : GlbCompanyCampaignContactFilterModuleStrategy
	{
		protected override Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter GetNewOrgRelatedPartiesModuleFilter()
		{
			var orgRelatedPartiesModulesFilter = new EDIOrgRelatedPartiesModuleFilter("Related Parties", GetCustomerIntelligenceQuery);
			orgRelatedPartiesModulesFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			orgRelatedPartiesModulesFilter.MultilingualDescription = ResString.GetMultilingualString("b32e8de0-c432-4e4b-93c9-460b40debaf9", "Related Parties");
			return orgRelatedPartiesModulesFilter;
		}
	}
}
