using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using ErrorMessages = Enterprise.Accounting.Business.AccountingConstants.ChequeNumberAllocationErrorMessages;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class PaymentProcessingModuleTest : ZModuleBasherTest
	{
		#region Payment Approval With Deal

		public void TestPostingPaymentApprovalWithDeal()
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
				new AccountingPeriodTestHelper().SetupPeriods();
				Factory.Save();

				var approvalWithUnconfirmedDeal = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
				approvalWithUnconfirmedDeal.AV_PaymentType = ReceiptTypes.EPayment;
				approvalWithUnconfirmedDeal.AV_AB = ofxBankAccount.PK;
				approvalWithUnconfirmedDeal.AV_ChequeOrReference = "0002";
				approvalWithUnconfirmedDeal.AV_PayExRate = 1m;
				approvalWithUnconfirmedDeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				approvalWithUnconfirmedDeal.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed, approvalWithUnconfirmedDeal);

				var approvalWithConfirmedDeal = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
				approvalWithConfirmedDeal.AV_PaymentType = ReceiptTypes.EPayment;
				approvalWithConfirmedDeal.AV_AB = ofxBankAccount.PK;
				approvalWithConfirmedDeal.AV_ChequeOrReference = "0004";
				approvalWithConfirmedDeal.AV_PayExRate = 1m;
				approvalWithConfirmedDeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				approvalWithConfirmedDeal.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithConfirmedDeal);

				var approvalWithoutADealAndPaymentTypeIsEPA = GetNewPaymentApprovalReadyToPost("00001006", "0006", 1000M);
				approvalWithoutADealAndPaymentTypeIsEPA.AV_PaymentType = ReceiptTypes.EPayment;
				approvalWithoutADealAndPaymentTypeIsEPA.AV_AB = ofxBankAccount.PK;
				approvalWithoutADealAndPaymentTypeIsEPA.AV_ChequeOrReference = "0006";
				approvalWithoutADealAndPaymentTypeIsEPA.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;

				var approvalWithoutADealAndPaymentTypeIsNotEPA = GetNewPaymentApprovalReadyToPost("00001008", "0008", 1000M);

				Factory.Save();

				var selectedPayments = new[]
				{
				approvalWithUnconfirmedDeal,
				approvalWithConfirmedDeal,
				approvalWithoutADealAndPaymentTypeIsEPA,
				approvalWithoutADealAndPaymentTypeIsNotEPA
			};

				using (var module = GetNewModule())
				{
					SelectBusinessObjects(module, selectedPayments);
					AssertEquals(4, module.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					Assert(!approvalWithUnconfirmedDeal.IsPosted);
					Assert(!approvalWithConfirmedDeal.IsPosted);
					Assert(!approvalWithoutADealAndPaymentTypeIsEPA.IsPosted);
					Assert(!approvalWithoutADealAndPaymentTypeIsNotEPA.IsPosted);

					module.PostPaymentApprovals(null, null);

					var expectedMessage = $@"The following Payments cannot be processed:
{approvalWithUnconfirmedDeal.GetDescription()}
Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.
{approvalWithoutADealAndPaymentTypeIsEPA.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.

The following Payments will be processed:
{approvalWithConfirmedDeal.GetDescription()}
{approvalWithoutADealAndPaymentTypeIsNotEPA.GetDescription()}

Do you want to post these transactions?";

					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!approvalWithUnconfirmedDeal.IsPosted);
					Assert(approvalWithConfirmedDeal.IsPosted);
					Assert(!approvalWithoutADealAndPaymentTypeIsEPA.IsPosted);
					Assert(approvalWithoutADealAndPaymentTypeIsNotEPA.IsPosted);

					var paymentProcessingGuiHelper = module.PaymentProcessingGUIHelper;
					AssertEquals(1, paymentProcessingGuiHelper.bankTransferFormsPrompted.Count);
					AssertEquals(typeof(GUI.CashBook.Transfer.BankTransferForm), paymentProcessingGuiHelper.bankTransferFormsPrompted[0].GetType());
					paymentProcessingGuiHelper.bankTransferFormsPrompted[0].Dispose();
				}
			}
		}

		public void TestCancelPaymentApprovalsWithDeal()
		{
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			new AccountingPeriodTestHelper().SetupPeriods();
			Factory.Save();

			var dealWithActiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted);
			var dealWithInactiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed);

			var approvalWithActiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithActiveStatus.Quote.PaymentApproval.PK);
			approvalWithActiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithActiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithInactiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithInactiveStatus.Quote.PaymentApproval.PK);
			approvalWithInactiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithInactiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithoutADeal = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);

			Factory.Save();

			Env.Security.APPaymentProcessingCancelApproval.IsAllowed = true;
			Env.Security.ARPaymentProcessingCancelApproval.IsAllowed = true;

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, new[] { approvalWithActiveDeal, approvalWithInactiveDeal, approvalWithoutADeal });
				AssertEquals(3, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithInactiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithoutADeal.AV_Status);

				module.CancelPaymentApprovals_ForTestOnly(null, null);

				var expectedMessage = $@"The following Payments cannot be canceled:
{approvalWithActiveDeal.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

The following Payments will be canceled:
{approvalWithInactiveDeal.GetDescription()}
{approvalWithoutADeal.GetDescription()}

Do you want to cancel these Payments?";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.Cancelled, approvalWithInactiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.Cancelled, approvalWithoutADeal.AV_Status);
			}
		}

		public void TestRejectPaymentApprovalsWithDeal()
		{
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			new AccountingPeriodTestHelper().SetupPeriods();
			Factory.Save();

			var dealWithActiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted);
			var dealWithInactiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed);

			var approvalWithActiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithActiveStatus.Quote.PaymentApproval.PK);
			approvalWithActiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithActiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithInactiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithInactiveStatus.Quote.PaymentApproval.PK);
			approvalWithInactiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithInactiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithoutADeal = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);

			Factory.Save();

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, new[] { approvalWithActiveDeal, approvalWithInactiveDeal, approvalWithoutADeal });
				AssertEquals(3, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				void setReason(object x)
				{
					var rejectionForm = x as PaymentRejectionReasonForm;
					rejectionForm.SetReason("INS", "Not enough money");
				}

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setReason);

				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithInactiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithoutADeal.AV_Status);

				module.RejectPaymentApprovals_ForTestOnly(null, null);

				var expectedMessage = $@"The following Payments cannot be rejected:
{approvalWithActiveDeal.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.Rejected, approvalWithInactiveDeal.AV_Status);
				AssertEquals(PaymentApprovalStatus.Rejected, approvalWithoutADeal.AV_Status);
			}
		}

		public void TestUnauthorizePaymentApprovalsWithDeal()
		{
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			new AccountingPeriodTestHelper().SetupPeriods();
			Factory.Save();

			var approvalWithActiveDeal = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1200M);
			approvalWithActiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithActiveDeal.AV_PayExRate = 1m;
			approvalWithActiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
			approvalWithActiveDeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;

			var approvalWithInactiveDeal = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1300M);
			approvalWithInactiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithInactiveDeal.AV_PayExRate = 1m;
			approvalWithInactiveDeal.AV_Amount = 1300M;
			approvalWithInactiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
			approvalWithInactiveDeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;

			var approvalWithoutADeal = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1400M);
			approvalWithoutADeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;

			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed, approvalWithInactiveDeal);

			Factory.Save();

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, new[] { approvalWithActiveDeal, approvalWithInactiveDeal, approvalWithoutADeal });
				AssertEquals(3, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithInactiveDeal.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithoutADeal.AV_GS_NKApproval1st);

				module.UnAuthorisePaymentApprovals_ForTestOnly(null, null);

				var expectedMessage = $@"The following Payments cannot be unauthorized:
{approvalWithActiveDeal.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

The following Payments will be unauthorized:
{approvalWithInactiveDeal.GetDescription()}
{approvalWithoutADeal.GetDescription()}

Do you want to unauthorize these Payments?";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal.AV_GS_NKApproval1st);
				AssertEquals(ZString.Empty, approvalWithInactiveDeal.AV_GS_NKApproval1st);
				AssertEquals(ZString.Empty, approvalWithoutADeal.AV_GS_NKApproval1st);
			}
		}

		public void TestDeletePaymentApprovalsWithDeal()
		{
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			new AccountingPeriodTestHelper().SetupPeriods();
			Factory.Save();

			var dealWithActiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted);
			var dealWithInactiveStatus = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed);

			var approvalWithActiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithActiveStatus.Quote.PaymentApproval.PK);
			approvalWithActiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithActiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithInactiveDeal = Factory.Load<PaymentApprovalWithAuthorisation>(dealWithInactiveStatus.Quote.PaymentApproval.PK);
			approvalWithInactiveDeal.AV_AB = ofxBankAccount.PK;
			approvalWithInactiveDeal.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalWithoutADeal = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);

			Factory.Save();

			using (var module = GetNewModule())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				var deleteformForApprovalWithActiveDeal = module.ShowDeleteForm_ForTestOnly(approvalWithActiveDeal);
				var expectedMessage = "The payment has an active E-Payment Deal. Action not permitted.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(deleteformForApprovalWithActiveDeal);

				UnitTestUserNotification.Instance.ClearMessages();
				using (var deleteformForApprovalWithInactiveDeal = module.ShowDeleteForm_ForTestOnly(approvalWithInactiveDeal))
				{
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull(deleteformForApprovalWithInactiveDeal);
				}

				UnitTestUserNotification.Instance.ClearMessages();
				using (var deleteformForApprovalWithoutDeal = module.ShowDeleteForm_ForTestOnly(approvalWithoutADeal))
				{
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull(deleteformForApprovalWithoutDeal);
				}
			}
		}

		#endregion

		#region Draft Payment Approval

		public PaymentApprovalWithAuthorisation[] PrepareDraftPayment()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 4, 3);
			chequeBook.AK_AutoPrintCheque = false;

			var approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 500M);
			approval1.AV_PaymentType = ReceiptTypes.Cheque;
			approval1.AV_AB = chequeBook.AK_AB;
			approval1.AV_AK = chequeBook.PK;
			approval1.AV_Status = PaymentApprovalStatus.Draft;

			var approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = chequeBook.AK_AB;
			approval2.AV_AK = chequeBook.PK;

			Factory.Save();

			return new[] { approval1, approval2 };
		}

		public void TestDealWithDraftPaymentSuccess_Cancel()
		{
			var approvals = PrepareDraftPayment();

			AssertNotEquals(PaymentApprovalStatus.Cancelled, approvals[0].AV_Status);
			AssertNotEquals(PaymentApprovalStatus.Cancelled, approvals[1].AV_Status);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.CancelPaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments will be canceled:
Payment ZOrg CHQ  (AUD 500.00)
Payment ZOrg CHQ  (AUD 1000.00)

Do you want to cancel these Payments?");

			AssertEquals(PaymentApprovalStatus.Cancelled, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.Cancelled, approvals[1].AV_Status);
		}

		public void TestDealWithDraftPaymentError_Authorise()
		{
			var approvals = PrepareDraftPayment();
			approvals[1].AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.AwaitingApproval, approvals[1].AV_Status);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments cannot be authorized:
Payment ZOrg CHQ  (AUD 500.00)
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.

The following Payments will be authorized:
Payment ZOrg CHQ  (AUD 1000.00)

Do you want to authorize these Payments?");

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);
		}

		public void TestDealWithDraftPaymentError_UnAuthorise()
		{
			var approvals = PrepareDraftPayment();
			approvals[0].AV_Amount = 2000M;
			approvals[1].AV_Amount = 2000M;
			approvals[0].AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			approvals[1].AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			AssertEquals(PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised, approvals[0].Level1AuthorisationStatus);
			AssertEquals(PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised, approvals[0].Level1AuthorisationStatus);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments cannot be unauthorized:
Payment ZOrg CHQ  (AUD 2000.00)
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.

The following Payments will be unauthorized:
Payment ZOrg CHQ  (AUD 2000.00)

Do you want to unauthorize these Payments?");

			AssertEquals(PaymentApprovalWithAuthorisation.AuthorisationStatus.Authorised, approvals[0].Level1AuthorisationStatus);
			AssertEquals(PaymentApprovalWithAuthorisation.AuthorisationStatus.AwaitingAuthorisation, approvals[1].Level1AuthorisationStatus);
		}

		public void TestDealWithDraftPaymentError_Reject()
		{
			var approvals = PrepareDraftPayment();
			approvals[1].AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.AwaitingApproval, approvals[1].AV_Status);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			TestDealWithDraftPaymentCore(approvals, (module) => { module.RejectPaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments cannot be rejected:
Payment ZOrg CHQ  (AUD 500.00)
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.");

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.Rejected, approvals[1].AV_Status);
		}

		public void TestDealWithDraftPaymentError_Post()
		{
			var approvals = PrepareDraftPayment();
			approvals[1].AV_ChequeOrReference = GetAutoPrintChequeBook(1, 4, 3).AK_CurrentNo.ToString();
			Factory.Save();

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.PostPaymentApprovals(null, new EventArgs()); },
@"The following Payments cannot be processed:
Payment ZOrg CHQ  (AUD 500.00)
The Check Number for this Payment is empty
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.

The following Payments will be processed:
Payment ZOrg CHQ 3 (AUD 1000.00)

Do you want to post these transactions?");

			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.Posted, approvals[1].AV_Status);
		}

		public void TestDealWithDraftPaymentError_PopulateChequeNo()
		{
			var approvals = PrepareDraftPayment();

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNullOrEmpty(approvals[1].AV_ChequeOrReference);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.PopulateChequeNoForPaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments cannot be processed:
Payment ZOrg CHQ  (AUD 500.00)
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.

The following Payments will be processed:
Payment ZOrg CHQ  (AUD 1000.00)

Do you want to populate the cheque number for these transactions?");

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNotNullOrEmpty(approvals[1].AV_ChequeOrReference);
		}

		public void TestDealWithDraftPaymentError_PopulateChequeNoAndPost()
		{
			var approvals = PrepareDraftPayment();

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNullOrEmpty(approvals[1].AV_ChequeOrReference);
			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);

			TestDealWithDraftPaymentCore(approvals, (module) => { module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs()); },
@"The following Payments cannot be processed:
Payment ZOrg CHQ  (AUD 500.00)
Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again.

The following Payments will be processed:
Payment ZOrg CHQ  (AUD 1000.00)

Do you want to populate the cheque number and post these transactions?");

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNotNullOrEmpty(approvals[1].AV_ChequeOrReference);
			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.Posted, approvals[1].AV_Status);
		}

		public void TestDealWithDraftPaymentCore(PaymentApprovalWithAuthorisation[] approvals, Action<PaymentProcessingModule> testAction, string errorMessage)
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, approvals);
				AssertEquals("Precondition: BizObjs are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals("Percondition", PaymentApprovalStatus.Draft, approvals[0].AV_Status);
				AssertNotEquals("Percondition", PaymentApprovalStatus.Draft, approvals[1].AV_Status);

				testAction(module);

				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Submit for Approval/Approve for Posting Action Menu Item

		public void TestWhenPaymentAuthorisationRequiredSettingUpdated_ActionMenuItemChanges()
		{
			var approvals = PrepareDraftPayment();
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty()))
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, approvals);
				AssertEquals(0, AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.Count);

				AssertNotNull(module.FormActionMenu);
				var submitForApprovalMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Submit for Approval");
				AssertNull(submitForApprovalMenuItem);

				var approveForPostingMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Approve for Posting");
				AssertNotNull(approveForPostingMenuItem);
				AssertEquals(0, approveForPostingMenuItem.MenuItems.Count);
			}
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, approvals);
				AssertNotNull(module.FormActionMenu);
				var submitForApprovalMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Submit for Approval");
				AssertNotNull(submitForApprovalMenuItem);
				AssertEquals(0, submitForApprovalMenuItem.MenuItems.Count);

				var approveForPostingMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Approve for Posting");
				AssertNull(approveForPostingMenuItem);
			}

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettings(RangeCodes.Above, 0, AuthorisationCodes.NoApprovalRequired)))
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, approvals);
				AssertNotNull(module.FormActionMenu);
				var submitForApprovalMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Submit for Approval");
				AssertNull(submitForApprovalMenuItem);

				var approveForPostingMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Approve for Posting");
				AssertNotNull(approveForPostingMenuItem);
				AssertEquals(0, approveForPostingMenuItem.MenuItems.Count);
			}
		}

		#endregion

		#region Submit for Approval

		public void TestSubmitForApproval()
		{
			var approvals = PrepareDraftPayment();

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNullOrEmpty(approvals[1].AV_ChequeOrReference);
			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			{
				TestDealWithDraftPaymentCore(approvals, (module) =>
				  {
					  AssertNotNull(module.FormActionMenu);
					  var submitForApprovalMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Submit for Approval");
					  AssertNotNull(submitForApprovalMenuItem);
					  submitForApprovalMenuItem.PerformClick();
				  },
	  @"The following Payments cannot be submitted for approval:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment is not in Draft status

The following Payments will be submitted for approval:
Payment ZOrg CHQ  (AUD 500.00)

Do you want to submit these Payments for approval?");
			}

			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);
		}

		#endregion

		#region Approve for Posting

		public void TestApproveForPosting()
		{
			var approvals = PrepareDraftPayment();

			AssertNullOrEmpty(approvals[0].AV_ChequeOrReference);
			AssertNullOrEmpty(approvals[1].AV_ChequeOrReference);
			AssertEquals(PaymentApprovalStatus.Draft, approvals[0].AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty()))
			{
				TestDealWithDraftPaymentCore(approvals, (module) =>
				{
					AssertNotNull(module.FormActionMenu);
					var approveForPostingMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Approve for Posting");
					AssertNotNull(approveForPostingMenuItem);
					approveForPostingMenuItem.PerformClick();
				},
	@"The following Payments cannot be approved for posting:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment is not in Draft status

The following Payments will be approved for posting:
Payment ZOrg CHQ  (AUD 500.00)

Do you want to approve these Payments for posting?");

				AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[0].AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvals[1].AV_Status);
			}
		}

		#endregion

		[ExpectNoExceptions("Menu used for this document has been deleted from the system. Document not printed. MenuName-Payment Voucher")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestPrintVoucherInForeignLanguage()
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				GlbStaff newStaff = module.Factory_ForTestOnly.New<GlbStaff>();
				newStaff.GS_Code = "XYZ";
				newStaff.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
				module.Factory_ForTestOnly.Save();

				Env.SetUserContext(new UserContext(newStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval(module.Factory_ForTestOnly);
				approval1.AV_Amount = 1000m; // None Required
				approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

				BusinessObject[] businessObjects = new BusinessObject[1];
				businessObjects[0] = approval1;

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);
				module.PrintCheckpoint_ForTestOnly.IsAllowed = true;

				module.PrintPaymentApproval_ForTestOnly(null, new EventArgs());
			}
		}

		public void TestAuthorisePaymentApprovalsDoesNotLoadTransactionsFromDB()
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				GlbStaff newStaff = module.Factory_ForTestOnly.New<GlbStaff>();
				newStaff.GS_Code = "XYZ";

				PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval(module.Factory_ForTestOnly);
				approval1.AV_Amount = 1000m; // None Required
				approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

				PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval(module.Factory_ForTestOnly);
				approval2.AV_Amount = 2000m; // 1st Required
				approval2.AV_GS_NKApproval1st = newStaff.GS_Code;
				approval2.AV_Status = PaymentApprovalStatus.AwaitingApproval;

				BusinessObject[] businessObjects = new BusinessObject[2];
				businessObjects[0] = approval1;
				businessObjects[1] = approval2;

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);
				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				int beforeHit = module.Factory_ForTestOnly.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				int afterHit = module.Factory_ForTestOnly.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
				AssertEquals("AccTransactionHeader table should not be loaded", beforeHit, afterHit);
			}
		}

		public void TestUnAuthorisePaymentApprovalsDoesNotLoadTransactionsFromDB()
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				GlbStaff newStaff = module.Factory_ForTestOnly.New<GlbStaff>();
				newStaff.GS_Code = "XYZ";

				PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval(module.Factory_ForTestOnly);
				approval1.AV_Amount = 1000m; // None Required

				PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval(module.Factory_ForTestOnly);
				approval2.AV_Amount = 2000m; // 1st Required
				approval2.AV_GS_NKApproval1st = newStaff.GS_Code;

				BusinessObject[] businessObjects = new BusinessObject[2];
				businessObjects[0] = approval1;
				businessObjects[1] = approval2;

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);
				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;

				int beforeHit = module.Factory_ForTestOnly.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
				module.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				int afterHit = module.Factory_ForTestOnly.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
				AssertEquals("AccTransactionHeader table should not be loaded", beforeHit, afterHit);
			}
		}

		public void TestAuthorisePaymentApprovals()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "new@test.com";
			Factory.Save();

			GlbStaff currentUser = GlbStaff.CurrentUser;

			PaymentApprovalWithAuthorisation approval1;
			PaymentApprovalWithAuthorisation approval2;
			PaymentApprovalWithAuthorisation approval3;
			PaymentApprovalWithAuthorisation approval4;
			PaymentApprovalWithAuthorisation approval5;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval1 = GetNewPaymentApproval();
				approval1.AV_Amount = 1000m; // None Required

				approval2 = GetNewPaymentApproval();
				approval2.AV_Amount = 2000m; // 1st Required
				approval2.AV_GS_NKApproval1st = newStaff.GS_Code;

				approval3 = GetNewPaymentApproval();
				approval3.AV_Amount = 3000m; // 2nd Required
				approval3.AV_GS_NKApproval2nd = newStaff.GS_Code;

				approval4 = GetNewPaymentApproval();
				approval4.AV_Amount = 4000m; // 3rd Required
				approval4.AV_GS_NKApproval3rd = newStaff.GS_Code;

				approval5 = GetNewPaymentApproval();
				approval5.AV_Amount = 10000m; // All 3 Required
				approval5.AV_GS_NKApproval1st = newStaff.GS_Code;
				approval5.AV_GS_NKApproval2nd = newStaff.GS_Code;
				approval5.AV_GS_NKApproval3rd = ZString.Empty;
				Factory.Save();
			}

			BusinessObject[] businessObjects = new BusinessObject[5];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;
			businessObjects[4] = approval5;

			AssertCorrectAuthorisation("Precondition: Approval1", approval1, null, null, null);
			AssertCorrectAuthorisation("Precondition: Approval2", approval2, newStaff, null, null);
			AssertCorrectAuthorisation("Precondition: Approval3", approval3, null, newStaff, null);
			AssertCorrectAuthorisation("Precondition: Approval4", approval4, null, null, newStaff);
			AssertCorrectAuthorisation("Precondition: Approval5", approval5, newStaff, newStaff, null);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);
				FirstApprovalCheckPoint.IsAllowed = false;
				SecondApprovalCheckPoint.IsAllowed = false;
				ThirdApprovalCheckPoint.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be authorized:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 2000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 3000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 4000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 10000.00)
You do not have sufficient rights to authorize this payment. Required authorization level: Level 3.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, newStaff, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, newStaff, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, newStaff);
				AssertCorrectAuthorisation("Approval5", approval5, newStaff, newStaff, null);

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);
				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be authorized:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 2000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 3000.00)
This Payment is already Fully Approved

Payment ZOrg CHQ  (AUD 4000.00)
This Payment is already Fully Approved

The following Payments will be authorized:
Payment ZOrg CHQ  (AUD 10000.00)

Do you want to authorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, newStaff, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, newStaff, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, newStaff);
				AssertCorrectAuthorisation("Approval5", approval5, newStaff, newStaff, currentUser);

				SetAuthorisationOnApproval(approval1, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval2, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval3, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval4, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval5, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, null, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, null, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, null);
				AssertCorrectAuthorisation("Approval5", approval5, null, null, null);
				Factory.Save();

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be authorized:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment is already Fully Approved

The following Payments will be authorized:
Payment ZOrg CHQ  (AUD 2000.00)
Payment ZOrg CHQ  (AUD 3000.00)
Payment ZOrg CHQ  (AUD 4000.00)
Payment ZOrg CHQ  (AUD 10000.00)

Do you want to authorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, currentUser, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, currentUser, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, currentUser);
				AssertCorrectAuthorisation("Approval5", approval5, currentUser, currentUser, currentUser);

				var emails = Env.OutgoingMailManager.EmailsCreated;
				var expected = new[]
				{
					(Invariant($"{AR_AP} Payment Request Approved - ZOrg AUD 10000.00"), "new@test.com"),
					(Invariant($"{AR_AP} Payment Request Approved - ZOrg AUD 10000.00"), "new@test.com"),
					(Invariant($"{AR_AP} Payment Request Approved - ZOrg AUD 2000.00"), "new@test.com"),
					(Invariant($"{AR_AP} Payment Request Approved - ZOrg AUD 3000.00"), "new@test.com"),
					(Invariant($"{AR_AP} Payment Request Approved - ZOrg AUD 4000.00"), "new@test.com"),
				};
				var actual = emails.Select(x => (x.Subject, x.Recipients.Cast<RecipientDef>().Single().Email));
				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestApproveWhenNoAuthorisationIsRequired()
		{
			PaymentApprovalWithAuthorisation approval = GetNewPaymentApproval();
			approval.AV_Amount = 1000m;

			using (PaymentProcessingModule module = GetNewModule())
			{
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();

				SelectBusinessObjects(module, new BusinessObject[] { approval });
				AssertEquals("Precondition: BizObjs are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);
				AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.AwaitingApproval, approval.AV_Status);

				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertCorrectAuthorisation("Approval", approval, null, null, null);
				AssertEquals("Approval Status", PaymentApprovalStatus.FullyApproved, approval.AV_Status);

				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();

				SelectBusinessObjects(module, new BusinessObject[] { approval });
				AssertEquals("Precondition: BizObjs are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);
				AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.AwaitingApproval, approval.AV_Status);

				FirstApprovalCheckPoint.IsAllowed = false;
				SecondApprovalCheckPoint.IsAllowed = false;
				ThirdApprovalCheckPoint.IsAllowed = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertCorrectAuthorisation("Approval", approval, null, null, null);
				AssertEquals("Approval Status", PaymentApprovalStatus.FullyApproved, approval.AV_Status);
			}
		}

		protected abstract string AR_AP { get; }

		public void TestAuthorisePaymentApprovalsDontAutoPostThem()
		{
			GlbStaff currentUser = GlbStaff.CurrentUser;

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			approval1.AV_Amount = 1000m; // None Required

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval2.AV_Amount = 2000m; // 1st Required

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApproval();
			approval3.AV_Amount = 3000m; // 2nd Required

			PaymentApprovalWithAuthorisation approval4 = GetNewPaymentApproval();
			approval4.AV_Amount = 4000m; // 3rd Required

			PaymentApprovalWithAuthorisation approval5 = GetNewPaymentApproval();
			approval5.AV_Amount = 10000m; // All 3 Required

			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[5];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;
			businessObjects[4] = approval5;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SetAuthorisationOnApproval(approval1, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval2, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval3, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval4, ZString.Empty, ZString.Empty, ZString.Empty);
				SetAuthorisationOnApproval(approval5, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertCorrectAuthorisation("Precondition: Approval1", approval1, null, null, null);
				Assert("Precondition: Approval1 is fully approved.", approval1.IsFullyApproved);
				AssertCorrectAuthorisation("Precondition: Approval2", approval2, null, null, null);
				Assert("Precondition: Approval2 is not fully approved.", !approval2.IsFullyApproved);
				AssertCorrectAuthorisation("Precondition: Approval3", approval3, null, null, null);
				Assert("Precondition: Approval3 is not fully approved.", !approval3.IsFullyApproved);
				AssertCorrectAuthorisation("Precondition: Approval4", approval4, null, null, null);
				Assert("Precondition: Approval4 is not fully approved.", !approval4.IsFullyApproved);
				AssertCorrectAuthorisation("Precondition: Approval5", approval5, null, null, null);
				Assert("Precondition: Approval5 is not fully approved.", !approval5.IsFullyApproved);

				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());

				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				Assert("Approval1 is fully approved.", approval1.IsFullyApproved);
				AssertNull("A Payment was not Posted for Approval1.", approval1.NewPayment);
				AssertCorrectAuthorisation("Approval2", approval2, currentUser, null, null);
				Assert("Approval2 is fully approved.", approval2.IsFullyApproved);
				AssertNull("A Payment was not Posted for Approval2.", approval2.NewPayment);
				AssertCorrectAuthorisation("Approval3", approval3, null, currentUser, null);
				Assert("Approval3 is fully approved.", approval3.IsFullyApproved);
				AssertNull("A Payment was not Posted for Approval3.", approval3.NewPayment);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, currentUser);
				Assert("Approval4 is fully approved.", approval4.IsFullyApproved);
				AssertNull("A Payment was not Posted for Approval4.", approval4.NewPayment);
				AssertCorrectAuthorisation("Approval5", approval5, currentUser, currentUser, currentUser);
				Assert("Approval5 is fully approved.", approval5.IsFullyApproved);
				AssertNull("A Payment was not Posted for Approval5.", approval5.NewPayment);
			}
		}

		public void TestUnAuthorisePaymentApprovals()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "test@test.com";
			Factory.Save();

			PaymentApprovalWithAuthorisation approval1;
			PaymentApprovalWithAuthorisation approval2;
			PaymentApprovalWithAuthorisation approval3;
			PaymentApprovalWithAuthorisation approval4;
			PaymentApprovalWithAuthorisation approval5;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval1 = GetNewPaymentApproval();
				approval1.AV_Amount = 1000m; // None Required

				approval2 = GetNewPaymentApproval();
				approval2.AV_Amount = 2000m; // 1st Required
				approval2.AV_GS_NKApproval1st = newStaff.GS_Code;

				approval3 = GetNewPaymentApproval();
				approval3.AV_Amount = 3000m; // 2nd Required
				approval3.AV_GS_NKApproval2nd = newStaff.GS_Code;

				approval4 = GetNewPaymentApproval();
				approval4.AV_Amount = 4000m; // 3rd Required
				approval4.AV_GS_NKApproval3rd = newStaff.GS_Code;

				approval5 = GetNewPaymentApproval();
				approval5.AV_Amount = 10000m; // All 3 Required
				approval5.AV_GS_NKApproval1st = newStaff.GS_Code;
				approval5.AV_GS_NKApproval2nd = newStaff.GS_Code;
				approval5.AV_GS_NKApproval3rd = newStaff.GS_Code;

				Factory.Save();
			}

			BusinessObject[] businessObjects = new BusinessObject[5];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;
			businessObjects[4] = approval5;

			AssertCorrectAuthorisation("Precondition: Approval1", approval1, null, null, null);
			AssertCorrectAuthorisation("Precondition: Approval2", approval2, newStaff, null, null);
			AssertCorrectAuthorisation("Precondition: Approval3", approval3, null, newStaff, null);
			AssertCorrectAuthorisation("Precondition: Approval4", approval4, null, null, newStaff);
			AssertCorrectAuthorisation("Precondition: Approval5", approval5, newStaff, newStaff, newStaff);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);

				FirstApprovalCheckPoint.IsAllowed = false;
				SecondApprovalCheckPoint.IsAllowed = false;
				ThirdApprovalCheckPoint.IsAllowed = false;
				AssertEquals("UserCanAuthoriseLevel1", false, approval1.UserHasAuthoriseLevel1Security);
				AssertEquals("UserCanAuthoriseLevel2", false, approval1.UserHasAuthoriseLevel2Security);
				AssertEquals("UserCanAuthoriseLevel3", false, approval1.UserHasAuthoriseLevel3Security);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be unauthorized:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment requires no authorization level

Payment ZOrg CHQ  (AUD 2000.00)
You do not have sufficient rights to unauthorize this payment. Required authorization level: Level 1.

Payment ZOrg CHQ  (AUD 3000.00)
You do not have sufficient rights to unauthorize this payment. Required authorization level: Level 2.

Payment ZOrg CHQ  (AUD 4000.00)
You do not have sufficient rights to unauthorize this payment. Required authorization level: Level 3.

Payment ZOrg CHQ  (AUD 10000.00)
You do not have sufficient rights to unauthorize this payment. Required authorization level: Level 1 and Level 2 and Level 3.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, newStaff, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, newStaff, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, newStaff);
				AssertCorrectAuthorisation("Approval5", approval5, newStaff, newStaff, newStaff);

				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				AssertEquals("UserCanAuthoriseLevel1", true, approval1.UserHasAuthoriseLevel1Security);
				AssertEquals("UserCanAuthoriseLevel2", true, approval1.UserHasAuthoriseLevel2Security);
				AssertEquals("UserCanAuthoriseLevel3", true, approval1.UserHasAuthoriseLevel3Security);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be unauthorized:
Payment ZOrg CHQ  (AUD 1000.00)
This Payment requires no authorization level

The following Payments will be unauthorized:
Payment ZOrg CHQ  (AUD 2000.00)
Payment ZOrg CHQ  (AUD 3000.00)
Payment ZOrg CHQ  (AUD 4000.00)
Payment ZOrg CHQ  (AUD 10000.00)

Do you want to unauthorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCorrectAuthorisation("Approval1", approval1, null, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, null, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, null, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, null);
				AssertCorrectAuthorisation("Approval5", approval5, null, null, null);

				var emails = Env.OutgoingMailManager.EmailsCreated;
				var expected = new[]
				{
					(Invariant($"{AR_AP} Payment Request Unauthorized - ZOrg AUD 2000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Unauthorized - ZOrg AUD 3000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Unauthorized - ZOrg AUD 4000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Unauthorized - ZOrg AUD 10000.00"), "test@test.com"),
				};
				var actual = emails.Select(x => (x.Subject, x.Recipients.Cast<RecipientDef>().Single().Email));
				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestUnApproveFirstApproval()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_GS_NKApproval1st = newStaff.GS_Code;

			using (PaymentProcessingModule module = GetNewModule())
			{
				FirstApprovalCheckPoint.IsAllowed = false;
				approval1.UnApproveFirstApproval();
				approval2.UnApproveFirstApproval();

				AssertEquals("Approval.FirstApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval1st);
				AssertEquals("Approval.FirstApproval", ZString.Empty, approval2.AV_GS_NKApproval1st);

				FirstApprovalCheckPoint.IsAllowed = true;
				approval1.UnApproveFirstApproval();
				approval2.UnApproveFirstApproval();

				AssertEquals("Approval.FirstApproval", ZString.Empty, approval1.AV_GS_NKApproval1st);
				AssertEquals("Approval.FirstApproval", ZString.Empty, approval2.AV_GS_NKApproval1st);
			}
		}

		public void TestUnApproveSecondApproval()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_GS_NKApproval2nd = newStaff.GS_Code;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SecondApprovalCheckPoint.IsAllowed = false;
				approval1.UnApproveSecondApproval();
				approval2.UnApproveSecondApproval();

				AssertEquals("Approval.SecondApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval2nd);
				AssertEquals("Approval.SecondApproval", ZString.Empty, approval2.AV_GS_NKApproval2nd);

				SecondApprovalCheckPoint.IsAllowed = true;
				approval1.UnApproveSecondApproval();
				approval2.UnApproveSecondApproval();

				AssertEquals("Approval.SecondApproval", ZString.Empty, approval1.AV_GS_NKApproval2nd);
				AssertEquals("Approval.SecondApproval", ZString.Empty, approval2.AV_GS_NKApproval2nd);
			}
		}

		public void TestUnApproveThirdApproval()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_GS_NKApproval3rd = newStaff.GS_Code;

			using (PaymentProcessingModule module = GetNewModule())
			{
				ThirdApprovalCheckPoint.IsAllowed = false;
				approval1.UnApproveThirdApproval();
				approval2.UnApproveThirdApproval();

				AssertEquals("Approval.ThirdApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval3rd);
				AssertEquals("Approval.ThirdApproval", ZString.Empty, approval2.AV_GS_NKApproval3rd);

				ThirdApprovalCheckPoint.IsAllowed = true;
				approval1.UnApproveThirdApproval();
				approval2.UnApproveThirdApproval();

				AssertEquals("Approval.ThirdApproval", ZString.Empty, approval1.AV_GS_NKApproval3rd);
				AssertEquals("Approval.ThirdApproval", ZString.Empty, approval2.AV_GS_NKApproval3rd);
			}
		}

		#region Cancel

		public void TestCancelPaymentApprovals()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";

			var approval1 = GetNewPaymentApproval();
			approval1.AV_Amount = 1000m; // None Required

			var approval2 = GetNewPaymentApproval();
			approval2.AV_Amount = 2000m; // 1st Required
			approval2.AV_GS_NKApproval1st = newStaff.GS_Code;

			var approval3 = GetNewPaymentApproval();
			approval3.AV_Amount = 3000m; // 2nd Required
			approval3.AV_GS_NKApproval2nd = newStaff.GS_Code;

			Factory.Save();

			var approvals = new[] { approval1, approval2, approval3 };

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, approvals);
				AssertEquals("Precondition: BizObjs are selected", 3, module.SelectedBusinessObjects_ForTestOnly.Length);

				CancelApprovalCheckPoint.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				module.CancelPaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals($@"The following Payments cannot be canceled:
Payment ZOrg CHQ  (AUD 1000.00)
You do not have sufficient rights to cancel this payment. You have not been granted security rights to {CancelApprovalCheckPoint.DisplayTextPathToSecurityRight}.

Payment ZOrg CHQ  (AUD 2000.00)
You do not have sufficient rights to cancel this payment. You have not been granted security rights to {CancelApprovalCheckPoint.DisplayTextPathToSecurityRight}.

Payment ZOrg CHQ  (AUD 3000.00)
You do not have sufficient rights to cancel this payment. You have not been granted security rights to {CancelApprovalCheckPoint.DisplayTextPathToSecurityRight}.", UnitTestUserNotification.Instance.LastMessage.Text);

				CancelApprovalCheckPoint.IsAllowed = true;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				module.CancelPaymentApprovals_ForTestOnly(null, new EventArgs());

				AssertEquals(@"The following Payments will be canceled:
Payment ZOrg CHQ  (AUD 1000.00)
Payment ZOrg CHQ  (AUD 2000.00)
Payment ZOrg CHQ  (AUD 3000.00)

Do you want to cancel these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Expect all be cancelled if user answers Yes", approvals.All(x => x.AV_Status == PaymentApprovalStatus.Cancelled));
			}
		}

		public void TestCancelPaymentApprovals_NoPayments()
		{
			CancelPayments(null);
			AssertEquals("Please select one or more Payments to Cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCancelPaymentApprovals_AllPaymentsValid()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "test@test.com";
			Factory.Save();

			PaymentApprovalWithAuthorisation approvalAWA, approvalAPP;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approvalAWA = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1100m);
				approvalAWA.AV_Status = PaymentApprovalStatus.AwaitingApproval;

				approvalAPP = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1200m);
				approvalAPP.AV_Status = PaymentApprovalStatus.FullyApproved;

				Factory.Save();
			}

			var approvalREJ = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1300m);
			approvalREJ.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();

			var approvals = new[] { approvalAWA, approvalAPP, approvalREJ };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			CancelPayments(approvals);
			AssertEquals(PaymentApprovalStatus.AwaitingApproval, approvalAWA.AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvalAPP.AV_Status);
			AssertEquals(PaymentApprovalStatus.Rejected, approvalREJ.AV_Status);

			Assert("No changes made if user answers No", approvals.All(x => x.AV_Status != PaymentApprovalStatus.Cancelled));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			CancelPayments(approvals);

			Assert("Expect all be cancelled if user answers Yes", approvals.All(x => x.AV_Status == PaymentApprovalStatus.Cancelled));

			var emails = Env.OutgoingMailManager.EmailsCreated;
			var expected = new[]
			{
				(Invariant($"{AR_AP} Payment Request Cancelled - ZOrg AUD 1100.00"), "test@test.com"),
				(Invariant($"{AR_AP} Payment Request Cancelled - ZOrg AUD 1200.00"), "test@test.com"),
			};

			var actual = emails.Select(x => (x.Subject, x.Recipients.Cast<RecipientDef>().Single().Email));
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestCancelPaymentApprovals_NoPaymentsValid()
		{
			var approvalPST = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000m);
			approvalPST.AV_Status = PaymentApprovalStatus.Posted;

			var approvalCAN = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000m);
			approvalCAN.AV_Status = PaymentApprovalStatus.Cancelled;

			Factory.Save();

			var expectedMessage = $@"The following Payments cannot be canceled:
{approvalPST.GetDescription()}
This Payment is already Posted

{approvalCAN.GetDescription()}
This Payment is already Canceled";

			var approvals = new[] { approvalPST, approvalCAN };
			CancelPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Error is shown instead of confirmation if there are no valid Apporovals", UnitTestUserNotification.Instance.LastMessage.WasError);
			Assert("Approvals are unchagned if there are no valid Approvals to cancel", approvals.All(x => x == approvalCAN || x.AV_Status != PaymentApprovalStatus.Cancelled));
		}

		public void TestCancelPaymentApprovals_SomePaymentsValid()
		{
			var approvalAWA = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000m);
			approvalAWA.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			var approvalAPP = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000m);
			approvalAPP.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalPST = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000m);
			approvalPST.AV_Status = PaymentApprovalStatus.Posted;

			var approvalREJ = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000m);
			approvalREJ.AV_Status = PaymentApprovalStatus.Rejected;

			var approvalCAN = GetNewPaymentApprovalReadyToPost("00001006", "0006", 1000m);
			approvalCAN.AV_Status = PaymentApprovalStatus.Cancelled;

			Factory.Save();

			var expectedMessage = $@"The following Payments cannot be canceled:
{approvalPST.GetDescription()}
This Payment is already Posted

{approvalCAN.GetDescription()}
This Payment is already Canceled

The following Payments will be canceled:
{approvalAWA.GetDescription()}
{approvalAPP.GetDescription()}
{approvalREJ.GetDescription()}

Do you want to cancel these Payments?";

			var validApprovals = new[] { approvalAWA, approvalAPP, approvalREJ };
			var invalidApprovals = new[] { approvalPST, approvalCAN };
			var approvals = validApprovals.Concat(invalidApprovals).ToArray();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			CancelPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("No changes made if user answers No", approvals.All(x => x == approvalCAN || x.AV_Status != PaymentApprovalStatus.Cancelled));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			CancelPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Valid Approvals are cancelled if user answers Yes", validApprovals.All(x => x.AV_Status == PaymentApprovalStatus.Cancelled));
			Assert("Invalid Approvals are unchanged if user answers Yes", invalidApprovals.All(x => x == approvalCAN || x.AV_Status != PaymentApprovalStatus.Cancelled));
		}

		void CancelPayments(BusinessObject[] payments)
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, payments);
				module.CancelPaymentApprovals_ForTestOnly(null, new EventArgs());
			}
		}

		#endregion

		#region Reject

		public void TestRejectPaymentApprovals()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "test@test.com";
			Factory.Save();

			var currentUser = GlbStaff.CurrentUser;
			PaymentApprovalWithAuthorisation approval1;
			PaymentApprovalWithAuthorisation approval2;
			PaymentApprovalWithAuthorisation approval3;
			PaymentApprovalWithAuthorisation approval4;
			PaymentApprovalWithAuthorisation approval5;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval1 = GetNewPaymentApproval();
				approval1.AV_Amount = 1000m; // None Required

				approval2 = GetNewPaymentApproval();
				approval2.AV_Amount = 2000m; // 1st Required
				approval2.AV_GS_NKApproval1st = newStaff.GS_Code;

				approval3 = GetNewPaymentApproval();
				approval3.AV_Amount = 3000m; // 2nd Required
				approval3.AV_GS_NKApproval2nd = newStaff.GS_Code;

				approval4 = GetNewPaymentApproval();
				approval4.AV_Amount = 4000m; // 3rd Required
				approval4.AV_GS_NKApproval3rd = newStaff.GS_Code;

				approval5 = GetNewPaymentApproval();
				approval5.AV_Amount = 10000m; // All 3 Required
				approval5.AV_GS_NKApproval1st = newStaff.GS_Code;
				approval5.AV_GS_NKApproval2nd = newStaff.GS_Code;
				approval5.AV_GS_NKApproval3rd = newStaff.GS_Code;

				Factory.Save();
			}

			var businessObjects = new BusinessObject[5];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;
			businessObjects[4] = approval5;

			AssertCorrectAuthorisation("Precondition: Approval1", approval1, null, null, null);
			AssertCorrectAuthorisation("Precondition: Approval2", approval2, newStaff, null, null);
			AssertCorrectAuthorisation("Precondition: Approval3", approval3, null, newStaff, null);
			AssertCorrectAuthorisation("Precondition: Approval4", approval4, null, null, newStaff);
			AssertCorrectAuthorisation("Precondition: Approval5", approval5, newStaff, newStaff, newStaff);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObjs are selected", 5, module.SelectedBusinessObjects_ForTestOnly.Length);

				FirstApprovalCheckPoint.IsAllowed = false;
				SecondApprovalCheckPoint.IsAllowed = false;
				ThirdApprovalCheckPoint.IsAllowed = false;
				AssertEquals("UserCanAuthoriseLevel1", false, approval1.UserHasAuthoriseLevel1Security);
				AssertEquals("UserCanAuthoriseLevel2", false, approval1.UserHasAuthoriseLevel2Security);
				AssertEquals("UserCanAuthoriseLevel3", false, approval1.UserHasAuthoriseLevel3Security);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				module.RejectPaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The following Payments cannot be rejected:
Payment ZOrg CHQ  (AUD 2000.00)
You do not have sufficient rights to reject this payment. Required authorization level: Level 1.

Payment ZOrg CHQ  (AUD 3000.00)
You do not have sufficient rights to reject this payment. Required authorization level: Level 2.

Payment ZOrg CHQ  (AUD 4000.00)
You do not have sufficient rights to reject this payment. Required authorization level: Level 3.

Payment ZOrg CHQ  (AUD 10000.00)
You do not have sufficient rights to reject this payment. Required authorization level: Level 1 and Level 2 and Level 3.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCorrectAuthorisation("Approval1", approval1, currentUser, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, newStaff, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, null, newStaff, null);
				AssertCorrectAuthorisation("Approval4", approval4, null, null, newStaff);
				AssertCorrectAuthorisation("Approval5", approval5, newStaff, newStaff, newStaff);

				var emails = Env.OutgoingMailManager.EmailsCreated;
				var expected = new[] { (AR_AP + " Payment Request Rejected - ZOrg AUD 1000.00", "test@test.com") };
				var actual = emails.Select(x => (x.Subject, x.Recipients.Cast<RecipientDef>().Single().Email));
				AssertContainsExactElementsInAnyOrder(expected, actual);

				approval1.AV_GS_NKApproval1st = ZString.Empty;
				approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				AssertEquals("UserCanAuthoriseLevel1", true, approval1.UserHasAuthoriseLevel1Security);
				AssertEquals("UserCanAuthoriseLevel2", true, approval1.UserHasAuthoriseLevel2Security);
				AssertEquals("UserCanAuthoriseLevel3", true, approval1.UserHasAuthoriseLevel3Security);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				PaymentRejectionReasonForm rejectionForm = null;
				void captureForm(object x) => rejectionForm = x as PaymentRejectionReasonForm;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(captureForm);

				module.RejectPaymentApprovals_ForTestOnly(null, new EventArgs());

				var expectedPaymentsLabelText = @"Payment ZOrg CHQ  (AUD 1000.00)
Payment ZOrg CHQ  (AUD 2000.00)
Payment ZOrg CHQ  (AUD 3000.00)
Payment ZOrg CHQ  (AUD 4000.00)
Payment ZOrg CHQ  (AUD 10000.00)";
				var actualPaymentsLabelText = rejectionForm.PaymentsLabel.GetExtension<ILabelCaptionRenderer>().Caption;

				AssertEquals(expectedPaymentsLabelText, actualPaymentsLabelText);
				AssertCorrectAuthorisation("Approval1", approval1, currentUser, null, null);
				AssertCorrectAuthorisation("Approval2", approval2, currentUser, null, null);
				AssertCorrectAuthorisation("Approval3", approval3, currentUser, null, null);
				AssertCorrectAuthorisation("Approval4", approval4, currentUser, null, null);
				AssertCorrectAuthorisation("Approval5", approval5, currentUser, null, null);

				emails = Env.OutgoingMailManager.EmailsCreated;
				expected = new[]
				{
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 1000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 1000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 2000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 3000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 4000.00"), "test@test.com"),
					(Invariant($"{AR_AP} Payment Request Rejected - ZOrg AUD 10000.00"), "test@test.com"),
				};

				actual = emails.Select(x => (x.Subject, x.Recipients.Cast<RecipientDef>().Single().Email));
				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestRejectPaymentApprovals_NoPayments()
		{
			RejectPayments(null);
			AssertEquals("Please select one or more Payments to Reject.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRejectPaymentApprovals_AllPaymentsValid()
		{
			var approvalAWA = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000m);
			approvalAWA.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			var approvalAPP = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000m);
			approvalAPP.AV_Status = PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			var approvals = new[] { approvalAWA, approvalAPP };
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(x => ((PaymentRejectionReasonForm)x).SetReason("DIS", "Charges Disputed"));

			RejectPayments(approvals);
			AssertEquals(PaymentApprovalStatus.AwaitingApproval, approvalAWA.AV_Status);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approvalAPP.AV_Status);

			Assert("No changes made if user answers No", approvals.All(x => x.AV_Status != PaymentApprovalStatus.Rejected));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			PaymentRejectionReasonForm rejectionForm = null;
			void setReason(object x)
			{
				rejectionForm = x as PaymentRejectionReasonForm;
				rejectionForm.SetReason("INS", "Not enough money");
			}

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setReason);
			RejectPayments(approvals);
			var expected = new[]
			{
					((ZString)PaymentApprovalStatus.Rejected, (ZString)"INS", (ZString)"Not enough money"),
					((ZString)PaymentApprovalStatus.Rejected, (ZString)"INS", (ZString)"Not enough money")
				};

			var expectedPaymentsLabelText = $@"{approvalAWA.GetDescription()}
{approvalAPP.GetDescription()}";
			var actualPaymentsLabelText = rejectionForm.PaymentsLabel.GetExtension<ILabelCaptionRenderer>().Caption;
			AssertEquals(expectedPaymentsLabelText, actualPaymentsLabelText);
			AssertArrayEqualsByElements(expected, approvals.Select(x => (x.AV_Status, x.AV_RejectionReasonCode, x.AV_RejectionReasonDetails)).ToArray());
		}

		public void TestRejectPaymentApprovals_SomePaymentsValid()
		{
			var approvalAWA = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000m);
			approvalAWA.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			var approvalAPP = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000m);
			approvalAPP.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approvalPST = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000m);
			approvalPST.AV_Status = PaymentApprovalStatus.Posted;

			var approvalREJ = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000m);
			approvalREJ.AV_Status = PaymentApprovalStatus.Rejected;

			Factory.Save();

			var expectedMessage = $@"The following Payments cannot be rejected:
{approvalPST.GetDescription()}
This Payment is already Posted

{approvalREJ.GetDescription()}
This Payment is already Rejected";

			var validApprovals = new[] { approvalAWA, approvalAPP };
			var invalidApprovals = new[] { approvalPST, approvalREJ };
			var approvals = validApprovals.Concat(invalidApprovals).ToArray();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			RejectPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("No changes made if user answers No", approvals.All(x => x == approvalREJ || x.AV_Status != PaymentApprovalStatus.Rejected));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			RejectPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Valid Approvals are rejected if user answers Yes", validApprovals.All(x => x.AV_Status == PaymentApprovalStatus.Rejected));
			Assert("Invalid Approvals are unchanged if user answers Yes", invalidApprovals.All(x => x == approvalREJ || x.AV_Status != PaymentApprovalStatus.Rejected));
		}

		public void TestRejectPaymentApprovals_NoPaymentsValid()
		{
			var approvalPST = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000m);
			approvalPST.AV_Status = PaymentApprovalStatus.Posted;

			var approvalREJ = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000m);
			approvalREJ.AV_Status = PaymentApprovalStatus.Rejected;

			Factory.Save();

			var expectedMessage = $@"The following Payments cannot be rejected:
{approvalPST.GetDescription()}
This Payment is already Posted

{approvalREJ.GetDescription()}
This Payment is already Rejected";

			var approvals = new[] { approvalPST, approvalREJ };
			RejectPayments(approvals);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Error is shown instead of confirmation if there are no valid Apporovals", UnitTestUserNotification.Instance.LastMessage.WasError);
			Assert("Approvals are unchagned if there are no valid Approvals to reject", approvals.All(x => x == approvalREJ || x.AV_Status != PaymentApprovalStatus.Rejected));
		}

		void RejectPayments(BusinessObject[] payments)
		{
			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, payments);
				module.RejectPaymentApprovals_ForTestOnly(null, new EventArgs());
			}
		}

		public void TestSingleEventLogIsCreatedPer_Authorize_Unauthorize_RejectEvent()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			GlbStaff currentUser = GlbStaff.CurrentUser;

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001001", "0001", 1000M);
			approval1.AV_GS_NKApproval1st = ZString.Empty;
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			approval2.AV_GS_NKApproval1st = newStaff.GS_Code;
			approval2.AuthorisationRequired.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			//FirstApprovalCheckPoint.IsAllowed = false;
			//SecondApprovalCheckPoint.IsAllowed = false;
			//ThirdApprovalCheckPoint.IsAllowed = false;

			Factory.Save();

			var authorisedApprovals = new[] { approval1 };
			var withdrawnApprovals = new[] { approval2 };
			var rejectedApprovals = new[] { approval3 };

			using (PaymentProcessingModule module1 = GetNewModule())
			{
				SelectBusinessObjects(module1, authorisedApprovals);
				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				module1.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertCorrectAuthorisation("Approval1", approval1, currentUser, null, null);
			}

			using (PaymentProcessingModule module2 = GetNewModule())
			{
				SelectBusinessObjects(module2, withdrawnApprovals);
				FirstApprovalCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module2.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs());
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			void setReason(object x)
			{
				((PaymentRejectionReasonForm)x).SetReason("INS", "No money.");
			}

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setReason);
			RejectPayments(rejectedApprovals);

			Factory.Save();

			var expected = new[]
			{
					((ZString)AutoEvents.AuthorisedCode),
					((ZString)AutoEvents.AuthorisationWithdrawnCode),
					((ZString)AutoEvents.AuthorisationRejectedCode)
				};

			var query1 = new ZQuery(StmALogSchema.SL_Parent, approval1.PK);
			var query2 = new ZQuery(StmALogSchema.SL_Parent, approval2.PK);
			var query3 = new ZQuery(StmALogSchema.SL_Parent, approval3.PK);
			var log1 = Factory.Load<StmALog>(query1).SingleOrDefault(x => x.SL_SE_NKEvent == "ATH");
			var log2 = Factory.Load<StmALog>(query2).SingleOrDefault(x => x.SL_SE_NKEvent == "ATW");
			var log3 = Factory.Load<StmALog>(query3).SingleOrDefault(x => x.SL_SE_NKEvent == "ATR");

			var logs = new[] { log1, log2, log3 };

			AssertArrayEqualsByElements("Precondition:", expected, logs.Select(x => x.SL_SE_NKEvent).ToArray());
		}

		public void TestEventLogIsCreatedWhenRejectingPaymentApprovals()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001001", "0001", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			Factory.Save();

			var approvals = new[] { approval1 };

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			void setReason(object x)
			{
				((PaymentRejectionReasonForm)x).SetReason("INS", "No money");
			}

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setReason);

			RejectPayments(approvals);

			Factory.Save();

			var expected = new[]
			{
					(
						(ZString)"ADD",
						(ZString)"Added a record to the system",
						ZString.Empty,
						ZString.Empty
					),
					(
						(ZString)AutoEvents.AuthorisationRejectedCode,
						(ZString)AutoEvents.AuthorisationRejected.Description,
						(ZString)"Payment Request Rejected|RES=Insufficient Funds. No money",
						(ZString)" Authorization Rejected Reason: Insufficient Funds. No money, Payment Request Rejected"
					),
					(
						(ZString)"EDT",
						(ZString)"Edited a record",
						ZString.Empty,
						ZString.Empty
					)
				};

			(ZString, ZString, ZString, ZString) actualSelector(StmALog x) =>
			(                                           //UI column name
				x.SL_SE_NKEvent,
				x.SL_EventDescription,                  //Event Name
				x.SL_ReferenceForBinding,               //Reference
				x.DisplayEventReference                 //Event Details
			);

			var query = new ZQuery()
				.AddToFilter(StmALogSchema.SL_Parent, approval1.PK);

			var logs = Factory.Load<StmALog>(query);
			AssertContainsExactElementsInAnyOrder(expected, logs.Select(actualSelector).ToArray());
		}

		public void TestEventLogForPaymentApprovalsContainsRejectionType()
		{
			AssertNotNull(AutoEvents.AuthorisationRejected);
		}

		#endregion

		public void TestPostPaymentApprovals_WillPopulateChequeNo()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var unAutoPrintChequeBook = GetAutoPrintChequeBook(1, 4, 1);
			unAutoPrintChequeBook.AK_AutoPrintCheque = false;
			var autoPrintChequeBook = GetAutoPrintChequeBook(11, 14, 11);
			Factory.Save();

			var approval1 = GetNewPaymentApprovalReadyToPost("00001001", ZString.Empty, 1000M);
			approval1.AV_PaymentType = ReceiptTypes.Cheque;
			approval1.AV_AB = unAutoPrintChequeBook.AK_AB;
			approval1.AV_AK = unAutoPrintChequeBook.PK;
			approval1.AV_ChequeOrReference = unAutoPrintChequeBook.AK_CurrentNo.ToString();
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			var approval2 = GetNewPaymentApprovalReadyToPost("00001002", ZString.Empty, 2000M);
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoPrintChequeBook.AK_AB;
			approval2.AV_AK = autoPrintChequeBook.PK;
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, new[] { approval1, approval2 });
				AssertEquals("Precondition: BizObjs are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals("Percondition", "1", approval1.AV_ChequeOrReference);
				AssertNullOrEmpty("Percondition", approval2.AV_ChequeOrReference);

				module.PostPaymentApprovals(null, new EventArgs());

				AssertEquals(@"The following Payments will be processed:
Payment ZOrg CHQ 1 (AUD 1000.00)
Payment ZOrg CHQ  (AUD 2000.00)

Do you want to post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("ChequeOrReference still 1", "1", approval1.AV_ChequeOrReference);
				AssertEquals("ChequeOrReference will populate", "11", approval2.AV_ChequeOrReference);
			}
		}

		public void TestPostPaymentApprovals()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			approval3 = Factory.Load<PaymentApprovalWithAuthorisation>(approval3.PK);

			BusinessObject[] businessObjects = new BusinessObject[3];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;

			Assert("Precondition: Approval1 should have no errors", !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval3.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval3.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 3, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = false;
				AssertEquals(false, PostCheckPoint.IsAllowed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Message", PostCheckPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.FullyApproved, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertNull("There should not be any payments passed for printing yet", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting);

				PostCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertEquals("There should not be 1 payment passed for printing", 1, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Payment passed for printing should be Approval2's payment", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("Cheque printing should be activated", module.PaymentProcessingGUIHelper.Test_ActivateChequePrinting);
			}
		}

		public void TestErrorWhenPostingRejectedPaymentApprovals()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001001", "0001", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.Rejected;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.Rejected;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			BusinessObject[] businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			Assert("Precondition: Approval1 should have no errors", !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				AssertEquals(true, PostCheckPoint.IsAllowed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());

				AssertEquals("Message", @"The following Payments cannot be processed:
Payment ZOrg CSH 0001 (AUD 1000.00)
This payment has been rejected.
Payment ZOrg CSH 0002 (AUD 1000.00)
This payment has been rejected.

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.Rejected, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Rejected, approval2.AV_Status);

				Assert(!approval1.IsPosted);
				Assert(!approval2.IsPosted);
			}
		}

		public void TestPostPaymentApprovalsWithNullApproval()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = approval1;

			using (PaymentProcessingModule module = GetNewModule())
			{
				businessObjects[0].Delete();
				Factory.Save();
				var loadedApprovals = module.ReloadApprovalsInTheNewFactory_ForTestOnly(businessObjects, new BusinessObjectFactory());
				AssertEquals(0, loadedApprovals.Count);
			}
		}

		public void TestPostPaymentApprovals_KeepTheSameOrder()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			var approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			var approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			var approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			var bizOs = Factory.Load<PaymentApprovalBase>(new ZQuery()).OrderByDescending(bizo => bizo.PK).ToArray();

			using (PaymentProcessingModule module = GetNewModule())
			{
				var loadedApprovals = module.ReloadApprovalsInTheNewFactory_ForTestOnly(bizOs, new BusinessObjectFactory());
				AssertEquals(3, loadedApprovals.Count);

				CombineAssertions("The following members has wrong order:", delegate
				{
					for (var i = 0; i < bizOs.Length; ++i)
					{
						AssertEquals(bizOs[i].PK, loadedApprovals[i].PK);
					}
				});
			}
		}

		public void TestPostPaymentApprovalsWithNoPermissionToCreatePaymentType()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[3];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 3, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				NewCashPaymentCheckPoint.IsAllowed = false;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertContains("Message", string.Format("You cannot post Payment {0} CSH 0002 (AUD 1000.00). Please fix the following errors before posting", TestOrgHeader.OH_Code), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Message", string.Format("You cannot post Payment {0} CSH 0003 (AUD 1000.00). Please fix the following errors before posting", TestOrgHeader.OH_Code), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Message", string.Format("You cannot post Payment {0} CSH 0004 (AUD 1000.00). Please fix the following errors before posting", TestOrgHeader.OH_Code), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Number of security errors: 'You do not have appropriate security rights to select this payment type'", 3, UnitTestUserNotification.Instance.LastMessage.Text.CountMatches("You do not have appropriate security rights to select this payment type"));
				AssertEquals("Approval1 Status", PaymentApprovalStatus.FullyApproved, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.FullyApproved, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.FullyApproved, approval3.AV_Status);

				NewCashPaymentCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.Posted, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
			}
		}

		[TestDate(2012, 3, 8)]
		public void TestPostPaymentApprovalsWhenPeriodClosed()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var periodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2012, 1, 1));
			var firstPeriod = periodManager.Periods[0];
			AssertEquals("Period Not Closed", false, firstPeriod.AM_IsSubLedgerClosed);
			Factory.Save();

			var approval1 = GetNewPaymentApprovalReadyToPost("00001001", "0001", 1000M);
			approval1.AV_OH = TestObjectCreator.TestOrganisation.PK;
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval1.AV_PostDate = new ZDateTime(2012, 1, 15);

			var approval2 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 2000M);
			approval2.AV_OH = approval2.Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.ABIGAS.PK : TestObjectCreator.AALSHI.PK;
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PostDate = ZDateTime.Today;
			Factory.Save();

			firstPeriod.AM_IsSubLedgerClosed = true;
			AssertEquals("Period Closed ", true, firstPeriod.AM_IsSubLedgerClosed);
			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			var businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals(@"You cannot post Payment ZOrg CSH 0001 (AUD 1000.00). Please fix the following errors before posting.
- Post Date: This date falls into a period where the sub-ledger is closed

", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostPaymentApprovalsWithReversedTransaction()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
			TestObjectCreator testInvoiceCreator = new TestObjectCreator(invoiceCreationFactory);

			APInvoice invoice1 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001100", testInvoiceCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			testInvoiceCreator.AttachJobToAPLine(invoice1.Lines[0]);
			invoice1.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			testInvoiceCreator.AttachChargeToAPLine(invoice1.Lines[0]);
			invoiceCreationFactory.Save();
			invoice1 = Factory.Load<APInvoice>(invoice1.PK);

			APInvoice invoice2 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001101", testInvoiceCreator.AUD, 1m, 200m, 0m, 200m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			testInvoiceCreator.AttachJobToAPLine(invoice2.Lines[0]);
			invoice2.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			testInvoiceCreator.AttachChargeToAPLine(invoice2.Lines[0]);
			invoiceCreationFactory.Save();
			invoice2 = Factory.Load<APInvoice>(invoice2.PK);

			AccBankAccount bankAccount = testObjectCreator.AUDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			AccChequeBook chequeBook = testObjectCreator.AUDChequeBook;

			PaymentApprovalBase testPaymentApproval = GetNewPaymentApproval();
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = chequeBook.PK;
			testPaymentApproval.ExchangeRate.Currency = "AUD";
			testPaymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			testPaymentApproval.AV_Amount = 300m;

			testPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			testPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
			InvoicingBaseReversing reverser = new InvoicingBaseReversing(invoice1);
			reverser.Reverse();
			((TransactionHeader)reverser.ReverseTransaction).AH_TransactionNum = "00001000";
			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = testPaymentApproval;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				PostCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				AssertNull("There should be no payment passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting);
				AssertContains("This Payment has a reversed transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostAndAutoPrint_MixedPaymentApprovals()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoAllocateChequeBook.AK_AB;
			approval2.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval2).IsAutoAllocationEnabled);

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.FullyApproved;

			PaymentApprovalWithAuthorisation approval4 = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000M);
			approval4.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval4.AV_PaymentType = ReceiptTypes.Cheque;
			approval4.AV_AB = autoAllocateChequeBook.AK_AB;
			approval4.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval4).IsAutoAllocationEnabled);

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			approval3 = Factory.Load<PaymentApprovalWithAuthorisation>(approval3.PK);
			approval4 = Factory.Load<PaymentApprovalWithAuthorisation>(approval4.PK);

			BusinessObject[] businessObjects = new BusinessObject[4];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;

			Assert("Precondition: Approval1 should have no errors", !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval3.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval3.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval4.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval4.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 4, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval4.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertNotNull("Approval4 Payment", approval3.TransactionHeader);
				AssertNotNull("Approval4 Payment", approval4.TransactionHeader);

				AssertEquals("There should be only 1 collection passed for autoprinting", 1, module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting.Count);
				Assert("The collection passed for printing should have Payment of Approval2 inside", module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting[0].Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("The flag should be set on PaymentPrintManager level", module.PaymentProcessingGUIHelper.Test_Allocator.ChequeWasAutoPrinted);

				AssertEquals("There should be 3 payments passed for printing", 3, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Approval2's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("Approval3's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval3.TransactionHeader.PK));
				Assert("Approval4's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval4.TransactionHeader.PK));
				Assert("Cheque printing should be activated", module.PaymentProcessingGUIHelper.Test_ActivateChequePrinting);

				PaymentApprovalWithAuthorisation paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "3");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				AccTransactionHeader newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);

				paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "4");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
				autoAllocateChequeBook.Reload();
				AssertEquals("Cheque book should be correctly incremented", 5m, autoAllocateChequeBook.AK_CurrentNo);
			}
		}

		[ExpectNoExceptions]
		public void TestPostPaymentApprovals_WithDeletedOrganization()
		{
			OrgHeader testOrgHeader2 = Factory.New<OrgHeader>();
			testOrgHeader2.OH_Code = TestObjectCreator.GetRandomString(8);
			testOrgHeader2.CompanyData.OB_IsCreditor = true;
			Factory.Save();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);

			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoAllocateChequeBook.AK_AB;
			approval2.AV_AK = autoAllocateChequeBook.PK;
			approval2.AV_OH = testOrgHeader2.PK;
			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			BusinessObject[] businessObjects = new BusinessObject[] { approval1, approval2 };

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				PostCheckPoint.IsAllowed = true;
				testOrgHeader2.Delete();
				module.PostPaymentApprovals(null, new EventArgs());
				module.PopulateChequeNoForPaymentApprovals_ForTestOnly(null, new EventArgs());
				module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
			}
		}

		public void TestPostAndAutoPrint_PostMode()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoAllocateChequeBook.AK_AB;
			approval2.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval2).IsAutoAllocationEnabled);

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			PaymentApprovalWithAuthorisation approval4 = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000M);
			approval4.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval4.AV_PaymentType = ReceiptTypes.Cheque;
			approval4.AV_AB = autoAllocateChequeBook.AK_AB;
			approval4.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval4).IsAutoAllocationEnabled);

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			approval3 = Factory.Load<PaymentApprovalWithAuthorisation>(approval3.PK);
			approval4 = Factory.Load<PaymentApprovalWithAuthorisation>(approval4.PK);

			BusinessObject[] businessObjects = new BusinessObject[4];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;

			Assert("Precondition: Approval1 should have no errors", !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval3.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval3.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval4.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval4.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 4, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval4.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertNotNull("Approval4 Payment", approval4.TransactionHeader);

				AssertEquals("There should be only 1 collection passed for autoprinting", 1, module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting.Count);
				Assert("The collection passed for printing should have Payment of Approval2 inside", module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting[0].Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("The flag should be set on PaymentPrintManager level", module.PaymentProcessingGUIHelper.Test_Allocator.ChequeWasAutoPrinted);

				AssertEquals("There should be 2 payments passed for printing", 2, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Approval2's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("Approval4's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval4.TransactionHeader.PK));
				Assert("Cheque printing should not be activated", !module.PaymentProcessingGUIHelper.Test_ActivateChequePrinting);

				PaymentApprovalWithAuthorisation paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "3");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				AccTransactionHeader newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);

				paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "4");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
				autoAllocateChequeBook.Reload();
				AssertEquals("Cheque book should be correctly incremented", 5m, autoAllocateChequeBook.AK_CurrentNo);
			}
		}

		public void TestPopulateChequeNoForPaymentApprovals_TwoChecks()
		{
			AssertPopulateChequeNoWithTwoChecks(false);
		}

		public void TestPopulateChequeNoForPaymentApprovals_OneCheckOneCash()
		{
			AssertPopulateChequeNoWithOneChequeAndOneCash(false);
		}

		public void TestPopulateChequeNoForPaymentApprovals_NoCheckTwoCash()
		{
			AssertPopulateChequeNoWithNoCheckTwoCash(false);
		}

		public void TestPopulateChequeNoAndPostPaymentApprovals_TwoChecks()
		{
			AssertPopulateChequeNoWithTwoChecks(true);
		}

		public void TestPopulateChequeNoAndPostPaymentApprovals_OneCheckOneCash()
		{
			AssertPopulateChequeNoWithOneChequeAndOneCash(true);
		}

		public void TestPopulateChequeNoAndPostPaymentApprovals_NoCheckTwoCash()
		{
			AssertPopulateChequeNoWithNoCheckTwoCash(true);
		}

		#region Assertion Methods

		void AssertPopulateChequeNoWithTwoChecks(ZBool shouldPostApprovals)
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 4, 3);
			chequeBook.AK_AutoPrintCheque = false;

			var approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_PaymentType = ReceiptTypes.Cheque;
			approval1.AV_AB = chequeBook.AK_AB;
			approval1.AV_AK = chequeBook.PK;

			var approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = chequeBook.AK_AB;
			approval2.AV_AK = chequeBook.PK;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			var businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);

				if (shouldPostApprovals)
				{
					module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
				}
				else
				{
					module.PopulateChequeNoForPaymentApprovals_ForTestOnly(null, new EventArgs());
				}

				AssertEquals("checque number", "3", approval1.AV_ChequeOrReference);
				AssertEquals("checque number", "4", approval2.AV_ChequeOrReference);

				if (shouldPostApprovals)
				{
					AssertEquals(PaymentApprovalStatus.Posted, approval1.AV_Status);
					AssertEquals(PaymentApprovalStatus.Posted, approval2.AV_Status);
				}
			}
		}

		void AssertPopulateChequeNoWithOneChequeAndOneCash(ZBool shouldPostApprovals)
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 4, 3);
			chequeBook.AK_AutoPrintCheque = false;

			var approval1 = GetNewPaymentApprovalReadyToPost("1", "0002", 1000M);
			approval1.AV_PaymentType = ReceiptTypes.Cash;

			var approval2 = GetNewPaymentApprovalReadyToPost("2", "0003", 1000M);
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = chequeBook.AK_AB;
			approval2.AV_AK = chequeBook.PK;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			var businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);

				if (shouldPostApprovals)
				{
					module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
				}
				else
				{
					module.PopulateChequeNoForPaymentApprovals_ForTestOnly(null, new EventArgs());
				}

				AssertEquals("Not a check will retain original number", "0002", approval1.AV_ChequeOrReference);
				AssertEquals("Is only check in batch", "3", approval2.AV_ChequeOrReference);
				if (shouldPostApprovals)
				{
					AssertEquals("Last warning, only 1 check will be processed", "Only Payments of Cheque Type will have a Cheque Number Allocated and Posted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(PaymentApprovalStatus.FullyApproved, approval1.AV_Status);
					AssertEquals(PaymentApprovalStatus.Posted, approval2.AV_Status);
				}
				else
				{
					AssertEquals("Last warning, only 1 check will be processed", "Only Payments of Cheque Type will have a Cheque Number Allocated.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void AssertPopulateChequeNoWithNoCheckTwoCash(ZBool shouldPostApprovals)
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 4, 3);
			chequeBook.AK_AutoPrintCheque = false;

			var approval1 = GetNewPaymentApprovalReadyToPost("1", "0002", 1000M);
			approval1.AV_PaymentType = ReceiptTypes.Cash;
			var approval2 = GetNewPaymentApprovalReadyToPost("2", "0003", 1000M);
			approval2.AV_PaymentType = ReceiptTypes.Cash;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			var businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);

				if (shouldPostApprovals)
				{
					module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
				}
				else
				{
					module.PopulateChequeNoForPaymentApprovals_ForTestOnly(null, new EventArgs());
				}

				AssertEquals("Not a check will retain original number", "0002", approval1.AV_ChequeOrReference);
				AssertEquals("Not a check will retain original number", "0003", approval2.AV_ChequeOrReference);
				AssertEquals("Last warning, there were no checks selected", "No Cheques Selected", UnitTestUserNotification.Instance.LastMessage.Text);

				if (shouldPostApprovals)
				{
					AssertEquals(PaymentApprovalStatus.FullyApproved, approval1.AV_Status);
					AssertEquals(PaymentApprovalStatus.FullyApproved, approval2.AV_Status);
				}
			}
		}

		#endregion

		public void TestPostAndAutoPrint_AutoAllocateAndPostMode()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoAllocateChequeBook.AK_AB;
			approval2.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval2).IsAutoAllocationEnabled);

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			PaymentApprovalWithAuthorisation approval4 = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000M);
			approval4.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval4.AV_PaymentType = ReceiptTypes.Cheque;
			approval4.AV_AB = autoAllocateChequeBook.AK_AB;
			approval4.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval4).IsAutoAllocationEnabled);

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			approval3 = Factory.Load<PaymentApprovalWithAuthorisation>(approval3.PK);
			approval4 = Factory.Load<PaymentApprovalWithAuthorisation>(approval4.PK);

			BusinessObject[] businessObjects = new BusinessObject[4];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;

			Assert("Precondition: Approval1 should have no errors", !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval3.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval3.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors", !approval4.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval4.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 4, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval4.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertNotNull("Approval4 Payment", approval4.TransactionHeader);

				AssertEquals("There should be only 1 collection passed for autoprinting", 1, module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting.Count);
				Assert("The collection passed for printing should have Payment of Approval2 inside", module.PaymentProcessingGUIHelper.Test_Allocator.CollectionsPassedForPrinting[0].Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("The flag should be set on PaymentPrintManager level", module.PaymentProcessingGUIHelper.Test_Allocator.ChequeWasAutoPrinted);

				AssertEquals("There should be 2 payments passed for printing", 2, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Approval2's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("Approval4's payment should be passed for printing", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval4.TransactionHeader.PK));
				Assert("Cheque printing should not be activated", !module.PaymentProcessingGUIHelper.Test_ActivateChequePrinting);

				PaymentApprovalWithAuthorisation paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "3");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				AccTransactionHeader newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);

				paymentApproval = GetPaymentApprovalByChequeNumber(businessObjects, "4");
				AssertNotNull("Payment Approval should exist", paymentApproval);
				Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
				newPayment = Factory.Load<AccTransactionHeader>(paymentApproval.AV_AH);
				AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
				autoAllocateChequeBook.Reload();
				AssertEquals("Cheque book should be correctly incremented", 5m, autoAllocateChequeBook.AK_CurrentNo);
			}
		}

		public void TestPaymentsPassedForPostingAreLoadedInNewFactory()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = approval2;

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertEquals("There should not be 1 payment passed for printing", 1, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Payment passed for printing should be Approval2's payment", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				AssertNotEquals("Payment's factory should differ from the module's factory", module.Factory_ForTestOnly, approval2.TransactionHeader.Factory);
				Assert("Cheque printing should be activated", module.PaymentProcessingGUIHelper.Test_ActivateChequePrinting);
			}
		}

		[ExpectNoExceptions()]
		public void TestExceptionIsHandled_ChequeBookIsFull()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = autoAllocateChequeBook.AK_AB;
			approval2.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)approval2).IsAutoAllocationEnabled);

			Factory.Save();
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = approval2;

			Assert("Precondition: Approval2 should have no errors", !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				module.PaymentProcessingGUIHelper.Test_DeactivateChequeBookOnAllocation = ZBool.True;
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("There should be a message shown, saying that the Cheque Book is full.", ErrorMessages.ChequeBookIsFullExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.FullyApproved, approval2.AV_Status);
				AssertNull("Approval2 Payment", approval2.TransactionHeader);

				autoAllocateChequeBook.Reload();
				AssertEquals("Cheque book should not be incremented", 3m, autoAllocateChequeBook.AK_CurrentNo);
			}
		}

		public void TestPostPaymentApprovalsWithErrors()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_PayExRate = 0m; // To cause error
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_PayExRate = 0m; // To cause error
			approval2.AV_Status = PaymentApprovalStatus.Posted;

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_PayExRate = 0m; // To cause error
			approval3.AV_Status = PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[3];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 3, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = false;
				AssertEquals(false, PostCheckPoint.IsAllowed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Message", PostCheckPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.FullyApproved, approval3.AV_Status);
				AssertNull(module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed);

				PostCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				AssertContains("You cannot post", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.FullyApproved, approval3.AV_Status);
				//Approvals 1 and 2 can not be processed because Approval 1 is awaiting approval and Approval 2 is already posted
				AssertEquals(1, module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed.Count);
				AssertEquals(approval3.PK, module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed[0].PK);
				AssertContains(approval3.AV_ChequeOrReference, UnitTestUserNotification.Instance.LastMessage.Text);

				businessObjects = new BusinessObject[2];
				businessObjects[0] = approval1;
				businessObjects[1] = approval2;
				SelectBusinessObjects(module, businessObjects);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				module.PostPaymentApprovals(null, new EventArgs());
				AssertContains("The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(approval1.AV_ChequeOrReference, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(approval2.AV_ChequeOrReference, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed.Count);
			}
		}

		public void TestPostPaymentApprovalsWithWarningOnChequeBook()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 1000;
			chequeBook.AK_CurrentNo = 1;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_StartNo = 1;
			book2.AK_LastNo = 1000;
			book2.AK_CurrentNo = 1;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			Factory.Save();

			PaymentApprovalWithAuthorisation approval = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			approval.AV_PaymentType = ReceiptTypes.Cheque;
			approval.AV_AB = testBank.PK;
			approval.AV_AK = chequeBook.PK;
			approval.AV_ChequeOrReference = "999";
			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = approval;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Last message should be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostPaymentApprovalsWithPostDateWarning()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_PostDate = ZDateTime.Today.AddDays(-5);
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = approval1;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				ZString expectedMessage = @"Alternatively, set the Registry 'Allow Back Posting Sub Ledger Transaction' to YES and ensure that you are allowed to back post transactions in Staff and Resources before posting these transactions.";

				AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDelete()
		{
			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			approval1.AV_Status = PaymentApprovalStatus.Posted;
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			Factory.Save();

			using (PaymentProcessingModule module = GetNewModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.ShowDeleteForm_ForTestOnly(approval1);
				AssertEquals(approval1.ReasonForNotAbleToDelete, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPopulateChequeNoAndPostPaymentApprovals_ErrorMessage()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var autoAllocateChequeBook = GetAutoPrintChequeBook(1, 4, 3);

			var paymentApproval = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			paymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			paymentApproval.AV_AB = autoAllocateChequeBook.AK_AB;
			paymentApproval.AV_AK = autoAllocateChequeBook.PK;
			Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)paymentApproval).IsAutoAllocationEnabled);

			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			paymentApproval = Factory.Load<PaymentApprovalWithAuthorisation>(paymentApproval.PK);

			var businessObjects = new BusinessObject[] { paymentApproval };

			Assert("Precondition: paymentApproval should have no errors", !paymentApproval.HasErrors);
			Assert("Precondition: paymentApproval should have no errors. Errors: ", !paymentApproval.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals("Precondition: BizObject are selected", 1, module.SelectedBusinessObjects_ForTestOnly.Length);

				PostCheckPoint.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();

				var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });
				Factory.Save();

				module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());

				var expectedMsg = string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
			, Env.CurrentDepartment.Code, Env.CurrentBranch.Code);
				AssertContains("Message", expectedMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPaymentApprovalsWithErrosAreRemovedFromApprovalsToProcess()
		{
			var periodSetupHelper = new AccountingPeriodTestHelper(Factory);
			periodSetupHelper.SetupPeriods();

			var paymentApprovalWithoutError = CreatePaymentApprovalMatchedWithAnInvoiceWithLines(50m, 50m, "INV1");
			paymentApprovalWithoutError.PaymentMatchingBaseObject.MatchAndClearTransactions();
			AssertEquals(0m, paymentApprovalWithoutError.PaymentMatchingBaseObject.Balance);

			var paymentApprovalWithError = CreatePaymentApprovalMatchedWithAnInvoiceWithLines(100m, 120m, "INV2");
			var exchangeDifference = paymentApprovalWithError.PaymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			paymentApprovalWithError.PaymentMatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
			paymentApprovalWithError.PaymentMatchingBaseObject.MatchAndClearTransactions();
			AssertEquals(0m, paymentApprovalWithError.PaymentMatchingBaseObject.Balance);

			var staff = TestObjectCreator.CreateStaff("ABC");
			staff.GS_EmailAddress = "jane.smith@gmail.com";
			var group = TestObjectCreator.CreateStaffGroup("STF");
			group.Staff.Add(staff);
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var targetAPInvoice = (InvoicingBase)paymentApprovalWithError.PaymentMatchingBaseObject.MatchedTransactions[1];
			new APInvoiceReversing(targetAPInvoice).Reverse();

			var apCreditNote = (APCreditNote)targetAPInvoice.ReverseTransaction;
			apCreditNote.AH_TransactionNum = "0000001";

			Factory.Save();

			Assert(((APInvoice)paymentApprovalWithError.PaymentMatchingBaseObject.MatchedTransactions[1]).IsReversed);
			AssertEquals(0m, paymentApprovalWithoutError.AV_ExchangeDifference);
			AssertEquals(20m, paymentApprovalWithError.AV_ExchangeDifference);

			var paymentsToProcess = new BusinessObject[] { paymentApprovalWithError, paymentApprovalWithoutError };

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, paymentsToProcess);
				AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);
				PostCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => module.PostPaymentApprovals(null, new EventArgs()));
				AssertEquals(1, module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed.Count);
				AssertEquals(paymentApprovalWithoutError.PK, module.PaymentProcessingGUIHelper.Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed[0].PK);
			}
		}

		public void TestPopulateChequeNoWithChequesFromDifferentChequeBooks_WithAllowEditCheckNumberBeforePostingRegistrySetting()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBookA = GetAutoPrintChequeBook(1, 4, 3);
			chequeBookA.AK_AutoPrintCheque = false;
			var chequeBookB = GetAutoPrintChequeBook(10, 40, 30);
			chequeBookB.AK_AutoPrintCheque = false;

			var approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_PaymentType = ReceiptTypes.Cheque;
			approval1.AV_AB = chequeBookA.AK_AB;
			approval1.AV_AK = chequeBookA.PK;

			var approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_PaymentType = ReceiptTypes.Cheque;
			approval2.AV_AB = chequeBookB.AK_AB;
			approval2.AV_AK = chequeBookB.PK;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowEditCheckNumberBeforePosting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);

			var businessObjects = new BusinessObject[2];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);
				module.PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(null, new EventArgs());
				AssertEquals(@"The registry 'Allow users to modify cheque number before posting a payment' is set to 'Yes'.

However, this function can only be used when all selected payments use the same cheque book and the cheque book is valid.

Please select payments using the same cheque book.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!approval1.IsPosted);
				Assert(!approval2.IsPosted);
			}
		}

		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 4, 3);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			var approval = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval.AV_PaymentType = ReceiptTypes.Cheque;
			approval.AV_AB = chequeBook.AK_AB;
			approval.AV_AK = chequeBook.PK;
			Factory.Save();

			using (var module = GetNewModule())
			{
				SelectBusinessObjects(module, new BusinessObject[] { approval });
				PostCheckPoint.IsAllowed = true;
				Assert("Error shown to user", PaymentDocumentsPrinter.PerformTestAutoAllocationAndPrintChequesFailure(() => module.PostPaymentApprovals(null, new EventArgs())));
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var paymentProcessingModule = GetNewModule())
			{
				AssertEquals("The payment processing module should support workflow.", true, paymentProcessingModule.SupportsWorkflow);
				AssertEquals("The WorkflowType of payment processing module should be empty.", string.Empty, paymentProcessingModule.WorkflowType);
			}
		}

		public void TestEPaymentColumnsVisibility()
		{
			AssertEPaymentColumnsVisibility(true);
			AssertEPaymentColumnsVisibility(false);
		}

		void AssertEPaymentColumnsVisibility(bool isOFXEPaymentEnabled)
		{
			var ePaymentColumnsNames = new string[] { "DealStatusDescription", "DealSubmittedLocalTime", "DealLastResponseLocalTime", "DealErrorMessage", "DealProviderReference" };

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			using (var paymentProcessingModule = GetNewModule())
			using (var filterControl = paymentProcessingModule.GetNewFilterControlForGrid())
			{
				var columnStyles = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => ePaymentColumnsNames.Contains(c.ColumnName));

				if (isOFXEPaymentEnabled)
				{
					AssertEquals(5, columnStyles.Count());
					AssertEquals(true, columnStyles.All(c => c.IsVisible));
				}
				else
				{
					AssertEquals(0, columnStyles.Count());
				}
			}
		}

		#region Helper methods

		PaymentApprovalWithAuthorisation CreatePaymentApprovalMatchedWithAnInvoiceWithLines(ZDecimal paymentAmount, ZDecimal invoiceAmount, ZString invoiceNumber)
		{
			var paymentApproval = GetNewPaymentApproval();
			paymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.EUR.RX_Code;
			paymentApproval.AV_PayExRate = 0.75m;
			paymentApproval.AV_Amount = paymentAmount;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			paymentApproval.AV_ChequeOrReference = "CSH";
			paymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			paymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), invoiceNumber, TestObjectCreator.EUR, 0.75m, (invoiceAmount * 0.75m), 0m, invoiceAmount, 0m);
			paymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Add(invoice);
			paymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(new BusinessObject[] { invoice });
			return paymentApproval;
		}

		#endregion

		#region Concurrency Error Unit Tests

		public void TestAuthorisePaymentSelectedApprovalsForConcurrencyErrors()
		{
			using (var module1 = GetNewModule())
			using (var module2 = GetNewModule())
			{
				var approvalInModule1 = GetNewPaymentApproval(module1.Factory_ForTestOnly);
				module1.Factory_ForTestOnly.RefreshEnabled = false;
				module1.Factory_ForTestOnly.Save();
				AssertEquals("Approval Status", "AWA", approvalInModule1.AV_Status);

				var approvalInModule2 = module2.Factory_ForTestOnly.Load<PaymentApprovalWithAuthorisation>(approvalInModule1.PK);
				module2.Factory_ForTestOnly.RefreshEnabled = false;
				AssertEquals("Approval Status", "AWA", approvalInModule2.AV_Status);

				approvalInModule1.AV_Status = PaymentApprovalStatus.Posted;
				module1.Factory_ForTestOnly.Save();
				AssertEquals("Approval Status", "PST", approvalInModule1.AV_Status);
				AssertEquals("Approval Status", "AWA", approvalInModule2.AV_Status);

				AssertNoConcurrencyErrorOccurs(module2, approvalInModule2, true);
			}
		}

		public void TestUnauthorisePaymentSelectedApprovalsForConcurrencyErrors()
		{
			using (var module1 = GetNewModule())
			using (var module2 = GetNewModule())
			{
				var approvalInModule1 = GetNewPaymentApproval(module1.Factory_ForTestOnly);
				module1.Factory_ForTestOnly.RefreshEnabled = false;
				module1.Factory_ForTestOnly.Save();
				AssertEquals("Approval Status", "AWA", approvalInModule1.AV_Status);

				var approvalInModule2 = module2.Factory_ForTestOnly.Load<PaymentApprovalWithAuthorisation>(approvalInModule1.PK);
				module2.Factory_ForTestOnly.RefreshEnabled = false;
				AssertEquals("Approval Status", "AWA", approvalInModule2.AV_Status);

				AssertNoConcurrencyErrorOccurs(module1, approvalInModule1, true);
				AssertNoConcurrencyErrorOccurs(module2, approvalInModule2, true);

				AssertEquals("Approval Status", "APP", approvalInModule1.AV_Status);
				AssertEquals("Approval Status", "APP", approvalInModule2.AV_Status);

				approvalInModule1.AV_Status = PaymentApprovalStatus.Posted;
				module1.Factory_ForTestOnly.Save();
				AssertEquals("Approval Status", "PST", approvalInModule1.AV_Status);
				AssertEquals("Approval Status", "APP", approvalInModule2.AV_Status);

				AssertNoConcurrencyErrorOccurs(module2, approvalInModule2, false);
			}
		}

		void AssertNoConcurrencyErrorOccurs(PaymentProcessingModule module, PaymentApprovalWithAuthorisation approval, bool isAuthorizingPayment)
		{
			BusinessObject[] businessObjects = new BusinessObject[] { approval };
			SelectBusinessObjects(module, businessObjects);
			var selectedBizO = module.SelectedBusinessObjects_ForTestOnly[0];
			var selectedApproval = selectedBizO as PaymentApprovalWithAuthorisation;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			if (isAuthorizingPayment)
			{
				AssertNoExceptionThrown("Concurrency errors are not expected.", () => module.AuthorisePaymentApprovals_ForTestOnly(null, new EventArgs()));
			}
			else
			{
				AssertNoExceptionThrown("Concurrency errors are not expected.", () => module.UnAuthorisePaymentApprovals_ForTestOnly(null, new EventArgs()));
			}
		}

		#endregion

		void SetAuthorisationOnApproval(PaymentApprovalWithAuthorisation approval, ZString firstApproval, ZString secondApproval, ZString thirdApproval)
		{
			approval.AV_GS_NKApproval1st = firstApproval;
			approval.AV_GS_NKApproval2nd = secondApproval;
			approval.AV_GS_NKApproval3rd = thirdApproval;
		}

		protected PaymentApprovalWithAuthorisation GetNewPaymentApprovalReadyToPost(ZString invoiceTransactionNum, ZString chequeOrReference, ZDecimal amount)
		{
			PaymentApprovalWithAuthorisation approval = GetNewPaymentApproval();
			approval.AV_PaymentType = ReceiptTypes.Cash;
			approval.AV_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			approval.AV_OH = TestOrgHeader.PK;
			approval.AV_ChequeOrReference = chequeOrReference;
			approval.AV_Amount = amount;
			approval.AV_Status = PaymentApprovalStatus.FullyApproved;

			TestObjectCreator.FillPaymentMatchTransactions(invoiceTransactionNum, typeof(APInvoice), TestOrgHeader, approval, amount);

			AssertEquals("Precondition: Session should balance to zero", true, approval.PaymentMatchingBaseObject.SessionBalancesToZero);

			return approval;
		}

		protected void AssertCorrectAuthorisation(ZString description, PaymentApprovalWithAuthorisation approval, GlbStaff first, GlbStaff second, GlbStaff third)
		{
			ZString firstExpected = first != null ? first.GS_Code : ZString.Empty;
			ZString secondExpected = second != null ? second.GS_Code : ZString.Empty;
			ZString thirdExpected = third != null ? third.GS_Code : ZString.Empty;

			AssertEquals(description + " (First Approval)", firstExpected, approval.AV_GS_NKApproval1st);
			AssertEquals(description + " (Second Approval)", secondExpected, approval.AV_GS_NKApproval2nd);
			AssertEquals(description + " (Third Approval)", thirdExpected, approval.AV_GS_NKApproval3rd);
		}

		#region Implementation

		protected abstract PaymentProcessingModule GetNewModule();

		protected abstract PaymentApprovalWithAuthorisation GetNewPaymentApproval();

		protected abstract PaymentApprovalWithAuthorisation GetNewPaymentApproval(BusinessObjectFactory factory);

		protected abstract SecurityCheckpoint FirstApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint SecondApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint CancelApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint PostCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint NewCashPaymentCheckPoint
		{
			get;
		}

		protected abstract void SelectBusinessObjects(PaymentProcessingModule module, BusinessObject[] businessObjects);

		void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value;

			PaymentAuthorisationSettingsCollection valuesForTest = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 4000, AuthorisationCodes.ThirdApprovalRequiredOnly);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 5000, AuthorisationCodes.FirstAndSecondApprovalRequired);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 6000, AuthorisationCodes.FirstAndThirdApprovalRequired);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 7000, AuthorisationCodes.SecondAndThirdApprovalRequired);
			MakeNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 7000, AuthorisationCodes.AllThreeApprovalRequired);

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		void MakeNewAuthorisationSetting(PaymentAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetUpRegistryForTest();
		}

		protected override void TearDown()
		{
			ResetRegistryForTest();

			base.TearDown();
		}

		protected OrgHeader TestOrgHeader
		{
			get { return TestObjectCreator.TestOrganisation; }
		}
		PaymentAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		AccChequeBook GetAutoPrintChequeBook(ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		PaymentApprovalWithAuthorisation GetPaymentApprovalByChequeNumber(BusinessObject[] collection, ZString chequeNumber)
		{
			foreach (PaymentApprovalWithAuthorisation payment in collection)
			{
				if (payment.AV_ChequeOrReference == chequeNumber)
				{
					return payment;
				}
			}
			return null;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
		}

		#endregion
	}
}
