using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.MarketingManager.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Module.Testing
{
	[TestedType(typeof(EDICRMGlbCompanyCampaignController))]
	class EDICRMGlbCompanyCampaignControllerTest : TestCRMGlbCompanyCampaignController
	{
		public void TestShowFormForNewEntity()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			EDICRMGlbCompanyCampaignController controller = new EDICRMGlbCompanyCampaignController();
			using (IZForm form = controller.ShowFormForNewEntity(campaign))
			{
				AssertEquals(typeof(EDIGlbCompanyCampaignForm), form.GetType());
			}
		}
	}
}
