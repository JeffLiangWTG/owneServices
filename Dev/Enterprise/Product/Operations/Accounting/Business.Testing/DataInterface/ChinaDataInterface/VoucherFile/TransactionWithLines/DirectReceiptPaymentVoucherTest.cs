using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(DirectReceiptPaymentVoucher))]
	public class DirectReceiptPaymentVoucherTest : TransactionWithLinesTestCase
	{
		public void TestLineCount()
		{
			DirectReceipt directReceipt = GetDirectReceipt(GLAccount1, LineAmount1);
			DirectReceiptPaymentVoucher testVoucher = new DirectReceiptPaymentVoucher(directReceipt, new ControlAccountProvider());
			AssertEquals(2, testVoucher.VoucherLines.Length);
		}

		public void TestLineCountWithGST()
		{
			DirectReceipt directReceipt = GetDirectReceipt(GLAccount1, LineAmount1);
			directReceipt.AH_GSTAmount = 12.0m;
			DirectReceiptPaymentVoucher testVoucher = new DirectReceiptPaymentVoucher(directReceipt, new ControlAccountProvider());
			AssertEquals(3, testVoucher.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			DirectReceipt directReceipt = GetDirectReceipt(GLAccount1, LineAmount1);
			directReceipt.AH_AB = Bank.PK;
			DirectReceiptPaymentVoucher testVoucher = new DirectReceiptPaymentVoucher(directReceipt, new ControlAccountProvider());
			AssertEquals(2, testVoucher.VoucherLines.Length);
			AssertEquals(LocalAccountNumber1, testVoucher.VoucherLines[1].AccountNumber);
		}

		public void TestAmountForReceipt()
		{
			DirectReceipt directReceipt = GetDirectReceipt(GLAccount1, LineAmount1);
			directReceipt.AH_AB = Bank.PK;
			directReceipt.AH_InvoiceAmount = LineAmount1;
			DirectReceiptPaymentVoucher testVoucher = new DirectReceiptPaymentVoucher(directReceipt, new ControlAccountProvider());
			AssertEquals(2, testVoucher.VoucherLines.Length);
			AssertEquals(LineAmount1, testVoucher.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucher.VoucherLines[0].CreditAmount);
		}

		public void TestAmountForPayment()
		{
			DirectPayment directPayment = GetDirectPayment(GLAccount1, -LineAmount1);
			directPayment.AH_AB = Bank.PK;
			directPayment.AH_InvoiceAmount = -LineAmount1;
			DirectReceiptPaymentVoucher testVoucher = new DirectReceiptPaymentVoucher(directPayment, new ControlAccountProvider());
			AssertEquals(2, testVoucher.VoucherLines.Length);
			AssertEquals(0m, testVoucher.VoucherLines[1].DebitAmount);
			AssertEquals(LineAmount1, testVoucher.VoucherLines[1].CreditAmount);
		}

		protected DirectPayment GetDirectPayment(AccGLHeader gLAccount, decimal lineAmount)
		{
			DirectPayment paymentToReturn = Factory.NewWithValidTestData(typeof(DirectPayment)) as DirectPayment;
			paymentToReturn.Lines.AddNew(typeof(DirectPaymentLine));
			paymentToReturn.Lines[0].AL_AG = gLAccount.PK;
			paymentToReturn.Lines[0].AL_LineAmount = lineAmount;
			paymentToReturn.Lines[0].AL_OSAmount = lineAmount;
			return paymentToReturn;
		}

		protected DirectReceipt GetDirectReceipt(AccGLHeader gLAccount, decimal lineAmount)
		{
			DirectReceipt receiptToReturn = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			receiptToReturn.Lines.AddNew(typeof(DirectReceiptLine));
			receiptToReturn.Lines[0].AL_AG = gLAccount.PK;
			receiptToReturn.Lines[0].AL_LineAmount = lineAmount;
			receiptToReturn.Lines[0].AL_OSAmount = lineAmount;
			return receiptToReturn;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DirectReceiptPaymentVoucher(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			DirectPayment directPayment = GetDirectPayment(GLAccount1, -LineAmount1);
			directPayment.AH_AB = Bank.PK;
			directPayment.AH_InvoiceAmount = -LineAmount1;
			directPayment.AH_OSTotal = -LineAmount1;
			return directPayment;
		}

		protected override void SetLineDescription(AccTransactionHeader header)
		{
			base.SetLineDescription(header);
			DirectPayment headerWithLines = Factory.Load<DirectPayment>(header.PK);
			headerWithLines.AH_Desc = "TRANSACTION_HEADER_DESCRIPTION";
			foreach (AccTransactionLines line in headerWithLines.Lines)
			{
				line.AL_Desc = "TRANSACTION_LINE_DESCRIPTION";
			}
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new DirectReceiptPaymentVoucher(transaction, new ControlAccountProvider());
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 1;
		}

		protected AccBankAccount Bank
		{
			get
			{
				if (fBank == null)
				{
					fBank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					fBank.AB_AG = GLAccount1.PK;
				}

				return fBank;
			}
		}

		protected AccBankAccount fBank;
		protected override bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}
	}
}
