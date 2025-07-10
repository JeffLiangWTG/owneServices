using System;
using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	static class ControlTestExtensions
	{
		public static void SendCmdKey(this Control control, Keys key)
		{
			if (control != null)
			{
				const ushort WM_KEYDOWN = 0x100;

				var processCmdKey = control.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);

				var message = new Message();
				message.HWnd = control.Handle;
				message.Msg = WM_KEYDOWN;
				message.WParam = (IntPtr)key;

				processCmdKey.Invoke(control, new object[] { message, key });
			}
		}
	}
}