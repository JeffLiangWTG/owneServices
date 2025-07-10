using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;

namespace ZClientEDI.Business.Test.MarketingManager
{
	public class EDIGlbCompanyCampaignValidationTest : GlbCompanyCampaignValidationTest
	{
		protected override GlbCompanyCampaign GetCampaignForTest()
		{
			return Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
		}
	}
}
