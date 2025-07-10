using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEProcessQueueUserControl : ProcessQueueUserControl
	{
		protected override CurrentQueueUserControl GetCurrentQueueUserControl()
		{
			return new UPECurrentQueueUserControl();
		}
	}
}
