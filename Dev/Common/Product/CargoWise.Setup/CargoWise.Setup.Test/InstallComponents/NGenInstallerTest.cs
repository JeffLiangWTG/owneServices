using System.Diagnostics;
using CargoWise.Setup.InstallComponents;
using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.InstallComponents;

[Property("DAT:CapabilityRequirements", "ADMIN")]
internal class NGenInstallerTest : InstallComponentTestBase
{
	public NGenInstallerTest()
	{
		Setup();
	}

	[SetUp]
	public void Setup()
	{
		mockLogger = new Mock<ILogger<NGenInstaller>>();
		mockPathProvider = new Mock<IInstallPathProvider>();
		mockPathProvider.Setup(x => x.GetInstallPath()).Returns(installPathForTest);
		mockProcessRunner = new Mock<IProcessRunner>();
	}

	protected override IInstallationComponent CreateComponentForIdempotencyTest()
	{
		var realProcessRunner = new ProcessRunnerProxy();
		var realInstallPathProvider = new InstallPathProvider();
		return new NGenInstaller(realInstallPathProvider, GetRegistryProxy(false, null), realProcessRunner, mockLogger.Object);
	}

	protected override void AssertSuccess()
	{
		mockLogger.VerifyLog(LogLevel.Error, Times.Never());
	}

	readonly string installPathForTest = "C:\\InstallPath\\1.2.3.4";
	Mock<ILogger<NGenInstaller>> mockLogger;
	Mock<IProcessRunner> mockProcessRunner;
	Mock<IInstallPathProvider> mockPathProvider;

	[Test]
	public void TestInstallStartsProcessWithCorrectArguments()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(true, 0));
		var expectedNgenRootDll = $"{installPathForTest}\\CargoWise.NGenRoot.dll";

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(false, null), mockProcessRunner.Object, mockLogger.Object).Install(null!, CancellationToken.None);

		// Assert
		var expectedArgs = $"Install \"{expectedNgenRootDll}\" \"\"";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(y => y.Arguments == expectedArgs && y.FileName == $"{installPathForTest}\\CargoWise.NGenInstaller.exe")), Times.Once());
	}

	[Test]
	public void TestInstallStartsProcessWithCorrectArgumentWithTimeout()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(true, 0));
		var expectedNgenRootDll = $"{installPathForTest}\\CargoWise.NGenRoot.dll";

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(false, 100), mockProcessRunner.Object, mockLogger.Object).Install(null!, CancellationToken.None);

		// Assert
		var expectedArgs = $"Install \"{expectedNgenRootDll}\" \"100\"";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(y => y.Arguments == expectedArgs && y.FileName == $"{installPathForTest}\\CargoWise.NGenInstaller.exe")), Times.Once());
	}

	[Test]
	public void TestDoesNothingWhenDisabledAndInstalling()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(true, 0));

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(true, null), mockProcessRunner.Object, mockLogger.Object).Install(null!, CancellationToken.None);

		// Assert
		mockProcessRunner.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Never);
	}

	[Test]
	public void TestRunsProcessWhenDisabledAndUninstalling()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(true, 0));
		var expectedNgenRootDll = $"{installPathForTest}\\CargoWise.NGenRoot.dll";

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(true, null), mockProcessRunner.Object, mockLogger.Object).Remove(null!, CancellationToken.None);

		// Assert
		var expectedArgs = $"Uninstall \"{expectedNgenRootDll}\" \"\"";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(y => y.Arguments == expectedArgs && y.FileName == $"{installPathForTest}\\CargoWise.NGenInstaller.exe")), Times.Once());
	}

	[Test]
	public void TestLogsErrorWhenProcessNotStarted()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(false, 0));
		var expectedNgenRootDll = $"{installPathForTest}\\CargoWise.NGenRoot.dll";

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(false, null), mockProcessRunner.Object, mockLogger.Object).Install(null!, CancellationToken.None);

		// Assert
		var expectedArgs = $"Install \"{expectedNgenRootDll}\" \"\"";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(y => y.Arguments == expectedArgs && y.FileName == $"{installPathForTest}\\CargoWise.NGenInstaller.exe")), Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, $"Failed to start the NGenInstaller process at '{installPathForTest}\\CargoWise.NGenInstaller.exe'", Times.Once());
	}

	[Test]
	public void TestLogsErrorWhenProcessExitsWithNonZeroCode()
	{
		// Arrange
		mockProcessRunner.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns(TestProcessHelper.MockProcess(true, -123));
		var expectedNgenRootDll = $"{installPathForTest}\\CargoWise.NGenRoot.dll";

		// Act
		new NGenInstaller(mockPathProvider.Object, GetRegistryProxy(false, null), mockProcessRunner.Object, mockLogger.Object).Install(null!, CancellationToken.None);

		// Assert
		var expectedArgs = $"Install \"{expectedNgenRootDll}\" \"\"";
		mockProcessRunner.Verify(x => x.Start(It.Is<ProcessStartInfo>(y => y.Arguments == expectedArgs && y.FileName == $"{installPathForTest}\\CargoWise.NGenInstaller.exe")), Times.Once());
		mockLogger.VerifyLog(LogLevel.Error, $"NGenInstaller process exited with error code -123", Times.Once());
	}

	IWindowsRegistryProxy GetRegistryProxy(bool nongen, int? timeout)
	{
		var mock = new Mock<IWindowsRegistryProxy>();
		mock.Setup(x => x.GetValue(It.IsAny<string>(), "nongen", It.IsAny<object>())).Returns(nongen ? 1 : null);
		mock.Setup(x => x.GetValue(It.IsAny<string>(), "NGen_TimeoutSecondsPerExecution", It.IsAny<object>())).Returns(timeout);
		return mock.Object;
	}
}
