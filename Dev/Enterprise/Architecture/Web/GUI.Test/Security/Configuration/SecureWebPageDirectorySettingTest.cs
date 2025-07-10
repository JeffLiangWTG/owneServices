using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureWebPageDirectorySettingTest : SecureWebPageItemSettingTest
	{
		public void TestConstructorWithPathSecurityAndRecurse()
		{
			SecureWebPageDirectorySetting setting = new SecureWebPageDirectorySetting("default.aspx", SecurityType.Insecure, true);
			AssertNotNull("Constructed", setting);
			AssertEquals("Path", "default.aspx", setting.Path);
			AssertEquals("Secure", SecurityType.Insecure, setting.Secure);
			AssertEquals("Recurse", true, setting.Recurse);
		}

		#region Implementation

		protected override void AssertDefaultValueOfAdditionalProperties(SecureWebPageItemSetting setting)
		{
			base.AssertDefaultValueOfAdditionalProperties(setting);
			AssertEquals("Recurse", false, ((SecureWebPageDirectorySetting)setting).Recurse);
		}

		protected override SecureWebPageItemSetting GetNewSetting()
		{
			return new SecureWebPageDirectorySetting();
		}

		protected override SecureWebPageItemSetting GetNewSettingWithPath(string path)
		{
			return new SecureWebPageDirectorySetting(path);
		}

		protected override SecureWebPageItemSetting GetNewSettingWithPathAndSecurity(string path, SecurityType secure)
		{
			return new SecureWebPageDirectorySetting(path, secure);
		}

		#endregion
	}
}
