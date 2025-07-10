using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureWebPageDirectorySettingCollectionTest : SecureWebPageItemSettingCollectionTest
	{
		#region Implementation

		protected override SecureWebPageItemSettingCollection GetNewCollection()
		{
			return new SecureWebPageDirectorySettingCollection();
		}

		protected override SecureWebPageItemSetting GetNewSetting()
		{
			return new SecureWebPageDirectorySetting("default.aspx");
		}

		#endregion
	}
}
