using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Module;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	public class EDIGlbCompanyCampaignFilterBusinessObject : GlbCompanyCampaignFilterBusinessObject
	{
		protected override GlbCompanyCampaignLookups GetNewLookups()
		{
			return new EDIGlbCompanyCampaignLookups(Factory);
		}
	}
}
