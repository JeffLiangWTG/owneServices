using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class EDIGlbCompanyCampaignContactFilterControl : GlbCompanyCampaignContactFilterControl
	{
		public EDIGlbCompanyCampaignContactFilterControl(IBusinessObjectCollection collection, GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject, GlbCompanyCampaign campaign)
			: base(collection, filterBusinessObject, campaign)
		{
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new EDICampaignFilterStrip();
		}
	}
}