using Microsoft.Win32;

namespace CargoWise.Data.Providers.Common
{
	public class WindowsRegistry : IWindowsRegistry
	{
		public object GetValue(string keyName, string valueName, object defaultValue)
		{
			return Registry.GetValue(keyName, valueName, defaultValue);
		}
	}
}
