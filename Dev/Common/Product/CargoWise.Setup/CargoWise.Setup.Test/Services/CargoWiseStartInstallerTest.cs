using System.Diagnostics;
using System.Security;
using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

class CargoWiseStartInstallerTest
{
	[SetUp]
	public void Setup()
	{
		mockWindowsRegistryProxy = new Mock<IWindowsRegistryProxy>();
		mockProcessRunner = new Mock<IProcessRunner>();
		mockPathProvider = new Mock<IInstallPathProvider>();
		mockPathProvider.Setup(x => x.GetInstallPath()).Returns(installPath);
		mockPathProvider.Setup(x => x.GetCurrentVersionFilePath()).Returns(currentVersionPath);
		mockPathProvider.Setup(x => x.GetVersionInfoFromDirectory()).Returns(versionInfo);
		mockExeUpdater = new Mock<ICargoWiseStartExeUpdater>();
		mockLogger = new Mock<ILogger<CargoWiseStartInstaller>>();
		installer = new CargoWiseStartInstaller(mockWindowsRegistryProxy.Object, mockProcessRunner.Object, mockPathProvider.Object, mockExeUpdater.Object, mockLogger.Object);
	}

	const string rootInstallPath = "C:\\installPath";
	const string versionInfo = "versionNum";
	const string installPath = $"{rootInstallPath}\\{versionInfo}\\";
	const string currentVersionPath = $"{rootInstallPath}\\CurrentVersion";

	CargoWiseStartInstaller installer;
	Mock<IWindowsRegistryProxy> mockWindowsRegistryProxy;
	Mock<IProcessRunner> mockProcessRunner;
	Mock<IInstallPathProvider> mockPathProvider;
	Mock<ICargoWiseStartExeUpdater> mockExeUpdater;
	Mock<ILogger<CargoWiseStartInstaller>> mockLogger;

	[Test]
	public void TestInstallClientMsi()
	{
		// Arrange
		mockWindowsRegistryProxy.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns(null!);
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(MockProcess(true, 0));

		// Act
		installer.Install();

		// Assert
		var expectedMsiPath = $@"{installPath}CargoWiseSetup.msi";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(s => s.Arguments == "/qn" && s.CreateNoWindow && s.FileName.Equals(expectedMsiPath))), Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, Times.Never());
		mockLogger.VerifyLog(LogLevel.Information, "Installing CargoWise.Start.exe using CargoWiseSetup.msi.", Times.Once());
	}

	[Test]
	public void TestSkipsIfAlreadyInstalled()
	{
		// Arrange
		mockWindowsRegistryProxy.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns(1);
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(MockProcess(true, 0));

		// Act
		installer.Install();

		// Assert
		mockProcessRunner.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Never());
		mockExeUpdater.Verify(x => x.UpdateCargoWiseStartExe("C:\\installPath\\CurrentVersion", versionInfo));
		mockLogger.VerifyLog(LogLevel.Information, "CargoWise.Start.exe is already installed, updating file if needed.", Times.Once());
	}

	[Test]
	public void TestHandlesSecurityException()
	{
		// Arrange
		mockWindowsRegistryProxy.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Throws<SecurityException>();
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(MockProcess(true, 0));

		// Act
		installer.Install();

		// Assert
		var expectedMsiPath = $@"{installPath}CargoWiseSetup.msi";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(s => s.Arguments == "/qn" && s.CreateNoWindow && s.FileName.Equals(expectedMsiPath))), Times.Once());
		mockLogger.VerifyLog(LogLevel.Warning, $"Could not read registry 'HKEY_LOCAL_MACHINE\\SOFTWARE\\WiseTech Global\\CargoWise\\ClientInstalled', running CargoWise.Start.exe installer anyway", Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, Times.Never());
	}

	[Test]
	public void TestInstallHandlesNoLaunch()
	{
		// Arrange
		mockWindowsRegistryProxy.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns(null!);
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(MockProcess(false));

		// Act
		installer.Install();

		// Assert
		mockProcessRunner.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, "Failed to launch the CargoWise.Start.exe installer.", Times.Once());
	}

	[Test]
	public void TestInstallHandlesError()
	{
		// Arrange
		mockWindowsRegistryProxy.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns(null!);
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(MockProcess(true, 1234));

		// Act
		installer.Install();

		// Assert
		mockProcessRunner.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, "Unable to install CargoWise.Start.exe. Installer exit code: 1234", Times.Once());
	}

	IProcess MockProcess(bool present, int exitCode = -1)
	{
		var procMock = new Mock<IProcess>();
		procMock.SetupGet(x => x.Present).Returns(present);
		procMock.SetupGet(x => x.ExitCode).Returns(exitCode);
		return procMock.Object;
	}
}
