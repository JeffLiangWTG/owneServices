using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Moq;
using Newtonsoft.Json.Linq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	public class ClientLinkWindowLauncherTest : TestCaseWithFactory
	{
		public void TestLaunchClientLinkAsync_ThirdPartyUserValidationRequiredAsync()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsync("clientlink/create", It.IsAny<HttpContent>())).ThrowsAsync(new AuthorizationFailureException(AuthenticationResult.ThirdPartyUserValidationRequired));
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var launcher = new ClientLinkWindowLauncher("", new Uri("https://address/"));
			using (launcher.form = new ClientLinkModal(""))
			{
				launcher.LaunchClientLinkAsync(CancellationToken.None).Wait();
				AssertEquals(launcher.form.statusLabel.Text, $"The logged in user {Env.CurrentUser.LoginName} requires an external login to access Glow, which is not supported by Client Link at this time.\r\n\r\nPlease try again with another user.");
			}
		}

		public void TestClientLinkModal_ShouldDisposedAfterShowDialogRunningAndHasCompleted()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var autoResetEvent = new AutoResetEvent(false);
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(() =>
			{
				autoResetEvent.Set();
				throw new AuthorizationFailureException(AuthenticationResult.ThirdPartyUserValidationRequired);
			});
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var launcher = new ClientLinkWindowLauncher("", new Uri("https://address/"));
			var task = Task.Factory.StartNew(async () =>
			{
				autoResetEvent.WaitOne();
				await Task.Delay(2000);
				launcher.form.Invoke(() =>
				{
					launcher.form.Close();
				});
			});
			launcher.ShowDialog();
			task.Wait();

			AssertEquals("ClientLinkModal should be disposed otherwise it will cause FinalizerError", true, launcher.form.IsDisposed);
		}

		public void TestClientLinkModal_CloseButtonShouldNotHangMainForm()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
			httpResponse.Content = new StringContent("{\"id\":\"123\"}");
			clientMock.Setup(c => c.PostAsync("clientlink/create", It.IsAny<HttpContent>())).ReturnsAsync(httpResponse);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var launchWebUrl = new Action<string>(url => { });

			var getHubConnectionAndProxy = new Func<string, IHubConnectionAndProxy>((clientLinkId) =>
			{
				var hubConnectionAndProxyMock = new Mock<IHubConnectionAndProxy>();
				var proxyMock = new Mock<IHubProxy>();
				proxyMock.Setup(t => t.Subscribe(It.IsAny<string>())).Returns(new Subscription());
				hubConnectionAndProxyMock.Setup(h => h.HubProxy).Returns(proxyMock.Object);
				return hubConnectionAndProxyMock.Object;
			});

			var launcher = new ClientLinkWindowLauncher("", new Uri("https://localhost:59415/CCA"), launchWebUrl, getHubConnectionAndProxy);
			launcher.ClientLinkLaunched += (s, e) =>
			{
				launcher.form.Invoke(() =>
				{
					launcher.form.CloseButton_Click(s, e);
				});
			};
			launcher.ShowDialog();

			AssertEquals(true, launcher.form.IsDisposed);
		}

		public void TestClientLinkModal_CloseWhenWaitingForBrowserShouldWork()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
			httpResponse.Content = new StringContent("{\"id\":\"123\"}");
			clientMock.Setup(c => c.PostAsync("clientlink/create", It.IsAny<HttpContent>())).ReturnsAsync(httpResponse);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var launchWebUrl = new Action<string>(url => { });

			var getHubConnectionAndProxy = new Func<string, IHubConnectionAndProxy>((clientLinkId) =>
			{
				var hubConnectionAndProxyMock = new Mock<IHubConnectionAndProxy>();
				var proxyMock = new Mock<IHubProxy>();
				proxyMock.Setup(t => t.Subscribe(It.IsAny<string>())).Returns(new Subscription());
				hubConnectionAndProxyMock.Setup(h => h.HubProxy).Returns(proxyMock.Object);
				return hubConnectionAndProxyMock.Object;
			});

			var launcher = new ClientLinkWindowLauncher("", new Uri("https://localhost:59415/CCA"), launchWebUrl, getHubConnectionAndProxy);
			launcher.ClientLinkLaunched += (s, e) =>
			{
				launcher.form.Invoke(() =>
				{
					launcher.form.Close();
				});
			};
			launcher.ShowDialog();

			AssertEquals(true, launcher.form.IsDisposed);
		}

		// This test would fail in several ways if it used the signalR thread instead of the main thread in the callback (AddBrowserCloseCommandHandler):
		// * Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()
		// * Attempted to access an object owned by another thread
		public void TestLaunchClientLinkAsync_ActionsRunInMainThreadUsingDispatcher()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var clientMock = new Mock<IGlowServiceClient>();
			var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
			httpResponse.Content = new StringContent("{\"id\":\"123\"}");
			clientMock.Setup(c => c.PostAsync("clientlink/create", It.IsAny<HttpContent>())).ReturnsAsync(httpResponse);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var launchWebUrl = new Action<string>(url => { });

			var getHubConnectionAndProxy = new Func<string, IHubConnectionAndProxy>((clientLinkId) =>
			{
				var hubConnectionAndProxyMock = new Mock<IHubConnectionAndProxy>();
				var proxyMock = new Mock<IHubProxy>();
				proxyMock.Setup(t => t.Subscribe(It.IsAny<string>())).Returns(new Subscription());
				hubConnectionAndProxyMock.Setup(h => h.HubProxy).Returns(proxyMock.Object);
				return hubConnectionAndProxyMock.Object;
			});

			var launcher = new ClientLinkWindowLauncher("", new Uri("https://localhost:59415/CCA"), launchWebUrl, getHubConnectionAndProxy);

			var bizo = Factory.New<IForwardingShipment>();

			BrowserInteropHelperExtensions.AddBrowserCloseCommandHandler(launcher, data =>
			{
				bizo.JS_UniqueConsignRef = "55";
				AssertNotNull((Factory as IDbConnected).Connection);
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			});

			launcher.ClientLinkLaunched += (s, e) =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals("Connection to Glow established, launching browser...", launcher.form.statusLabel.Text);
				var message = new BrowserMessageEventArgs<JToken> { Kind = WebViewCommands.SelectRow, Payload = "CCA001" };
				launcher.HandleMessage(new List<JToken> { JToken.FromObject(message) });
			};

			launcher.ShowDialog();

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();
			ClientLinkWindowLauncher.DispatcherForTest = DispatcherSynchronizationContext.Current;
		}

		protected override void TearDown()
		{
				ClientLinkWindowLauncher.DispatcherForTest = null;
				base.TearDown();
		}

		#endregion
	}
}
