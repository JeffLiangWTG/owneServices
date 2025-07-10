using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public class TransactionApprovalRequestValidationTest : GenApprovalRequestValidationTest
	{
		public void TestCheckApprovingUsersContainsDuplicate()
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.XP_GS_NKApprovingUser1 = "AD1";
			approvalRequest.XP_GS_NKApprovingUser2 = "AD2";
			approvalRequest.XP_GS_NKApprovingUser3 = "AD3";
			approvalRequest.XP_GS_NKApprovingUser4 = "AD4";
			approvalRequest.XP_GS_NKApprovingUser5 = "AD5";
			approvalRequest.XP_GS_NKApprovingUser6 = "AD6";
			AssertNoErrors(approvalRequest.XP_GS_NKApprovingUser2Info);
			AssertNoErrors(approvalRequest.XP_GS_NKApprovingUser3Info);
			AssertNoErrors(approvalRequest.XP_GS_NKApprovingUser4Info);
			AssertNoErrors(approvalRequest.XP_GS_NKApprovingUser5Info);
			AssertNoErrors(approvalRequest.XP_GS_NKApprovingUser6Info);
			approvalRequest.XP_GS_NKApprovingUser2 = "AD1";
			AssertHasError(approvalRequest.XP_GS_NKApprovingUser2Info, "Approving users can not be the same user.");
			approvalRequest.XP_GS_NKApprovingUser3 = "AD1";
			AssertHasError(approvalRequest.XP_GS_NKApprovingUser3Info, "Approving users can not be the same user.");
			approvalRequest.XP_GS_NKApprovingUser4 = "AD1";
			AssertHasError(approvalRequest.XP_GS_NKApprovingUser4Info, "Approving users can not be the same user.");
			approvalRequest.XP_GS_NKApprovingUser5 = "AD1";
			AssertHasError(approvalRequest.XP_GS_NKApprovingUser5Info, "Approving users can not be the same user.");
			approvalRequest.XP_GS_NKApprovingUser6 = "AD1";
			AssertHasError(approvalRequest.XP_GS_NKApprovingUser6Info, "Approving users can not be the same user.");
		}

		public void TestCheckXP_ReasonDescription()
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.XP_ReasonDescription = "A";
			AssertNoErrors(approvalRequest.XP_ReasonDescriptionInfo);

			approvalRequest.XP_ReasonDescription = "";
			AssertHasErrors(approvalRequest.XP_ReasonDescriptionInfo);
		}

		protected override GenApprovalRequestValidation GetNewValidation(GenApprovalRequest approvalRequest)
		{
			return new TransactionApprovalRequestValidation(approvalRequest);
		}

		protected override GenApprovalRequest GetNewParentBusinessObject()
		{
			return Factory.New<ARCreditNoteApprovalRequest>();
		}
	}
}