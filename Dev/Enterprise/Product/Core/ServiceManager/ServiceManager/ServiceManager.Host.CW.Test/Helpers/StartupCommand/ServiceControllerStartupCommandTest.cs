using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	class ServiceControllerStartupCommandTest : TestCase
	{
		public void TestWrongConstructorParams()
		{
			// Arrange Act Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceControllerStartupCommand(null, hostOptions.Object, serviceControllerManager.Object));
			AssertEquals("hostLogger", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceControllerStartupCommand(hostLogger.Object, null, serviceControllerManager.Object));
			AssertEquals("hostOptions", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceControllerStartupCommand(hostLogger.Object, hostOptions.Object, null));
			AssertEquals("serviceControllerManager", result.ParamName);
		}

		[ExpectNoExceptions]
		public void TestExecuteWithNoAssemblyMetaDataMissingReturnsAndLogsError()
		{
			string errorMessage = null;
			hostLogger
				.Setup(x => x.Log(LogLevel.Error, It.IsAny<string>()))
				.Callback<LogLevel, string>((o, x) => errorMessage = x);

			var missingAssemblyMetaDataFile = new[] { System.IO.Path.Combine(Env.TempPath, "AssemblyMetaDataMissing.xml") };
			using (ObjectFactory.Substitute(Mock.Of<IAssemblyMetaDataReader>(a => a.AssemblyMetaDataFiles == missingAssemblyMetaDataFile)))
			{
				var command = CreateStartupCommand();

				// Act
				var result = command.Execute();

				// Assert
				AssertEquals(0x3EB, result);

				var expectedErrorMessage = "AssemblyMetaDataFiles are missing.";
				AssertEquals("Error must be logged.", expectedErrorMessage, errorMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestExecuteWithNoServiceFoundReturnsNegativeResult()
		{
			var serviceControllers = new Dictionary<ServiceType, IServiceController>();
			CheckExecuteWithNoPrcFoundReturnsNegativeResult(serviceControllers);
		}

		[ExpectNoExceptions]
		public void TestExecuteWithOnlyExtraServiceFoundReturnsNegativeResult()
		{
			var serviceController = new Mock<IServiceController>(MockBehavior.Strict);
			serviceController.Setup(x => x.Dispose());
			var serviceControllers = new Dictionary<ServiceType, IServiceController>()
			{
				{ ServiceType.LauncherSecurity, serviceController.Object },
			};
			CheckExecuteWithNoPrcFoundReturnsNegativeResult(serviceControllers);
			serviceController.Verify(x => x.Dispose(), Times.Once);
			serviceController.VerifyNoOtherCalls();
		}

		void CheckExecuteWithNoPrcFoundReturnsNegativeResult(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers)
		{
			// Arrange
			hostOptions.SetupGet(x => x.Host).Returns("prcHost.wtg.zone");
			hostOptions.SetupGet(x => x.ServerName).Returns("dbHost.wtg.zone");
			hostOptions.SetupGet(x => x.DatabaseName).Returns("database");

			serviceControllerManager
				.Setup(x => x.GetServiceControllers(It.IsAny<IServiceManagerHostOptions>()))
				.Returns(serviceControllers);

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			AssertEquals(0x424, result);
			serviceControllerManager.Verify(x => x.GetServiceControllers(hostOptions.Object), Times.Once);
		}

		public void TestExecuteWithServiceFoundCallExecuteCore()
		{
			var serviceController = new Mock<IServiceController>(MockBehavior.Strict);
			serviceController
				.SetupGet(x => x.ServiceName)
				.Returns(ServiceHostProcess.GetServiceName(ServiceType.ProcessController, "dbHost.wtg.zone", "database"));
			serviceController.Setup(x => x.Dispose());
			var serviceControllers = new Dictionary<ServiceType, IServiceController>()
			{
				{ ServiceType.ProcessController, serviceController.Object },
			};
			CheckExecuteWithServiceFoundCallExecuteCore(serviceControllers, "ediEnterpriseProcessController_dbHost.wtg.zone_database");
			serviceController.Verify(x => x.Dispose(), Times.Once);
			serviceController.VerifyGet(x => x.ServiceName, Times.AtLeastOnce);
			serviceController.VerifyNoOtherCalls();
		}

		public void TestExecuteWithServiceFoundCallExecuteCoreWithMultipleServices()
		{
			var prcService = new Mock<IServiceController>(MockBehavior.Strict);
			prcService
				.SetupGet(x => x.ServiceName)
				.Returns(ServiceHostProcess.GetServiceName(ServiceType.ProcessController, "dbHost.wtg.zone", "database"));
			prcService.Setup(x => x.Dispose());
			var secService = new Mock<IServiceController>(MockBehavior.Strict);
			secService
				.SetupGet(x => x.ServiceName)
				.Returns(ServiceHostProcess.GetServiceName(ServiceType.LauncherSecurity, "dbHost.wtg.zone", "database"));
			secService.Setup(x => x.Dispose());
			var serviceControllers = new Dictionary<ServiceType, IServiceController>()
			{
				{ ServiceType.ProcessController, prcService.Object },
				{ ServiceType.LauncherSecurity, secService.Object },
			};
			CheckExecuteWithServiceFoundCallExecuteCore(serviceControllers, "ediEnterpriseProcessController_dbHost.wtg.zone_database, ediEnterpriseLauncherSecurity_dbHost.wtg.zone_database");
			prcService.Verify(x => x.Dispose(), Times.Once);
			prcService.VerifyGet(x => x.ServiceName, Times.AtLeastOnce);
			prcService.VerifyNoOtherCalls();
			secService.Verify(x => x.Dispose(), Times.Once);
			secService.VerifyGet(x => x.ServiceName, Times.AtLeastOnce);
			secService.VerifyNoOtherCalls();
		}

		void CheckExecuteWithServiceFoundCallExecuteCore(
			IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers,
			string expectedServices)
		{
			// Arrange
			hostOptions.SetupGet(x => x.Host).Returns("prcHost.wtg.zone");
			hostOptions.SetupGet(x => x.ServerName).Returns("dbHost.wtg.zone");
			hostOptions.SetupGet(x => x.DatabaseName).Returns("database");

			serviceControllerManager
				.Setup(x => x.GetServiceControllers(It.IsAny<IServiceManagerHostOptions>()))
				.Returns(serviceControllers);

			var command = CreateStartupCommand();

			// Act
			var e = AssertExceptionThrown<NotImplementedException>(() => command.Execute());

			// Assert
			AssertEquals($"Call not implemented for unit test: {expectedServices}", e.Message);
			serviceControllerManager.Verify(x => x.GetServiceControllers(hostOptions.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestExecuteWithTimeoutExceptionReturnsNegativeResult()
		{
			// Arrange
			var prcService = new Mock<IServiceController>(MockBehavior.Strict);
			prcService
				.SetupGet(x => x.ServiceName)
				.Returns("ediEnterpriseProcessController");

			var timeoutException = new System.ServiceProcess.TimeoutException();
			timeoutException.Data.Add("waitRequest", "testOperation");
			timeoutException.Data.Add("serviceName", "testServiceName");
			hostOptions
				.SetupGet(m => m.Username)
				.Throws(timeoutException);

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			AssertEquals(1053, result);
			hostLogger.Verify(
				m => m.Log(
					LogLevel.Error,
					"The service testServiceName did not respond to the testOperation request in a timely fashion."),
				Times.Once);
		}

		TestServiceControllerStartupCommand CreateStartupCommand()
		{
			return new TestServiceControllerStartupCommand(hostLogger.Object, hostOptions.Object, serviceControllerManager.Object);
		}

		class TestServiceControllerStartupCommand : ServiceControllerStartupCommand
		{
			public TestServiceControllerStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IServiceControllerManager serviceControllerManager)
				: base(hostLogger, hostOptions, serviceControllerManager)
			{
			}

			protected override void ExecuteCore(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers)
			{
				AssertNotEquals("nameof(serviceControllers) - should not be [null]", default(IReadOnlyDictionary<ServiceType, IServiceController>), serviceControllers);
				var serviceNames = string.Join(", ", serviceControllers.Values.Select(sc => sc.ServiceName));
				throw new NotImplementedException($"Call not implemented for unit test: {serviceNames}");
			}
		}

		protected override void SetUp()
		{
			hostLogger = new Mock<IHostLogger>();
			hostOptions = new Mock<IServiceManagerHostOptions>();
			serviceControllerManager = new Mock<IServiceControllerManager>(MockBehavior.Strict);
		}

		protected override void TearDown()
		{
			serviceControllerManager.VerifyNoOtherCalls();
		}

		Mock<IHostLogger> hostLogger;
		Mock<IServiceManagerHostOptions> hostOptions;
		Mock<IServiceControllerManager> serviceControllerManager;
	}
}
