using Enterprise.MarketingManager.Business;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	public class EDIGlbCompanyCampaignItemCollection : GlbCompanyCampaignItemCampaignDependentCollection
	{
		public EDIGlbCompanyCampaignItemCollection(EDIGlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public new EDIGlbCompanyCampaignItem this[int index]
		{
			get { return (EDIGlbCompanyCampaignItem)Elements[index]; }
		}

		public new EDIGlbCompanyCampaignItem AddNew()
		{
			return (EDIGlbCompanyCampaignItem)base.AddNew();
		}
	}
}

