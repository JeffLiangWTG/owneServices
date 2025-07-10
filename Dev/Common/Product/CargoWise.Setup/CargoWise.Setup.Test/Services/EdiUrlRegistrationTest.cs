using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

internal class EdiUrlRegistrationTest
{
	[Test]
	public void TestRegisterUrlHandlers()
	{
		var mockRegistryProxy = new Mock<IWindowsRegistryProxy>();
		var mockLogger = new Mock<ILogger<EdiUrlRegistration>>();
		var cargowiseStartPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");
		var urlRegister = new EdiUrlRegistration(mockRegistryProxy.Object, mockLogger.Object);

		urlRegister.RegisterUrlHandlers();

		VerifyRegistrySet(@"SOFTWARE\Classes\edient", "", "URL:edient Protocol");
		VerifyRegistrySet(@"SOFTWARE\Classes\edient", "URL Protocol", "");
		VerifyRegistrySet(@"SOFTWARE\Classes\edient\DefaultIcon", "", $"\"{cargowiseStartPath}\",0");
		VerifyRegistrySet(@"SOFTWARE\Classes\edient\shell\open\command", "", $"\"{cargowiseStartPath}\" \"%1\"");

		VerifyRegistrySet(@"SOFTWARE\RegisteredApplications", "Enterprise", @"Software\EdiEnterprise\Capabilities");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationName", "EdiEnterprise");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationIcon", $"{cargowiseStartPath},0");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationDescription", "EdiEnterprise");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\UrlAssociations", "edi", "EdiEnterprise.edient");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\UrlAssociations", "edient", "EdiEnterprise.edient");
		VerifyRegistrySet(@"SOFTWARE\EdiEnterprise\Capabilities\FileAssociations", ".edient", "EdiEnterprise.edient");
		VerifyRegistrySet(@"SOFTWARE\Classes\EdiEnterprise.edient", "", "URL:edient Protocol");
		VerifyRegistrySet(@"SOFTWARE\Classes\EdiEnterprise.edient", "URL Protocol", "");
		VerifyRegistrySet(@"SOFTWARE\Classes\EdiEnterprise.edient\DefaultIcon", "", $"\"{cargowiseStartPath}\",0");
		VerifyRegistrySet(@"SOFTWARE\Classes\EdiEnterprise.edient\shell\open\command", "", $"\"{cargowiseStartPath}\" \"%1\"");

		mockLogger.VerifyLog(LogLevel.Information, "Registering edient protocol handler", Times.Once());
		mockLogger.VerifyLog(LogLevel.Information, "Registering EdiEnterprise.edient protocol handler", Times.Once());
		mockLogger.VerifyLog(LogLevel.Information, "Creating default program registration for edient", Times.Once());

		void VerifyRegistrySet(string keyPath, string valueName, string value)
		{
			mockRegistryProxy.Verify(x => x.SetLocalMachineValue(RegistryView.Default, keyPath, valueName, value));
		}
	}
}
