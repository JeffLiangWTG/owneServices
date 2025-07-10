using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace CargoWise.Setup.Services;

internal interface IBrowserCompatibilityConfigurator
{
	void ConfigureCompatibilityMode();
}

internal class BrowserCompatibilityConfigurator(IWindowsRegistryProxy registry, ILogger<BrowserCompatibilityConfigurator> logger) : IBrowserCompatibilityConfigurator
{
	public void ConfigureCompatibilityMode()
	{
		logger.LogInformation("Configuring embedded browser compatibility mode.");

		foreach (var exeName in allExeNames)
		{
			SetWebBrowserEmulationRegistry(RegistryView.Registry32, exeName);
			SetWebBrowserEmulationRegistry(RegistryView.Registry64, exeName);
		}

		void SetWebBrowserEmulationRegistry(RegistryView view, string exeName)
		{
			registry.SetLocalMachineValue(view, RegistryName, exeName, WebBrowserEmulationValue);
		}
	}

	string RegistryName => @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
	// "Web Browser Latest Emulation" Value
	const int WebBrowserEmulationValue = 0;

	readonly List<string> allExeNames = ["CargoWiseOneAnyCpu.exe", "CargoWiseAnyCpu.exe"];
}
