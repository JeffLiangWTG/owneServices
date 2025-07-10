using System.Windows.Forms;

namespace Enterprise.FaxRouter.Processor
{
	public partial class EventProgress : Form
	{
		public EventProgress()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void UpdateEventProgress(int stepSize)
		{
			Application.DoEvents();
		}
	}
}
