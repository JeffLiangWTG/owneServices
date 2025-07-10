using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Enterprise.ActivityLogger
{
	/// <summary>
	/// No C# code should interact with these methods except those listed in SafeNativeMethods.cs
	/// If you want to use native methods/pInvoke, write a safe wrapper in SafeNativeMethods first.
	/// </summary>
	internal static class UnsafeNativeMethods
	{
		#region kernel32.dll

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		#endregion

		#region user32.dll

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int GetWindowTextLengthW(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern int GetWindowTextW(IntPtr hWnd, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, [In][MarshalAs(UnmanagedType.I4)] Int32 nMaxCount);

		#endregion

		#region Syshook.dll

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		[DllImport(SyshookInterop.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 WaitForBlock([In] Int32 dwMillis, ref HookMessage hookMessage);

		[DllImport(SyshookInterop.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 AttachHooks();

		[DllImport(SyshookInterop.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 DetachHooks();

		[DllImport(SyshookInterop.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		internal static extern Int32 GetSyshookPath([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer, [In][MarshalAs(UnmanagedType.I4)] Int32 iSize);

		[DllImport(SyshookInterop.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 GetMaxPath();

		#endregion
	}
}
