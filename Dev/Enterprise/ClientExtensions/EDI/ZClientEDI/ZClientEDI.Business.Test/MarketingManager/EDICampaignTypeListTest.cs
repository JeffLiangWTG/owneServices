using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	class EDICampaignTypeListTest : TestCase
	{
		public void TestEDICampaignTypeList()
		{
			EDICampaignTypeList list = new EDICampaignTypeList();
			AssertEquals(EDICampaignTypeList.Descriptions.WiseServicePartnerSurvey, list[EDICampaignTypeList.Codes.WiseServicePartnerSurvey].Description);
			Assert("Product Consultant Survey should not be included in the list", !list.ContainsCode(EDICampaignTypeList.Codes.ProductConsultantSurvey));
		}
	}
}