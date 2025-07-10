using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class OverrideReceiptPaymentCashFlowCategoryForm : OverrideReceiptPaymentDetailsForm
	{
		public OverrideReceiptPaymentCashFlowCategoryForm(OverrideReceiptPaymentCashFlowCategoryHelper bo)
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
