using System;
using System.Collections.Concurrent;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public class WtsSessionChangeMessageHandlerTest : TestCase
	{
		public void TestGetSessionSwitchReason()
		{
			AssertEquals(SessionSwitchReason.ConsoleConnect, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x1));
			AssertEquals(SessionSwitchReason.ConsoleDisconnect, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x2));
			AssertEquals(SessionSwitchReason.RemoteConnect, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x3));
			AssertEquals(SessionSwitchReason.RemoteDisconnect, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x4));
			AssertEquals(SessionSwitchReason.SessionLogon, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x5));
			AssertEquals(SessionSwitchReason.SessionLogoff, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x6));
			AssertEquals(SessionSwitchReason.SessionLock, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x7));
			AssertEquals(SessionSwitchReason.SessionUnlock, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x8));
			AssertEquals(SessionSwitchReason.SessionRemoteControl, WtsSessionChangeMessageHandler.GetSessionSwitchReason(0x9));
		}

		public void TestIsWtsSessionChange()
		{
			AssertEquals(true, WtsSessionChangeMessageHandler.IsWtsSessionChange(WM_WTSSESSION_CHANGE));
			AssertEquals(false, WtsSessionChangeMessageHandler.IsWtsSessionChange(NonSessionChangeMessageCode));
		}

		public void TestCallbackOnSessionSwitch_ShouldBeCalledForSessionSwitchMessage()
		{
			var callback = new CallbackForTest(SessionSwitchReason.SessionRemoteControl);
			var callbacks = new ConcurrentDictionary<Guid, IWtsSessionSwitchCallback>();
			callbacks.TryAdd(Guid.NewGuid(), callback);
			var serverHandler = new WtsSessionChangeMessageHandler(callbacks);
			AssertEquals(SessionSwitchReason.SessionRemoteControl, callback.LastReason);

			serverHandler.WndProc(WM_WTSSESSION_CHANGE, new IntPtr(WTS_SESSION_LOCK));
			AssertEquals(SessionSwitchReason.SessionLock, callback.LastReason);

			serverHandler.WndProc(NonSessionChangeMessageCode, new IntPtr(WTS_REMOTE_DISCONNECT));
			AssertEquals("The callback's reason shouldn't have changed because the message was a non-session switching message, and yet...", SessionSwitchReason.SessionLock, callback.LastReason);
		}

		class CallbackForTest : IWtsSessionSwitchCallback
		{
			public SessionSwitchReason LastReason { get; set; }

			public CallbackForTest(SessionSwitchReason defaultReason)
			{
				LastReason = defaultReason;
			}

			public void OnSessionSwitch(SessionSwitchReason reason)
			{
				LastReason = reason;
			}

			public void OnSessionInitialConnect()
			{
			}
		}

		const int WM_WTSSESSION_CHANGE = 0x02B1;
		const int WTS_REMOTE_DISCONNECT = 0x4;
		const int WTS_SESSION_LOCK = 0x7;
		const int NonSessionChangeMessageCode = 0x02B2;
	}
}
