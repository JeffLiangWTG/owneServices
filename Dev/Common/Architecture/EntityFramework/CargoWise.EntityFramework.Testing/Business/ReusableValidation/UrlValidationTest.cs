using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class UrlValidationTest : TestCaseWithDummy
	{
		public void TestIsValidUrl()
		{
			AssertValidUrl("", false);

			AssertValidUrl("http://bl.ah", true);
			AssertValidUrl("http://blah.", false);
			AssertValidUrl("http://.blah", false);

			AssertValidUrl("https://bl.ah", true);
			AssertValidUrl("https://blah.", false);
			AssertValidUrl("https://.blah", false);

			AssertValidUrl("www.bl.ah", true);
			AssertValidUrl("www.blah.", false);
			AssertValidUrl("www..blah", false);
		}

		void AssertValidUrl(string url, bool expectValid)
		{
			Dummy.Z0_Description = url;
			AssertEquals(expectValid, UrlValidation.IsValidUrl(Dummy.Z0_Description));
		}

		public void TestIsValidAbsoluteUrl()
		{
			AssertValidAbsoluteUrl("", false);

			AssertValidAbsoluteUrl("http://bl.ah", false);
			AssertValidAbsoluteUrl("http://blah.", false);
			AssertValidAbsoluteUrl("http://.blah", false);

			AssertValidAbsoluteUrl("https://", false);
			AssertValidAbsoluteUrl("https://bl.ah", true);
			AssertValidAbsoluteUrl("https://blah.", true);
			AssertValidAbsoluteUrl("https://.blah", false);

			AssertValidAbsoluteUrl("www.bl.ah", false);
			AssertValidAbsoluteUrl("www.blah.", false);
			AssertValidAbsoluteUrl("www..blah", false);
		}

		void AssertValidAbsoluteUrl(string url, bool expectValid)
		{
			Dummy.Z0_Description = url;
			AssertEquals(expectValid, UrlValidation.IsValidAbsoluteUrl(Dummy.Z0_Description, Uri.UriSchemeHttps));
		}

		public void TestIsValidAbsoluteHttpOrHttpsUrl()
		{
			AssertValidAbsoluteUrl("", false);
			AssertValidAbsoluteUrl("ah", false);
			AssertValidAbsoluteUrl(" ", false);

			AssertValidAbsoluteHttpOrHttpsUrl("http://", false);
			AssertValidAbsoluteHttpOrHttpsUrl("http://bl.ah", true);
			AssertValidAbsoluteHttpOrHttpsUrl("http://blah.", true);
			AssertValidAbsoluteHttpOrHttpsUrl("http://.blah", false);

			AssertValidAbsoluteHttpOrHttpsUrl("https://", false);
			AssertValidAbsoluteHttpOrHttpsUrl("https://bl.ah", true);
			AssertValidAbsoluteHttpOrHttpsUrl("https://blah.", true);
			AssertValidAbsoluteHttpOrHttpsUrl("https://.blah", false);

			AssertValidAbsoluteHttpOrHttpsUrl("www.bl.ah", false);
			AssertValidAbsoluteHttpOrHttpsUrl("www.blah.", false);
			AssertValidAbsoluteHttpOrHttpsUrl("www..blah", false);

			AssertValidAbsoluteHttpOrHttpsUrl("ftp://", false);
			AssertValidAbsoluteHttpOrHttpsUrl("ftp://www.bl.ah", false);
			AssertValidAbsoluteHttpOrHttpsUrl("ftp://www.blah.", false);
			AssertValidAbsoluteHttpOrHttpsUrl("ftp://www..blah", false);

			AssertValidAbsoluteHttpOrHttpsUrl("file://", false);
			AssertValidAbsoluteHttpOrHttpsUrl("file://www.bl.ah", false);
			AssertValidAbsoluteHttpOrHttpsUrl("file://www.blah.", false);
			AssertValidAbsoluteHttpOrHttpsUrl("file://www..blah", false);
		}

		void AssertValidAbsoluteHttpOrHttpsUrl(string url, bool expectValid)
		{
			Dummy.Z0_Description = url;
			AssertEquals(expectValid, UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(Dummy.Z0_Description));
		}
	}
}
