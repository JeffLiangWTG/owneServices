using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DraftTransactionStatusReasonCodeControl : RegistryZUserControl
	{
		public DraftTransactionStatusReasonCodeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DraftTransactionStatusReasonCodeGrid.ReadOnly = readOnly;
		}
	}
}

