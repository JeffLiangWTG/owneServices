using System.Text.RegularExpressions;
using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

internal class BrowserCompatibilityConfiguratorTest
{
	[Test]
	public void TestConfigureCompatibility()
	{
		// Arrange
		var mockRegistry = new Mock<IWindowsRegistryProxy>();
		var mockLogger = new Mock<ILogger<BrowserCompatibilityConfigurator>>();
		var component = new BrowserCompatibilityConfigurator(mockRegistry.Object, mockLogger.Object);

		// Act
		component.ConfigureCompatibilityMode();

		// Assert
		var registryPath = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
		mockRegistry.Verify(x => x.SetLocalMachineValue(RegistryView.Registry32, registryPath, It.Is<string>(s => new Regex(@"CargoWiseOneAnyCpu\.exe").IsMatch(s)), 0), Times.Once());
		mockRegistry.Verify(x => x.SetLocalMachineValue(RegistryView.Registry64, registryPath, It.Is<string>(s => new Regex(@"CargoWiseOneAnyCpu\.exe").IsMatch(s)), 0), Times.Once());
		mockRegistry.Verify(x => x.SetLocalMachineValue(RegistryView.Registry32, registryPath, It.Is<string>(s => new Regex(@"CargoWiseAnyCpu\.exe").IsMatch(s)), 0), Times.Once());
		mockRegistry.Verify(x => x.SetLocalMachineValue(RegistryView.Registry64, registryPath, It.Is<string>(s => new Regex(@"CargoWiseAnyCpu\.exe").IsMatch(s)), 0), Times.Once());
		mockLogger.VerifyLog(LogLevel.Information, "Configuring embedded browser compatibility mode.", Times.Once());
	}
}
