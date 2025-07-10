using Microsoft.Win32;

namespace Enterprise.ServiceManager.Host
{
	public class WindowsRegistryAdapter : IWindowsRegistryAdapter
	{
		public int? ReadLocalMachineDwordRegistryValue(string parentKeyPath, string settingName)
		{
			using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			using (var parentKey = hklm32.OpenSubKey(parentKeyPath, false))
			{
				return parentKey?.GetValue(settingName) as int?;
			}
		}

		public void SetLocalMachineDwordRegistryValue(string parentKeyPath, string settingName, int settingValue)
		{
			using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			using (var parentKey = hklm32.OpenSubKey(parentKeyPath, true))
			{
				parentKey.SetValue(settingName, settingValue, RegistryValueKind.DWord);
			}
		}
	}
}
