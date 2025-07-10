using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2JobDocAddressProviderTest : DataProviderTestCase<ICS2JobDocAddressProvider>
	{
		public void TestNew()
		{
			AssertNull(GenerateProvider(null));
			AssertNull(GenerateProvider(Factory.New<ICS2JobDocAddress>()));
			AssertNotNull(GenerateProvider(address));
		}

		public void TestCity()
		{
			AssertEquals("City", "City", Provider.City);

			address.City = string.Empty;

			AssertEquals("City", string.Empty, Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country", "FR", Provider.Country);

			address.E2_RN_NKCountryCode = string.Empty;

			AssertEquals("Country", string.Empty, Provider.Country);
		}

		public void TestStreet()
		{
			AssertEquals("Street", "Address1", Provider.Street);

			address.Address1 = string.Empty;

			AssertNull("Street", Provider.Street);
		}

		public void TestStreetAdditionalLine()
		{
			AssertEquals("StreetAdditionalLine", "Address2", Provider.StreetAdditionalLine);

			address.Address2 = string.Empty;

			AssertNull("StreetAdditionalLine", Provider.StreetAdditionalLine);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode", "PostCode", Provider.PostCode);

			address.Postcode = string.Empty;

			AssertEquals("PostCode", string.Empty, Provider.PostCode);
		}

		public void TestSubDivision()
		{
			AssertEquals("Subdivision", "Subdivision", Provider.SubDivision);

			address.SubDivision = string.Empty;

			AssertNull("SubDivision", Provider.SubDivision);
		}

		public void TestNumber()
		{
			AssertEquals("Number", "Number", Provider.Number);

			address.Number = string.Empty;

			AssertEquals("Number", "0", Provider.Number);
		}

		public void TestPoBox()
		{
			AssertEquals("POBox", "POBox", Provider.PoBox);

			address.POBox = string.Empty;

			AssertNull("PoBox", Provider.PoBox);
		}

		protected override void SetUp()
		{
			base.SetUp();

			address = Factory.New<ICS2JobDocAddress>();
			address.E2_AddressOverride = true;
			address.City = "City";
			address.E2_RN_NKCountryCode = "FR";
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			address.Postcode = "PostCode";
			address.SubDivision = "Subdivision";
			address.Number = "Number";
			address.POBox = "POBox";
		}
		ICS2JobDocAddress address;

		ICS2JobDocAddressProvider GenerateProvider(ICS2JobDocAddress address) => ICS2JobDocAddressProvider.NewOrNull(address);

		protected sealed override ICS2JobDocAddressProvider GetProvider()
		{
			return GenerateProvider(address);
		}
	}
}
