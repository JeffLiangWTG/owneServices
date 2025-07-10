using System.Web;
using System.Web.Configuration;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	[HttpContextEnabledTest]
	sealed class RequestEvaluatorTest : TestCase
	{
		public void TestEvaluate()
		{
			AssertEquals("Request Url", "http://127.0.0.1/webapp/default.aspx", HttpContext.Current.Request.Url.AbsoluteUri);
			AssertSettingsDefaults();

			AssertEquals(SecurityType.Insecure, RequestEvaluator.Evaluate(HttpContext.Current.Request, Settings, false));
			AssertEquals(SecurityType.Insecure, RequestEvaluator.Evaluate(HttpContext.Current.Request, Settings, true));

			AssertNull("Actual Settings", WebConfigurationManager.GetSection("secureWebPages") as SecureWebPageSettings);
			try
			{
				RequestEvaluator.Evaluate(HttpContext.Current.Request);
				Fail("SHould be an exception because we cannot create secureWebPages in WebConfigManager");
			}
			catch
			{
			}
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
