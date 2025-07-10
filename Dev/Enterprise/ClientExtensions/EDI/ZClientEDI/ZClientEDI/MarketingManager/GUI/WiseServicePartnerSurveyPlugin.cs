using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class WiseServicePartnerSurveyPlugin : SurveyCampaignPlugIn
	{
		public WiseServicePartnerSurveyPlugin(EDIGlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		protected override string SupportedCampaignType
		{
			get { return EDICampaignTypeList.Codes.WiseServicePartnerSurvey; }
		}
	}
}
