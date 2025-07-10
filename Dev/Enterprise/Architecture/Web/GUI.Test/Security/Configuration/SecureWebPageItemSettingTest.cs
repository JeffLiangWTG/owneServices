using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public abstract class SecureWebPageItemSettingTest : TestCase
	{
		public void TestDefaultConstructor()
		{
			SecureWebPageItemSetting setting = GetNewSetting();
			AssertNotNull("Constructed", setting);
			AssertEquals("Path", string.Empty, setting.Path);
			AssertEquals("Secure", SecurityType.Secure, setting.Secure);
			AssertDefaultValueOfAdditionalProperties(setting);
		}

		public void TestConstructorWithPath()
		{
			SecureWebPageItemSetting setting = GetNewSettingWithPath("default.aspx");
			AssertNotNull("Constructed", setting);
			AssertEquals("Path", "default.aspx", setting.Path);
			AssertEquals("Secure", SecurityType.Secure, setting.Secure);
			AssertDefaultValueOfAdditionalProperties(setting);
		}

		public void TestConstructorWithPathAndSecurity()
		{
			SecureWebPageItemSetting setting = GetNewSettingWithPathAndSecurity("default.aspx", SecurityType.Insecure);
			AssertNotNull("Constructed", setting);
			AssertEquals("Path", "default.aspx", setting.Path);
			AssertEquals("Secure", SecurityType.Insecure, setting.Secure);
			AssertDefaultValueOfAdditionalProperties(setting);
		}

		#region Implementation

		protected virtual void AssertDefaultValueOfAdditionalProperties(SecureWebPageItemSetting setting)
		{
		}

		protected abstract SecureWebPageItemSetting GetNewSetting();

		protected abstract SecureWebPageItemSetting GetNewSettingWithPath(string path);

		protected abstract SecureWebPageItemSetting GetNewSettingWithPathAndSecurity(string path, SecurityType secure);

		#endregion
	}
}
