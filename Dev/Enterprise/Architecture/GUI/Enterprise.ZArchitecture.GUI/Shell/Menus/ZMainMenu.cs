using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZMainMenu : MainMenu
	{
		public ZMainMenu() { }

		public ZMainMenu(IContainer container)
			: base(container)
		{
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (CmdKeyPressed != null)
			{
				CmdKeyPressed(this, new CmdKeyEventArgs(msg, keyData));
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		public event EventHandler<CmdKeyEventArgs> CmdKeyPressed;
	}

	public class CmdKeyEventArgs : EventArgs
	{
		public CmdKeyEventArgs(Message msg, Keys keyData)
		{
			Msg = msg;
			KeyData = keyData;
		}

		public Message Msg { get; private set; }
		public Keys KeyData { get; private set; }
	}
}
