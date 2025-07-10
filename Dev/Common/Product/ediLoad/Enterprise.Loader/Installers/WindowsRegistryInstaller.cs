using System;
using System.Security;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Microsoft.Win32;

namespace Enterprise.Loader
{
	class WindowsRegistryInstaller : AppManagerInvoker, IAppManagerInvocable
	{
		public WindowsRegistryInstaller(Installation installation) : base(installation)
		{
		}

		/// <summary>
		/// Parameterless constructor is required by AppManager
		/// </summary>
		public WindowsRegistryInstaller() : base(null)
		{
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			return InvokeAppManager<WindowsRegistryInstaller>(string.Empty, null);
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			SetWebBrowserEmulationRegistry32and64();
			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		void SetWebBrowserEmulationRegistry32and64()
		{
			SetWebBrowserEmulationRegistry(RegistryView.Registry32);
			SetWebBrowserEmulationRegistry(RegistryView.Registry64);
		}

		void SetWebBrowserEmulationRegistry(RegistryView view)
		{
			try
			{
				using (var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
				{
					var subKey = rootKey.CreateSubKey(WebBrowserEmulationKeyName);
					if (subKey != null)
					{
						subKey.SetValue(ExeFileNames.CargoWiseWindowsDesktopExe, WebBrowserEmulationValue, RegistryValueKind.DWord);
					}
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (SecurityException) { }
		}

		bool HasWebBrowserEmulationRegistry(RegistryKey rootKey)
		{
			try
			{
				using (var subKey = rootKey.OpenSubKey(WebBrowserEmulationKeyName))
				{
					if (subKey != null)
					{
						var cargoWiseWindowsDesktopExe = subKey.GetValue(ExeFileNames.CargoWiseWindowsDesktopExe);

						if (cargoWiseWindowsDesktopExe is int valueAnyCpu)
						{
							return valueAnyCpu == WebBrowserEmulationValue;
						}
					}

					return false;
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (SecurityException) { }

			return true;
		}

		protected virtual string WebBrowserEmulationKeyName => @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

		const int WebBrowserEmulationValue = 0; // Web Browser Latest Emulation Value

		protected override bool NeedsToInstallCore()
		{
			return !HasWebBrowserEmulationRegistry(Registry.LocalMachine);
		}
	}
}

