using System.Text;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class PasswordHashingTransformationTestHelper
	{
		public static void SetPasswordHistoryCount(int value, bool isDefault = false)
		{
			var registryHelper = new RegistryTransformationHelper();
			registryHelper.DeleteStmDataRow("PasswordHistoryCount");
			if (!isDefault)
			{
				registryHelper.InsertStmDataRow("PasswordHistoryCount", "INT", Encoding.Unicode.GetBytes(value.ToString()));
			}
		}

		public static void SetPasswordHashingIterationsCount(int value, bool isDefault = false)
		{
			var registryHelper = new RegistryTransformationHelper();
			registryHelper.DeleteStmDataRow("PasswordHashingIterationsCount");
			if (!isDefault)
			{
				registryHelper.InsertStmDataRow("PasswordHashingIterationsCount", "INT", Encoding.Unicode.GetBytes(value.ToString()));
			}
		}

		public static void SetEnableADIntegration(bool value, bool isDefault = false)
		{
			var registryHelper = new RegistryTransformationHelper();
			registryHelper.DeleteStmDataRow("ADConfig");
			if (!isDefault)
			{
				var enabledString = value ? "Y" : "N";
				var xml = $@"<ADConfig><IsADIntegrationEnabled>{enabledString}</IsADIntegrationEnabled><EntitiesToSyncCode>ALL</EntitiesToSyncCode><SyncModeCode>AD</SyncModeCode><IsSingleSignOn>N</IsSingleSignOn></ADConfig>";
				var bytes = System.Text.Encoding.Unicode.GetBytes(xml);
				registryHelper.InsertStmDataRow("ADConfig", "BIN", bytes);
			}
		}
	}
}
