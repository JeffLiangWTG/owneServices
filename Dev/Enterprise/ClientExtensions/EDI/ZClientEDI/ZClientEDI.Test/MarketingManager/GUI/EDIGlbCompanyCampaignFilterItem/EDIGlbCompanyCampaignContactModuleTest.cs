using CargoWise.Types;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.MarketingManager.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	[TestedType(typeof(EDIGlbCompanyCampaignContactModule))]
	public class EDIGlbCompanyCampaignContactModuleTest : GlbCompanyCampaignContactModuleTest
	{
		public void TestGetNewFilterBusinessObject_ShouldBeSetUpForFilterRuleMode()
		{
			using (var module = new EDIGlbCompanyCampaignContactModule())
			{
				var filterBizo = module.FilterBusinessObject;
				AssertEquals("This module's filters are always used in filter rule mode, so if this property is false, it means the filter business object wasn't created correctly using RelatedModuleFiltersHelper and will cause errors. SAD!", true, filterBizo.IsInFilterRuleMode);
				AssertEquals(module, filterBizo.ParentModule);
			}
		}

		public override void TestDripMarketingModuleName()
		{
			using (var module = new EDIGlbCompanyCampaignContactModuleForTest())
			{
				AssertEquals(ExpectedModuleName, module.DripMarketingFilterRuleModuleName_Exposed);
			}
		}

		class EDIGlbCompanyCampaignContactModuleForTest : EDIGlbCompanyCampaignContactModule
		{
			public ZString DripMarketingFilterRuleModuleName_Exposed
			{
				get
				{
					return DripMarketingFilterRuleModuleName;
				}
			}
		}

		protected override ZString ExpectedModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleEDI.Name;
			}
		}
	}
}
