using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AlterReceiptForm : ReceiptForm
	{
		public AlterReceiptForm(Receipt receipt)
			: base(receipt)
		{
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.Yes;
		}

		public override void ShowOtherUsersCurrentlyAccessingThisEntity()
		{
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		protected override void SetupPostingButtons()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
			ReceiptDetailButton.Visible = false;
			ReceiptDetailButton.Enabled = false;
			PostWithoutMatchingButton.Visible = false;
			PostWithoutMatchingButton.Enabled = false;
			CloseButton.Text = Res.GetString("AlterReceiptForm|CloseButton", "OK");
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

