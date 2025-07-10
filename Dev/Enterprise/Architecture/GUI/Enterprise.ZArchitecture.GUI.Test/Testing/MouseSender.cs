using System;
using System.Runtime.InteropServices;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class MouseSender
	{
		public static void SendMessage(object handleOwner, IntPtr handle, int msg, nint wParam, nint lParam)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), msg, wParam, lParam);
		}
		public static void PostMessage(object handleOwner, IntPtr handle, int msg, nint wParam, nint lParam)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(handleOwner, handle), msg, wParam, lParam);
		}
	}
}
