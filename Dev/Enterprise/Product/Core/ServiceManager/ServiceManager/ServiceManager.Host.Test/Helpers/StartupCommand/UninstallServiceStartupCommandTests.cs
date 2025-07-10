using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	[TestClass]
	public class UninstallServiceStartupCommandTests : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestExecuteWithInvalidHostReturnsNegativeResult()
		{
			// Arrange
			var exception = new InvalidOperationException("Invalid host name");
			serviceControllerManager
				.Setup(m => m.GetServiceControllers(It.IsAny<IServiceManagerHostOptions>()))
				.Throws(exception);

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(-1));
		}

		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulUninstallWithoutServicesReturnsZero()
		{
			var serviceControllers = new Dictionary<ServiceType, IServiceController>();
			CheckExecuteWithSuccessfulUninstallReturnsZero(serviceControllers, false);
		}

		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulUninstallWithServicesReturnsZero()
		{
			var serviceControllers = new Dictionary<ServiceType, IServiceController>
			{
				{ ServiceType.ProcessController, Mock.Of<IServiceController>() },
				{ ServiceType.LauncherSecurity, Mock.Of<IServiceController>() },
			};
			CheckExecuteWithSuccessfulUninstallReturnsZero(serviceControllers, false);
			foreach (var serviceController in serviceControllers.Values)
			{
				serviceControllerManager.Verify(m => m.Stop(serviceController), Times.Once);
				Mock.Get(serviceController).Verify(m => m.Dispose(), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulUninstallWithoutServicesAndDbRecordReturnsZero()
		{
			var serviceControllers = new Dictionary<ServiceType, IServiceController>();
			CheckExecuteWithSuccessfulUninstallReturnsZero(serviceControllers, true);
		}

		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulUninstallWithServicesAndDbRecordReturnsZero()
		{
			var serviceControllers = new Dictionary<ServiceType, IServiceController>
			{
				{ ServiceType.ProcessController, Mock.Of<IServiceController>() },
				{ ServiceType.LauncherSecurity, Mock.Of<IServiceController>() },
			};
			CheckExecuteWithSuccessfulUninstallReturnsZero(serviceControllers, true);
			foreach (var serviceController in serviceControllers.Values)
			{
				serviceControllerManager.Verify(m => m.Stop(serviceController), Times.Once);
				Mock.Get(serviceController).Verify(m => m.Dispose(), Times.Once);
			}
		}

		void CheckExecuteWithSuccessfulUninstallReturnsZero(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers, bool removeDbRecord)
		{
			// Arrange
			hostOptions.Setup(o => o.Host).Returns("localhost");
			hostOptions.Setup(o => o.ServerName).Returns("servername");
			hostOptions.Setup(o => o.DatabaseName).Returns("database");
			hostOptions.Setup(o => o.RemoveDbRecord).Returns(removeDbRecord);
			serviceControllerManager
				.Setup(m => m.GetServiceControllers(It.IsAny<IServiceManagerHostOptions>()))
				.Returns(serviceControllers);

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestExecuteWithFailedUninstallReturnsNegativeResult()
		{
			// Arrange
			hostOptions.Setup(o => o.Host).Returns("localhost");
			hostOptions.Setup(o => o.ServerName).Returns("servername");
			hostOptions.Setup(o => o.DatabaseName).Returns("database");

			serviceControllerManager
				.Setup(m => m.GetServiceControllers(It.IsAny<IServiceManagerHostOptions>()))
				.Returns(new Dictionary<ServiceType, IServiceController>());

			managedInstallerHelper
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws<Exception>();

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(-1));

			managedInstallerHelper.Verify(m => m.Install(It.IsAny<string[]>()), Times.Once);
		}

		UninstallServiceStartupCommand CreateStartupCommand()
		{
			return new UninstallServiceStartupCommand(hostLogger, hostOptions.Object, managedInstallerHelper.Object, serviceControllerManager.Object, productRegistration);
		}

		protected override void SetUp()
		{
			hostLogger = Mock.Of<IHostLogger>();
			hostOptions = new Mock<IServiceManagerHostOptions>();
			managedInstallerHelper = new Mock<IManagedInstallerAdapter>();
			serviceControllerManager = new Mock<IServiceControllerManager>();
			productRegistration = ObjectFactory.Get<IProductRegistration>();
		}

		IHostLogger hostLogger;
		Mock<IServiceManagerHostOptions> hostOptions;
		Mock<IManagedInstallerAdapter> managedInstallerHelper;
		Mock<IServiceControllerManager> serviceControllerManager;
		IProductRegistration productRegistration;
	}
}
