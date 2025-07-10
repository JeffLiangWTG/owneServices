using System.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public class BankReconDirectPaymentForm : DirectPaymentForm, IDoDisplayModeEditOverride
	{
		public BankReconDirectPaymentForm(BankReconDirectPayment directPayment) : base(directPayment)
		{
			InitializeComponent();
		}

		void IDoDisplayModeEditOverride.DoDisplayModeEdit()
		{
			fPostButton.Visible = false;
			fPostButton.Enabled = false;
			fApplyButton.Visible = false;
			fApplyButton.Enabled = false;
			fCancelButton.Text = AccountingConstants.CloseButtonText;
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
		}
	}
}
