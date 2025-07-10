using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	sealed class HyperlinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCaption()
		{
			var hyperlink = new Hyperlink { Caption = ZString.Empty };
			AssertMandatoryValidationError(hyperlink.CaptionInfo, true);

			hyperlink.Caption = " ";
			AssertMandatoryValidationError(hyperlink.CaptionInfo, true);

			hyperlink.Caption = "Test";
			AssertMandatoryValidationError(hyperlink.CaptionInfo, false);
		}

		public void TestValidateUrl()
		{
			var malformedErrorMessage = "Please enter a valid URL.";

			var hyperlink = new Hyperlink { Url = ZString.Empty };
			AssertMandatoryValidationError(hyperlink.UrlInfo, true);

			hyperlink.Url = "www.test.com";
			Assert(hyperlink.UrlInfo.Notifications.Contains(malformedErrorMessage));

			hyperlink.Url = "https://www.test.com";
			Assert(!hyperlink.UrlInfo.HasErrors());

			hyperlink.Url = "http://www.test.com";
			Assert(!hyperlink.UrlInfo.HasErrors());
		}
	}
}
