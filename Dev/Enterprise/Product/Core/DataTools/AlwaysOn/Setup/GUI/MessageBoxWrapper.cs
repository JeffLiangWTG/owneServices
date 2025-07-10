using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public class MessageBoxWrapper : IMessageBox
	{
		public DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			return MessageBox.Show(text, caption, buttons); // Cannot use Global.ShowMessage here as that requires Db.Connection which we are not using
		}

		public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return MessageBox.Show(text, caption, buttons, icon); // Cannot use Global.ShowMessage here as that requires Db.Connection which we are not using
		}
	}
}
