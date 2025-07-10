using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureWebPageFileSettingTest : SecureWebPageItemSettingTest
	{
		#region Implementation

		protected override SecureWebPageItemSetting GetNewSetting()
		{
			return new SecureWebPageFileSetting();
		}

		protected override SecureWebPageItemSetting GetNewSettingWithPath(string path)
		{
			return new SecureWebPageFileSetting(path);
		}

		protected override SecureWebPageItemSetting GetNewSettingWithPathAndSecurity(string path, SecurityType secure)
		{
			return new SecureWebPageFileSetting(path, secure);
		}

		#endregion
	}
}
