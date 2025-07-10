using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideGovernmentAllocatedNumberForm : OverrideInvoiceDetailsForm
	{
		public OverrideGovernmentAllocatedNumberForm(OverrideGovernmentAllocatedNumberHelper bo)
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
