using System;

using System.Runtime.InteropServices;
using System.Text;

namespace Enterprise.DocumentScanning.Launch
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class WinAPIGeneralWindowsUtils
	{
		public static string GetWindowText(IntPtr windowPtr)
		{
			int length = GetWindowTextLength(windowPtr);
			StringBuilder sb = new StringBuilder(length + 1);
			GetWindowText(windowPtr, sb, sb.Capacity);
			return sb.ToString();
		}

		public static int GetWindowProcessID(IntPtr windowPtr)
		{
			int processID = 0;
			GetWindowThreadProcessId(windowPtr, ref processID);
			return processID;
		}

		public static IntPtr FindWindow(string className, string windowName)
		{
			return FindWindowWin32(className, windowName);
		}

		[DllImport("user32.dll")]
		static extern int GetWindowText(
			IntPtr window,
			[In][Out] StringBuilder text,
			int copyCount);

		[DllImport("user32.dll")]
		static extern bool SetWindowText(
			IntPtr window,
			[MarshalAs(UnmanagedType.LPTStr)]
			string text);

		[DllImport("user32.dll")]
		static extern int GetWindowTextLength(IntPtr window);

		[DllImport("user32.dll")]
		static extern int GetWindowThreadProcessId(IntPtr window, ref int processId);

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		static extern IntPtr FindWindowWin32(string className, string windowName);

		[DllImport("user32.dll")]
		public static extern bool BringWindowToTop(IntPtr window);

		[DllImport("user32.dll")]
		public static extern bool OpenIcon(IntPtr window);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr window);

		/*
		 * ShowWindow() Commands
		#define SW_HIDE             0
		#define SW_SHOWNORMAL       1
		#define SW_NORMAL           1
		#define SW_SHOWMINIMIZED    2
		#define SW_SHOWMAXIMIZED    3
		#define SW_MAXIMIZE         3
		#define SW_SHOWNOACTIVATE   4
		#define SW_SHOW             5
		#define SW_MINIMIZE         6
		#define SW_SHOWMINNOACTIVE  7
		#define SW_SHOWNA           8
		#define SW_RESTORE          9
		#define SW_SHOWDEFAULT      10
		#define SW_FORCEMINIMIZE    11
		#define SW_MAX              11
		*/

		public enum SW_Flags : int
		{
			Hide = 0,
			ShowNormal = 1,
			Normal = 1,
			ShowMinimized = 2,
			ShowMaximized = 3,
			Maximize = 3,
			ShowNoActivate = 4,
			Show = 5,
			Minimize = 6,
			ShowMinNoActive = 7,
			ShowNa = 8,
			Restore = 9,
			ShowDefault = 10,
			ForceMinimize = 11,
			Max = 11
		}

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr window, SW_Flags nCmdShow);

		[DllImport("user32.dll", ExactSpelling = false)]
		static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		/// <summary>
		/// Returns class name for the passed window handle.
		/// </summary>
		/// <param name="windowHandle"></param>
		/// <returns></returns>
		public static string GetClassName(IntPtr windowHandle)
		{
			StringBuilder sb = new StringBuilder(512);
			int count = GetClassName(windowHandle, sb, sb.Capacity);
			sb.Length = count;
			return sb.ToString();
		}
	}
}
