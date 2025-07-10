using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MDeclarantProviderTest : DataProviderTestCase<MDeclarantProvider>
	{
		public void TestContactPerson()
		{
			AssertEquals("Contact Person : Name", "Joe Bloggs", Provider.ContactPerson.Name);
			AssertEquals("Contact Person : Phone", "555 12345", Provider.ContactPerson.PhoneNumber);
			AssertEquals("Contact Person : Email", "test@example.com", Provider.ContactPerson.EmailAddress);
		}

		public void TestContactDetails()
		{
			// To be completed in a future WI
			Assert(true);
		}

		public void TestAddress()
		{
			AssertEquals("Address should be null when EORI specified", null, Provider.Address);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			PopulateAddressAndContact(orgHeader);
			var provider = GetProvider();
			AssertEquals("Add should be populated when no EORI specified", false, provider.Address == null);
		}

		public void TestName()
		{
			AssertEquals("Name should be null when EORI specified", null, Provider.Name);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			PopulateAddressAndContact(orgHeader);
			var provider = GetProvider();
			AssertEquals("Name should be populated when no EORI specified", "Test Co", provider.Name);
		}

		public void TestId()
		{
			AssertEquals("EORI", "IE123456789", Provider.Id);
		}

		protected override MDeclarantProvider GetProvider()
		{
			return MDeclarantProvider.New(address);
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			PopulateAddressAndContact(orgHeader);
		}

		void PopulateAddressAndContact(OrgHeader orgHeader)
		{
			address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Test Co";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "A12B3C4";
			address.OA_RN_NKCountryCode = "IE";

			var contact = orgHeader.Contacts.AddNew();
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "555 12345";
			contact.OC_Email = "test@example.com";
		}
		OrgAddress address;
	}
}
