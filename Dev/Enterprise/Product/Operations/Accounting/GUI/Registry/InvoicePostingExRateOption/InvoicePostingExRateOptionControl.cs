using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoicePostingExRateOptionControl : RegistryZUserControl
	{
		public InvoicePostingExRateOptionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			InvoicePostingExRateOptionGrid.ReadOnly = readOnly;
		}
	}
}
