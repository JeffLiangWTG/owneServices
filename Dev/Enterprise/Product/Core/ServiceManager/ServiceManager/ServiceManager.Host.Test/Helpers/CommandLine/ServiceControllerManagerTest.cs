using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using WTG.NUnit;
using Range = Moq.Range;
using ServiceControllerStatus = System.ServiceProcess.ServiceControllerStatus;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.CommandLine;

class ServiceControllerManagerTest
{
	sealed class ServiceControllerMocks : IDisposable
	{
		public Mock<IServiceController> ProcessControllerForServer { get; } = new(MockBehavior.Strict);
		public Mock<IServiceController> ProcessControllerForLocalhost { get; } = new(MockBehavior.Strict);
		public Mock<IServiceController> ProcessControllerForOtherDb { get; } = new(MockBehavior.Strict);
		public Mock<IServiceController> LauncherSecurityForServer { get; } = new(MockBehavior.Strict);
		public Mock<IServiceController> LauncherSecurityForLocalhost { get; } = new(MockBehavior.Strict);
		public Mock<IServiceControllerFactory> ServiceControllerFactory { get; } = new(MockBehavior.Strict);
		Mock<IServiceManagerHostOptions> HostOptions { get; } = new(MockBehavior.Strict);
		Mock<IProcessFactory> ProcessFactory { get; } = new(MockBehavior.Strict);

		public ServiceControllerMocks(string hostname, string serverName, string databaseName)
		{
			ProcessControllerForServer
				.Setup(m => m.ServiceName)
				.Returns($"ediEnterpriseProcessController_{serverName}_{databaseName}");
			ProcessControllerForLocalhost
				.Setup(m => m.ServiceName)
				.Returns($"ediEnterpriseProcessController_localhost_{databaseName}");
			ProcessControllerForOtherDb
				.Setup(m => m.ServiceName)
				.Returns("ediEnterpriseProcessController_{serverName}_otherDatabase");
			LauncherSecurityForServer
				.Setup(m => m.ServiceName)
				.Returns($"ediEnterpriseLauncherSecurity_{serverName}_{databaseName}");
			LauncherSecurityForLocalhost
				.Setup(m => m.ServiceName)
				.Returns($"ediEnterpriseLauncherSecurity_localhost_{databaseName}");

			HostOptions.SetupGet(o => o.Host).Returns(hostname);
			HostOptions.SetupGet(o => o.ServerName).Returns(serverName);
			HostOptions.SetupGet(o => o.DatabaseName).Returns(databaseName);
		}

		public void Dispose()
		{
			if (Marshal.GetExceptionPointers() != IntPtr.Zero)
			{	// avoid mock exception masking other test failures
				return;
			}
			ServiceControllerFactory.VerifyNoOtherCalls();
			ProcessControllerForServer.VerifyNoOtherCalls();
			ProcessControllerForLocalhost.VerifyNoOtherCalls();
			ProcessControllerForOtherDb.VerifyNoOtherCalls();
			LauncherSecurityForServer.VerifyNoOtherCalls();
			LauncherSecurityForLocalhost.VerifyNoOtherCalls();
			ProcessFactory.VerifyNoOtherCalls();
		}

		public IReadOnlyDictionary<ServiceType, IServiceController> CallGetServiceControllers()
		{
			var serviceControllerManager = new ServiceControllerManager(ProcessFactory.Object, ServiceControllerFactory.Object);
			return serviceControllerManager.GetServiceControllers(HostOptions.Object);
		}
	}

	[Test]
	public void TestGetServiceControllers_WhenFoundByServerName_ReturnsServiceControllers()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myServerName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.LauncherSecurityForLocalhost.Object,
				mocks.ProcessControllerForServer.Object,
				mocks.ProcessControllerForLocalhost.Object,
				mocks.ProcessControllerForOtherDb.Object,
				mocks.LauncherSecurityForServer.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(new KeyValuePair<ServiceType, IServiceController>[]
			{
				new(ServiceType.ProcessController, mocks.ProcessControllerForServer.Object),
				new(ServiceType.LauncherSecurity, mocks.LauncherSecurityForServer.Object)
			}).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForServer.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ProcessControllerForLocalhost.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.LauncherSecurityForServer.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.LauncherSecurityForLocalhost.VerifyGet(m => m.ServiceName, Times.Once);
	}

	[Test]
	public void TestGetServiceControllers_WhenFoundByServerNameAndSameHost_ReturnsServiceControllers()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myHostName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForServer.Object,
				mocks.ProcessControllerForLocalhost.Object,
				mocks.ProcessControllerForOtherDb.Object,
				mocks.LauncherSecurityForServer.Object,
				mocks.LauncherSecurityForLocalhost.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(new KeyValuePair<ServiceType, IServiceController>[]
			{
				new(ServiceType.ProcessController, mocks.ProcessControllerForServer.Object),
				new(ServiceType.LauncherSecurity, mocks.LauncherSecurityForServer.Object)
			}).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForServer.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.ProcessControllerForLocalhost.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.LauncherSecurityForServer.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.LauncherSecurityForLocalhost.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
	}

	[Test]
	public void TestGetServiceControllers_WhenNotFoundByServerNameAndNotSameHost_ReturnsEmptyDictionary()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myServerName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForLocalhost.Object,
				mocks.ProcessControllerForOtherDb.Object,
				mocks.LauncherSecurityForLocalhost.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(Array.Empty<KeyValuePair<ServiceType, IServiceController>>()).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForLocalhost.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.LauncherSecurityForLocalhost.VerifyGet(m => m.ServiceName, Times.Once);
	}

	[Test]
	public void TestGetServiceControllers_WhenNotFoundByServerNameAndSameHostAndFoundByLocalhost_ReturnsServiceControllers()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myHostName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForLocalhost.Object,
				mocks.ProcessControllerForOtherDb.Object,
				mocks.LauncherSecurityForLocalhost.Object
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(new KeyValuePair<ServiceType, IServiceController>[]
			{
				new(ServiceType.ProcessController, mocks.ProcessControllerForLocalhost.Object),
				new(ServiceType.LauncherSecurity, mocks.LauncherSecurityForLocalhost.Object)
			}).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForLocalhost.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
		mocks.LauncherSecurityForLocalhost.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
	}

	[Test]
	public void TestGetServiceControllers_WhenNotFoundByServerNameAndSameHostAndNotFoundByLocalhost_ReturnsEmptyDictionary()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myHostName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForOtherDb.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(Array.Empty<KeyValuePair<ServiceType, IServiceController>>()).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Between(1, 2, Range.Inclusive));
	}

	[Test]
	public void TestGetServiceControllers_WhenNoServiceFound_ReturnsEmptyDictionary()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myServerName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(f => f.GetServices(It.IsAny<string>()))
			.Returns(Array.Empty<IServiceController>());

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(Array.Empty<KeyValuePair<ServiceType, IServiceController>>()).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
	}

	[Test]
	public void TestGetServiceControllers_WhenOnlyProcessControllerFound_ReturnsServiceControllers()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myServerName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForServer.Object,
				mocks.ProcessControllerForOtherDb.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(new KeyValuePair<ServiceType, IServiceController>[]
			{
				new(ServiceType.ProcessController, mocks.ProcessControllerForServer.Object),
			}).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ProcessControllerForServer.VerifyGet(m => m.ServiceName, Times.Once);
	}

	[Test]
	public void TestGetServiceControllers_WhenOnlyLauncherSecurityFound_ReturnsServiceControllers()
	{
		// Arrange
		using var mocks = new ServiceControllerMocks("myHostName", "myServerName", "myDatabaseName");
		mocks.ServiceControllerFactory
			.Setup(m => m.GetServices(It.IsAny<string>()))
			.Returns(new[]
			{
				mocks.ProcessControllerForOtherDb.Object,
				mocks.LauncherSecurityForServer.Object,
			});

		// Act
		var serviceControllerDict = mocks.CallGetServiceControllers();

		// Assert
		Assert.That(serviceControllerDict, Is.EquivalentTo(new KeyValuePair<ServiceType, IServiceController>[]
			{
				new(ServiceType.LauncherSecurity, mocks.LauncherSecurityForServer.Object),
			}).Using(CustomComparers.TypeComparison));
		mocks.ServiceControllerFactory.Verify(m => m.GetServices("myHostName"), Times.Once);
		mocks.ProcessControllerForOtherDb.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.LauncherSecurityForServer.VerifyGet(m => m.ServiceName, Times.Once);
	}

	[Test]
	public void TestStart_WhenServiceIsRunning_DoesNotStartService()
	{
		// Arrange
		using var mocks = new StartStopMocks("SomeService", ServiceControllerStatus.Running);

		// Act
		Assert.DoesNotThrow(mocks.CallStart);

		// Assert
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
	}

	[Test]
	public void TestStart_WhenServiceIsStopped_StartsService()
	{
		// Arrange
		var capturedStartInfos = new List<ProcessStartInfo>();
		var recoveryActionProcess = new Mock<IProcess>(MockBehavior.Strict);
		recoveryActionProcess.Setup(m => m.Start()).Returns(true);
		using var mocks = new StartStopMocks("ediEnterpriseProcessController", ServiceControllerStatus.Stopped);
		mocks.ProcessFactory
			.Setup(m => m.Create(Capture.In(capturedStartInfos), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
			.Returns(recoveryActionProcess.Object);
		mocks.ServiceController.Setup(m => m.Start());
		mocks.ServiceController.Setup(m => m.WaitForStatus(It.IsAny<ServiceControllerStatus>(), It.IsAny<TimeSpan>()));

		// Act
		mocks.CallStart();

		// Assert
		mocks.ServiceController.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
		mocks.ServiceController.Verify(m => m.Start(), Times.Once);
		mocks.ServiceController.Verify(m => m.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2)), Times.Once);
		mocks.ProcessFactory.Verify(m => m.Create(It.IsAny<ProcessStartInfo>(), false, ProcessPriorityClass.Normal), Times.Once);
		var startInfo = capturedStartInfos.Single();
		Assert.That(startInfo.FileName, Is.EqualTo("sc"));
		Assert.That(startInfo.Arguments, Is.EqualTo("failure ediEnterpriseProcessController reset= 0 actions= restart/180000/restart/180000/restart/180000"));
		recoveryActionProcess.Verify(m => m.Start(), Times.Once);
		recoveryActionProcess.VerifyNoOtherCalls();
	}

	[Test]
	public void TestStart_WhenServiceFailsToStart_ThrowsTimeoutException()
	{
		// Arrange
		using var mocks = new StartStopMocks("ediEnterpriseProcessController", ServiceControllerStatus.Stopped);
		mocks.ServiceController.Setup(m => m.Start());
		mocks.ServiceController.Setup(m => m.WaitForStatus(It.IsAny<ServiceControllerStatus>(), It.IsAny<TimeSpan>()))
			.Throws(new TimeoutException("Failure for unit test"));

		// Act
		var ex = Assert.Throws<TimeoutException>(mocks.CallStart);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(ex.Message, Is.EqualTo("Failure for unit test"));
			Assert.That(ex.Data.Keys, Is.EquivalentTo(new[] { "waitRequest", "serviceName", }).Using(CustomComparers.TypeComparison));
			Assert.That(ex.Data["waitRequest"], Is.EqualTo("start").Using(CustomComparers.TypeComparison));
			Assert.That(ex.Data["serviceName"], Is.EqualTo("ediEnterpriseProcessController").Using(CustomComparers.TypeComparison));
		});
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
		mocks.ServiceController.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ServiceController.Verify(m => m.Start(), Times.Once);
		mocks.ServiceController.Verify(m => m.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2)), Times.Once);
	}

	[Test]
	public void TestStop_WhenServiceIsStopped_DoesNotStopService()
	{
		// Arrange
		using var mocks = new StartStopMocks("SomeService", ServiceControllerStatus.Stopped);

		// Act
		Assert.DoesNotThrow(mocks.CallStop);

		// Assert
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
	}

	[Test]
	public void TestStop_WhenServiceIsRunning_StopsService()
	{
		// Arrange
		using var mocks = new StartStopMocks("SomeService", ServiceControllerStatus.Running);
		mocks.ServiceController.Setup(m => m.Stop());
		mocks.ServiceController.Setup(m => m.WaitForStatus(It.IsAny<ServiceControllerStatus>(), It.IsAny<TimeSpan>()));

		// Act
		Assert.DoesNotThrow(mocks.CallStop);

		// Assert
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
		mocks.ServiceController.Verify(m => m.Stop(), Times.Once);
		mocks.ServiceController.Verify(m => m.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2)), Times.Once);
	}

	[Test]
	public void TestStop_WhenServiceFailsToStop_ThrowsTimeoutException()
	{
		// Arrange
		using var mocks = new StartStopMocks("ediLauncherSecurity", ServiceControllerStatus.Running);
		mocks.ServiceController.Setup(m => m.Stop());
		mocks.ServiceController.Setup(m => m.WaitForStatus(It.IsAny<ServiceControllerStatus>(), It.IsAny<TimeSpan>()))
			.Throws(new TimeoutException("Failure for unit test"));

		// Act
		var ex = Assert.Throws<TimeoutException>(mocks.CallStop);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(ex.Message, Is.EqualTo("Failure for unit test"));
			Assert.That(ex.Data.Keys, Is.EquivalentTo(new[] { "waitRequest", "serviceName", }).Using(CustomComparers.TypeComparison));
			Assert.That(ex.Data["waitRequest"], Is.EqualTo("stop").Using(CustomComparers.TypeComparison));
			Assert.That(ex.Data["serviceName"], Is.EqualTo("ediLauncherSecurity").Using(CustomComparers.TypeComparison));
		});
		mocks.ServiceController.VerifyGet(m => m.Status, Times.Once);
		mocks.ServiceController.VerifyGet(m => m.ServiceName, Times.Once);
		mocks.ServiceController.Verify(m => m.Stop(), Times.Once);
		mocks.ServiceController.Verify(m => m.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2)), Times.Once);
	}

	sealed class StartStopMocks : IDisposable
	{
		public Mock<IServiceController> ServiceController { get; } = new(MockBehavior.Strict);
		public Mock<IProcessFactory> ProcessFactory { get; } = new(MockBehavior.Strict);
		Mock<IServiceControllerFactory> ServiceControllerFactory { get; } = new(MockBehavior.Strict);

		public StartStopMocks(string serviceName, ServiceControllerStatus status)
		{
			ServiceController.SetupGet(m => m.ServiceName).Returns(serviceName);
			ServiceController.SetupGet(m => m.Status).Returns(status);
		}

		public void Dispose()
		{
			ServiceController.VerifyNoOtherCalls();
			ServiceControllerFactory.VerifyNoOtherCalls();
			ProcessFactory.VerifyNoOtherCalls();
		}

		public void CallStart()
		{
			var serviceControllerManager = new ServiceControllerManager(ProcessFactory.Object, ServiceControllerFactory.Object);
			serviceControllerManager.Start(ServiceController.Object);
		}

		public void CallStop()
		{
			var serviceControllerManager = new ServiceControllerManager(ProcessFactory.Object, ServiceControllerFactory.Object);
			serviceControllerManager.Stop(ServiceController.Object);
		}
	}
}
