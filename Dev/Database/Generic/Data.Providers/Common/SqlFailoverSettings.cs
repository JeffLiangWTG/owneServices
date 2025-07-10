using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.Providers.Common
{
	public static class SqlFailoverSettings
	{
		public static bool ShouldSpecifyMultiSubnetFailover(string serverName)
		{
			var registryValueName = serverName.Replace('\\', '$');
			var value = windowsRegistry.GetValue(MultiSubnetFailoverRegistryKey, registryValueName, 0);

			return EnabledValue.Equals(value);
		}

		[ThreadSafe]
		static IWindowsRegistry windowsRegistry = new WindowsRegistry();

		const string MultiSubnetFailoverRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\WiseTech Global\SqlFailoverSettings";
		const int EnabledValue = 1;

#if DEBUG
		public static System.IDisposable SetWindowsRegistryForTest(IWindowsRegistry registry)
		{
			windowsRegistry = registry;
			return new WindowsRegistryResetter();
		}

		class WindowsRegistryResetter : System.IDisposable
		{
			public void Dispose()
			{
				windowsRegistry = new WindowsRegistry();
			}
		}
#endif
	}
}
