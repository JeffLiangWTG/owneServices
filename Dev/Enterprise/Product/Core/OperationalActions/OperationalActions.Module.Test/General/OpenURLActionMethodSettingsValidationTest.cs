using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Module
{
	sealed class OpenURLActionMethodSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateURL()
		{
			var settings = new OpenURLActionMethodSettings();

			settings.URL = ZString.Empty;
			AssertHasError(settings.URLInfo, "Please enter a value.");

			settings.URL = "http://www.cargowise.com";
			AssertNoErrors(settings.URLInfo);

			settings.URL = "IamNotAValidURL";
			AssertHasError(settings.URLInfo, "Please enter a valid URL.\r\n\r\nA valid address is commonly found in the format \"http://\" or \"www.\"");
		}
	}
}
