namespace Enterprise.ServiceManager.Host
{
	public interface IWindowsRegistryAdapter
	{
		void SetLocalMachineDwordRegistryValue(string parentKeyPath, string settingName, int settingValue);
		int? ReadLocalMachineDwordRegistryValue(string parentKeyPath, string settingName);
	}
}
