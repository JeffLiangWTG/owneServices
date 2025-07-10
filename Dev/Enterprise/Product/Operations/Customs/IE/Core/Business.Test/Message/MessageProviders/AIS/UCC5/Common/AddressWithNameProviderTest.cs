using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class AddressWithNameProviderTest : DataProviderTestCase<AddressWithNameProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", AddressProvider.New(null));
				AssertNotNull("Valid argument", AddressWithNameProvider.New(address));
			});
		}

		public void TestName()
		{
			var orgHeader = Factory.New<OrgHeader>();
			PopulateAddress(orgHeader);
			var provider = GetProvider();
			AssertEquals("Test Co", provider.Name);
		}

		public void TestPostcode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			PopulateAddress(orgHeader);
			var provider = GetProvider();
			AssertEquals("A12B3C4", provider.Postcode);
		}

		public void TestPostcodeEmpty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			PopulateAddress(orgHeader);
			address.OA_PostCode = string.Empty;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			var provider = GetProvider();
			AssertEquals("000", provider.Postcode);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			provider = GetProvider();
			AssertEquals(string.Empty, provider.Postcode);
		}

		protected override AddressWithNameProvider GetProvider()
		{
			return AddressWithNameProvider.New(address);
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			address = orgHeader.MainAddress;
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
