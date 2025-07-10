using System;
using System.Diagnostics;
using static WTG.TestHelpers.IISExpress.NativeMethods;

namespace WTG.TestHelpers.IISExpress
{
	static class IISExpressStopper
	{
		public static void SendStopMessage(Process process)
		{
			try
			{
				for (var hwnd = GetTopWindow(IntPtr.Zero); hwnd != IntPtr.Zero; hwnd = GetWindow(hwnd, GW_HWNDNEXT))
				{
					_ = GetWindowThreadProcessId(hwnd, out var windowThreadPID);

					if (windowThreadPID == process.Id)
					{
						PostMessage(hwnd, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
						return;
					}
				}
			}
			catch (ArgumentException)
			{
			}
		}
	}
}
