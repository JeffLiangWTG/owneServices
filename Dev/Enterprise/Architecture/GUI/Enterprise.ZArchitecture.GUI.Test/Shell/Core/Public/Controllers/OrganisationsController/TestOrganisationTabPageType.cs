using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class TestOrganisationTabPageType : TestCase
	{
		public void TestOrganisationTabPageTypes()
		{
			AssertEquals("DetailsTabPage", OrganisationTabPages.Details.Name);
			AssertEquals("AddressesTabPage", OrganisationTabPages.Address.Name);
			AssertEquals("ContactsTabPage", OrganisationTabPages.Contacts.Name);
			AssertEquals("SalesTabPage", OrganisationTabPages.Sales.Name);
			AssertEquals("SalesTabPage+SalesClientRelTabPage", OrganisationTabPages.Sales_ClientRelationship.Name);
			AssertEquals("DetailsTabPage+ConfigTabPage+EDICodeMappingTabPage", OrganisationTabPages.EDICodeMappings.Name);
			AssertEquals("TransportTabPage+TransportTabPage1+ServiceLevelTabPage", OrganisationTabPages.ServiceLevel.Name);
		}
	}
}
