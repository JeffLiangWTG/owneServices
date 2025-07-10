using System;
using System.Collections.Generic;
using Enterprise.Registry.Business;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand;

class StartServiceStartupCommandTest : TransactionedTestCase
{
	sealed class StartServiceStartupCommandForTesting : StartServiceStartupCommand
	{
		public StartServiceStartupCommandForTesting(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IServiceControllerManager serviceControllerManager)
			: base(hostLogger, hostOptions, serviceControllerManager)
		{
		}

		public void CallExecuteCore(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers, string dataProtectionMechanism)
		{
			using var temp = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dataProtectionMechanism);
			// only testing the protected method, the public method is tested in ServiceControllerStartupCommandTest
			ExecuteCore(serviceControllers);
		}
	}

	public void TestExecuteCore_SingleService_WithoutLauncherSecurity_Success()
	{
		CheckExecuteCore_SingleService(DataProtectionMechanisms.Codes.None);
	}

	public void TestExecuteCore_SingleService_WithLauncherSecurity_Success()
	{
		CheckExecuteCore_SingleService(DataProtectionMechanisms.Codes.ActiveDirectory);
	}

	void CheckExecuteCore_SingleService(string dataProtectionMechanism)
	{
		// Arrange
		var serviceControllers = new Dictionary<ServiceType, IServiceController>
		{
			{ ServiceType.ProcessController, mockPrcServiceController.Object },
		};

		// Act
		AssertNoExceptionThrown(() => startServiceStartupCommand.CallExecuteCore(serviceControllers, dataProtectionMechanism));

		// Assert
		mockHostLogger.Verify(m => m.Log(LogLevel.Information, "Services have been successfully started: ediProcessController"), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockPrcServiceController.Object), Times.Once);
		mockPrcServiceController.VerifyGet(m => m.ServiceName, Times.Once);
	}

	public void TestExecuteCore_MultipleServices_WithLauncherSecurity_Success()
	{
		// Arrange
		var serviceControllers = new Dictionary<ServiceType, IServiceController>
		{
			{ ServiceType.ProcessController, mockPrcServiceController.Object },
			{ ServiceType.LauncherSecurity, mockSecServiceController.Object },
		};

		// Act
		AssertNoExceptionThrown(() => startServiceStartupCommand.CallExecuteCore(serviceControllers, DataProtectionMechanisms.Codes.ActiveDirectory));

		// Assert
		mockHostLogger.Verify(m => m.Log(LogLevel.Information, "Services have been successfully started: ediLauncherSecurity, ediProcessController"), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockPrcServiceController.Object), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockSecServiceController.Object), Times.Once);
		mockPrcServiceController.VerifyGet(m => m.ServiceName, Times.Once);
		mockSecServiceController.VerifyGet(m => m.ServiceName, Times.Once);
	}

	public void TestExecuteCore_MultipleServices_WithoutLauncherSecurity_Success()
	{
		// Arrange
		var serviceControllers = new Dictionary<ServiceType, IServiceController>
		{
			{ ServiceType.ProcessController, mockPrcServiceController.Object },
			{ ServiceType.LauncherSecurity, mockSecServiceController.Object },
		};

		// Act
		AssertNoExceptionThrown(() => startServiceStartupCommand.CallExecuteCore(serviceControllers, DataProtectionMechanisms.Codes.None));

		// Assert
		mockHostLogger.Verify(m => m.Log(LogLevel.Information, "Services have been successfully started: ediProcessController"), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockPrcServiceController.Object), Times.Once);
		mockPrcServiceController.VerifyGet(m => m.ServiceName, Times.Once);
	}

	public void TestExecuteCore_SecFailure_Exception()
	{
		// Arrange
		var exception = new InvalidOperationException("Exception for unit test");
		mockServiceControllerManager
			.Setup(m => m.Start(It.IsAny<IServiceController>()))
			.Throws(exception);
		var serviceControllers = new Dictionary<ServiceType, IServiceController>
		{
			{ ServiceType.ProcessController, mockPrcServiceController.Object },
			{ ServiceType.LauncherSecurity, mockSecServiceController.Object },
		};

		// Act
		var e = AssertExceptionThrown<InvalidOperationException>(() => startServiceStartupCommand.CallExecuteCore(serviceControllers, DataProtectionMechanisms.Codes.ActiveDirectory));

		// Assert
		NUnit.Framework.Assert.That(e, NUnit.Framework.Is.EqualTo(exception));
		mockServiceControllerManager.Verify(m => m.Start(mockSecServiceController.Object), Times.Once);
	}

	public void TestExecuteCore_PrcFailure_Exception()
	{
		// Arrange
		var exception = new InvalidOperationException("Exception for unit test");
		mockServiceControllerManager
			.SetupSequence(m => m.Start(It.IsAny<IServiceController>()))
			.Pass()
			.Throws(exception);
		var serviceControllers = new Dictionary<ServiceType, IServiceController>
		{
			{ ServiceType.ProcessController, mockPrcServiceController.Object },
			{ ServiceType.LauncherSecurity, mockSecServiceController.Object },
		};

		// Act
		var e = AssertExceptionThrown<InvalidOperationException>(() => startServiceStartupCommand.CallExecuteCore(serviceControllers, DataProtectionMechanisms.Codes.ActiveDirectory));

		// Assert
		NUnit.Framework.Assert.That(e, NUnit.Framework.Is.EqualTo(exception));
		mockSecServiceController.VerifyGet(m => m.ServiceName, Times.Once);
		mockHostLogger.Verify(m => m.Log(LogLevel.Information, "Services have been successfully started: ediLauncherSecurity"), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockSecServiceController.Object), Times.Once);
		mockServiceControllerManager.Verify(m => m.Start(mockPrcServiceController.Object), Times.Once);
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockServiceControllerManager = new Mock<IServiceControllerManager>(MockBehavior.Strict);
		mockServiceControllerManager.Setup(m => m.Start(It.IsAny<IServiceController>()));
		mockHostLogger = new Mock<IHostLogger>(MockBehavior.Strict);
		mockHostLogger.Setup(m => m.Log(It.IsAny<LogLevel>(), It.IsAny<string>()));
		mockHostOptions = new Mock<IServiceManagerHostOptions>(MockBehavior.Strict);
		mockPrcServiceController = new Mock<IServiceController>(MockBehavior.Strict);
		mockPrcServiceController.SetupGet(m => m.ServiceName).Returns("ediProcessController");
		mockSecServiceController = new Mock<IServiceController>(MockBehavior.Strict);
		mockSecServiceController.SetupGet(m => m.ServiceName).Returns("ediLauncherSecurity");
		startServiceStartupCommand = new StartServiceStartupCommandForTesting(mockHostLogger.Object, mockHostOptions.Object, mockServiceControllerManager.Object);
	}

	protected override void TearDown()
	{
		mockServiceControllerManager.VerifyNoOtherCalls();
		mockHostLogger.VerifyNoOtherCalls();
		mockHostOptions.VerifyNoOtherCalls();
		mockPrcServiceController.VerifyNoOtherCalls();
		mockSecServiceController.VerifyNoOtherCalls();
		base.TearDown();
	}

	Mock<IServiceControllerManager> mockServiceControllerManager;
	Mock<IHostLogger> mockHostLogger;
	Mock<IServiceManagerHostOptions> mockHostOptions;
	Mock<IServiceController> mockPrcServiceController;
	Mock<IServiceController> mockSecServiceController;
	StartServiceStartupCommandForTesting startServiceStartupCommand;
}
