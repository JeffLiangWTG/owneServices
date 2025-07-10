using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentNotifyPartyAddressWrapperTest : DataProviderTestCase<DeclarationConsignmentNotifyPartyAddressWrapper>
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

		public void TestCountrySubDivisionId()
		{
			AssertNull("CountrySubDivisionID", Provider.CountrySubDivisionId);
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
			AssertNull("When asycudaBill is null", DeclarationConsignmentNotifyPartyAddressWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentNotifyPartyAddressWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentNotifyPartyAddressWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_NotifyPartyCity = "City1";
			asycudaBill.ABL_RN_NKNotifyPartyCountry = "IL";
			asycudaBill.ABL_NotifyPartyStreet1 = "Street1";
			asycudaBill.ABL_NotifyPartyPostcode = "Postcode1";

			return DeclarationConsignmentNotifyPartyAddressWrapper.NewOrNull(asycudaBill);
		}
	}
}
