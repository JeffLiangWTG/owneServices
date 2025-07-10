using CargoWise.Types;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class EDIGlbCompanyCampaignContactModule : GlbCompanyCampaignContactModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			var campaign = (EDIGlbCompanyCampaign)Campaign;
			var moduleId = campaign?.DripMarketingFilterRuleModule ?? ModuleIDs.DripMarketingFilterRuleEDI;

			return (FilterBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleId, campaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIGlbCompanyCampaignContactFilterControl(GridCollection, (GlbCompanyCampaignContactFilterBusinessObject)FilterBusinessObject, Campaign);
		}

		protected override ZString DripMarketingFilterRuleModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleEDI.Name;
			}
		}
	}
}
