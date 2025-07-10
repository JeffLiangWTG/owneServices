using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class SessionSwitchHandlerTest : TestCase
	{
		public void TestHandle()
		{
			var callback = new SessionSwitchCallbackForTest(SessionSwitchReason.SessionRemoteControl);
			var handler = new SessionSwitchHandlerForTest(callback);

			handler.Handle_Exposed(new SessionSwitchMessage(SessionSwitchReason.ConsoleConnect));

			AssertEquals(SessionSwitchReason.ConsoleConnect, callback.LastReason);
		}

		class SessionSwitchCallbackForTest : IWtsSessionSwitchCallback
		{
			public SessionSwitchCallbackForTest(SessionSwitchReason defaultReason)
			{
				LastReason = defaultReason;
			}

			public SessionSwitchReason LastReason { get; set; }

			public void OnSessionInitialConnect()
			{
			}

			public void OnSessionSwitch(SessionSwitchReason reason)
			{
				LastReason = reason;
			}
		}

		class SessionSwitchHandlerForTest : SessionSwitchHandler
		{
			public SessionSwitchHandlerForTest(IWtsSessionSwitchCallback callback)
				: base(callback)
			{
			}

			public void Handle_Exposed(SessionSwitchMessage message)
			{
				Handle(EnterpriseChannel.Instance, message);
			}
		}
	}
}
