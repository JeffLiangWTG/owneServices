using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	abstract class WebUrlLauncherTest : TransactionedTestCase
	{
		sealed class ServerOnlyTest : WebUrlLauncherTest
		{
			public void TestWebBrowserIsLaunchOnTerminalServerUrl()
			{
				TestWebBrowserIsLaunchOnTerminalServerInternal(RequestUrl_Https, ExpectedMessage_Https);
			}

			public void TestWebBrowserIsLaunchOnTerminalServerEdiEntCS01839659()
			{
				TestWebBrowserIsLaunchOnTerminalServerInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}

			void TestWebBrowserIsLaunchOnTerminalServerInternal(string url, byte[] expectedMessage)
			{
				// Arrange
				// Act
				WebUrlLauncher.Launch(url);

				// Assert
				AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
				virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessage)), 0, It.IsAny<int>(), null, null), Times.Never());
			}

			protected override void SetUp()
			{
				base.SetUp();
				rawDataRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ServerOnly);
			}
		}

		sealed class ConnectorOnlyTest : WebUrlLauncherTest
		{
			public void TestErrorIsPromptedIfEnterpriseChannelIsUninitializedUrl()
			{
				TestErrorIsPromptedIfEnterpriseChannelIsUninitializedInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestErrorIsPromptedIfEnterpriseChannelIsUninitializedEdiEntCS01839659()
			{
				TestErrorIsPromptedIfEnterpriseChannelIsUninitializedInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}

			void TestErrorIsPromptedIfEnterpriseChannelIsUninitializedInternal(string url, byte[] expectedMessage)
			{
				// Arrange
				// Act
				WebUrlLauncher.Launch(url);

				// Assert
				var lastError = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals(true, lastError.WasError);
				AssertEquals(ZTerminalService.ClientPluginApplicationNotInstalledError, lastError.Text);

				AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
				virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessage)), 0, It.IsAny<int>(), null, null), Times.Never());
			}

			public void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelUrl()
			{
				TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelEdiEntCS01839659()
			{
				TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}

			void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(string url, byte[] expectedMessage)
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					// Act
					WebUrlLauncher.Launch(url);

					// Assert
					AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
					virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessage)), 0, It.IsAny<int>(), null, null), Times.Once());
				}
			}

			public void TestErrorIsPromptedIfEnterpriseChannelIsUnavailableUrl()
			{
				TestErrorIsPromptedIfEnterpriseChannelIsUnavailableInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestErrorIsPromptedIfEnterpriseChannelIsUnavailableEdiEntCS01839659()
			{
				TestErrorIsPromptedIfEnterpriseChannelIsUnavailableInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}

			void TestErrorIsPromptedIfEnterpriseChannelIsUnavailableInternal(string url, byte[] expectedMessage)
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					EnterpriseChannel.Instance.Close();

					// Act
					WebUrlLauncher.Launch(url);

					// Assert
					var lastError = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals(true, lastError.WasError);
					AssertEquals(WebUrlLauncher.UnableToLaunchUrlOnClientSideError, lastError.Text);

					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessage)), 0, It.IsAny<int>(), null, null), Times.Never());
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				rawDataRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOnly);
			}
		}

		sealed class ConnectorOrServerTest : WebUrlLauncherTest
		{
			public void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUninitializedUrl()
			{
				TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUninitializedInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUninitializedEdiEntCS01839659()
			{
				TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUninitializedInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_Https);
			}

			void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUninitializedInternal(string url, byte[] expectedMessageWritten)
			{
				// Arrange
				// Act
				WebUrlLauncher.Launch(url);

				// Assert
				AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
				virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessageWritten)), 0, It.IsAny<int>(), null, null), Times.Never());
			}

			public void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelUrl()
			{
				TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelEdiEntCS01839659()
			{
				TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}

			void TestWebBrowserIsLaunchOnClientSideViaEnterpriseChannelInternal(string url, byte[] expectedMessageWritten)
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					// Act
					WebUrlLauncher.Launch(url);

					// Assert
					AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
					virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessageWritten)), 0, It.IsAny<int>(), null, null), Times.Once());
				}
			}

			public void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUnavailableUrl()
			{
				TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUnavailableInternal(RequestUrl_Https, ExpectedMessage_Https);
			}
			public void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUnavailableEdiEntCS01839659()
			{
				TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUnavailableInternal(RequestUrl_EdiEntCS01839659, ExpectedMessage_EdiEntCS01839659);
			}
			void TestWebBrowserIsLaunchOnTerminalServerIfEnterpriseChannelIsUnavailableInternal(string url, byte[] expectedMessageWritten)
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					EnterpriseChannel.Instance.Close();

					// Act
					WebUrlLauncher.Launch(url);

					// Assert
					AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
					virtualChannelStream.Verify(x => x.BeginWrite(It.Is<byte[]>(v => v.SequenceEqual(expectedMessageWritten)), 0, It.IsAny<int>(), null, null), Times.Never());
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				rawDataRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOrServer);
			}
		}

		sealed class IsRemoteTest : WebUrlLauncherTest
		{
			public void TestEnterpriseChannelIsUnavailable()
			{
				AssertEquals(false, WebUrlLauncher.IsRemote);
			}

			public void TestEnterpriseChannelIsAvailable()
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					// Act
					// Assert
					AssertEquals(true, WebUrlLauncher.IsRemote);
				}
			}

			public void TestEnterpriseChannelIsUninitialized()
			{
				// Arrange
				using (InitializeEnterpriseChannel())
				{
					EnterpriseChannel.Instance.Close();

					// Act
					// Assert
					AssertEquals(false, WebUrlLauncher.IsRemote);
				}
			}
		}

		string originalHostedLocation;
		RawDataRegistry rawDataRegistry;
		Mock<Stream> virtualChannelStream;

		byte[] ExpectedMessage_Https;
		byte[] ExpectedMessage_EdiEntCS01839659;

		const string RequestUrl_Https = "https://www.whatever.url.is.this";
		const string RequestUrl_EdiEntCS01839659 = "edient:Command=ShowEditForm&LicenceCode=HYEDUKCM2&ControllerID=EMCS&BusinessEntityPK=2258b2fb-bb82-4fc4-a824-792a3251dd9a&VersionNumber=25.1.6.183&Domain=wtg.zone&Instance=UAT+CMR+Message+Testing+2&Hash=%2bT6wa4hTNcC%2fRP%2bpKj6nyRiTzd7sP9tTE";

		protected override void SetUp()
		{
			base.SetUp();

			originalHostedLocation = EnvProxy.HostedLocation;
			// We enforce self-hosted because we only care about RemoteAppAllowEDocAccessWithoutConnectorMode rather than how it is calculated
			EnvProxy.SetHostedLocationForTest(string.Empty);

			rawDataRegistry = new RawDataRegistry();

			virtualChannelStream = CreateVirtualChannelStream();
			SetUpWtsApi(virtualChannelStream.Object);

			var msgTypeData = Encoding.ASCII.GetBytes(EnterpriseChannelMessageTypes.WebUrl);

			// Expected Message - Basic
			ConvertToMessage(msgTypeData, RequestUrl_Https, out ExpectedMessage_Https);

			// Expected Message - EdiEntCS01839659
			ConvertToMessage(msgTypeData, RequestUrl_EdiEntCS01839659, out ExpectedMessage_EdiEntCS01839659);
		}

		void ConvertToMessage(byte[] msgTypeData, string expectedUrl, out byte[] message)
		{
			var urlData = Encoding.UTF8.GetBytes(expectedUrl);
			message = new byte[msgTypeData.Length + urlData.Length];
			Array.Copy(msgTypeData, message, msgTypeData.Length);
			Array.Copy(urlData, 0, message, msgTypeData.Length, urlData.Length);
		}

		static void SetUpWtsApi(Stream stream)
		{
			var handle = (IntPtr)123;
			var wtsApi = new Mock<IWtsApi>(MockBehavior.Strict);
			wtsApi.Setup(x => x.VirtualChannelClose(handle)).Returns(true);
			wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>())).Returns(handle);
			wtsApi.Setup(x => x.VirtualChannelGetStream(handle)).Returns(stream);
			wtsApi.Setup(x => x.RegisterSessionNotification(It.IsAny<IntPtr>())).Returns(true);
			WtsApi.Instance = wtsApi.Object;
		}

		Mock<Stream> CreateVirtualChannelStream()
		{
			var stream = new Mock<Stream>();
			stream.Setup(x => x.BeginRead(It.IsAny<byte[]>(), 0, It.IsAny<int>(), null, null))
				.Returns(() => new TaskCompletionSource<bool>().Task); // block ReadLoop thread infinitely

			stream.Setup(x => x.BeginWrite(It.IsAny<byte[]>(), 0, It.IsAny<int>(), null, null))
				.Returns<byte[], int, int, AsyncCallback, object>((buf, offset, count, asyncCallback, state) =>
				{
					var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
					var asyncResult = new Mock<IAsyncResult>();
					asyncResult.SetupGet(x => x.AsyncState).Returns(cts);
					asyncResult.SetupGet(x => x.AsyncWaitHandle).Returns(cts.Token.WaitHandle);

					HandleMessage(Encoding.ASCII.GetString(buf, 0, 4), buf);

					return asyncResult.Object;
				}).Verifiable();
			stream.Setup(x => x.EndWrite(It.IsAny<IAsyncResult>())).Callback<IAsyncResult>(x => ((CancellationTokenSource)x.AsyncState).Dispose());
			return stream;

			void HandleMessage(string requestMessageType, byte[] wholeBuffer)
			{
				switch (requestMessageType)
				{
					case EnterpriseChannelMessageTypes.ServerMessageLoopReady:
						HandleRequestFromClient(EnterpriseChannelMessageTypes.Initialization, CreateInitializationMessage());
						break;
					case EnterpriseChannelMessageTypes.OpenFileSupported:
						var serverRequestId = new byte[16];
						Array.Copy(wholeBuffer, 4, serverRequestId, 0, serverRequestId.Length);
						HandleReplyFromClient(true, new Guid(serverRequestId));
						break;
				}
			}

			void HandleRequestFromClient<T>(string requestMessageType, T request)
			{
				using (var ms = new MemoryStream())
				{
					new XmlSerializer(typeof(T)).Serialize(ms, request);
					ms.Position = 0;
					MessageHandlers.HandleMessage(EnterpriseChannel.Instance, requestMessageType, ms);
				}
			}

			void HandleReplyFromClient<T>(T request, Guid serverRequestId)
			{
				using (var ms = new MemoryStream())
				{
					ms.Write(serverRequestId.ToByteArray(), 0, 16);
					new XmlSerializer(typeof(T)).Serialize(ms, request);
					ms.Position = 0;
					MessageHandlers.HandleMessage(EnterpriseChannel.Instance, EnterpriseChannelMessageTypes.ReturnCallback, ms);
				}
			}

			InitializationMessage CreateInitializationMessage()
			{
				return new InitializationMessage(
					new[]
					{
						EnterpriseChannelMessageTypes.WCAAuthentication,
						EnterpriseChannelMessageTypes.ServerRDPVersion,
						EnterpriseChannelMessageTypes.UrlAuthenticationRequired,
						EnterpriseChannelMessageTypes.OpenFileSupported,
					},
					ClientVersion.Version.ToString());
			}
		}

		protected override void TearDown()
		{
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
			WtsApi.Instance = null;
			InitializationMessageHandler.RegisteredRemoteMessageTypes = Array.Empty<string>();
			InitializationMessageHandler.InitializationCompleted.Reset();
			EnterpriseChannel.Reset();
			WebUrlLauncher.ClearLastUrlLaunched();

			base.TearDown();
		}

		static IDisposable InitializeEnterpriseChannel()
		{
			EnterpriseChannel.Initialize();
			return new DisposableAction(() => EnterpriseChannel.Instance.Close());
		}
	}
}
