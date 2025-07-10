using System;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class WindowsRegistryChecker : InstallationItem
	{
		readonly string[] registryKeys;
		readonly string pluginProductName;
		public WindowsRegistryChecker(Installation installation, string[] registryKeys, string pluginProductName)
			: base(installation)
		{
			this.registryKeys = registryKeys;
			this.pluginProductName = pluginProductName;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				return GetDisableAutoUpgradeValue()
					? InstallationResult.Error($"{pluginProductName} Auto Update is turned off. Users may not be able to use all features. If an updated feature is needed, please contact your system administrator to manually install the new client.")
					: InstallationResult.OK();
			}
			catch (Exception ex)
			{
				return InstallationResult.Error($"Failed to check Windows Registry value for {pluginProductName}. Error: {ex}");
			}
		}

		bool GetDisableAutoUpgradeValue()
		{
			foreach (var registryKey in registryKeys)
			{
				if (GetDisableAutoUpgradeValueCore(registryKey))
				{
					return true;
				}
			}

			return false;
		}

		static bool GetDisableAutoUpgradeValueCore(string registryKey)
		{
			bool regValue;
			using (var currentUserRegKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\WiseTech Global\{registryKey}", writable: false))
			{
				try
				{
					regValue = bool.Parse(currentUserRegKey?.GetValue("DisableAutoUpgrade")?.ToString());
				}
				catch (ArgumentNullException)
				{
					using (var localMachineRegKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\WiseTech Global\{registryKey}", writable: false))
					{
						regValue = bool.Parse(localMachineRegKey?.GetValue("DisableAutoUpgrade")?.ToString() ?? bool.FalseString);
					}
				}
			}

			return regValue;
		}

		protected override bool IsRemoteExclusive
		{
			get { return true; }
		}
	}
}
