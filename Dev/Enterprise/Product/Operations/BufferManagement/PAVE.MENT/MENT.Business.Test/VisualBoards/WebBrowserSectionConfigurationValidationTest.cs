using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class WebBrowserSectionConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUrlIsValidURL()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = "WEB";

			var sectionConfiguration = (WebBrowserSectionConfiguration)section.Configuration;

			sectionConfiguration.URL = "SWEETHAIRCUTBRO";
			sectionConfiguration.Validation.ValidateURL();

			AssertHasError(sectionConfiguration.URLInfo, "The URL you have entered does not contain a protocol. Please include http://, https:// in front of the address. The address should look like: http://www.somewebsite.com");

			sectionConfiguration.URL = "www.danielkeogh.com";
			sectionConfiguration.Validation.ValidateURL();

			AssertHasError(sectionConfiguration.URLInfo, "The URL you have entered does not contain a protocol. Please include http://, https:// in front of the address. The address should look like: http://www.somewebsite.com");

			sectionConfiguration.URL = "www.danielkeogh.comhttp://";

			sectionConfiguration.Validation.ValidateURL();
			AssertNoErrors(sectionConfiguration.URLInfo);

			sectionConfiguration.URL = "http://www.hasthelhcdestroyedtheearth.com/";
			sectionConfiguration.Validation.ValidateURL();

			AssertNoErrors(sectionConfiguration.URLInfo);

			sectionConfiguration.URL = "https://www.hasthelhcdestroyedtheearth.com/";
			sectionConfiguration.Validation.ValidateURL();

			AssertNoErrors(sectionConfiguration.URLInfo);
		}
	}
}
