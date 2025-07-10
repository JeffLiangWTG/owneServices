using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CargoWise.Windows.UI.Interop
{
	[SuppressUnmanagedCodeSecurity]
	internal static class SafeNativeMethods
	{
		#region Windows GUI

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowEnabled(HandleRef hWnd);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(HandleRef hWnd);

		[DllImport("user32.dll")]
		public static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern bool ReleaseCapture();

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern bool PatBlt(HandleRef hdc, int left, int top, int width, int height, int rop);

		#endregion

		#region Windows Message Hooking

		[DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern int GetCurrentThreadId();

		#endregion
	}
}
