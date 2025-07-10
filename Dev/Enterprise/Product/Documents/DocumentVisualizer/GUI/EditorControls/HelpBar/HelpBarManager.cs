using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.GUI
{
	class HelpBarManager : IMessageFilter
	{
		HelpBarManager(IHaveHelpBar parent)
		{
			this.parent = parent;
		}

		internal static void AttachToParent(Control control)
		{
			var helpBarParent = control as IHaveHelpBar;

			if (helpBarParent != null)
			{
				var manager = new HelpBarManager(helpBarParent);
				Application.AddMessageFilter(manager);
				control.Disposed += (s, e) => Application.RemoveMessageFilter(manager);
			}
		}

		readonly IHaveHelpBar parent;

		const int WM_KEYDOWN = 0x0100;

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			if (m.Msg == WM_KEYDOWN)
			{
				var key = (Keys)m.WParam & Keys.KeyCode;

				if (key == Keys.F1)
				{
					if (parent.HelpBar == null)
					{
						HelpBarUserControl.AttachToParent(parent);
					}

					parent.HelpBar.Visible = !parent.HelpBar.Visible;
				}
				else if (parent.HelpBar != null)
				{
					parent.HelpBar.Visible = false;
				}
			}

			return false;
		}
	}
}
