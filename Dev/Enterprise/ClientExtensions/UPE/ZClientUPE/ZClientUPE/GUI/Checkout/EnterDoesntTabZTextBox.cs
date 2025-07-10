using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.GUI
{
	public class EnterDoesntTabZTextBox : ZTextBox
	{
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return keyData != Keys.Enter && base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
