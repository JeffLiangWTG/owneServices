using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceLineSequenceForm : OverrideInvoiceDetailsForm
	{
		public OverrideInvoiceLineSequenceForm(InvoiceLineOverrideForEditingSequenceAdaptor bo)
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
