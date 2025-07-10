using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherProviderFactoryTest : TestCaseWithFactory
	{
		public void TestSubLedgerTransfer_IgnoreEvenNumberedTransaction()
		{
			AccTransactionHeader testARTransfer = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testARTransfer.AH_Ledger = LedgerTypes.AccountsReceivable;
			testARTransfer.AH_TransactionType = TransactionTypes.Transfer;
			testARTransfer.AH_TransactionCount = (ZByte)1;
			testARTransfer.AH_TransactionNum = "TGF04327";
			AssertNotNull("ARAP Transfer with Count 1 should be processed as normal.", VoucherFactory.GetProvider(testARTransfer));
			testARTransfer.AH_TransactionCount = (ZByte)2;
			AssertNull("ARAP Transfer with Count 2 would be ignored.", VoucherFactory.GetProvider(testARTransfer));
		}

		public void TestInvoiceCreditAdjustmentVoucherProvider_ForInvoice()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertVoucherProvider(result, typeof(InvoiceCreditAdjustmentVoucherProvider));
		}

		public void TestInvoiceCreditAdjustmentVoucherProvider_ForAdjustmentNote()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);
			AssertVoucherProvider(result, typeof(InvoiceCreditAdjustmentVoucherProvider));
		}

		public void TestReceiptPaymentVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt);
			AssertVoucherProvider(result, typeof(ReceiptPaymentVoucherProvider));
		}

		public void TestARAPJournalVoucherProivder()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsReceivable, TransactionTypes.Journal);
			AssertVoucherProvider(result, typeof(ARAPJournalVoucherProivder));
		}

		public void TestCFXVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.JobCosting, TransactionTypes.Journal);
			AssertVoucherProvider(result, typeof(CFXVoucherProvider));
		}

		public void TestCBTransferVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.CashBook, TransactionTypes.Transfer);
			AssertVoucherProvider(result, typeof(CBTransferVoucherProvider));
		}

		public void TestDiscountVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsPayable, TransactionTypes.Discount);
			AssertVoucherProvider(result, typeof(DiscountVoucherProvider));
		}

		public void TestOverpaymentVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment);
			AssertVoucherProvider(result, typeof(OverpaymentVoucherProvider));
		}

		public void TestExchangeDiffVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference);
			AssertVoucherProvider(result, typeof(ExchangeDiffVoucherProvider));
		}

		public void TestCBExchangeDiffVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.CashBook, TransactionTypes.ExchangeDifference);
			AssertVoucherProvider(result, typeof(CBExchangeDiffVoucherProvider));
		}

		public void TestDirectReceiptPaymentVoucher_Receipt()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.CashBook, TransactionTypes.DirectReceipt);
			AssertVoucherProvider(result, typeof(DirectReceiptPaymentVoucher));
		}

		public void TestDirectReceiptPaymentVoucher_Payment()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.CashBook, TransactionTypes.DirectPayment);
			AssertVoucherProvider(result, typeof(DirectReceiptPaymentVoucher));
		}

		public void TestGLJournalVoucherProvider()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.General, TransactionTypes.GLStandardJournal);
			AssertVoucherProvider(result, typeof(GLJournalVoucherProvider));
		}

		public void TestGLJournalVoucherProvider_ForJobRevenueJournal()
		{
			VoucherProvider result = GetVoucherUsingVoucherFactory(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal);
			AssertVoucherProvider(result, typeof(CFXVoucherProvider));
		}

		void AssertVoucherProvider(VoucherProvider result, Type expectedType)
		{
			AssertNotNull("VoucherProvider should not be null.", result);
			AssertEquals("VoucherProvider Type.", expectedType, result.GetType());
		}

		VoucherProvider GetVoucherUsingVoucherFactory(string ledger, string transactionType)
		{
			AccTransactionHeader testTransaction = SetTransaction(ledger, transactionType);
			return VoucherFactory.GetProvider(testTransaction);
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType)
		{
			AccTransactionHeader testTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testTransaction.AH_Ledger = ledger;
			testTransaction.AH_TransactionType = transactionType;
			return testTransaction;
		}

		VoucherProviderFactory fVoucherFactory;
		VoucherProviderFactory VoucherFactory
		{
			get
			{
				if (fVoucherFactory == null)
				{
					fVoucherFactory = new VoucherProviderFactory(Factory);
				}

				return fVoucherFactory;
			}
		}
	}
}