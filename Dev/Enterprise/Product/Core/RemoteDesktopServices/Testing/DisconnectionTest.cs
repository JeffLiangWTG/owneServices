using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DisconnectionTest : RemoteDesktopServicesTest
	{
		public void TestServerDisconnect()
		{
			// Arrange
			Assert(RemoteFile.IsSupported);

			SessionSwitchEventArgs sessionSwitchEventArgs = null;
			EnterpriseChannel.Instance.ServerSessionSwitch += (sender, e) =>
			{
				sessionSwitchEventArgs = e;
			};

			var sessionMonitorMessenger = new SessionMonitorMessenger(((MockWtsApi)WtsApi.Instance).SessionNotificationHandleDispatcher);

			// Act
			sessionMonitorMessenger.DisconnectServer();

			// Assert
			AssertEquals(SessionSwitchReason.RemoteDisconnect, sessionSwitchEventArgs.Reason);
			AssertExceptionThrown(typeof(OperationCanceledException), "Sending to the remote desktop failed", () => CheckFileExists());
		}

		public void TestServerReconnect()
		{
			// Arrange
			var previousRemoteAppWaitingForReconnectionTimeoutInSeconds = EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds;
			using (new DisposableAction(
				() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = 5,
				() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = previousRemoteAppWaitingForReconnectionTimeoutInSeconds))
			{
				SessionSwitchEventArgs sessionSwitchEventArgs = null;
				EnterpriseChannel.Instance.ServerSessionSwitch += (sender, e) =>
				{
					sessionSwitchEventArgs = e;
				};

				var sessionMonitorMessenger = new SessionMonitorMessenger(((MockWtsApi)WtsApi.Instance).SessionNotificationHandleDispatcher);
				sessionMonitorMessenger.DisconnectServer();

				// Act
				sessionMonitorMessenger.ReconnectServer();

				// Assert
				AssertEquals(SessionSwitchReason.RemoteConnect, sessionSwitchEventArgs.Reason);
				Assert("can query after reconnect", CheckFileExists());
			}
		}

		public void TestReconnectAfterChannelReadOperationCanceled()
		{
			AssertReconnectAfterReadException(new OperationCanceledException());
		}

		public void TestReconnectAfterChannelReadOperationIOException()
		{
			AssertReconnectAfterReadException(new IOException("The specified network name is no longer available.", EnterpriseChannel.ERROR_NETNAME_DELETED));
		}

		void AssertReconnectAfterReadException(Exception ex)
		{
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.lastStream.ReadException = ex;
			api.callsToVirtualChannelGetStream = 0;

			EnterpriseChannel.Instance.ReadLoopStartEventForTest.Reset();
			AssertExceptionThrown(typeof(OperationCanceledException), () => CheckFileExists());
			EnterpriseChannel.Instance.ReadLoopStartEventForTest.WaitOne(TimeSpan.FromSeconds(30));
			Application.DoEvents();
			AssertEquals("channel reconnected", 1, api.callsToVirtualChannelGetStream);
			Assert("can query after exception reconnect", CheckFileExists());
		}

		bool CheckFileExists()
		{
			string filePath = TempForTest.GetTempFileName();
			try
			{
				var doesFileExists = EnterpriseChannel.Instance.SendMessage<string, bool>(EnterpriseChannelMessageTypes.CheckFileExists, filePath);
				return doesFileExists;
			}
			finally
			{
				try
				{
					File.Delete(filePath);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}
	}

	class DisconectionDuringInitializeTest : RemoteDesktopServicesTest
	{
		[ExpectNoExceptions]
		public void TestSendFailsDuringInitialize()
		{
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.VirtualChannelStreamCreated += (o, e) => api.MakeFileHandleInaccessible();
			InitializeServer(waitForInitialization: false);
		}

		[DeveloperOnlyTest]
		public void TestReadOperationIOExceptionHandledDuringIntialization()
		{
			var previousRemoteAppWaitingForReconnectionTimeoutInSeconds = EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds;
			using (new DisposableAction(
				() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = 5,
				() => EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = previousRemoteAppWaitingForReconnectionTimeoutInSeconds))
			{
				var initMessageHandler = new MockInitializationMessageHandler();
				var initializationThread = new Thread(() =>
					EnterpriseChannel.Initialize(initMessageHandler));
				initializationThread.Start();
				try
				{
					initMessageHandler.HandleCalledEvent.WaitOne();
					Assert("Read loop started", EnterpriseChannel.Instance.ReadLoopStartEventForTest.WaitOne(TimeSpan.FromSeconds(30)));
					MockWtsApi api = (MockWtsApi)WtsApi.Instance;
					api.lastStream.ReadException = new IOException("No process is on the other end of the pipe.\r\n");
					api.callsToVirtualChannelGetStream = 0;
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					do
					{
						Thread.Sleep(100);
					}
					while (api.callsToVirtualChannelGetStream == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(25));
					initializationThread.Join();

					Assert("channel reconnected", api.callsToVirtualChannelGetStream > 0);
					Assert("can query after exception reconnect", !RemoteFile.IsSupported);
				}
				finally
				{
					InitializationMessageHandler.InitializationCompleted.Set();
				}
			}
		}

		class MockInitializationMessageHandler : XmlMessageHandler<InitializationMessage>
		{
			protected override void Handle(IEnterpriseChannel channel, InitializationMessage message)
			{
				HandleCalledEvent.Set();
			}

			public ManualResetEvent HandleCalledEvent = new ManualResetEvent(false);
		}

		protected override void SetUp()
		{
			SetupMock();
			InitializationMessageHandler.InitializationCompleted.Reset();
		}
	}
}
