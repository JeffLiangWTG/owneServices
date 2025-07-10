using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	sealed class AddressWrapperTest : TestCaseWithFactory
	{
		public void TestAddressWrapperFieldsAreTruncated()
		{
			var addressLine = "THIS IS A RATHER LONG LINE OF OVER THIRTY FIVE CHARACTERS";
			var cityName = "THIS IS A RATHER LONG CITY NAME OF OVER THIRTY FIVE CHARACTERS";
			var countryCode = "ABC";
			var postCode = "ABCDE1234567890";
			IAddress addressWrapper = AddressWrapper.New(addressLine, cityName, countryCode, postCode);

			var expectedLine35 = "THIS IS A RATHER LONG LINE OF OVER ";
			var expectedCityName = "THIS IS A RATHER LONG CITY NAME OF ";
			var expectedCountryCode = "AB";
			var expectedPostCode = "ABCDE1234";

			AssertEquals("Line should be truncated to 35 characters", expectedLine35, addressWrapper.Line);
			AssertEquals("CityName should be truncated to 35 characters", expectedCityName, addressWrapper.CityName);
			AssertEquals("CountryCode should be truncated to 2 characters", expectedCountryCode, addressWrapper.CountryCode);
			AssertEquals("PostCodeID should be truncated to 9 characters", expectedPostCode, addressWrapper.PostcodeID);

			var addressLine70 = "THIS IS A RATHER RATHER RATHER RATHER RATHER RATHER LONG LINE OF OVER 70 CHARACTERS";
			var expectedLine70 = "THIS IS A RATHER RATHER RATHER RATHER RATHER RATHER LONG LINE OF OVER ";
			addressWrapper = AddressWrapper.New(addressLine70, cityName, countryCode, postCode, 70);
			AssertEquals("Line should be truncated to 70 characters", expectedLine70, addressWrapper.Line);
		}

		public void TestAddressWithNewlines()
		{
			const string addressLine = "THIS IS A RATHER\r\nLONG LINE OF OVER THIRTY FIVE CHARACTERS";
			const string cityName = "THIS IS A RATHER LONG CITY\r\nNAME OF\r\nOVER THIRTY FIVE CHARACTERS";
			const string countryCode = "ABC\r\n";
			const string postCode = "ABC\r\nDE1234567890";
			IAddress addressWrapper = AddressWrapper.New(addressLine, cityName, countryCode, postCode);

			const string expectedLine35 = "THIS IS A RATHER LONG LINE OF OVER ";
			const string expectedCityName = "THIS IS A RATHER LONG CITY NAME OF ";
			const string expectedCountryCode = "AB";
			const string expectedPostCode = "ABC DE123";

			AssertEquals("Line should be truncated to 35 characters and not include the CR/LF characters", expectedLine35, addressWrapper.Line);
			AssertEquals("CityName should be truncated to 35 characters and not include the CR/LF characters", expectedCityName, addressWrapper.CityName);
			AssertEquals("CountryCode should be truncated to 2 characters and not include the CR/LF characters", expectedCountryCode, addressWrapper.CountryCode);
			AssertEquals("PostCodeID should be truncated to 9 characters and not include the CR/LF characters", expectedPostCode, addressWrapper.PostcodeID);
		}

		public void TestDefaultPostcodeID()
		{
			IAddress addressWrapper = AddressWrapper.New("Address 1", "City 1", "Country 1", "12345");
			AssertEquals("PostcodeID should be the same as passed in AddressWrapper.New()", "12345", addressWrapper.PostcodeID);

			addressWrapper = AddressWrapper.New("Address 1", "City 1", "Country 1", ZString.Empty);
			AssertEquals("PostcodeID should equal to \"NA\"", "NA", addressWrapper.PostcodeID);

			addressWrapper = AddressWrapper.New(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("PostcodeID should be empty", ZString.Empty, addressWrapper.PostcodeID);
		}
	}
}
