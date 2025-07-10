using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	public class InstallServiceStartupCommandTests : TestCase
	{
		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulInstallationReturnsZero()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestExecuteWithFailedInstallationReturnsNegativeResult()
		{
			// Arrange
			managedInstallerHelperMock
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws<Exception>();

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(-1));
		}

		InstallServiceStartupCommand CreateStartupCommand()
		{
			return new InstallServiceStartupCommand(hostLoggerMock, hostOptionsMock.Object, managedInstallerHelperMock.Object, productRegistration);
		}

		protected override void SetUp()
		{
			hostLoggerMock = Mock.Of<IHostLogger>();
			hostOptionsMock = new Mock<IServiceManagerHostOptions>();
			managedInstallerHelperMock = new Mock<IManagedInstallerAdapter>();
			productRegistration = ObjectFactory.Get<IProductRegistration>();
		}

		IHostLogger hostLoggerMock;
		Mock<IServiceManagerHostOptions> hostOptionsMock;
		Mock<IManagedInstallerAdapter> managedInstallerHelperMock;
		IProductRegistration productRegistration;
	}
}
