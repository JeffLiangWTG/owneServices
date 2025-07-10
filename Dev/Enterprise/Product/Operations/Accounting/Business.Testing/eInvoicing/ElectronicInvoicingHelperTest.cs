using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.eInvoicing.Testing
{
	class ElectronicInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestTryConvertUTCDateTimeToBranchLocalDateTime()
		{
			var gbUnlocoCode = "GBABE";
			var sgUnlocoCode = "SGSIN";
			var gbBranch = Factory.NewWithValidTestData<GlbBranch>();
			gbBranch.GB_RL_NKHomePort = gbUnlocoCode;
			var sgBranch = Factory.NewWithValidTestData<GlbBranch>();
			sgBranch.GB_RL_NKHomePort = sgUnlocoCode;

			var dateWithRefUNLOCOUTCOffset = new ZDateTime(2019, 5, 15);
			var dateWithoutRefUNLOCOUTCOffset = new ZDateTime(2021, 5, 15);

			var sql = $@"
DELETE RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCOde IN ('{gbUnlocoCode}', '{sgUnlocoCode}');
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK,RLO_RL_NKCode,RLO_StartTimeUTC,RLO_EndTimeUTC,RLO_OffsetMinutesFromUTC)
VALUES (NEWID(), '{gbUnlocoCode}', '2019-01-01 00:00:00', '2020-01-01 00:00:00', 0),
(NEWID(), '{sgUnlocoCode}', '2019-01-01 00:00:00', '2020-01-01 00:00:00', 480)";
			TestConnection.ExecuteNonQuery(sql);

			AssertEquals((short)480, Db.Connection.ExecuteScalar<short>($"SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('{sgUnlocoCode}', '{dateWithRefUNLOCOUTCOffset}')"));
			AssertEquals((short)0, Db.Connection.ExecuteScalar<short>($"SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('{gbUnlocoCode}', '{dateWithRefUNLOCOUTCOffset}')"));
			AssertNull(Db.Connection.ExecuteScalar($"SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('{sgUnlocoCode}', '{dateWithoutRefUNLOCOUTCOffset}')"));
			AssertNull(Db.Connection.ExecuteScalar($"SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('{gbUnlocoCode}', '{dateWithoutRefUNLOCOUTCOffset}')"));

			AssertConversion(gbBranch, dateWithRefUNLOCOUTCOffset, true, dateWithRefUNLOCOUTCOffset);
			AssertConversion(gbBranch, dateWithoutRefUNLOCOUTCOffset, false, ZDateTime.Empty);
			AssertConversion(sgBranch, dateWithRefUNLOCOUTCOffset, true, dateWithRefUNLOCOUTCOffset.AddMinutes(480));
			AssertConversion(sgBranch, dateWithoutRefUNLOCOUTCOffset, false, ZDateTime.Empty);

			void AssertConversion(GlbBranch branch, ZDateTime utcDateTime, bool expectedConversionResult, ZDateTime expectedLocalBranchDateTime)
			{
				ZDateTime actualLocalBranchDateTime;
				var result = ElectronicInvoicingHelper.TryConvertUTCDateTimeToBranchLocalDateTime(branch, utcDateTime, out actualLocalBranchDateTime);
				AssertEquals(expectedConversionResult, result);
				AssertEquals(expectedLocalBranchDateTime, actualLocalBranchDateTime);
			}
		}

		public void TestHasPendingApprovalRequest_TransactionPendingAllocation()
		{
			var paTransaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();

			AssertNull("Unsaved transaction does not have any request", paTransaction.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending approval request if it is not saved",
				!paTransaction.HasPendingApprovalRequest());

			Factory.Save();

			AssertNotNull("Saved transaction has approval request", paTransaction.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending approval request if it is recently saved",
				!paTransaction.HasPendingApprovalRequest());

			var approvalRequest = paTransaction.TransactionRelatedApprovalRequest;
			AssertEquals("Default Approval Status of the recently created approval request",
				GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);
			Assert("Transaction does not have pending approval request",
				!paTransaction.HasPendingApprovalRequest());

			approvalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Error;
			Assert("Approval status has changes", approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction does not have pending approval request if approval status is not expected one",
				!paTransaction.HasPendingApprovalRequest());

			((INeedRow)approvalRequest).Row.AcceptChanges();
			Assert("Transaction does not have pending approval request if approval status does not have changes",
				!paTransaction.HasPendingApprovalRequest());

			approvalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.ApprovalRequested;
			Assert("Approval status has changes", approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction has pending approval request if approval status is the expected status and if approval status has changes",
				paTransaction.HasPendingApprovalRequest());

			((INeedRow)approvalRequest).Row.AcceptChanges();
			Assert("Approval status does not have changes", !approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction does not have pending approval request if approval status is expected status and if it does not have changes",
				!paTransaction.HasPendingApprovalRequest());
		}

		public void TestHasPendingApprovalRequest_TransactionIsNotTransactionPendingAllocation()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			AssertNull("Unsaved transaction does not have any request", apInvoice.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending approval request if it is not saved",
				!apInvoice.HasPendingRejectionRequest());

			Factory.Save();

			AssertNull("Saved transaction has approval request", apInvoice.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending approval request if it is a different type than TransactionPendingAllocation",
				!apInvoice.HasPendingRejectionRequest());
		}

		public void TestHasPendingRejectionRequest_TransactionPendingAllocation()
		{
			var paTransaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();

			AssertNull("Unsaved transaction does not have any request", paTransaction.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending rejection request if it is not saved",
				!paTransaction.HasPendingRejectionRequest());

			Factory.Save();

			AssertNotNull("Saved transaction has request", paTransaction.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending rejection request if it is recently saved",
				!paTransaction.HasPendingRejectionRequest());

			var approvalRequest = paTransaction.TransactionRelatedApprovalRequest;
			AssertEquals("Default Approval Status of the recently created request",
				GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);
			Assert("Transaction does not have pending rejection request",
				!paTransaction.HasPendingRejectionRequest());

			approvalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Error;
			Assert("Approval status has changes", approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction does not have pending rejection request if approval status is not expected one",
				!paTransaction.HasPendingRejectionRequest());

			((INeedRow)approvalRequest).Row.AcceptChanges();
			Assert("Transaction does not have pending rejection request if approval status does not have changes",
				!paTransaction.HasPendingRejectionRequest());

			approvalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.RejectionRequested;
			Assert("Approval status has changes", approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction has pending rejection request if approval status is the expected status and if approval status has changes",
				paTransaction.HasPendingRejectionRequest());

			((INeedRow)approvalRequest).Row.AcceptChanges();
			Assert("Approval status does not have changes", !approvalRequest.XP_ApprovalStatusInfo.HasChanges);
			Assert("Transaction does not have pending rejection request if approval status is expected status and if it does not have changes",
				!paTransaction.HasPendingRejectionRequest());
		}

		public void TestHasPendingRejectionRequest_TransactionIsNotTransactionPendingAllocation()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			AssertNull("Unsaved transaction does not have any request", apInvoice.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending rejection request if it is not saved",
				!apInvoice.HasPendingRejectionRequest());

			Factory.Save();

			AssertNull("Saved transaction has request", apInvoice.TransactionRelatedApprovalRequest);
			Assert("Transaction does not have pending rejection request if it is a different type than TransactionPendingAllocation",
				!apInvoice.HasPendingRejectionRequest());
		}

		public void TestIsCancellationRequest_TransactionIsInDatabase()
		{
			var transaction = Factory.NewWithValidTestData<ARInvoice>();
			((INeedRow)transaction).Row.AcceptChanges();
			Assert("Transaction is not cancelled", !transaction.IsCancellationRequest());

			transaction.AH_IsCancelled = true;
			Assert("Transaction does not belong to group", !transaction.IsCancellationRequest());

			transaction.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			Assert("Transaction is not AR Credit Note or AP Invoice", !transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			Assert("Transaction is not AR Credit Note or AP Invoice", !transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			Assert("Transaction is AR Credit Note", transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			Assert("Transaction is AP Invoice", transaction.IsCancellationRequest());
		}

		public void TestIsCancellationRequest_TransactionIsNotInDatabase()
		{
			var transaction = Factory.NewWithValidTestData<ARInvoice>();
			Assert("Transaction is not reverse transaction", !transaction.IsCancellationRequest());

			transaction.IsReverseTransaction = true;
			Assert("Transaction is not in expected ledger and transaction type", !transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			Assert("Transaction is not in expected ledger and transaction type", !transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			Assert("Transaction is AR Credit Note", transaction.IsCancellationRequest());

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			Assert("Transaction is AP Invoice", transaction.IsCancellationRequest());
		}
	}
}
