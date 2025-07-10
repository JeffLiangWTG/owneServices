using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEProcessQueuePlugin : ProcessQueuePlugIn
	{
		public UPEProcessQueuePlugin(IProcessQueueParent processQueueParent)
			: base(processQueueParent)
		{
		}

		protected override Control GetNewUserControl()
		{
			Control result = new UPEProcessQueueUserControl();
			result.Dock = DockStyle.Fill;
			return result;
		}
	}
}
