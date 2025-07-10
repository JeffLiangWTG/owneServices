using System;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using WinzorFramework;

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
		public static void SendKeyPress(object handleOwner, IntPtr handle, char keyChar)
		{
			// The KeyPress event has been deprecated in modern browsers.
			// However, we simulate it here to ensure WinForms tests pass in Winzor.
			var key = SendKeys.GetKeyFromChar(keyChar);
			var control = (handleOwner as Control);
			var keyCode = (key & Keys.KeyCode);
			control.ProcessKeyCharMessage(keyCode, keyChar.ToString(), (key & Keys.Modifiers) == Keys.Alt);
			if (control is TextBox textBox)
			{
				SendKeys.SimulateBrowserOnInput(keyCode, keyChar.ToString(), control, textBox.Text, textBox.SelectionStart, textBox.SelectionLength);
			}
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
			var keyCode = (key & Keys.KeyCode);
			var keyChar = (char)keyCode;
			SendKeyPress(handleOwner, handle, keyChar);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A Keys Enum member representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyDown(object handleOwner, IntPtr handle, Keys key)
		{
			var control = (handleOwner as Control);
#pragma warning disable CW1118 // Use Keys.KeyCode and Keys.Modifiers Bitmask
			SendKeys.SimulateBrowserKeyDown(key & Keys.KeyCode, control, (key & Keys.Shift) == Keys.Shift);
#pragma warning restore CW1118 // Use Keys.KeyCode and Keys.Modifiers Bitmask
			control.InvokeProcessKeyEvent("KeyDown", key.ToString(), key, false);
		}

		public static void SendKeyUp(object handleOwner, IntPtr handle, Keys key)
		{
			var control = (handleOwner as Control);
			control.InvokeProcessKeyEvent("KeyUp", key.ToString(), key, false);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Control.
		/// </summary>
		/// <param name="handleOwner">The owner of the handle, to be used as part of a HandleRef to prevent unexpected garbage collection of the owner while message is sent.</param>
		/// <param name="handle">The handle to the destination control.</param>
		/// <param name="key">A char representing the key for which to simulate a WM_KEYPRESS message.</param>
		public static void SendKeyDown(object handleOwner, IntPtr handle, char key)
		{
			SendKeyDown(handleOwner, handle, (Keys)key);
		}

		/// <summary>
		/// Send the WM_KEYDOWN message to a Controls ProcessCmdKey method
		/// </summary>
		/// <param name="Control">Target Control</param>
		/// <param name="Key">An int representing the key for which to simulate a WM_KEYDOWN message.</param>
		public static void SendKeyDownToProcessCmdKey(Control control, int key)
		{
			var message = GetKeyDownMessage(control.Handle, (Keys)key);
			control.PreProcessMessage(ref message, (Keys)key);
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
			PostKeyDown(handleOwner as Control, key);
		}

		public static void PostKeyDown(Control handleOwner, Keys key)
		{
			WinzorDispatcher.Current.Queue(() =>
			{
				SendKeys.SendInput(handleOwner, key);
			});
		}

		public static void PostKeyUp(object handleOwner, IntPtr handle, Keys key)
		{
			WinzorDispatcher.Current.Queue(() =>
			{
				SendKeyUp(handleOwner, handle, key);
			});
		}

		public static Message GetKeyDownMessage(IntPtr handle, Keys key)
		{
			return new Message { HWnd = handle, Msg = WindowsMessage.WM_KEYDOWN, WParam = (IntPtr)key, LParam = IntPtr.Zero };
		}

		public static Message GetKeyUpMessage(IntPtr handle, Keys key)
		{
			return new Message { HWnd = handle, Msg = WindowsMessage.WM_KEYUP, WParam = (IntPtr)key, LParam = IntPtr.Zero };
		}

		public static Message GetKeyCharMessage(IntPtr handle, Keys key)
		{
			return new Message { HWnd = handle, Msg = WindowsMessage.WM_CHAR, WParam = (IntPtr)key, LParam = IntPtr.Zero };
		}
	}
}
