using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class CancelWorkflowForm : ZChildForm
	{
		public CancelWorkflowForm()
		{
			InitializeComponent();
		}

		public ZString CancellationReasonText
		{
			get { return cancellationReasonTextBox.Text; }
		}
	}
}
