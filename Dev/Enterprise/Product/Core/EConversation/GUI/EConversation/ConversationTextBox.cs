using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.EConversation.GUI
{
	public class ConversationTextBox : KRichTextBox
	{
		#if !WINZOR

		const int WM_MOUSEWHEEL = 0x020A;
		const int WM_HSCROLL = 0x114;
		const int WM_VSCROLL = 0x115;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEWHEEL && ScrollBars == RichTextBoxScrollBars.None)
			{
				NativeMethods.SendMessage(Parent.Handle, m.Msg, m.WParam, m.LParam);
				m.Result = IntPtr.Zero;
			}
			else
			{
				// https://stackoverflow.com/questions/13660530/ws-ex-composited-and-scrollbar-not-drawing-whilst-being-dragged
				if ((m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL) && (((int)m.WParam & 0xFFFF) == 5))
				{
					// Change SB_THUMBTRACK to SB_THUMBPOSITION
#if NETFRAMEWORK
					m.WParam = (IntPtr)(((int)m.WParam & ~0xFFFF) | 4);
#else
					m.WParam = (((int)m.WParam & ~0xFFFF) | 4);
#endif
				}

				base.WndProc(ref m);
			}
		}

		static class NativeMethods
		{
			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		}

		#endif
	}
}
