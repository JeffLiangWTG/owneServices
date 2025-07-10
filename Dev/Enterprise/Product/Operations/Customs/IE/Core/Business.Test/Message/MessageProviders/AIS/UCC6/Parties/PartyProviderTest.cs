using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class PartyProviderTest : DataProviderTestCase<PartyProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("address missing", () => new PartyProvider(null));
		}

		public void TestAddress()
		{
			AssertEquals("Address should be null when EORI specified", null, Provider.Address);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			PopulateAddress(orgHeader);
			var provider = GetProvider();
			AssertEquals("Add should be populated when no EORI specified", false, provider.Address == null);
		}

		public void TestName()
		{
			AssertNull("Name should be null when EORI specified", Provider.Name);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Org Name";
			PopulateAddress(orgHeader);
			var provider = GetProvider();
			AssertEquals("No EORI, has OA_CompanyNameOverride", "Test Co", provider.Name);

			address.OA_CompanyNameOverride = string.Empty;
			provider = GetProvider();
			AssertEquals("No EORI, no OA_CompanyNameOverride, has OH_FullName", "Org Name", provider.Name);

			orgHeader.OH_FullName = string.Empty;
			provider = GetProvider();
			AssertNull("No EORI, no OA_CompanyNameOverride, no OH_FullName", provider.Name);
		}

		public void TestId()
		{
			AssertEquals("EORI", "IE123456789", Provider.Id);
		}

		protected override PartyProvider GetProvider()
		{
			return new PartyProvider(address);
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			PopulateAddress(orgHeader);
		}

		void PopulateAddress(OrgHeader orgHeader)
		{
			address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Test Co";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "A12B3C4";
			address.OA_RN_NKCountryCode = "IE";
		}
		OrgAddress address;
	}
}
