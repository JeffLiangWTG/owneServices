using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACountryPreferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_CountryCode()
		{
			var errorMessage = "Country/Region Code should be composed of 2 characters.";
			var country = Factory.New<CACountryPreference>();
			country.CA_CountryCode = "A";
			AssertHasError(country.CA_CountryCodeInfo, errorMessage);

			country.CA_CountryCode = "AQ";
			AssertNoError(country.CA_CountryCodeInfo, errorMessage);
			Factory.Save();

			errorMessage = "Country/Region Code is not unique.";
			var country2 = Factory.New<CACountryPreference>();
			country2.CA_CountryCode = "AQ";

			country2.Validation.ValidateCA_CountryCode();
			AssertHasError(country2.CA_CountryCodeInfo, errorMessage);

			country.CA_CountryCode = "WF";
			AssertNoError(country.CA_CountryCodeInfo, errorMessage);

			country.Validation.ValidateCA_CountryCode();
			AssertNoError(country.CA_CountryCodeInfo, errorMessage);
		}
	}
}
