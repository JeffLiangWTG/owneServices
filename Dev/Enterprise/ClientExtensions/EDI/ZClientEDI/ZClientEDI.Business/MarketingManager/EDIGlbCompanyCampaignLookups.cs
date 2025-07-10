using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	public class EDIGlbCompanyCampaignLookups : GlbCompanyCampaignLookups
	{
		public EDIGlbCompanyCampaignLookups(EDIGlbCompanyCampaign parent)
			: base(parent)
		{
		}

		public EDIGlbCompanyCampaignLookups(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override CampaignTypeList GetNewCampaignTypeList()
		{
			return new EDICampaignTypeList();
		}
	}
}

