using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Enterprise.Upgrades.UnitTestHelper
{
	public static class Win32Native
	{
		public const int WM_CLOSE = 0x10;

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		public static extern Int32 PostMessage(IntPtr hWnd, Int32 wMsg, IntPtr wParam, IntPtr lParam);

		public static void FindInstallationResultsFormAndClose(string windowTitle, CancellationToken cancellationToken)
		{
			var hWnd = FindWindow(null, windowTitle);
			while (hWnd == IntPtr.Zero && !cancellationToken.IsCancellationRequested)
			{
				Thread.Sleep(100);
				hWnd = FindWindow(null, windowTitle);
			}

			if (hWnd != IntPtr.Zero)
			{
				PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			}
		}
	}
}
