using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class EnterpriseChannelInitializeTest : RemoteDesktopServicesTest
	{
		public void TestInitialize_SendWCAAuthenticationInfo()
		{
			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.DomainHint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDI"))
			{
				EnterpriseChannel.Initialize();
				var wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
				AssertNotNull("WCAAuthenticationMessage", wcaAuthenticationMessage);

				var authenticationMessage = (WCAAuthenticationMessage)new XmlSerializer(typeof(WCAAuthenticationMessage)).Deserialize(wcaAuthenticationMessage.Data);
				AssertEquals("EDI", authenticationMessage.enterpriseCode);
				AssertEquals(WCAAuthenticationMessage.WCAAuthenticationType.TokenBased, authenticationMessage.authenticationType);
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					EnterpriseChannel.Initialize();
					wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
					AssertNotNull("WCAAuthenticationMessage", wcaAuthenticationMessage);
					authenticationMessage = (WCAAuthenticationMessage)new XmlSerializer(typeof(WCAAuthenticationMessage)).Deserialize(wcaAuthenticationMessage.Data);
					AssertEquals(WCAAuthenticationMessage.WCAAuthenticationType.UsernameAndPassword, authenticationMessage.authenticationType);
					AssertEquals("", authenticationMessage.enterpriseCode);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}

		public void TestInitialize_SendWCAAuthenticationInfo_RevertToUsernameAndPasswordAuthentication()
		{
			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EnterpriseChannel.Initialize();
				var wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
				AssertNotNull("WCAAuthenticationMessage", wcaAuthenticationMessage);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}

			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				messages.Clear();
				EnterpriseChannel.Initialize();
				var wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
				AssertNotNull("WCAAuthenticationMessage", wcaAuthenticationMessage);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}

			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				messages.Clear();
				EnterpriseChannel.Initialize();
				var wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
				AssertNull("wcaAuthenticationMessage", wcaAuthenticationMessage);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}

			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				messages.Clear();
				EnterpriseChannel.Initialize();
				var wcaAuthenticationMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.WCAAuthentication);
				AssertNotNull("wcaAuthenticationMessage", wcaAuthenticationMessage);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestInitialize_SendTimeoutForCheckDriveMapping()
		{
			// Arrange
			using (RawDataRegistry.Instance.RemoteAppCheckDriveMappingTimeoutInSeconds.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				10))
			{
				// Act
				EnterpriseChannel.Initialize();
			}

			// Assert
			var appSettingMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.RemoteAppSettings);
			AssertNotNull("RemoteAppSettings", appSettingMessage);

			var appSettings = (RemoteAppSettingsMessage)new XmlSerializer(typeof(RemoteAppSettingsMessage))
				.Deserialize(appSettingMessage.Data);
			AssertNull("licenseKey", appSettings.licenseKey);
			AssertNull("branchCode", appSettings.branchCode);
			AssertNull("rdpFileContents", appSettings.rdpFileContents);
			AssertEquals(TimeSpan.FromSeconds(10d), appSettings.TimeoutForCheckDriveMapping);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestInitialize_SendMicrosoftOffice365AppSettings()
		{
			// Arrange
			using (RawDataRegistry.Instance.MicrosoftOffice365TenantIdForDragDrop.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TenantIdForDragDrop"))
			using (RawDataRegistry.Instance.MicrosoftOffice365ApplicationIdForDragDrop.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ApplicationIdForDragDrop"))
			using (RawDataRegistry.Instance.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000))
			{ 
				// Act
				EnterpriseChannel.Initialize();
			}

			// Assert
			var appSettingMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.RemoteAppSettings);
			AssertNotNull("RemoteAppSettings", appSettingMessage);

			var appSettings = (RemoteAppSettingsMessage)new XmlSerializer(typeof(RemoteAppSettingsMessage))
				.Deserialize(appSettingMessage.Data);
			AssertEquals("TenantIdForDragDrop", appSettings.tenantId);
			AssertEquals("ApplicationIdForDragDrop", appSettings.applicationId);
			AssertEquals(1000, appSettings.attachmentEmailFetchLimit);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestInitializeUI_SendTimeoutForCheckDriveMapping()
		{
			// Arrange
			using (RawDataRegistry.Instance.RemoteAppCheckDriveMappingTimeoutInSeconds.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				10))
			{
				EnterpriseChannel.Initialize();
			}

			using (RawDataRegistry.Instance.RemoteAppCheckDriveMappingTimeoutInSeconds.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				5))
			{
				// Act
				EnterpriseChannel.InitializeUI("EDIEDIDAT", "DEMHQ");
			}

			// Assert
			var appSettingMessage = messages.FindLast(x => x.Type == EnterpriseChannelMessageTypes.RemoteAppSettings);
			AssertNotNull("RemoteAppSettings", appSettingMessage);

			var appSettings = (RemoteAppSettingsMessage)new XmlSerializer(typeof(RemoteAppSettingsMessage)).Deserialize(appSettingMessage.Data);

			AssertEquals("TestDomain", appSettings.domain);
			AssertEquals("TestInstance", appSettings.instance);
			AssertEquals("EDIEDIDAT", appSettings.licenseKey);
			AssertEquals("DEMHQ", appSettings.branchCode);
			AssertNull("rdpFileContents", appSettings.rdpFileContents);
			AssertEquals(TimeSpan.FromSeconds(5d), appSettings.TimeoutForCheckDriveMapping);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestInitialize_SendRDPVersionInfo()
		{
			// Arrange
			// Act
			EnterpriseChannel.Initialize();

			// Assert
			var serverRDPVersionMessage = messages.SingleOrDefault(x => x.Type == EnterpriseChannelMessageTypes.ServerRDPVersion);
			AssertNotNull("ServerRDPVersion is sent", serverRDPVersionMessage);

			var serverRDPVersion = (ServerRDPVersionMessage)new XmlSerializer(typeof(ServerRDPVersionMessage))
				.Deserialize(serverRDPVersionMessage.Data);
			AssertEquals(ClientVersion.Version.ToString(), serverRDPVersion.serverRDPVersion);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestInitialize_States()
		{
			// Arrange
			// Act
			EnterpriseChannel.Initialize();

			// Assert
			AssertEquals(true, EnterpriseChannel.Instance.IsConnected);
			AssertEquals(true, InitializationMessageHandler.InitializationCompleted.WaitOne(0));
			AssertEquals(true, InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.OpenFileSupported));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		class ErrorReportsOrLogsTest : TransactionedTestCase
		{
			public void TestActivityLoggerWillLogWhenConnectFailed()
			{
				// Arrange
#if NETFRAMEWORK
				wtsApiDisposable = WtsApiHelper.SetUpDisposable((IntPtr)0, null);
#else
				wtsApiDisposable = WtsApiHelper.SetUpDisposable(0, null);
#endif

				// Act
				EnterpriseChannel.Initialize();

				// Assert
				AssertContains("Connection failed due to [CouldNotConnect] on [SecondAttempOnInitializing] with the error code [0]", EnterpriseChannel.Instance.ActivityLogger.ToString());
			}

			public void TestErrorLogIfInitializationCompletedTimeout()
			{
				// Arrange
#if NETFRAMEWORK
				wtsApiDisposable = WtsApiHelper.SetUpDisposable((IntPtr)123, null);
#else
				wtsApiDisposable = WtsApiHelper.SetUpDisposable(123, null);
#endif

				// Act
				EnterpriseChannel.Initialize();

				// Assert
				AssertContains($"Connection failed due to [InitializationMessageTimeout] on [SecondAttempOnInitializing] within " +
					$"[{TimeSpan.FromSeconds(EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds)}]",
					EnterpriseChannel.Instance.ActivityLogger.ToString());
			}

			public void TestErrorIsReportedIfOpenFileSupportedNotRegistered()
			{
				// Arrange
#if NETFRAMEWORK
				wtsApiDisposable = WtsApiHelper.SetUpDisposable((IntPtr)123, delegate
#else
				wtsApiDisposable = WtsApiHelper.SetUpDisposable(123, delegate
#endif
				{
					InitializationMessageHelper.Handle(new InitializationMessage(new[] { "TTTT" }, "0.0.0.1"));
				});

				// Act
				EnterpriseChannel.Initialize();

				// Assert
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContainsExactElementsInExactOrder(
					new (string Key, string Message, Exception Exception)[]
					{
						("EnterpriseChannel-Connect OpenFileSupported NotRegistered [SecondAttempOnInitializing]", EnterpriseChannel.Instance.ActivityLogger.ToString(), null),
					},
					errorsReported);
			}

			public void TestErrorLogIfOpenFileSupportedNotReplied()
			{
				// Arrange
#if NETFRAMEWORK
				wtsApiDisposable = WtsApiHelper.SetUpDisposable((IntPtr)123, delegate
#else
				wtsApiDisposable = WtsApiHelper.SetUpDisposable(123, delegate
#endif
				{
					InitializationMessageHelper.Handle();
				});

				// Act
				EnterpriseChannel.Initialize();

				// Assert
				AssertContains($"Connection failed due to [OpenFileSupportedNotReplied] on [SecondAttempOnInitializing] for no replied by client side " +
					$"[{InitializationMessageHandler.RemoteVersion}] in [{TimeSpan.FromSeconds(EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds)}]",
					EnterpriseChannel.Instance.ActivityLogger.ToString());
			}

			IDisposable remoteAppWaitingForReconnectionTimeoutInSecondsDisposable;
			IDisposable errorReporterDisposable;
			List<(string Key, string Message, Exception Exception)> errorsReported;
			IDisposable wtsApiDisposable;
			protected override void SetUp()
			{
				base.SetUp();

				var previousRemoteAppWaitingForReconnectionTimeoutInSeconds = EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds;
				remoteAppWaitingForReconnectionTimeoutInSecondsDisposable = new DisposableAction(
					() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = 5,
					() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = previousRemoteAppWaitingForReconnectionTimeoutInSeconds);

				var errorReporter = new Mock<IErrorReporter>();
				errorReporter.Setup(x => x.Clear());

				errorsReported = new List<(string Key, string Message, Exception Exception)>();
				errorReporter.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback<string, string, Exception>((key, message, ex) => errorsReported.Add((key, message, ex)));
				errorReporterDisposable = ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object);
			}

			protected override void TearDown()
			{
				errorReporterDisposable.Dispose();
				ErrorReporter.Clear();

				remoteAppWaitingForReconnectionTimeoutInSecondsDisposable.Dispose();

				EnterpriseChannel.Instance.Close();
				EnterpriseChannel.Reset();
				wtsApiDisposable?.Dispose();

				base.TearDown();
			}
		}

		protected override void SetUp()
		{
			SetupMock();
			messages = new List<Message>();
			EnterpriseChannel.OnSendingMessage += EnterpriseChannel_OnSendingMessage;
		}

		protected override void TearDown()
		{
			EnterpriseChannel.OnSendingMessage -= EnterpriseChannel_OnSendingMessage;
			base.TearDown();
		}

		List<Message> messages;
		void EnterpriseChannel_OnSendingMessage(object sender, byte[] e)
			=> messages.Add(new Message
			{
				Type = Encoding.ASCII.GetString(e, 0, 4),
				Data = new MemoryStream(e, 4, e.Length - 4),
			});

		class Message
		{
			public string Type { get; set; }
			public Stream Data { get; set; }
		}

		static class WtsApiHelper
		{
			public static IDisposable SetUpDisposable(IntPtr handle, Action<byte[], int, int, AsyncCallback, object> beginWriteCallback = null)
			{
				return new DisposableAction(
					() => Setup(handle, beginWriteCallback),
					() => TearDown());
			}

			static void Setup(IntPtr handle, Action<byte[], int, int, AsyncCallback, object> beginWriteCallback)
			{
				var wtsApi = new Mock<IWtsApi>();
				wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>())).Returns(handle);
				wtsApi.Setup(x => x.VirtualChannelGetStream(handle)).Returns(CreateVirtualChannelStream);
				WtsApi.Instance = wtsApi.Object;

				Stream CreateVirtualChannelStream()
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

							beginWriteCallback?.Invoke(buf, offset, count, asyncCallback, state);
							return asyncResult.Object;
						});
					stream.Setup(x => x.EndWrite(It.IsAny<IAsyncResult>())).Callback<IAsyncResult>(x => ((CancellationTokenSource)x.AsyncState).Dispose());
					return stream.Object;
				}
			}

			static void TearDown()
			{
				InitializationMessageHandler.InitializationCompleted.Reset();
				InitializationMessageHandler.RemoteInitializationMessage = null;
				WtsApi.Instance = null;
			}
		}
	}
}
