using Enterprise.Customs.Business.Testing;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AddressProviderTest : DataProviderTestCase<AddressProvider>
	{
		public void TestNew()
		{
			AssertNull(GenerateProvider(null));
			AssertNotNull(GenerateProvider(orgAddress));
		}

		public void TestCity()
		{
			orgAddress.City = "City";
			AssertEquals("City", "City", Provider.City);

			orgAddress.City = string.Empty;
			AssertNull("City", Provider.City);
		}

		public void TestCountry()
		{
			orgAddress.OA_RN_NKCountryCode = "FR";
			AssertEquals("Country", "FR", Provider.Country);

			orgAddress.OA_RN_NKCountryCode = string.Empty;
			AssertNull("Country", Provider.Country);
		}

		public void TestStreet()
		{
			orgAddress.Address1 = "Address1";
			AssertEquals("Street", "Address1", Provider.Street);

			orgAddress.Address1 = string.Empty;
			AssertNull("Street", Provider.Street);
		}

		public void TestStreetAdditionalLine()
		{
			orgAddress.Address2 = "Address2";
			AssertEquals("StreetAdditionalLine", "Address2", Provider.StreetAdditionalLine);

			orgAddress.Address2 = string.Empty;
			AssertNull("StreetAdditionalLine", Provider.StreetAdditionalLine);
		}

		public void TestPostCode()
		{
			orgAddress.Postcode = "PostCode";
			AssertEquals("PostCode", "PostCode", Provider.PostCode);

			orgAddress.Postcode = string.Empty;
			AssertNull("PostCode", Provider.PostCode);
		}

		public void TestSubDivision()
		{
			AssertNull("SubDivision", Provider.SubDivision);
		}

		public void TestNumber()
		{
			AssertEquals("Number", "0", Provider.Number);
		}

		public void TestPoBox()
		{
			AssertNull("PoBox", Provider.PoBox);
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgAddress = Factory.New<OrgAddress>();
		}
		OrgAddress orgAddress;

		AddressProvider GenerateProvider(OrgAddress orgAddress) => AddressProvider.NewOrNull(orgAddress);

		protected sealed override AddressProvider GetProvider()
		{
			return GenerateProvider(orgAddress);
		}
	}
}
