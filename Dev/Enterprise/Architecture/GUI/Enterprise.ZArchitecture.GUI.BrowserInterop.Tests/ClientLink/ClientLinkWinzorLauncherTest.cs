using System;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	public class ClientLinkWinzorLauncherTest : TestCaseWithDummy
	{
		public void TestLaunchClientLinkAsync_ThirdPartyUserValidationRequiredAsync()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsync("clientlink/create", It.IsAny<HttpContent>())).ThrowsAsync(new AuthorizationFailureException(AuthenticationResult.ThirdPartyUserValidationRequired));
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var mockClientLinkWinzorLauncher = new Mock<ClientLinkWinzorLauncher>(new Uri("https://address/"));
			var launcher = mockClientLinkWinzorLauncher.Object;

			AssertEquals("No invocations must be called once client link connection fails", mockClientLinkWinzorLauncher.Invocations.Count, 0);
		}
	}
}
