using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureWebPageFileSettingCollectionTest : SecureWebPageItemSettingCollectionTest
	{
		#region Implementation

		protected override SecureWebPageItemSettingCollection GetNewCollection()
		{
			return new SecureWebPageFileSettingCollection();
		}

		protected override SecureWebPageItemSetting GetNewSetting()
		{
			return new SecureWebPageFileSetting("default.aspx");
		}

		#endregion
	}
}
