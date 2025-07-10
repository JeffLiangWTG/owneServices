using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	[HttpContextEnabledTest]
	sealed class SslHelperTest : TestCase
	{
		public void TestDetermineSecurePage()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			AssertEquals("https://127.0.0.1/webapp/default.aspx", SslHelper.DetermineSecurePage(Settings, false));
			AssertEquals("https://127.0.0.1/webapp/default.aspx", SslHelper.DetermineSecurePage(Settings, true));

			Settings.EncryptedUri = "https://www.cargowise.com";
			AssertEquals("https://www.cargowise.com/webapp/default.aspx", SslHelper.DetermineSecurePage(Settings, false));
			AssertEquals("https://www.cargowise.com/webapp/default.aspx", SslHelper.DetermineSecurePage(Settings, true));

			Settings.MaintainPath = false;
			AssertEquals("https://www.cargowise.com/default.aspx", SslHelper.DetermineSecurePage(Settings, false));
			AssertEquals("https://www.cargowise.com/default.aspx", SslHelper.DetermineSecurePage(Settings, true));
		}

		public void TestDetermineUnsecurePage()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			AssertEquals(null, SslHelper.DetermineUnsecurePage(Settings, false));
			AssertEquals("http://127.0.0.1/webapp/default.aspx", SslHelper.DetermineUnsecurePage(Settings, true));

			Settings.UnencryptedUri = "http://www.wisetechglobal.com/";
			AssertEquals(null, SslHelper.DetermineUnsecurePage(Settings, false));
			AssertEquals("http://www.wisetechglobal.com/webapp/default.aspx", SslHelper.DetermineUnsecurePage(Settings, true));

			Settings.MaintainPath = false;
			AssertEquals(null, SslHelper.DetermineUnsecurePage(Settings, false));
			AssertEquals("http://www.wisetechglobal.com/default.aspx", SslHelper.DetermineUnsecurePage(Settings, true));
		}

		public void TestRequestSecurePageWithDefaultSettings()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			SslHelper.RequestSecurePage(Settings);
			AssertEquals("https://127.0.0.1/webapp/default.aspx", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRequestSecurePageWithEncryptedUri()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			Settings.EncryptedUri = "https://www.cargowise.com";
			SslHelper.RequestSecurePage(Settings);
			AssertEquals("https://www.cargowise.com/webapp/default.aspx", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRequestSecurePageWithEncryptedUriAndWithoutMaintainingPath()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			Settings.EncryptedUri = "https://www.cargowise.com";
			Settings.MaintainPath = false;
			SslHelper.RequestSecurePage(Settings);
			AssertEquals("https://www.cargowise.com/default.aspx", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRequestUnsecurePageWithDefaultSettings()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			SslHelper.RequestUnsecurePage(Settings);
			AssertEquals(null, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRequestUnsecurePageWithEncryptedUri()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			Settings.EncryptedUri = "https://www.cargowise.com";
			SslHelper.RequestUnsecurePage(Settings);
			AssertEquals(null, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRequestUnsecurePageWithEncryptedUriAndWithoutMaintainingPath()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			Settings.EncryptedUri = "https://www.cargowise.com";
			Settings.MaintainPath = false;
			SslHelper.RequestUnsecurePage(Settings);
			AssertEquals(null, HttpContext.Current.Response.RedirectLocation);
		}

		#region Implementation

		SecureWebPageSettings Settings
		{
			get { return settings ?? (settings = new SecureWebPageSettings()); }
		}
		SecureWebPageSettings settings;

		void AssertSettingsDefaults()
		{
			AssertEquals("Mode", SecureWebPageMode.On, Settings.Mode);
			Assert("Encrypted Uri should be Empty", string.IsNullOrEmpty(Settings.EncryptedUri));
			Assert("Unencrypted Uri should be Empty", string.IsNullOrEmpty(Settings.UnencryptedUri));
			AssertEquals("MaintainPath should be True by default", true, Settings.MaintainPath);
		}

		#endregion
	}
}
