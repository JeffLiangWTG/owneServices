using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(ARAPTransferVoucherProvider))]
	public class ARAPTransferVoucherProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public override void TestDebitInFirstRow()
		{
			ARTransfer testTransfer = Transfer.New(typeof(ARTransfer), Factory) as ARTransfer;
			testTransfer.AH_FromAccount = FromAccount.PK;
			testTransfer.AH_ToAccount = ToAccount.PK;
			testTransfer.AH_Desc = "Transfer Description";
			testTransfer.AH_InvoiceAmount = -120m;
			testTransfer.AH_OSTotal = 120m;
			Factory.Save();
			ARAPTransferVoucherProvider testProvider = new ARAPTransferVoucherProvider(testTransfer.TransferFrom);
			AssertEquals(120m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testProvider.VoucherLines[0].CreditAmount);
			testProvider = new ARAPTransferVoucherProvider(testTransfer.TransferTo);
			AssertEquals(120m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testProvider.VoucherLines[0].CreditAmount);
		}

		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestOutstandingAmount()
		{
			TestTransaction.AH_OutstandingAmount = 11m;
			AssertEquals(11m, TestProvider.VoucherLines[0].OutstandingAmount);
		}

		public void TestOrganisationCode()
		{
			TestTransaction.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertEquals("AALSHI", TestProvider.VoucherLines[0].OrganisationCode);
		}

		public void TestTheSecondLine()
		{
			ARTransfer testARTransfer = ARTransfer.New(typeof(ARTransfer), Factory) as ARTransfer;
			testARTransfer.AH_FromAccount = FromAccount.PK;
			testARTransfer.AH_ToAccount = ToAccount.PK;
			testARTransfer.AH_Desc = "Transfer Description";
			testARTransfer.AH_OSTotal = 120m;
			Factory.Save();
			AssertNotNull(testARTransfer.TransferFrom);
			AssertNotNull(testARTransfer.TransferTo);
			AssertEquals((ZByte)1, testARTransfer.TransferFrom.AH_TransactionCount);
			AssertEquals((ZByte)2, testARTransfer.TransferTo.AH_TransactionCount);
			AssertEquals(-120m, testARTransfer.TransferFrom.AH_OSTotal);
			AssertEquals(120m, testARTransfer.TransferTo.AH_OSTotal);
			TestProvider = GetVoucherProvider(testARTransfer.TransferFrom) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(2, TestProvider.VoucherLines.Length);
			AssertEquals(testARTransfer.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
			AssertEquals(testARTransfer.AH_TransactionNum, TestProvider.VoucherLines[1].VoucherNumber);
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
		}

		public void TestControlAccountNumber()
		{
			var mockControlAccount = new Mock<IControlAccountProvider>(MockBehavior.Strict);
			ARAPTransferVoucherProvider testProvider = new ARAPTransferVoucherProvider(TestTransaction);
			mockControlAccount.Setup(m => m.SetTransaction(TestTransaction));
			mockControlAccount.Setup(m => m.PK).Returns(ControlAccount.PK);
			testProvider.ControlAccountProvider = mockControlAccount.Object;
			AssertEquals(ControlAccountNumber, testProvider.VoucherLines[0].AccountNumber);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherTypeForAR()
		{
			//string ExpectedVoucherType = "应收帐款-JNL";
			string expectedVoucherType = "AR-TRF";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherTypeForAP()
		{
			//string ExpectedVoucherType = "应收帐款-JNL";
			string expectedVoucherType = "AP-TRF";
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherAmount()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARAPTransferVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ARAPTransferVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			AccTransactionHeader transaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			transaction.AH_TransactionType = TransactionTypes.Transfer;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			return transaction;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override void AssertValuesForControlLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			AssertEquals("Voucher Line count should be 2", 2, standardProvider.VoucherLines.Length);
			AssertNull("2nd Voucher Line should be null", standardProvider.VoucherLines[1]);
		}
	}
}
