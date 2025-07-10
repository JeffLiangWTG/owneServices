using Enterprise.MarketingManager.Business.Testing;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	class EDIGlbCompanyCampaignLookupsTest : GlbCompanyCampaignLookupsTest
	{
		public new void TestCampaignTypeList()
		{
			var campaign = Factory.New<EDIGlbCompanyCampaign>();
			AssertEquals(typeof(EDICampaignTypeList), campaign.Lookups.CampaignTypeList.GetType());
		}
	}
}
