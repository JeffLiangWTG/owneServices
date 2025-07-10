using CargoWise.ApplicationManager.Common;

namespace Enterprise.Client.Common
{
	public interface ICurrentVersionCleanerConfigManager
	{
		ICurrentVersionCleanerConfigWithLogs LoadConfiguration(string baseInstallationPath);
		AppManagerResult SaveConfigurationViaAppManager(string baseInstallationPath, ICurrentVersionCleanerConfigWithLogs config);
	}
}
