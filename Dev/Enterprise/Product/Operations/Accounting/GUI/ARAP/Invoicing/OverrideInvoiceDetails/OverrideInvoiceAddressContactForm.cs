using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceAddressContactForm : OverrideInvoiceDetailsForm
	{
		public OverrideInvoiceAddressContactForm(OverrideInvoiceAddressContactHelper bo)
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
