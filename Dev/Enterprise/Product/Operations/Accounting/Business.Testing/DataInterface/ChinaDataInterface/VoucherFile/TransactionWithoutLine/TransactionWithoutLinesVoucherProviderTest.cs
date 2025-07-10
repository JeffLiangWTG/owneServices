using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(ReceiptPaymentVoucherProvider))]
	public abstract class TransactionWithoutLinesVoucherProviderTest : VoucherProviderTestCase
	{
		public virtual void TestDebitInFirstRow()
		{
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReceiptPaymentVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ReceiptPaymentVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return InsertARReceipt(Factory);
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 1;
		}
	}
}
