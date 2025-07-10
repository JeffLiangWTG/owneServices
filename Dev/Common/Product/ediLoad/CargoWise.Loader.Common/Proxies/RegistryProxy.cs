using Microsoft.Win32;

namespace CargoWise.Loader.Common
{
	sealed class RegistryProxy : IRegistryProxy
	{
		public object GetValue(string keyName, string valueName, object defaultValue)
		{
			return Registry.GetValue(keyName, valueName, defaultValue);
		}
	}
}
