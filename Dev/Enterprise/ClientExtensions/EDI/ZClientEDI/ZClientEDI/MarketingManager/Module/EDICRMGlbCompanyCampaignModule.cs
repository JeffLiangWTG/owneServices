using Enterprise.MarketingManager.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	public class EDICRMGlbCompanyCampaignModule : CRMGlbCompanyCampaignModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIGlbCompanyCampaignFilterBusinessObject();
		}
	}
}
