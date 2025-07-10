using Enterprise.Customs.FR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class AmendmentReasonAndCommentForm : AmendmentReasonForm
	{
		public AmendmentReasonAndCommentForm(AmendmentWithdrawalReasonAndComment amendmentWithdrawalReasonAndComment) : base(amendmentWithdrawalReasonAndComment)
		{
			this.amendmentWithdrawalReasonAndComment = amendmentWithdrawalReasonAndComment;
			InitializeComponent();
		}

		readonly AmendmentWithdrawalReasonAndComment amendmentWithdrawalReasonAndComment;

		public override string FormHeading
		{
			get { return Res.GetString("43F7BF59-0255-4C7B-8C9C-938C39459978", "Please enter a reason and a comment"); }
		}

		protected void OKButton_Click(object sender, System.EventArgs e)
		{
			amendmentWithdrawalReasonAndComment.RunPreSaveValidation();
			if (amendmentWithdrawalReasonAndComment.HasErrors)
			{
				amendmentWithdrawalReasonAndComment.IsCancelled = true;
				Globals.Message.ShowWarning(Res.GetString("DCD2C07D-4945-4BC3-BB57-4180478C382E", "Please enter a reason and a comment for the amendment or withdrawal."), "");
			}
			else
			{
				amendmentWithdrawalReasonAndComment.IsCancelled = false;
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}
	}
}
