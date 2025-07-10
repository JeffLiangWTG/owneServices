using System.Windows.Forms;

namespace CargoWise.Loader.Common
{
	public sealed class MessageBoxProxy : IMessageBoxProxy
	{
		public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return MessageBox.Show(text, caption, buttons, icon);
		}

		public DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return MessageBox.Show(owner, text, caption, buttons, icon);
		}

		public DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
		}

		public DialogResult ShowDialog(Form form)
		{
			return form.ShowDialog();
		}
	}
}
