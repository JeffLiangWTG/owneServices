using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.JobInvoicing.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	public abstract class BaseBatchInvoicingPostManagerGUIWrapperTest_PostingTransactionApprovalTest : PostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		public override void TestApprovalRequestForPostingAction()
		{
			SetupSecurity();

			var jobA = SetupJobData("S001");
			var chargeA = SetupChargeData(jobA, 200m, TestObjectCreator.CC3);
			var jobB = SetupJobData("S002");
			var chargeB = SetupChargeData(jobB, 200m, TestObjectCreator.CC3);

			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wrapper = GetNewGUIWrapper(jobA, jobB);
			wrapper.Post();

			Assert("IsAnyChargePosted", !IsAnyChargePosted(chargeA, chargeB));
			AssertNull("Should not prompt any form", ZFormModaliser.LastFormShownDialogForTest);

			ZGuid parentId;
			ZString parentTableCode;
			GetParentForPostingAction(jobB, out parentId, out parentTableCode);
			bool isConsolPosting = parentTableCode == JobConsolSchema.Constants.Prefix;
			var expectedPostingObjectName = isConsolPosting ? "Consol" : "Posting Object Name";
			string expectedMessageTemplate;
			if (IsARCreditNoteTesting)
			{
				expectedMessageTemplate = "\r\n{0} posting failed. {1}\r\n";
				SetupApprovedApprovalRequest(parentId, parentTableCode, jobB, isDetailsDifferent: true);
			}
			else
			{
				expectedMessageTemplate = @"
{0} posting has warning. Creditor: ZCreditor1, Number: S002_123, Ref. Number:S002, Amount:200.0000.
{1}

-----------";
				SetupApprovedApprovalRequest(jobB.PK, jobB.TablePrefix, jobB, isDetailsDifferent: true);
			}
			AssertEquals(string.Format(expectedMessageTemplate, expectedPostingObjectName, GetExpectedPartOfOnPostingFinishMessage()), lastOnPostingFinishMessage);

			wrapper = GetNewGUIWrapper(jobA, jobB);
			wrapper.Post();

			Assert("IsAnyChargePosted", !IsAnyChargePosted(chargeA, chargeB));
			AssertNull("Should not prompt any form", ZFormModaliser.LastFormShownDialogForTest);

			AssertEquals(string.Format(expectedMessageTemplate, expectedPostingObjectName,
				"There is an approved request for this posting action, but data for approval is different.\r\n" +
				GetExpectedPartOfOnPostingFinishMessage()), lastOnPostingFinishMessage);
		}

		public override void TestApprovalRequestForPreviewActionShouldNotBeCreated()
		{
			Assert("Not applicable here.", true);
		}

		public override void TestApprovalRequestIncludeChargesForInvoicesNeedAuthorizationOnly()
		{
			Assert("Not applicable here.", true);
		}

		public override void TestPostApprovedRequestOnNextPosting()
		{
			Assert("Not applicable here.", true);
		}

		public override void TestApprovalRequestForARCreditNoteAndAPInvoiceCreatedInOneGo()
		{
			Assert("Not applicable here.", true);
		}

		public override void TestApprovalRequestForARCreditNoteCancelledAndAPInvoiceCreatedInOneGo()
		{
			Assert("Not applicable here.", true);
		}

		public override void TestApprovalRequestForARCreditNoteCreatedAndAPInvoiceCancelledInOneGo()
		{
			Assert("Not applicable here.", true);
		}

		public void TestPostWithApprovedApprovalRequest()
		{
			SetupSecurity();

			var job = SetupJobData("S001");
			var charge = SetupChargeData(job, 200m, TestObjectCreator.CC3);

			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wrapper = GetNewGUIWrapper(job);
			wrapper.Post();

			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge));
			AssertNull("Should not prompt any form", ZFormModaliser.LastFormShownDialogForTest);

			ZGuid parentId;
			ZString parentTableCode;
			GetParentForPostingAction(job, out parentId, out parentTableCode);
			bool isConsolPosting = parentTableCode == JobConsolSchema.Constants.Prefix;
			var expectedPostingObjectName = isConsolPosting ? "Consol" : "Posting Object Name";
			string expectedMessageTemplate;
			if (IsARCreditNoteTesting)
			{
				expectedMessageTemplate = "\r\n{0} posting failed. {1}\r\n";
				var request = (ARCreditNoteApprovalRequest)SetupApprovedApprovalRequest(parentId, parentTableCode, job);
				request.PostingDetails.MaxAuthorisationLevelRequired = 1;
				Factory.Save();
			}
			else
			{
				expectedMessageTemplate = @"
{0} posting has warning. Creditor: ZCreditor1, Number: S001_123, Ref. Number:S001, Amount:200.0000.
{1}

-----------";
				SetupApprovedApprovalRequest(job.PK, job.TablePrefix, job);
			}
			AssertEquals(string.Format(expectedMessageTemplate, expectedPostingObjectName, GetExpectedPartOfOnPostingFinishMessage()), lastOnPostingFinishMessage);

			wrapper = GetNewGUIWrapper(job);
			wrapper.Post();

			charge.Reload();
			Assert("IsAnyChargePosted", IsAnyChargePosted(charge));
			AssertEquals("Should create one invoice", 1, GetNumerOfPostedInvoices(charge));
			AssertNull("Should not prompt any form", ZFormModaliser.LastFormShownDialogForTest);

			AssertEquals("", lastOnPostingFinishMessage);
		}

		string GetExpectedPartOfOnPostingFinishMessage()
		{
			return "You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.";
		}

		GenApprovalRequest SetupApprovedApprovalRequest(ZGuid parentId, string parentTableCode, Job job, bool isDetailsDifferent = false)
		{
			GenApprovalRequest request;
			ApprovalRequestDetails requestDetails;
			ApprovalRequestChargeDetails requestCharge;

			if (IsARCreditNoteTesting)
			{
				var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
				request = approvalRequest;

				approvalRequest.PostingDetails.PostingOption = nameof(JobInvoicingPostingOption.All);
				approvalRequest.PostingDetails.ApprovingOption = ApprovalCredentialOption.SingleLogin;
				requestDetails = approvalRequest.PostingDetails;

				var charge = approvalRequest.PostingDetails.Charges.AddNew();
				requestCharge = charge;
				charge.SellAccount = TestObjectCreator.LocalClient.OH_Code;
				charge.SellCurrency = TestObjectCreator.AUD.RX_Code;
				charge.OSSellAmount = -200M;
				charge.LocalSellAmount = -200M;
				charge.InvoiceType = "FIN";
				charge.ExchangeRate = 1.0;
				charge.TaxCode = TestObjectCreator.GSTFREE1.AT_Code;
			}
			else
			{
				var approvalRequest = Factory.New<APInvoiceChargesApprovalRequest>();
				request = approvalRequest;
				requestDetails = approvalRequest.PostingDetails;

				approvalRequest.PostingDetails.Creditor = "ZCreditor1";
				approvalRequest.PostingDetails.TransactionNumber = job.JH_JobNum + "_123";

				var charge = approvalRequest.PostingDetails.Charges.AddNew();
				requestCharge = charge;
				charge.CostCurrency = TestObjectCreator.AUD.RX_Code;
				charge.OSCostAmount = 200M;
				charge.LocalCostAmount = 200M;
			}

			request.XP_ParentID = parentId;
			request.XP_ParentTableCode = parentTableCode;
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			requestDetails.MaxAmountToApprove = 200M;

			requestCharge.JobNumber = job.JH_JobNum + (isDetailsDifferent ? "_ANOTHER" : "");
			requestCharge.ChargeCode = TestObjectCreator.CC3.AC_Code;
			requestCharge.Branch = GlbBranch.CurrentBranch.GB_Code;
			requestCharge.Department = GlbDepartment.CurrentDepartment.GE_Code;

			Factory.Save();
			return request;
		}

		protected sealed override PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs)
		{
			var guiWrapper = GetNewBatchGUIWrapper(jobs);
			lastOnPostingFinishMessage = "";
			guiWrapper.OnObjectPostingFinished += guiWrapper_OnObjectPostingFinished;

			return guiWrapper;
		}

		void guiWrapper_OnObjectPostingFinished(ZString resultMessage, ZBool isObjectPosted)
		{
			lastOnPostingFinishMessage = resultMessage;
		}

		string lastOnPostingFinishMessage;

		protected abstract BaseBatchInvoicingPostManagerGUIWrapper GetNewBatchGUIWrapper(params Job[] jobs);
	}
}
