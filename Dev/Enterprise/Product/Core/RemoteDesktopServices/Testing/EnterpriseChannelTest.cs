using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using EnterpriseChannel = Enterprise.RemoteDesktopServices.Server.EnterpriseChannel;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public class EnterpriseChannelTest : RemoteDesktopServicesTest
	{
		sealed class ConnectTest : TestCase
		{
			public void TestNormal()
			{
				// Arrange
				wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>())).Returns(handle).Verifiable();
				wtsApi.Setup(x => x.VirtualChannelGetStream(handle)).Returns(CreateVirtualChannelStream).Verifiable();
				WtsApi.Instance = wtsApi.Object;

				// Act
				var errorCode = enterpriseChannel.Object.Connect_ForTest();

				// Assert
				AssertEquals(0, errorCode);
				AssertEquals(true, enterpriseChannel.Object.IsConnected);
				wtsApi.Verify(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
				wtsApi.Verify(x => x.VirtualChannelGetStream(handle), Times.Once);
				enterpriseChannel.Verify(x => x.Send(It.IsAny<byte[]>()), Times.Once);
			}

			public void TestChannelConnectIsIdempotent()
			{
				wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>())).Returns(handle).Verifiable();
				wtsApi.Setup(x => x.VirtualChannelGetStream(handle)).Returns(CreateVirtualChannelStream).Verifiable();
				WtsApi.Instance = wtsApi.Object;

				using (new DisposableAction(() => ErrorReporter.Clear()))
				{
					var errorCode1 = enterpriseChannel.Object.Connect_ForTest();
					var errorCode2 = enterpriseChannel.Object.Connect_ForTest();

					AssertEquals(0, errorCode1);
					AssertEquals(0, errorCode2);

					AssertEquals(1, enterpriseChannel.Object.ConnectedCount);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}

			public void TestWaitBetweenRetriesOnVirtualChannelOpenFail()
			{
				using (new DisposableAction(() => { EnterpriseChannel.Instance?.Close(); ErrorReporter.Clear(); }))
				{
					wtsApi.Setup(x => x.RegisterSessionNotification(It.IsAny<IntPtr>())).Returns(true);
					wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
					{
						return IntPtr.Zero;
					});
					WtsApi.Instance = wtsApi.Object;

					var timer = new Stopwatch();
					timer.Start();
					EnterpriseChannel.Initialize();

					AssertGreaterThanOrEqualTo(timer.ElapsedMilliseconds, 5000);
				}
			}

			public void TestErrorCodeIsReturnedWhenOpenFails()
			{
				// Arrange
				const int ErrorCode = 31;

				wtsApi.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>()))
					.Callback(() =>
					{
						NativeMethods.SetLastErrorEx(ErrorCode, 0);
					})
					.Returns(IntPtr.Zero);
				wtsApi.Setup(x => x.VirtualChannelGetStream(handle)).Verifiable();
				WtsApi.Instance = wtsApi.Object;

				// Act
				var errorCode = enterpriseChannel.Object.Connect_ForTest();

				// Assert
				AssertEquals(ErrorCode, errorCode);
				AssertEquals(false, enterpriseChannel.Object.IsConnected);
				wtsApi.Verify(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
				wtsApi.Verify(x => x.VirtualChannelGetStream(handle), Times.Never);
				enterpriseChannel.Verify(x => x.Send(It.IsAny<byte[]>()), Times.Never);
			}

			static Stream CreateVirtualChannelStream()
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

						return asyncResult.Object;
					});
				stream.Setup(x => x.EndWrite(It.IsAny<IAsyncResult>())).Callback<IAsyncResult>(x => ((CancellationTokenSource)x.AsyncState).Dispose());
				return stream.Object;
			}

			IntPtr handle;
			Mock<IWtsApi> wtsApi;
			Mock<EnterpriseChannel> enterpriseChannel;
			protected override void SetUp()
			{
				base.SetUp();
#if NETFRAMEWORK
				handle = (IntPtr)123;
#else
				handle = 123;
#endif
				wtsApi = new Mock<IWtsApi>(MockBehavior.Strict);
				wtsApi.Setup(x => x.VirtualChannelClose(handle)).Returns(true);

				enterpriseChannel = new Mock<EnterpriseChannel>() { CallBase = true };
				enterpriseChannel.Setup(x => x.Send(It.IsAny<byte[]>())).CallBase().Verifiable();
			}

			protected override void TearDown()
			{
				enterpriseChannel.Object.Close();
				WtsApi.Instance = null;
				base.TearDown();
			}

			static class NativeMethods
			{
				[DllImport("user32.dll", SetLastError = true)]
				internal static extern void SetLastErrorEx(uint dwErrCode, uint dwType);
			}
		}

		#region ReadLoop

		public void TestReadLoop_HandleInvalid()
		{
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.MakeWinIOError();

			AssertNoExceptionThrown("Should not throw an exception", () =>
			{
				EnterpriseChannel.Instance.OnDisconnect();
				EnterpriseChannel.Instance.Connect_ForTest();
				Thread.Sleep(TimeSpan.FromSeconds(3));
				AssertEquals(String.Empty, CargoWise.Common.ErrorReporter.LastMessageReported);
			});
		}
		public void TestReadLoop_ReportsUnknownIOException()
		{
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.MakeWinUnknownIOError();

			AssertNoExceptionThrown("Should not throw an exception", () =>
			{
				EnterpriseChannel.Instance.OnDisconnect();
				EnterpriseChannel.Instance.Connect_ForTest();
				Thread.Sleep(TimeSpan.FromSeconds(3));
				AssertEquals("Unknown IO Exception (HRESULT: 0x81234567)", ErrorReporter.LastMessageReported);
			});
		}

		public void TestReadLoop_HandleOutOfOrderChunks()
		{
			// Arrange.

			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			// Act.

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.OutOfOrderLastChunkButRecoverable));

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertWarningMessageWhenHandlingOutOfOrderChunks(null);

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.OutOfOrderMiddleAndLastChunksButRecoverable));
			AssertWarningMessageWhenHandlingOutOfOrderChunks(null);

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.AlreadyInOrder));
			AssertWarningMessageWhenHandlingOutOfOrderChunks(null);

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.LastChunkInCorrectPlaceButMiddleChunksOutOfOrder));
			AssertWarningMessageWhenHandlingOutOfOrderChunks(null);

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.OutOfOrderChunksAndNotRecoverable));
			AssertWarningMessageWhenHandlingOutOfOrderChunks("Unable to receive data successfully. Please try again in a few minutes.");
		}

		public void TestReadLoop_HandleMoreThanOneMiddleChunks()
		{
			ErrorReporter.Instance.Clear();
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			ExecuteOutOfOrderChunksScenario(api, TestSamples.MoreThanOneMiddleChunks);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertType<EnterpriseChannel.MiddleChunkException>(ErrorReporter.LastExceptionReported);
			AssertEquals("Chuncks contains more than one 'Middle' chunk", ErrorReporter.LastExceptionReported.Message);
			Assert(ErrorReporter.LastMessageReported.Contains("Chuncks contains more than one 'Middle' chunk"));
			Assert(ErrorReporter.LastMessageReported.Contains(TestSamples.MoreThanOneMiddleChunks.Message.Substring(4).Replace("<", "&lt;").Replace(">", "&gt;")));

			ErrorReporter.Instance.Clear();
		}

		public void TestReadLoop_HandleMiddleChunksShouldNotThrowException()
		{
			ErrorReporter.Instance.Clear();
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			ExecuteOutOfOrderChunksScenario(api, TestSamples.MiddleChunksWithOtherLength);
			ExecuteOutOfOrderChunksScenario(api, TestSamples.MiddleChunksWithOtherLength2);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertNull("MiddleChunkException", ErrorReporter.LastExceptionReported);
		}

		public void TestReadLoop_HandleNoMiddleChunk()
		{
			ErrorReporter.Instance.Clear();
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			AssertNoExceptionThrown(() => ExecuteOutOfOrderChunksScenario(api, TestSamples.NoMiddleChunk));
			AssertWarningMessageWhenHandlingOutOfOrderChunks("Unable to receive data successfully. Please try again in a few minutes.");
		}

		public void TestReadLoop_HandleOutOfOrderChunksOnlyForDragDropMessage()
		{
			// Arrange.

			var api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			// Scenario 1: Read drag-drop message type using the out-of-order chunks handler.

			var samples = new DragDropSamples();
			api.InjectRawMessagesToReadLoop(samples.AlreadyInOrder.CreateChunkedMessages().ToArray());

			AssertEquals(
				"Expecting out-of-order chunks handling applied to drag-drop message",
				true,
				EnterpriseChannel.Instance.IsHandlingDragDropMessage);

			// Scenario 2: Read other message type using the original chunks handler.

			var fileCheckMessage = string.Format(
				CultureInfo.InvariantCulture,
				"{0}Potato.png",
				EnterpriseChannelMessageTypes.CheckFileExists);

			api.InjectRawMessagesToReadLoop(CreateSingleChunkEncodedMessage(fileCheckMessage));

			AssertEquals(
				"Expecting normal chunks handling applied to NON drag-drop message",
				false,
				EnterpriseChannel.Instance.IsHandlingDragDropMessage);
		}

		#endregion

		#region Reconnect

		public void TestReconnect_ReturnDefaultTimeoutWhenExceptionOccurWhileObtainingReconnectionTimeout()
		{
			var envMock = new Mock<IEnv>();
			var environmentMock = new Mock<IEnvironment>();

			environmentMock.Setup(x => x.Registry).Returns(() => throw new Win32Exception());
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				AssertEquals(30, EnterpriseChannel.GetReconnectionTimeout_ForTest().Seconds);
			}
		}

		#endregion Reconnect

		public void TestEnterpriseChannelMutex()
		{
			EnterpriseChannel.Instance.ApplicationThreadExit();

			var enterpriseChannel1 = new EnterpriseChannel();
			AssertEquals(true, enterpriseChannel1.CreatedNewMutex);

			var enterpriseChannel2 = new EnterpriseChannel();
			AssertEquals(false, enterpriseChannel2.CreatedNewMutex);

			enterpriseChannel1.ApplicationThreadExit();
			enterpriseChannel2.ApplicationThreadExit();

			var enterpriseChannel3 = new EnterpriseChannel();
			AssertEquals(true, enterpriseChannel3.CreatedNewMutex);
		}

		[ExpectNoExceptions]
		public void TestInitialize_MessageHandlers_WhenApplicationDispatcherIsNull()
		{
			var initialDispatcher = ApplicationDispatcher.Current;
			try
			{
				// The MessageHandlers have been initialized by the base class Setup().
				var allMessageTypes = MessageHandlers.RegisteredMessageTypes;
				ApplicationDispatcher.Current = null;
				foreach (var messageType in allMessageTypes)
				{
					var stream = new MemoryStream(Encoding.ASCII.GetBytes(""));
					try
					{
						MessageHandlers.HandleMessage(new MessageChannelForTest(), messageType, stream);
					}
					catch (NullReferenceException)
					{
						Fail("Message type handler throws NullReferenceException either due to ApplicationDispatcher.Current is null or an empty stream: " + messageType);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
			}
			finally
			{
				ApplicationDispatcher.Current = initialDispatcher;
			}
		}

		[ExpectNoExceptions]
		public void TestServerSessionSwitchSendInitializationMessageWhenConnects()
		{
			// Arrange
			var mockClientMessageHandler = new Mock<EventHandler<byte[]>>();
			EnterpriseChannel.OnSendingMessage += mockClientMessageHandler.Object;
			var builtConnectionCallbackTask = new Task(() =>
			{
				EnterpriseChannel.Instance.ReadLoopStartEventForTest.WaitOne();
				InitializationMessageHelper.Handle();
			});

			using (new DisposableAction(() => EnterpriseChannel.OnSendingMessage -= mockClientMessageHandler.Object))
			using (SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.DomainHint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDI"))
			{
				var channel = EnterpriseChannel.Instance;
				EnterpriseChannel.Instance.ReadLoopStartEventForTest.Reset();
				channel.OnDisconnect();
				builtConnectionCallbackTask.Start();

				// Act
				channel.OnServerSessionSwitch(Microsoft.Win32.SessionSwitchReason.RemoteConnect);

				// Assert
				Assert(builtConnectionCallbackTask.Wait(10 * 1000));
				mockClientMessageHandler
					.Verify(x => x.Invoke(
						It.IsAny<object>(),
						It.Is<byte[]>(
							y => Encoding.ASCII.GetString(y, 0, 4) == EnterpriseChannelMessageTypes.WCAAuthentication)), Times.Once);
				mockClientMessageHandler
					.Verify(x => x.Invoke(
						It.IsAny<object>(),
						It.Is<byte[]>(
							y => Encoding.ASCII.GetString(y, 0, 4) == EnterpriseChannelMessageTypes.ServerRDPVersion)), Times.Once);
				mockClientMessageHandler
					.Verify(x => x.Invoke(
						It.IsAny<object>(),
						It.Is<byte[]>(
							y => Encoding.ASCII.GetString(y, 0, 4) == EnterpriseChannelMessageTypes.RemoteAppSettings)), Times.Once);
			}
		}

		public void TestOperationCanceledExceptionOfSendErrorMessageWontBeWrapped()
		{
			// Arrange
			var enterpriseChannel = new EnterpriseChannel();
			enterpriseChannel.OnDisconnect();

			// Act
			// Assert
			var exceptionThrown = AssertExceptionThrown<OperationCanceledException>(() => enterpriseChannel.Send(new byte[] { 1 }));
			AssertNull(exceptionThrown.InnerException);
			AssertEquals("Sending to the remote desktop failed", exceptionThrown.Message);
		}

		public void TestExceptionIsRecordedWhenSendingReturnCallbackMessageFails()
		{
			// Arrange
			var enterpriseChannel = new Mock<EnterpriseChannel>() { CallBase = false };
			var unhandledException = new Exception("Error on send");
			enterpriseChannel.Setup(x => x.Send(It.IsAny<byte[]>())).Throws(unhandledException);
			enterpriseChannel.Protected().Setup("StartAsTask", ItExpr.IsAny<Action>()).Callback<Action>(x => x());

			using (new DisposableAction(() => enterpriseChannel.Object.Close()))
			{
				// Act
				enterpriseChannel.Object.SendReturnCallbackMessage(Guid.NewGuid(), 12);

				// Assert
				enterpriseChannel.Verify(x => x.Send(It.IsAny<byte[]>()), Times.Once);
				AssertEquals("OnErrorDuringSendingReturnCallbackMessage", ErrorReporter.LastKeyReported);
				AssertEquals("Return: [System.Int32 12]", ErrorReporter.LastMessageReported);
				AssertEquals(unhandledException, ErrorReporter.LastExceptionReported);

				// Cleanup
				ErrorReporter.Clear();
			}
		}

		public void TestActivityLoggerWillLogWhenConnecting()
		{
			var activityLog = EnterpriseChannel.Instance.ActivityLogger.ToString();

			// Assert
			AssertContains($"OnInitialConnect method has been called when connecting", activityLog);
			var terminalService = ObjectFactory.Get<TerminalService>();
			var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;
			AssertContains($@"IsCitrix: {terminalService.IsCitrixICA}, IsRemoteAppSession: {terminalService.IsRemoteAppSession}, IsWTSSession: {terminalService.IsWTSSession}
ClientSessionProtocolType: {terminalService.GetClientSessionProtocolType(numberOfTries: 1)}, TerminalService.LastWin32Error: {terminalService.LastWin32Error}
ChannelName: EDIENT, DynamicChannel: True
SupportedClientVersion: {supportedClientVersion}", activityLog);
			AssertContains($@"Initialization completed, client version: [{InitializationMessageHandler.RemoteVersion}], client supported message types: [{string.Join(";", InitializationMessageHandler.RegisteredRemoteMessageTypes)}]", activityLog);
			AssertContains($"thread ID", activityLog);
			AssertContains($"session ID [{Process.GetCurrentProcess().SessionId}]", activityLog);
			AssertContains("Connection completed", activityLog);
		}

		public void TestActivityLoggerWillLogWhenDisconnecting()
		{
			// Act
			EnterpriseChannel.Instance.OnDisconnect();

			// Assert
			AssertContains($"TestActivityLoggerWillLogWhenDisconnecting method has been called when disconnecting, thread ID [{Thread.CurrentThread.ManagedThreadId}], session ID [{Process.GetCurrentProcess().SessionId}].", EnterpriseChannel.Instance.ActivityLogger.ToString());
		}

		public void TestExceptionIsRecordedWhenSendingReturnCallbackErrorFails()
		{
			// Arrange
			var enterpriseChannel = new Mock<EnterpriseChannel>() { CallBase = false };
			var unhandledException = new Exception("Error on send");
			enterpriseChannel.Setup(x => x.Send(It.IsAny<byte[]>())).Throws(unhandledException);
			enterpriseChannel.Protected().Setup("StartAsTask", ItExpr.IsAny<Action>()).Callback<Action>(x => x());

			using (new DisposableAction(() => enterpriseChannel.Object.Close()))
			{
				// Act
				enterpriseChannel.Object.SendErrorReturnCallbackMessage(Guid.NewGuid(), new Exception("Error during handle"));

				// Assert
				enterpriseChannel.Verify(x => x.Send(It.IsAny<byte[]>()), Times.Once);
				AssertEquals("OnErrorDuringSendingReturnCallbackMessage", ErrorReporter.LastKeyReported);
				AssertEquals("Return: [System.Exception: Error during handle]", ErrorReporter.LastMessageReported);
				AssertEquals(unhandledException, ErrorReporter.LastExceptionReported);

				// Cleanup
				ErrorReporter.Clear();
			}
		}

		public void TestExceptionIsRecordedWhenMessageHandlersThrowedXmlException()
		{
			// Arrange
			MessageHandlers.Register(EnterpriseChannelMessageTypes.Test, new XmlMessageHandler_ForTest());
			const string message = $"{EnterpriseChannelMessageTypes.Test}Deserializing\u0015Failed\u0015Message";

			using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(message)))
			{
				// Act
				var exception = AssertExceptionThrown<InvalidOperationException>(() => MessageHandlers.HandleMessage(EnterpriseChannel.Instance, EnterpriseChannelMessageTypes.Test, ms));

				// Assert
				AssertEquals("RecordMessageHandlerException", ErrorReporter.LastKeyReported);
				AssertEquals($"Deserializing XML message failed with the message content [{message}]", ErrorReporter.LastMessageReported);
				AssertEquals(exception, ErrorReporter.LastExceptionReported);
			}

			// Cleanup
			ErrorReporter.Clear();
		}

		public void TestOperationCanceledExceptionOfSendErrorMessageWontBeReportedWhenSendingReturnCallbackMessageFails()
		{
			// Arrange
			var enterpriseChannel = new Mock<EnterpriseChannel>() { CallBase = true };
			enterpriseChannel.Protected().Setup("StartAsTask", ItExpr.IsAny<Action>()).Callback<Action>(x => x());
			AssertEquals(false, enterpriseChannel.Object.IsConnected);

			using (new DisposableAction(() => enterpriseChannel.Object.Close()))
			{
				// Act
				enterpriseChannel.Object.SendReturnCallbackMessage(Guid.NewGuid(), 12);

				// Assert
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestSessionMonitorStopsWorkingBeforeDisconnectingWhenClosingEnterpriseChannel()
		{
			// Arrange
			initializationEvent.WaitOne();
			AssertEquals(true, EnterpriseChannel.Instance.IsConnected);

			var sessionMonitorMessenger = new SessionMonitorMessenger(((MockWtsApi)WtsApi.Instance).SessionNotificationHandleDispatcher);
			EnterpriseChannel.Instance.OnDisconnecting += Instance_OnDisconnecting;

			// Act
			EnterpriseChannel.Instance.Close();

			// Assert
			AssertEquals(
				"Connection caused by RemoteConnect session switch should fail as session monitor shouldn't work any longer when closing",
				false,
				EnterpriseChannel.Instance.IsConnected);

			void Instance_OnDisconnecting(object sender, EventArgs e)
			{
				EnterpriseChannel.Instance.OnDisconnecting -= Instance_OnDisconnecting;
				AssertExceptionThrown<OperationCanceledException>(() => sessionMonitorMessenger.ReconnectServer()); // post RemoteConnect message to session monitor to let it connect
			}
		}

		public void TestShouldRecordInitialLogWhenCallShowDragDropTrackingInfoForm()
		{
			// Arrange
			var enterpriseChannelMock = new Mock<EnterpriseChannel>() { CallBase = true };
			var enterpriseChannel = enterpriseChannelMock.Object;
			using (new DisposableAction(() => enterpriseChannel.Close()))
			{
				// Act
				enterpriseChannel.ShowDragDropTrackingInfoForm();
				Application.DoEvents();

				// Assert
				AssertEquals("Form should is visible", true, enterpriseChannel.DragDropTrackingInfoForm.Visible);
				AssertContains("Should recorded initial logs", enterpriseChannel.ActivityLogger.ToString(), enterpriseChannel.DragDropTrackingInfoForm.GetTextBoxContent());
			}
		}

		public void TestReportOnceIfConnectDoesNotReportWhenApplicationExits()
		{
			// Arrange
			Thread thread = null;
			var dispatcher = ((MockWtsApi)WtsApi.Instance).SessionNotificationHandleDispatcher;
			var sessionMonitorMessenger = new SessionMonitorMessenger(dispatcher);
			var goExitApplication = new ManualResetEvent(false);
			EnterpriseChannel.OnSendingMessage += EnterpriseChannel_OnSendingMessage;
			sessionMonitorMessenger.DisconnectServerAsync();

			// Act
			sessionMonitorMessenger.ReconnectServerAsync();
			goExitApplication.WaitOne();
			EnterpriseChannel.Instance.ApplicationThreadExit();

			dispatcher.Thread.Join();

			// Assert
			AssertEquals("ErrorReporter should not triggered", 0, ErrorReporter.TotalErrorCount);

			void EnterpriseChannel_OnSendingMessage(object sender, byte[] e)
			{
				if (EnterpriseChannel.Serialize(EnterpriseChannelMessageTypes.ServerMessageLoopReady, Array.Empty<byte>()).SequenceEqual(e))
				{
					// give send method some time to run to end
					thread = new Thread(() =>
					{
						Thread.Sleep(10 * 1000);
						goExitApplication.Set();
					});
					thread.Start();
				}
			}

			thread?.Join();
		}

		[ExpectNoExceptions]
		public void TestNotReportErrorWhenAnyChunksCombinationParsingXmlFailsOnlyForDragDropMessage()
		{
			// Arrange.
			var reporterMock = new Mock<IErrorReporter>();
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			initializationEvent.WaitOne();

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				// Act
				ExecuteOutOfOrderChunksScenario(api, TestSamples.OutOfOrderMiddleAndLastChunksButRecoverable);

				// Assert
				reporterMock.VerifyNoOtherCalls();
			}
		}

		#region Helpers

		void ExecuteOutOfOrderChunksScenario(MockWtsApi api, DragDropSample sample)
		{
			string[] messages = sample
				.CreateChunkedMessages()
				.ToArray();

			api.InjectRawMessagesToReadLoop(messages);
		}

		void AssertWarningMessageWhenHandlingOutOfOrderChunks(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			ErrorReporter.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessages();
		}

		string CreateSingleChunkEncodedMessage(string message)
		{
			byte[] buffer = new byte[message.Length + 8]; // Mimic CHANNEL_PDU_LENGTH.

			Array.Copy(BitConverter.GetBytes(message.Length), 0, buffer, 0, 4);
			Array.Copy(BitConverter.GetBytes((int)WtsApi.ChannelFlags.Only), 0, buffer, 4, 4);
			Array.Copy(Encoding.ASCII.GetBytes(message), 0, buffer, 8, message.Length);

			return Convert.ToBase64String(buffer);
		}

		class MessageChannelForTest : MessageChannel
		{
			public override bool IsConnected => true;
			public override bool Send(byte[] data) => true;
			protected override void StartAsTask(Action action) { }
			protected override void OnErrorDuringSendingReturnCallbackMessage(string returnTypeInfo, Exception unexpectedException)
			{
				throw new NotImplementedException();
			}

			protected override void RecordMessageHandlerException(Exception exception, string message)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		static readonly DragDropSamples TestSamples = new DragDropSamples();

		class XmlMessageHandler_ForTest : XmlMessageHandler<string>
		{
			protected override void Handle(IEnterpriseChannel channel, string message)
			{
			}
		}
	}
}
