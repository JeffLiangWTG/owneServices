using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceAmountBoundariesControl : RegistryZUserControl
	{
		public InvoiceAmountBoundariesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AmountsGrid.ReadOnly = readOnly;
		}
	}
}
