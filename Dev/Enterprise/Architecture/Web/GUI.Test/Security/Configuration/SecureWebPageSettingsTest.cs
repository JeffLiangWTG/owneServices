using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureWebPageSettingsTest : TestCase
	{
		public void TestDefaultValues()
		{
			AssertNotNull("Settings", Settings);

			AssertEquals("BypassQueryParamName", string.Empty, Settings.BypassQueryParamName);
			AssertEquals("EncryptedUri", string.Empty, Settings.EncryptedUri);
			AssertEquals("UnencryptedUri", string.Empty, Settings.UnencryptedUri);
			AssertEquals("IgnoreHandlers", SecureWebPageIgnoreHandlers.BuiltIn, Settings.IgnoreHandlers);
			AssertEquals("MaintainPath", true, Settings.MaintainPath);
			AssertEquals("Files", 0, Settings.Files.Count);
			AssertEquals("Directories", 0, Settings.Directories.Count);
		}

		public void TestBypassQueryParamName()
		{
			AssertEquals("BypassQueryParamName default", string.Empty, Settings.BypassQueryParamName);
			Settings.BypassQueryParamName = "bypass";
			AssertEquals("BypassQueryParamName as set", "bypass", Settings.BypassQueryParamName);
		}

		public void TestEncryptedUri()
		{
			AssertEquals("EncryptedUri default", string.Empty, Settings.EncryptedUri);
			Settings.EncryptedUri = "https://cargowise.com";
			AssertEquals("EncryptedUri as set", "https://cargowise.com", Settings.EncryptedUri);
		}

		public void TestUnencryptedUri()
		{
			AssertEquals("UnencryptedUri default", string.Empty, Settings.UnencryptedUri);
			Settings.UnencryptedUri = "http://cargowise.com";
			AssertEquals("UnencryptedUri as set", "http://cargowise.com", Settings.UnencryptedUri);
		}

		public void TestIgnoreHandlers()
		{
			AssertEquals("IgnoreHandlers default", SecureWebPageIgnoreHandlers.BuiltIn, Settings.IgnoreHandlers);
			Settings.IgnoreHandlers = SecureWebPageIgnoreHandlers.WithStandardExtensions;
			AssertEquals("IgnoreHandlers as set", SecureWebPageIgnoreHandlers.WithStandardExtensions, Settings.IgnoreHandlers);
		}

		public void TestMaintainPath()
		{
			AssertEquals("MaintainPath default", true, Settings.MaintainPath);
			Settings.MaintainPath = false;
			AssertEquals("MaintainPath as set", false, Settings.MaintainPath);
		}

		#region Implementation

		SecureWebPageSettings Settings
		{
			get { return settings ?? (settings = new SecureWebPageSettings()); }
		}
		SecureWebPageSettings settings;

		#endregion
	}
}
