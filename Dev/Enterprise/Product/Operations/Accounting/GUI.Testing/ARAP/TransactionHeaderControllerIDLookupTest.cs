using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing
{
	public class TransactionHeaderControllerIDLookupTest : TestCaseWithFactory
	{
		public void TestGetControllerIDForAccountsReceivableTransactions()
		{
			ARAdjustmentNote adjustmentNote = Factory.New<ARAdjustmentNote>();
			ARContraRow contraRow = Factory.New<ARContraRow>();
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			ARDiscount discount = Factory.New<ARDiscount>();
			ARExchangeDifference exchangeDifference = Factory.New<ARExchangeDifference>();
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARJournal journal = Factory.New<ARJournal>();
			AROverpayment overpayment = Factory.New<AROverpayment>();
			ARPayment payment = Factory.New<ARPayment>();
			ARReceipt receipt = Factory.New<ARReceipt>();
			ARTransferToRow transfer = Factory.New<ARTransferToRow>();

			AssertCorrectControllerIDIsReturned(ControllerIDs.ARAdjustmentNote, adjustmentNote);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARContra, contraRow);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARCreditNote, creditNote);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARDiscount, discount);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARExchangeDifference, exchangeDifference);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARInvoice, invoice);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ARJournal, journal);
			AssertCorrectControllerIDIsReturned(ControllerIDs.AROverpayment, overpayment);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ZARPayment, payment);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ZARReceipt, receipt);
		}

		public void TestGetControllerIDForAccountsPayableTransactions()
		{
			APAdjustmentNote adjustmentNote = Factory.New<APAdjustmentNote>();
			APContraRow contraRow = Factory.New<APContraRow>();
			APCreditNote creditNote = Factory.New<APCreditNote>();
			APDiscount discount = Factory.New<APDiscount>();
			APExchangeDifference exchangeDifference = Factory.New<APExchangeDifference>();
			APInvoice invoice = Factory.New<APInvoice>();
			APJournal journal = Factory.New<APJournal>();
			APOverpayment overpayment = Factory.New<APOverpayment>();
			APPayment payment = Factory.New<APPayment>();
			APReceipt receipt = Factory.New<APReceipt>();
			APTransferToRow transfer = Factory.New<APTransferToRow>();

			AssertCorrectControllerIDIsReturned(ControllerIDs.APAdjustmentNote, adjustmentNote);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APContra, contraRow);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APCreditNote, creditNote);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APDiscount, discount);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APExchangeDifference, exchangeDifference);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APInvoice, invoice);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APJournal, journal);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APOverpayment, overpayment);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ZAPPayment, payment);
			AssertCorrectControllerIDIsReturned(ControllerIDs.ZAPReceipt, receipt);
			AssertCorrectControllerIDIsReturned(ControllerIDs.APTransfer, transfer);
		}

		public void TestGetControllerIDForUnknownTransactions()
		{
			TransactionHeaderControllerIDLookup lookup = new TransactionHeaderControllerIDLookup();
			DirectPayment directPayment = Factory.New<DirectPayment>();
			AssertEquals(null, lookup.GetControllerID(directPayment));
			AssertEquals(null, lookup.GetControllerID("XX", "ZZZ"));
		}

		void AssertCorrectControllerIDIsReturned(ControllerID expectedID, TransactionHeader header)
		{
			AssertEquals(expectedID, ControllerIDLookup.GetControllerID(header));
			AssertEquals(expectedID, ControllerIDLookup.GetControllerID(header.AH_Ledger, header.AH_TransactionType));
		}

		TransactionHeaderControllerIDLookup ControllerIDLookup
		{
			get
			{
				if (fControllerIDLookup == null)
				{
					fControllerIDLookup = new TransactionHeaderControllerIDLookup();
				}

				return fControllerIDLookup;
			}
		}
		TransactionHeaderControllerIDLookup fControllerIDLookup;
	}
}
