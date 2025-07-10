using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APCashAdvanceNewForm : ZForm
	{
		public APCashAdvanceNewForm(ChargeWithCost charge) : base(charge)
		{
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
			ParentCharge = charge;
		}

		ZTextBox dateCreatedTextBox;
		ZTextBox statusTextBox;
		ZCalcEdit TotalInvoiceAmountIncGSTCalcEdit;
		ZCalcEdit GSTCostTaxAmountCalcEdit;
		ZTextBox TotalAmtOnInvoiceForJobCurrencyTextBox;
		ZTextBox GSTTaxCurrencyTextBox;
		internal readonly ChargeWithCost ParentCharge;
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
			if (ParentCharge.HasErrors)
			{
				ShowErrorsDialog();
				return ContinueWithSave.No;
			}
			else
			{
				var requestor = new APCashAdvanceRequestor(ParentCharge);
				requestor.GenerateRequest();
				ParentCharge.Factory.Save();
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

