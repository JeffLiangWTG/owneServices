using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(OrganizationIncidentsFilter))]
	class OrganizationIncidentsFilterTest : ModuleFilterTestCase<OrganizationIncidentsFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrganizationIncidentsFilter GetNewModuleFilter()
		{
			return new OrganizationIncidentsFilter("moo", ViewCampaignContactSchema.VCC_OH, IncidentMainSchema.IM_OH_Client, new SupportIncidentCollection(Factory), typeof(CampaignContact));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
