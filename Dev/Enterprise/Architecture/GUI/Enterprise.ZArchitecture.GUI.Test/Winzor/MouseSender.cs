using System;
using System.Windows.Forms;
using CargoWise.Interop;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class MouseSender
	{
		public static void SendMessage(object handleOwner, IntPtr handle, int msg, nint wParam, nint lParam)
		{
			ExtractMousePosition(lParam, out var x, out var y);
			switch (msg)
			{
				case WindowsMessage.WM_LBUTTONDOWN:
					(handleOwner as Control)?.InvokeMouseEvent("MouseDown", new MouseEventArgs(MouseButtons.Left, 1, x, y, 0));
					break;
				case WindowsMessage.WM_LBUTTONUP:
					(handleOwner as Control)?.InvokeMouseEvent("MouseUp", new MouseEventArgs(MouseButtons.Left, 1, x, y, 0));
					break;
				case WindowsMessage.WM_RBUTTONDOWN:
					(handleOwner as Control)?.InvokeMouseEvent("MouseDown", new MouseEventArgs(MouseButtons.Right, 1, x, y, 0));
					break;
				default:
					throw new NotImplementedException($"The message is {msg}. Only WM_LBUTTONDOWN, WM_LBUTTONUP and WM_RBUTTONDOWN are supported now.");
			}
		}

		public static void PostMessage(object handleOwner, IntPtr handle, int msg, nint x, nint y)
		{
			WinzorDispatcher.Current.Queue(() =>
			{
				SendMessage(handleOwner, handle, msg, x, y);
			});
		}

		static void ExtractMousePosition(IntPtr lParam, out int x, out int y)
		{
			unchecked
			{
				x = (short)(lParam.ToInt32() & 0xFFFF);
				y = (short)(lParam.ToInt32() >> 16);
			}
		}
	}
}
