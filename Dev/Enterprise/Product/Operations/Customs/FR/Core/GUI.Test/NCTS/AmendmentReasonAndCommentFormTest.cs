using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(AmendmentReasonAndCommentForm))]
	class AmendmentReasonAndCommentFormTest : ZFormBasherTest
	{
		public void TestOKButton_ClickWhenThereAreErrorsInBizO()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var testForm = new AmendmentReasonAndCommentFormForTest(AmendmentReasonAndComment))
			{
				AmendmentReasonAndComment.ReasonText = "";
				AmendmentReasonAndComment.CommentText = "comment";
				AssertEquals("PreCondition: Change reason should be there", true, AmendmentReasonAndComment.ReasonTextInfo.HasErrors());
				AssertEquals("PreCondition: Change comment should not be there", false, AmendmentReasonAndComment.CommentTextInfo.HasErrors());

				testForm.OKButton_Click(testForm.OKButton, EventArgs.Empty);
				ZString warningMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Error should have shown", true, warningMessage.Contains("Please enter a reason and a comment for the amendment or withdrawal."));
				AssertEquals("No result, should not proceed.", DialogResult.None, testForm.DialogResult);

				AmendmentReasonAndComment.ReasonText = "reason";
				AmendmentReasonAndComment.CommentText = "";
				AssertEquals("PreCondition: Change reason should not be there", false, AmendmentReasonAndComment.ReasonTextInfo.HasErrors());
				AssertEquals("PreCondition: Change comment should be there", false, AmendmentReasonAndComment.CommentTextInfo.HasErrors());

				AmendmentReasonAndComment.ReasonText = "reason";
				AmendmentReasonAndComment.CommentText = "comment";
				AssertEquals("PreCondition: Change reason should not be there", false, AmendmentReasonAndComment.ReasonTextInfo.HasErrors());
				AssertEquals("PreCondition: Change comment should not be there", false, AmendmentReasonAndComment.CommentTextInfo.HasErrors());

				testForm.OKButton_Click(testForm.OKButton, EventArgs.Empty);
				AssertEquals("IsOKToSendAMessage", false, AmendmentReasonAndComment.IsCancelled);
				AssertEquals("If this fails, please amend MYCustomsCargoManifestPlugInToConsol.DeclareManifest. It expects DialogResult.OK to proceed and declare", DialogResult.OK, testForm.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return (Form)Activator.CreateInstance(FormToBashType, new object[] { AmendmentReasonAndComment });
		}

		protected virtual AmendmentWithdrawalReasonAndComment GetNewAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReasonAndComment();
		}

		protected AmendmentWithdrawalReasonAndComment AmendmentReasonAndComment
		{
			get
			{
				if (fAmendmentReasonAndComment == null)
				{
					fAmendmentReasonAndComment = GetNewAmendmentWithdrawalReason();
				}
				return fAmendmentReasonAndComment;
			}
		}
		AmendmentWithdrawalReasonAndComment fAmendmentReasonAndComment;
	}

	class AmendmentReasonAndCommentFormForTest : AmendmentReasonAndCommentForm
	{
		public AmendmentReasonAndCommentFormForTest(AmendmentWithdrawalReasonAndComment amendmentWithdrawalReasonAndComment) : base(amendmentWithdrawalReasonAndComment)
		{
		}
		
		public new ZArchitecture.GUI.ZButton OKButton => base.OKButton;

		public new void OKButton_Click(object sender, EventArgs e) => base.OKButton_Click(sender, e);
	}
}
