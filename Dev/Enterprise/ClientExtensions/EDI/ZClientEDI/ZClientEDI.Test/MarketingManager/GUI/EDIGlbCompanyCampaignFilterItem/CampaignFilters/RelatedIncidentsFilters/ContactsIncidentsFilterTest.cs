using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ContactsIncidentsFilter))]
	class ContactsIncidentsFilterTest : ModuleFilterTestCase<ContactsIncidentsFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ContactsIncidentsFilter GetNewModuleFilter()
		{
			return new ContactsIncidentsFilter("moo", ViewCampaignContactSchema.PK, IncidentMainSchema.IM_OC_Contact, new SupportIncidentCollection(Factory), typeof(CampaignContact));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
