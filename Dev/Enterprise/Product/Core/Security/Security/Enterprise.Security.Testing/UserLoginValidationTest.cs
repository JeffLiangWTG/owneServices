using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Security.Testing
{
	sealed class UserLoginValidationTest : TestCaseWithFactory
	{
		Mock<IUser> userMock;
		Mock<TerminalService> terminalServiceMock;
		Mock<IProductRegistration> productRegistrationMock;
		Mock<IWiseCloudSecurityClient> wiseCloudSecurityClientMock;
		DisposableAction disposables;
		DisposableAction envProxyDisposables;
		protected override void SetUp()
		{
			base.SetUp();
			userMock = new Mock<IUser>();
			userMock.Setup(x => x.IsOperational)
				.Returns(true);
			userMock.Setup(x => x.IsSupportUser)
				.Returns(false);
			terminalServiceMock = new Mock<TerminalService>();
			terminalServiceMock.Setup(x => x.IsRemoteAppSession)
				.Returns(true);
			productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(x => x.Key)
				.Returns(Mock.Of<IProductRegistrationKey>());
			wiseCloudSecurityClientMock = new Mock<IWiseCloudSecurityClient>();

			disposables = GetDisposableAction();
			envProxyDisposables = ConfigureEnvProxy();
		}

		protected override void TearDown()
		{
			disposables.Dispose();
			envProxyDisposables.Dispose();

			disposables = null;
			envProxyDisposables = null;

			base.TearDown();
		}

		DisposableAction GetDisposableAction()
		{
			var clientIPAddressRestriction = RawDataRegistry.Instance.ClientIPAddressRestriction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0.0.0.0;0.0.0.0");
			var terminalService = ObjectFactory.Substitute(terminalServiceMock.Object);
			var productRegistration = ObjectFactory.Substitute(productRegistrationMock.Object);
			var wiseCloudSecurityClient = ObjectFactory.Substitute(wiseCloudSecurityClientMock.Object);
			return new DisposableAction(() =>
			{
				clientIPAddressRestriction?.Dispose();
				terminalService?.Dispose();
				productRegistration?.Dispose();
				wiseCloudSecurityClient?.Dispose();
			});
		}

		DisposableAction ConfigureEnvProxy()
		{
			var original = EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal;
			return new DisposableAction(
				() => EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = new[] { EnterpriseChannelMessageTypes.GetClientIPAddress },
				() => EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = original);
		}

		public void TestCheckIPAddressDoesNotHandleUnexpectedExceptions()
		{
			// Arrange
			var unexpectedException = new ArgumentException("An unexpected error occurred."); // Use an exception not specifically handled
			wiseCloudSecurityClientMock
				.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(unexpectedException);

			// Act & Assert
			var exception = AssertExceptionThrown<ArgumentException>(() => UserLoginValidation.CheckIPAddressRestriction(userMock.Object));
			AssertEquals(unexpectedException, exception);
		}

		public void TestCheckIPAddressRestrictionShouldHandleOperationCanceledExceptionWhenItsInnerExceptionIsRDPRemoteException()
		{
			//Arrange
			wiseCloudSecurityClientMock
				.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new OperationCanceledException(TerminalService.ClientExceptionMessage, new RDPRemoteException(new Exception("Inner Exception"))));

			//Act & Assert
			AssertNoExceptionThrown(() => UserLoginValidation.CheckIPAddressRestriction(userMock.Object));
		}

		public void TestCheckIPAddressRestrictionWhenFormatExceptionIsThrown()
		{
			// Arrange
			var exceptionMessage = "An invalid IP address was specified.";
			wiseCloudSecurityClientMock
				.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Returns("Invalid IP Address format.");

			// Act
			var result = UserLoginValidation.CheckIPAddressRestriction(userMock.Object);

			// Assert
			AssertEquals($"IP address format was invalid: {exceptionMessage}", result);
		}

		public void TestCheckIPAddressRestrictionWhenIOExceptionIsThrown()
		{
			// Arrange
			var exceptionMessage = "WiseCloud Client installation path not found.";
			wiseCloudSecurityClientMock
				.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new IOException(exceptionMessage));

			// Act
			var result = UserLoginValidation.CheckIPAddressRestriction(userMock.Object);

			// Assert
			AssertEquals($"Failed to find path: {exceptionMessage}", result);
		}

		public void TestIPAddressRestrictionWhenRDPRemoteExceptionIsThrownAsInnerException()
		{
			// Arrange
			var innerExceptionMessage = "An error occurred while processing the Remote Desktop request.";
			var outerExceptionMessage = $"Operation was cancelled due to: {innerExceptionMessage}";
			wiseCloudSecurityClientMock
				.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new OperationCanceledException(outerExceptionMessage, new RDPRemoteException("RDP Error", innerExceptionMessage)));

			// Act
			var result = UserLoginValidation.CheckIPAddressRestriction(userMock.Object);

			// Assert
			AssertEquals($"An error occurred while verifying your location due to a Remote Desktop issue: {innerExceptionMessage}", result);
		}
	}
}
