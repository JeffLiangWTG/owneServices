
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.MarketingManager.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	public class EDICRMGlbCompanyCampaignController : CRMGlbCompanyCampaignController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIGlbCompanyCampaignForm((EDIGlbCompanyCampaign)businessEntity);
		}

		public new IZForm ShowFormForNewEntity(IBusiness businessEntity)
		{
			return base.ShowFormForNewEntity(businessEntity);
		}
	}
}
