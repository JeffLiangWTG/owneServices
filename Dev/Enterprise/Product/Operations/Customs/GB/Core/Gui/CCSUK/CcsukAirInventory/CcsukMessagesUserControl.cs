using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukMessagesUserControl : ZUserControl
	{
		public CcsukMessagesUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning) // otherwise it tries to queue the idle worker at design time and barfs
			{
				AddMenuAndQueueIdleWorker();
			}

			MessageDetailsTabControl.AllowOverlap(InterpretedMessageTextBox);

			InterpretedMessageTextBox.AllowOutsideOfParent();
			MessageDetailsTabControl.AllowOutsideOfParent();
		}
	}
}
