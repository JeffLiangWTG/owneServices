#if DEBUG

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Class specialising in sending keys services for testing Core Controls.
	/// </summary>
	public static class KeySender
	{
		/// <summary>
		/// Send the WM_KEYPRESS message to a Control.
		/// </summary>
		/// <param name="control">The destination control</param>
		/// <param name="key">A char representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyPress(Control control, char key)
		{
			SendKeyPress(control, control.Handle, key);
		}

		/// <summary>
		/// Send the WM_KEYPRESS message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A char representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyPress(object handleOwner, IntPtr handle, char key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYPRESS, new IntPtr(key), IntPtr.Zero);
		}

		/// <summary>
		/// Send the WM_KEYPRESS message to a Control.
		/// </summary>
		/// <param name="control">The destination control</param>
		/// <param name="key">A char representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyPress(Control control, Keys key)
		{
			SendKeyPress(control, control.Handle, key);
		}

		/// <summary>
		/// Send the WM_KEYPRESS message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A Keys Enum member representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyPress(object handleOwner, IntPtr handle, Keys key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYPRESS, new IntPtr((int)key), IntPtr.Zero);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A Keys Enum member representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyDown(object handleOwner, IntPtr handle, Keys key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYDOWN, new IntPtr((int)key), IntPtr.Zero);
		}

		public static void SendKeyUp(object handleOwner, IntPtr handle, Keys key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYUP, new IntPtr((int)key), IntPtr.Zero);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A char representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyDown(object handleOwner, IntPtr handle, char key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYDOWN, new IntPtr(key), IntPtr.Zero);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Controls ProcessCmdKey method
		/// </summary>
		/// <param name="Control">Target Control</param>
		/// <param name="Key">An int representing the key for which to simulate a WM_KEYDOWN message.</param>
		public static void SendKeyDownToProcessCmdKey(Control control, int key)
		{
			Message m = Message.Create(control.Handle, WindowsMessage.WM_KEYDOWN, new IntPtr(key), new IntPtr(0));
			control.PreProcessMessage(ref m);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Controls ProcessCmdKey method
		/// </summary>
		/// <param name="control">Target Control</param>
		/// <param name="keys">A Keys Enum member representing the key(s) for which to simulate a WM_KEYDOWN message.</param>
		public static void SendKeyDownToProcessCmdKey(Control control, Keys keys)
		{
			SendKeyDownToProcessCmdKey(control, (int)keys);
		}

		/// <summary>
		/// Posts the WM_KEYDOWN message to a Control. Posted messages go into the queue of the windows message loop.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A Keys Enum member representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void PostKeyDown(object handleOwner, IntPtr handle, Keys key)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYDOWN, new IntPtr((int)key), new IntPtr(0));
		}

		public static void PostKeyDown(Control handleOwner, Keys key)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(handleOwner, handleOwner.Handle), WindowsMessage.WM_KEYDOWN, new IntPtr((int)key), new IntPtr(0));
		}

		public static void PostKeyUp(object handleOwner, IntPtr handle, Keys key)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(handleOwner, handle), WindowsMessage.WM_KEYUP, new IntPtr((int)key), new IntPtr(0));
		}

		public static Message GetKeyDownMessage(IntPtr handle, Keys key)
		{
#if NETFRAMEWORK
			return Message.Create(handle, WindowsMessage.WM_KEYDOWN, (IntPtr)key, (IntPtr)0);
#else
			return Message.Create(handle, WindowsMessage.WM_KEYDOWN, (nint)key, 0);
#endif
		}

		public static Message GetKeyUpMessage(IntPtr handle, Keys key)
		{
#if NETFRAMEWORK
			return Message.Create(handle, WindowsMessage.WM_KEYUP, (IntPtr)key, (IntPtr)0);
#else
			return Message.Create(handle, WindowsMessage.WM_KEYUP, (nint)key, 0);
#endif
		}

		public static Message GetKeyCharMessage(IntPtr handle, Keys key)
		{
#if NETFRAMEWORK
			return Message.Create(handle, WindowsMessage.WM_CHAR, (IntPtr)key, (IntPtr)0);
#else
			return Message.Create(handle, WindowsMessage.WM_CHAR, (nint)key, 0);
#endif
		}
	}
}

#endif
