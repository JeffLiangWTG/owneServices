using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public interface IMessageBox
	{
		DialogResult Show(string text, string caption, MessageBoxButtons buttons);
		DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
	}
}
