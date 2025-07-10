using System.Windows.Forms;
using Enterprise.RemoteDesktopServices.Client;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	sealed class EmptyMessageBoxService : IMessageBoxService
	{
		public DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return DialogResult.OK;
		}
	}
}
