using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	public class EDIGlbCompanyCampaignDripMarketing : GlbCompanyCampaignDripMarketing
	{
		public EDIGlbCompanyCampaignDripMarketing(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ModuleIdentifier DripMarketingFilterRuleModuleCore
		{
			get
			{
				return base.DripMarketingFilterRuleModuleCore == ModuleIDs.DripMarketingFilterRule ? ModuleIDs.DripMarketingFilterRuleEDI : ModuleIDs.DripMarketingFilterRuleHR;
			}
		}
	}
}

