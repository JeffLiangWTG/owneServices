using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignorAddressWrapperTest : DataProviderTestCase<DeclarationConsignmentConsignorAddressWrapper>
	{
		public void TestCityName()
		{
			AssertNotNull("CityName", Provider.CityName);
			AssertEquals("CityName should be equal to the expected value", "City1", Provider.CityName.Value);
			AssertEquals("CityName's language should be null", null, Provider.CityName.LanguageID);
		}

		public void TestCountryCode()
		{
			AssertNotNull("CountryCode", Provider.CountryCode);
			AssertEquals("CountryCode should be equal to the expected value", "IL", Provider.CountryCode.Value);
		}

		public void TestCountrySubDivisionID()
		{
			AssertNull("CountrySubDivisionID", Provider.CountrySubDivisionID);
		}

		public void TestLine()
		{
			AssertNotNull("Line", Provider.Line);
			AssertEquals("Line should be equal to the expected value", "Street1", Provider.Line.Value);
			AssertEquals("Line's language should be null", null, Provider.Line.LanguageID);
		}

		public void TestPostcodeId()
		{
			AssertNotNull("PostcodeID", Provider.PostcodeId);
			AssertEquals("PostcodeID should be equal to the expected value", "Postcode1", Provider.PostcodeId.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentConsignorAddressWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentConsignorAddressWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentConsignorAddressWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_ShipperCity = "City1";
			asycudaBill.ABL_RN_NKShipperCountry = "IL";
			asycudaBill.ABL_ShipperStreet1 = "Street1";
			asycudaBill.ABL_ShipperPostcode = "Postcode1";

			return DeclarationConsignmentConsignorAddressWrapper.NewOrNull(asycudaBill);
		}
	}
}
