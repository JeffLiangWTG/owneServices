#if NETFRAMEWORK
using System;
#endif
using CargoWise.EntityFramework.Testing;
using Enterprise.RemoteDesktopServices.Client;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class RequestKeepAliveTest : TestCaseWithFactory
	{
		public void TestPurgeStaleWindow()
		{
			AssertNoExceptionThrown(() =>
			{
#if NETFRAMEWORK
				KeepAlive.AddWindowHandle((IntPtr)1);
#else
				KeepAlive.AddWindowHandle(1);
#endif
				KeepAlive.SendWigglesToRailWindows();
			});
		}
	}
}
