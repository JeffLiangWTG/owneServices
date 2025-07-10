using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
	internal static class RegistryHelper
	{
		public static string GetCurrentlySelectedConfigForWindowsService(RegistryKey registryRoot, string topName)
		{
			List<string> subKeyNames;

			using (var topKey = registryRoot.OpenSubKey(topName))
			{
				if (topKey == null)
				{
					return string.Empty;
				}

				subKeyNames = topKey.GetSubKeyNames().Where(name => name.StartsWith(Constants.RegistryManager.WebPrintKeyName, StringComparison.Ordinal)).ToList();
				if (subKeyNames.Count == 1 && subKeyNames.First() == Constants.RegistryManager.WebPrintKeyName)
				{
					return Constants.RegistryManager.WebPrintKeyName;
				}
			}

			var fallbackReturnValue = string.Empty;

			foreach (var keyName in subKeyNames)
			{
				using (var key = registryRoot.OpenSubKey(topName + @"\" + keyName))
				{
					var configSelectedForServiceValue = key?.GetValue(WindowsServiceConfiguration, false);
					if (configSelectedForServiceValue != null && Convert.ToBoolean(configSelectedForServiceValue, CultureInfo.InvariantCulture))
					{
						if (keyName == Constants.RegistryManager.WebPrintKeyName)
						{
							fallbackReturnValue = Constants.RegistryManager.WebPrintKeyName;
						}
						else
						{
							return GetConfigurationShortName(keyName);
						}
					}
				}
			}

			return fallbackReturnValue;
		}

		public static string GetConfigurationShortName(string fullName)
		{
			var length = Constants.RegistryManager.WebPrintKeyName.Length + 1;
			return fullName.Substring(length, fullName.Length - length);
		}

		public const string WindowsServiceConfiguration = "WindowsServiceConfigurationSelected";

		public static RegistryKey DefaultRegistryRoot => Registry.LocalMachine;
	}
}
