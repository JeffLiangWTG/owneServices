using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public class ARTransactionApprovalRequestValidationTest : TransactionApprovalRequestValidationTest
	{
		public void TestCheckEmptyXP_ReasonCode()
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.XP_ReasonCode = "IOB";
			AssertNoErrors(approvalRequest.XP_ReasonCodeInfo);
			approvalRequest.XP_ReasonCode = "";
			AssertHasErrors(approvalRequest.XP_ReasonCodeInfo);
		}

		public void TestCheckInvalidXP_ReasonCode()
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.XP_ReasonCode = "IOB";
			AssertNoErrors(approvalRequest.XP_ReasonCodeInfo);
			approvalRequest.XP_ReasonCode = "UNKNOWN";
			AssertHasErrors(approvalRequest.XP_ReasonCodeInfo);
		}

		public void TestValidationIsNotRunForChildRequest()
		{
			var expectedErrorMessageForReasonCode = "Please enter a Reason Code.";
			var expectedErrorMessageForDescription = "Please enter a Description.";
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.XP_ParentTableCode = GenApprovalRequestSchema.Constants.Prefix;
			approvalRequest.XP_ReasonCode = "";
			approvalRequest.XP_ReasonDescription = "";
			AssertNoError(approvalRequest.XP_ReasonCodeInfo, expectedErrorMessageForReasonCode);
			AssertNoError(approvalRequest.XP_ReasonDescriptionInfo, expectedErrorMessageForDescription);
			approvalRequest.XP_ParentTableCode = JobHeaderSchema.Constants.Prefix;
			approvalRequest.XP_ReasonCode = "";
			approvalRequest.XP_ReasonDescription = "";
			AssertHasError(approvalRequest.XP_ReasonCodeInfo, expectedErrorMessageForReasonCode);
			AssertHasError(approvalRequest.XP_ReasonDescriptionInfo, expectedErrorMessageForDescription);
		}

		public void TestCheckXP_ReasonDescription_Empty()
		{
			var approvalRequest = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			Assert(!approvalRequest.IsInDatabase);

			approvalRequest.XP_ReasonDescription = string.Empty;
			AssertHasError("Empty string is not a valid value", approvalRequest.XP_ReasonDescriptionInfo, "Please enter a Description.");
			approvalRequest.XP_ReasonDescription = "description text";
			AssertNoErrors(approvalRequest.XP_ReasonDescriptionInfo);

			Factory.Save();
			Assert(approvalRequest.IsInDatabase);

			approvalRequest.XP_ReasonDescription = string.Empty;
			AssertHasError("changing the description to empty after it is saved should show a validation error", approvalRequest.XP_ReasonDescriptionInfo, "Please enter a Description.");

			//Save approval request with empty description (create bad data)
			approvalRequest.XP_ReasonDescription = string.Empty;
			Factory.Save();

			var approvalRequestReloaded = new BusinessObjectFactory().Load<ARCreditNoteApprovalRequest>(approvalRequest.PK);
			AssertEquals("Old approval request can have empty description (bad data)", string.Empty, approvalRequestReloaded.XP_ReasonCodeDescription);
			approvalRequestReloaded.Validation.ValidateXP_ReasonDescription();
			AssertNoErrors("we don't want to show validation error for existing bad data", approvalRequestReloaded.XP_ReasonDescriptionInfo);
		}
	}
}
