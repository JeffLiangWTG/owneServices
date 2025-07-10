using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MarketingManager.GUI;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public partial class EDIGlbCompanyCampaignForm : GlbCompanyCampaignForm
	{
		public EDIGlbCompanyCampaignForm(EDIGlbCompanyCampaign campaign)
			: base(campaign)
		{
			PlugIns.Add(ClientControllerRegistration.WiseServicePartnerSurveyPlugIn);
		}
	}
}
