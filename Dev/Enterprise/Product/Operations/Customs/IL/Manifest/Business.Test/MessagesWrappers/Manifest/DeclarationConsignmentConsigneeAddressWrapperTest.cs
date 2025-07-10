using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsigneeAddressWrapperTest : DataProviderTestCase<DeclarationConsignmentConsigneeAddressWrapper>
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

		public void TestCountrySubDivisionName()
		{
			AssertNull("CountrySubDivisionName", Provider.CountrySubDivisionName);
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
			AssertNull("When asycudaBill is null", DeclarationConsignmentConsigneeAddressWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentConsigneeAddressWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentConsigneeAddressWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_ConsigneeCity = "City1";
			asycudaBill.ABL_RN_NKConsigneeCountry = "IL";
			asycudaBill.ABL_ConsigneeStreet1 = "Street1";
			asycudaBill.ABL_ConsigneePostcode = "Postcode1";

			return DeclarationConsignmentConsigneeAddressWrapper.NewOrNull(asycudaBill);
		}
	}
}
