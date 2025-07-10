
using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(TurkeyComplianceInfoEInvoicingExtension))]
	public class TurkeyComplianceInfoEInvoicingExtensionTest : TurkeyComplianceInfoTest
	{
		public void TestGetCantAmendErrorMessage()
		{
			var (invoice, pivot) = CreateObjectsForErrorMessageTests();

			AssertNullOrEmpty("Empty message when E-Invoicing Functionality is not enabled", Extension.GetCantAmendErrorMessage(invoice, TransactionTypes.CreditNote));

			AssertNullOrEmpty("Empty message when Amending transaction is not Credit Note", Extension.GetCantAmendErrorMessage(invoice, TransactionTypes.Invoice));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Error message when E-Invoicing Functionality is enabled", "You can amend the transaction with Credit Note only for AR invoices with Successful (SUC) E-Reporting Status.", Extension.GetCantAmendErrorMessage(invoice, TransactionTypes.CreditNote));

			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			AssertNullOrEmpty("Empty message when submit pivot is succeeded", Extension.GetCantAmendErrorMessage(invoice, TransactionTypes.CreditNote));
		}

		public void TestGetCantReverseErrorMessage()
		{
			var (invoice, pivot) = CreateObjectsForErrorMessageTests();

			AssertNullOrEmpty("Empty message when E-Invoicing Functionality is not enabled", Extension.GetCantReverseErrorMessage(invoice));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Error message when submit pivot is not FAL, SUC, or BER", "You can reverse only AR Invoices where the E-Reporting status is Succeeded (SUC), Failed (FAL), or Batched with Errors (BER).", Extension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Failed;
			AssertNullOrEmpty("Empty message when submit pivot is failed", Extension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			AssertNullOrEmpty("Empty message when submit pivot is succeed", Extension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
			AssertNullOrEmpty("Empty message when submit pivot is succeed", Extension.GetCantReverseErrorMessage(invoice));

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			((INeedRow)creditNote).Row.AcceptChanges();
			AssertNullOrEmpty("Empty message when transaction is not AR invoice", Extension.GetCantReverseErrorMessage(creditNote));
		}

		(ARInvoice invoice, AccEInvoicingTransactionPivot pivot) CreateObjectsForErrorMessageTests()
		{
			var creator = new TestObjectCreator(Factory);
			var arInvoice = creator.CreateARInvoice<ARInvoice>("ARINV0001", creator.USD, 1m, creator.AALSHI);
			var pivot = creator.CreateEInvoicingTransactionPivot(arInvoice, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			((INeedRow)arInvoice).Row.AcceptChanges(); // This is for faster unit tests and for getting IsInDatabase as true

			return (arInvoice, pivot);
		}

		public void TestGetValidationMessageForAfterPostAction()
		{
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = EInvoicingPivotActionType.Adjustment;
			var pivots = new List<AccEInvoicingTransactionPivot>();
			pivots.Add(pivot);
			var message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);

			AssertEquals(Extension.NotEligibleForRequestsMessage, message);

			var submitSucceedPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			submitSucceedPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			submitSucceedPivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivots.Add(submitSucceedPivot);

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertEquals(Extension.NotEligibleForRequestsMessage, message);

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "Govt#2";
			submitSucceedPivot.AIP_AIB = batch.PK;

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertNullOrEmpty(message);

			var submitDeliveredPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			submitDeliveredPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			submitDeliveredPivot.AIP_Status = EInvoicingPivotState.Delivered;
			pivots.Add(submitDeliveredPivot);
			pivots.Remove(submitSucceedPivot);

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertEquals(Extension.NotEligibleForRequestsMessage, message);

			submitDeliveredPivot.AIP_AIB = batch.PK;

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertNullOrEmpty(message);
		}

		public void TestPivotErrorDescriptionInTurkey()
		{
			var testCases = new[]
			{
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Rejected, PivotStatus = EInvoicingPivotState.Succeed, ActionType = EInvoicingPivotActionType.Reject, ErrorPrefix = "Invoice Has Been Rejected" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Approved, PivotStatus = EInvoicingPivotState.Succeed, ActionType = EInvoicingPivotActionType.Approve, ErrorPrefix = "Invoice Has Been Approved, Awaiting Allocation" },

				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Queued, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Awaiting Approval or Rejection Action" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Batched, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Awaiting Approval or Rejection Action" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Sent, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Awaiting Approval or Rejection Action" },

				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Queued, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Batched, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Sent, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },

				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Queued, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Batched, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, ApprovalStatus = GenApprovalRequestApprovalStatus.Error, PivotStatus = EInvoicingPivotState.Sent, ActionType = EInvoicingPivotActionType.ConfirmTransactionReceived, ErrorPrefix = "Successfully Received" },

				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.ApprovalRequested, PivotStatus = EInvoicingPivotState.Queued, ActionType = EInvoicingPivotActionType.Approve, ErrorPrefix = "Approval Request Has Been Sent" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.ApprovalRequested, PivotStatus = EInvoicingPivotState.Batched, ActionType = EInvoicingPivotActionType.Approve, ErrorPrefix = "Approval Request Has Been Sent" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.ApprovalRequested, PivotStatus = EInvoicingPivotState.Sent, ActionType = EInvoicingPivotActionType.Approve, ErrorPrefix = "Approval Request Has Been Sent" },

				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.RejectionRequested, PivotStatus = EInvoicingPivotState.Queued, ActionType = EInvoicingPivotActionType.Reject, ErrorPrefix = "Rejection Request Has Been Sent" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.RejectionRequested, PivotStatus = EInvoicingPivotState.Batched, ActionType = EInvoicingPivotActionType.Reject, ErrorPrefix = "Rejection Request Has Been Sent" },
				new { SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, ApprovalStatus = GenApprovalRequestApprovalStatus.RejectionRequested, PivotStatus = EInvoicingPivotState.Sent, ActionType = EInvoicingPivotActionType.Reject, ErrorPrefix = "Rejection Request Has Been Sent" }
			};

			var tpa = Factory.New<TransactionPendingAllocation>();

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.XP_ParentID = tpa.PK;

			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = tpa.PK;

			var eInvoicingExtensionFactory = new Mock<ICountryComplianceEInvoicingExtensionFactory>();
			eInvoicingExtensionFactory.Setup(f => f.GetIEReportingStatusMessageProvider(It.IsAny<ZString>())).Returns(Extension);
			ObjectFactory.Substitute(eInvoicingExtensionFactory.Object);

			var someDescription = "Some description";
			foreach (var testCase in testCases)
			{
				tpa.AH_ComplianceSubType = testCase.SubType;

				request.XP_ApprovalStatus = testCase.ApprovalStatus;

				pivot.AIP_Status = testCase.PivotStatus;
				pivot.AIP_ActionType = testCase.ActionType;

				pivot.AIP_ErrorDescription = someDescription;
				AssertEquals($"{testCase.ErrorPrefix} | {someDescription}", pivot.AIP_ErrorDescription);

				pivot.AIP_ErrorDescription = ""; // testing the absence of the pipe character
				AssertEquals(testCase.ErrorPrefix, pivot.AIP_ErrorDescription);
			}
		}

		TurkeyComplianceInfoEInvoicingExtension Extension => fExtension ?? (fExtension = new TurkeyComplianceInfoEInvoicingExtension());
		TurkeyComplianceInfoEInvoicingExtension fExtension;
	}
}
