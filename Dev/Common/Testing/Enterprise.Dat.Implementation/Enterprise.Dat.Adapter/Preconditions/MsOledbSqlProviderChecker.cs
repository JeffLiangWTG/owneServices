using Microsoft.Win32;

namespace Enterprise.Dat.Implementation.Preconditions
{
	static class MsOledbSqlProviderChecker
	{
		public static bool IsMsOledbSqlProviderInstalled()
		{
			bool result = false;

			using (RegistryKey localMachineRoot = Registry.LocalMachine)
			using (RegistryKey providerKey = localMachineRoot.OpenSubKey(@"SOFTWARE\Microsoft\MSOLEDBSQL", false))
			{
				if (providerKey != null)
				{
					var installedVersion = providerKey.GetValue("InstalledVersion").ToString();
					result = !string.IsNullOrWhiteSpace(installedVersion);
				}
			}

			return result;
		}
	}
}
