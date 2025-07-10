using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APCashAdvanceViewForm : ZForm
	{
		public APCashAdvanceViewForm(CashAdvanceRequestHeader cashAdvanceHeader) : base(cashAdvanceHeader)
		{
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
			ParentCashAdvanceHeader = cashAdvanceHeader;
		}

		ZDateEdit dateCreatedTextBox;
		ZTextBox statusTextBox;
		ZCalcEdit TotalInvoiceAmountIncGSTCalcEdit;
		ZCalcEdit GSTCostTaxAmountCalcEdit;
		ZTextBox TotalAmtOnInvoiceForJobCurrencyTextBox;
		ZTextBox GSTTaxCurrencyTextBox;
		internal readonly CashAdvanceRequestHeader ParentCashAdvanceHeader;
		bool isCashAdvanceRequestCreated;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetReadOnlyIncludingChildren();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			this.ValidateAll(ValidationType.Full);
			if (ParentCashAdvanceHeader.HasErrors)
			{
				ShowErrorsDialog();
				return ContinueWithSave.No;
			}
			else
			{
				ParentCashAdvanceHeader.Factory.Save();
				isCashAdvanceRequestCreated = true;
				return ContinueWithSave.Yes;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (isCashAdvanceRequestCreated)
			{
				DialogResult = System.Windows.Forms.DialogResult.Yes;
			}
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

