using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceCopiesControl : RegistryZUserControl
	{
		public InvoiceCopiesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			InvoiceCopiesGrid.ReadOnly = readOnly;
		}
	}
}

