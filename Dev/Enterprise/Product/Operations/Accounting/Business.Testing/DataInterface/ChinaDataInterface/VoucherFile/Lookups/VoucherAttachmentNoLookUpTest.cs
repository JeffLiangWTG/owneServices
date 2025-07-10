using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherAttachmentNoLookUpTest : TestCaseWithFactory
	{
		public void TestAttachmentNoForAPInvCRDADJ()
		{
			APInvoice invoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			VoucherAttachmentNoLookUp testLookUp = new VoucherAttachmentNoLookUp(invoice);
			AssertEquals("Attachment no for AP Invoice must be 1", 1, testLookUp.GetAttachmentNo());
			APCreditNote creditNote = Factory.NewWithValidTestData(typeof(APCreditNote)) as APCreditNote;
			testLookUp = new VoucherAttachmentNoLookUp(creditNote);
			AssertEquals("Attachment no for AP CreditNote must be 1", 1, testLookUp.GetAttachmentNo());
			APAdjustmentNote adjustmentNote = Factory.NewWithValidTestData(typeof(APAdjustmentNote)) as APAdjustmentNote;
			testLookUp = new VoucherAttachmentNoLookUp(adjustmentNote);
			AssertEquals("Attachment no for AP AdjustmentNote must be 1", 1, testLookUp.GetAttachmentNo());
		}

		public void TestAttachmentNoForRecPay()
		{
			APReceipt aPReceipt = Factory.NewWithValidTestData(typeof(APReceipt)) as APReceipt;
			VoucherAttachmentNoLookUp testLookUp = new VoucherAttachmentNoLookUp(aPReceipt);
			AssertEquals("Attachment no for APReceipt must be 1", 1, testLookUp.GetAttachmentNo());
			ARReceipt aRReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testLookUp = new VoucherAttachmentNoLookUp(aRReceipt);
			AssertEquals("Attachment no for ARReceipt must be 1", 1, testLookUp.GetAttachmentNo());
			APPayment aPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testLookUp = new VoucherAttachmentNoLookUp(aPPayment);
			AssertEquals("Attachment no for APPayment must be 1", 1, testLookUp.GetAttachmentNo());
			ARPayment aRPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			testLookUp = new VoucherAttachmentNoLookUp(aRPayment);
			AssertEquals("Attachment no for ARPayment must be 1", 1, testLookUp.GetAttachmentNo());
		}

		public void TestAttachementForCashBook()
		{
			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			VoucherAttachmentNoLookUp testLookUp = new VoucherAttachmentNoLookUp(directPayment);
			AssertEquals("Attachment no for DirectPayment must be 1", 1, testLookUp.GetAttachmentNo());
			DirectReceipt directReceipt = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			testLookUp = new VoucherAttachmentNoLookUp(directReceipt);
			AssertEquals("Attachment no for DirectReceipt must be 1", 1, testLookUp.GetAttachmentNo());
			BankTransferFromRow transfer = Factory.NewWithValidTestData<BankTransferFromRow>();
			testLookUp = new VoucherAttachmentNoLookUp(transfer);
			AssertEquals("Attachment no for Cashbook Transfer must be 1", 1, testLookUp.GetAttachmentNo());
		}

		public void TestAttachementForOtherType()
		{
			ARExchangeDifference aREXX = Factory.NewWithValidTestData(typeof(ARExchangeDifference)) as ARExchangeDifference;
			VoucherAttachmentNoLookUp testLookUp = new VoucherAttachmentNoLookUp(aREXX);
			AssertEquals("Attachment no for other type must be 0", 0, testLookUp.GetAttachmentNo());
		}

		public void TestAttachementForNullTransactionType()
		{
			VoucherAttachmentNoLookUp testLookUp = new VoucherAttachmentNoLookUp(null);
			AssertEquals("Attachment no for Null Transaction type must be 0", 0, testLookUp.GetAttachmentNo());
		}
	}
}