using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceRemittanceTypeForm : OverrideInvoiceDetailsForm
	{
		public OverrideInvoiceRemittanceTypeForm(OverrideInvoiceRemittanceTypeHelper bo)
			: base(bo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
