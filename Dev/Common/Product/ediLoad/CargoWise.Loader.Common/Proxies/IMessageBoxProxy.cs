using System.Windows.Forms;

namespace CargoWise.Loader.Common
{
	public interface IMessageBoxProxy
	{
		DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
		DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
		DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton);
		DialogResult ShowDialog(Form form);
	}
}