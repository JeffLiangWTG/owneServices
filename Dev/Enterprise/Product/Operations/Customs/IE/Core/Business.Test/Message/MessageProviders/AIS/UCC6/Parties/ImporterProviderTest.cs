using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class ImporterProviderTest : DataProviderTestCase<ImporterProvider>
	{
		public void TestConstructor()
		{
			AssertNull("Importer missing", ImporterProvider.New(null));
		}

		public void TestContactDetails()
		{
			// To be implemented in a future WI
			AssertNull(Provider.ContactDetails);
		}

		public void TestAddress()
		{
			PopulateAddress();
			var provider = GetProvider();
			AssertEquals("Address should be null when EORI is provided.", null, provider.Address);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			address = orgHeader.MainAddress;
			PopulateAddress();
			provider = GetProvider();

			AssertEquals("Address should not be null when no EORI is provided.", false, provider.Address is null);
			AssertEquals("Address : StreetAndNumber", "Address 1, Address 2", provider.Address.StreetAndNumber);
			AssertEquals("Address : City", "Dublin", provider.Address.City);
			AssertEquals("Address : Postcode", "A12B3C4", provider.Address.Postcode);
			AssertEquals("Address : Country", "IE", provider.Address.Country);
		}

		public void TestName()
		{
			PopulateAddress();
			var provider = GetProvider();
			AssertEquals("Name should be null when EORI is provided.", null, provider.Name);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			address = orgHeader.MainAddress;
			PopulateAddress();
			provider = GetProvider();

			AssertEquals("Name should not be null when no EORI is provided.", false, provider.Name is null);
			AssertEquals("Name", "Test Co", provider.Name);
		}

		public void TestId()
		{
			AssertEquals("EORI", "IE123456789", Provider.Id);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			address = orgHeader.MainAddress;
			PopulateAddress();
			var provider = GetProvider();

			AssertEquals("No EORI, Id should be null", null, provider.Id);
		}

		public void TestForceIncludeNameAndAddressDetailsInMessage()
		{
			Assert(!Provider.ForceIncludeNameAndAddressDetailsInMessage);
		}

		protected override ImporterProvider GetProvider()
		{
			return ImporterProvider.New(address);
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			address = orgHeader.MainAddress;
		}
		OrgAddress address;

		void PopulateAddress()
		{
			address.OA_CompanyNameOverride = "Test Co";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "A12B3C4";
			address.OA_RN_NKCountryCode = "IE";
		}
	}
}
