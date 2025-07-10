using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Common.Helper
{
	public class EInvoicingControllerIdDeciderTest : TestCaseWithFactory
	{
		public void TestGetControllerIDRelatedToTransaction()
		{
			var arInvoiceResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var arCreditNoteResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			var arAdjustmentNoteResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);
			var apInvoiceResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			var apCreditNoteResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
			var apAdjustmentNoteResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote);
			var invalidLedgerResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction("InvalidLedger", TransactionTypes.Invoice);
			var invalidTransactionTypeResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsReceivable, "InvalidTransactionType");

			AssertEquals(ControllerIDs.ARInvoice, arInvoiceResult);
			AssertEquals(ControllerIDs.ARCreditNote, arCreditNoteResult);
			AssertEquals(ControllerIDs.ARAdjustmentNote, arAdjustmentNoteResult);
			AssertEquals(ControllerIDs.APInvoice, apInvoiceResult);
			AssertEquals(ControllerIDs.APCreditNote, apCreditNoteResult);
			AssertEquals(ControllerIDs.APAdjustmentNote, apAdjustmentNoteResult);
			AssertNull(invalidLedgerResult);
			AssertNull(invalidTransactionTypeResult);
		}

		public void TestGetControllerIDRelatedToComplianceDocument()
		{
			var arComplianceDocumentResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToComplianceDocument(LedgerTypes.AccountsReceivable);
			var apComplianceDocumentResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToComplianceDocument(LedgerTypes.AccountsPayable);
			var invalidLedgerResult = EInvoicingControllerIdDecider.GetControllerIDRelatedToComplianceDocument("InvalidLedger");

			AssertEquals(ControllerIDs.ARComplianceDocument, arComplianceDocumentResult);
			AssertEquals(ControllerIDs.APComplianceDocument, apComplianceDocumentResult);
			AssertNull(invalidLedgerResult);
		}
	}
}
