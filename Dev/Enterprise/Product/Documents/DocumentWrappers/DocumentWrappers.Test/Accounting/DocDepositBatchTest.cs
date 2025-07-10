using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocDepositBatch))]
	sealed class DocDepositBatchTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocDepositBatch.New(DepositBatchTransaction, Factory)
			};
		}

		public void TestDepositBatch()
		{
			ARReceipt cashDepositTransaction = CreateReceipt(ReceiptTypes.Cash, 100M, BankAccount.PK);

			ARReceipt chequeDepositTransaction = CreateReceipt(ReceiptTypes.Cheque, 500M, BankAccount.PK);
			chequeDepositTransaction.AH_ChequeOrReference = "000010001";
			chequeDepositTransaction.AH_ChequeDrawer = "Alicia";
			chequeDepositTransaction.AH_DrawerBank = "CBA";
			chequeDepositTransaction.AH_DrawerBranch = "Mascot Branch";

			ARReceipt creditCardDepositTransaction = CreateReceipt(ReceiptTypes.CreditCard, 300M, BankAccount.PK);

			DepositBatch depositBatchTransaction = CreateDepositBatch(BankAccount.PK);
			cashDepositTransaction.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			chequeDepositTransaction.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			creditCardDepositTransaction.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatchTransaction, Factory);

			AssertEquals(depositBatchTransaction.AH_ReceiptBatchNo, docDepositBatchObj.DepositBatchNum);
			AssertEquals("Wrong Bank account", "Test Bank Account", docDepositBatchObj.Account.BankName);
			AssertEquals("Wrong Branch", GlbBranch.CurrentBranch.GB_BranchName, docDepositBatchObj.Branch.BranchName);
			AssertEquals(3, docDepositBatchObj.TransactionHeaders.Count);

			AssertEquals(1, docDepositBatchObj.ChequeTransactions.Count);
			AssertEquals(500M, docDepositBatchObj.ChequeTransactions[0].OSTotal);

			AssertEquals(1, docDepositBatchObj.CashTransactions.Count);
			AssertEquals(100M, docDepositBatchObj.CashTransactions[0].OSTotal);

			AssertEquals(1, docDepositBatchObj.CreditCardTransactions.Count);
			AssertEquals(300M, docDepositBatchObj.CreditCardTransactions[0].OSTotal);

			depositBatchTransaction.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			AssertEquals(ZDateTime.Today.AddDays(-2), docDepositBatchObj.BatchDate);
		}

		public void TestTotalAmounts()
		{
			ARReceipt cashDepositTransaction1 = CreateReceipt(ReceiptTypes.Cash, 100M, BankAccount.PK);
			ARReceipt cashDepositTransaction2 = CreateReceipt(ReceiptTypes.Cash, 500M, BankAccount.PK);

			ARReceipt creditCardDepositTransaction1 = CreateReceipt(ReceiptTypes.CreditCard, 30M, BankAccount.PK);
			ARReceipt creditCardDepositTransaction2 = CreateReceipt(ReceiptTypes.CreditCard, 300M, BankAccount.PK);

			ARReceipt chequeDepositTransaction1 = CreateReceipt(ReceiptTypes.Cheque, 500M, BankAccount.PK);
			chequeDepositTransaction1.AH_ChequeOrReference = "000010001";
			chequeDepositTransaction1.AH_ChequeDrawer = "Alicia";
			chequeDepositTransaction1.AH_DrawerBank = "CBA";
			chequeDepositTransaction1.AH_DrawerBranch = "Mascot Branch";

			ARReceipt chequeDepositTransaction2 = CreateReceipt(ReceiptTypes.Cheque, 5.55M, BankAccount.PK);
			chequeDepositTransaction2.AH_ChequeOrReference = "000010011";
			chequeDepositTransaction2.AH_ChequeDrawer = "Alicia";
			chequeDepositTransaction2.AH_DrawerBank = "CBA";
			chequeDepositTransaction2.AH_DrawerBranch = "Mascot Branch";

			ARReceipt directCreditDepositTransaction1 = CreateReceipt(ReceiptTypes.DirectCredit, 200M, BankAccount.PK);
			ARReceipt directCreditDepositTransaction2 = CreateReceipt(ReceiptTypes.DirectCredit, 450.50M, BankAccount.PK);

			DepositBatch depositBatchTransaction = CreateDepositBatch(BankAccount.PK);

			cashDepositTransaction1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			cashDepositTransaction2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			creditCardDepositTransaction1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			creditCardDepositTransaction2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			chequeDepositTransaction1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			chequeDepositTransaction2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			directCreditDepositTransaction1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			directCreditDepositTransaction2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatchTransaction, Factory);

			AssertEquals(600M, docDepositBatchObj.TotalCashAmount);
			AssertEquals(330M, docDepositBatchObj.TotalCreditCardAmount);
			AssertEquals(505.55M, docDepositBatchObj.TotalChequeAmount);
			AssertEquals(650.5M, docDepositBatchObj.TotalDirectCreditAmount);
		}

		public void TestTotalAmountWithForeignCurency()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt.AH_RX_NKTransactionCurrency = "USD";
			testReceipt.AH_ExchangeRate = 7;
			testReceipt.AH_OSTotal = 100M;
			testReceipt.AH_OutstandingAmount = 700M;
			testReceipt.AH_InvoiceAmount = 700M;

			var testReceipt2 = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.USDBankAccount.PK);
			testReceipt2.AH_OSTotal = 200M;
			testReceipt2.AH_OutstandingAmount = 1400m;
			testReceipt2.AH_InvoiceAmount = 1400m;

			Factory.Save();

			var depositBatchTransaction1 = CreateDepositBatch(TestObjectCreator.AUDBankAccount.PK);
			testReceipt.AH_ReceiptBatchNo = depositBatchTransaction1.AH_ReceiptBatchNo;
			var docDepositBatchObj1 = DocDepositBatch.New(depositBatchTransaction1, Factory);

			AssertEquals(-700M, docDepositBatchObj1.TotalCashAmount);

			var depositBatchTransaction2 = CreateDepositBatch(TestObjectCreator.USDBankAccount.PK);
			testReceipt2.AH_ReceiptBatchNo = depositBatchTransaction2.AH_ReceiptBatchNo;
			var docDepositBatchObj2 = DocDepositBatch.New(depositBatchTransaction2, Factory);

			AssertEquals(-200M, docDepositBatchObj2.TotalCashAmount);
		}

		public void TestDirectCreditAmounts()
		{
			ARReceipt directCreditDepositTransaction1 = CreateReceipt(ReceiptTypes.DirectCredit, 200M, BankAccount.PK);
			DirectReceipt cashBookDirectReceipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Now, 60m, 0m, 60m, 0m);
			cashBookDirectReceipt.AH_AB = BankAccount.PK;
			cashBookDirectReceipt.AH_ExchangeRate = 1m;

			Factory.Save();

			DepositBatch depositBatchTransaction = CreateDepositBatch(BankAccount.PK);

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatchTransaction, Factory);
			AssertEquals(0M, docDepositBatchObj.TotalDirectCreditAmount);
		}

		public void TestTransactionHeaders()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			ARReceipt thisCompanyReceipt1 = CreateReceipt(ReceiptTypes.Cash, 100M, BankAccount.PK);
			ARReceipt otherCompanyReceipt1 = CreateReceipt(ReceiptTypes.Cash, 200M, BankAccount.PK);
			ARReceipt otherCompanyReceipt2 = CreateReceipt(ReceiptTypes.Cash, 300M, BankAccount.PK);

			otherCompanyReceipt1.AH_GB = branch.PK;
			otherCompanyReceipt2.AH_GB = branch.PK;
			DepositBatch depositBatch1 = CreateDepositBatch(BankAccount.PK);
			DepositBatch depositBatch2 = CreateDepositBatch(BankAccount.PK);
			depositBatch2.AH_GB = branch.PK;

			thisCompanyReceipt1.AH_ReceiptBatchNo = depositBatch1.AH_ReceiptBatchNo;
			otherCompanyReceipt1.AH_ReceiptBatchNo = depositBatch2.AH_ReceiptBatchNo;
			otherCompanyReceipt2.AH_ReceiptBatchNo = depositBatch2.AH_ReceiptBatchNo;

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatch1, Factory);

			AssertEquals("This company's transactions", 1, docDepositBatchObj.TransactionHeaders.Count);
			AssertEquals(100M, docDepositBatchObj.TransactionHeaders[0].OSTotal);
		}

		public void TestTotalChequeCount()
		{
			ARReceipt cash = CreateReceipt(ReceiptTypes.Cash, 100M, BankAccount.PK);

			ARReceipt cheque1 = CreateReceipt(ReceiptTypes.Cheque, 500M, BankAccount.PK);
			ARReceipt cheque2 = CreateReceipt(ReceiptTypes.Cheque, 200M, BankAccount.PK);

			ARReceipt creditCard = CreateReceipt(ReceiptTypes.CreditCard, 300M, BankAccount.PK);

			DepositBatch depositBatchTransaction = CreateDepositBatch(BankAccount.PK);
			cash.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			cheque1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			cheque2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			creditCard.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatchTransaction, Factory);
			AssertEquals("Total cheque count", 2, docDepositBatchObj.TotalChequeCount);
			AssertEquals("Total transactions", 4, docDepositBatchObj.TransactionHeaders.Count);
		}

		public void TestNot_CCD_CSH_CHQ_DCR_TransactionAmounts()
		{
			ARReceipt cash = CreateReceipt(ReceiptTypes.Cash, 100M, BankAccount.PK);
			ARReceipt cheque1 = CreateReceipt(ReceiptTypes.Cheque, 500M, BankAccount.PK);
			ARReceipt creditCard = CreateReceipt(ReceiptTypes.CreditCard, 300M, BankAccount.PK);

			ARReceipt directCreditDepositTransaction1 = CreateReceipt(ReceiptTypes.DirectCredit, 200M, BankAccount.PK);

			ARReceipt receipt1 = CreateReceipt(ReceiptTypes.AccountMaintenanceFee, 1M, BankAccount.PK);
			ARReceipt receipt2 = CreateReceipt(ReceiptTypes.BankDebitTax, 2M, BankAccount.PK);
			ARReceipt receipt3 = CreateReceipt(ReceiptTypes.BankDepositFee, 3M, BankAccount.PK);
			ARReceipt receipt4 = CreateReceipt(ReceiptTypes.InterestPaid, 4M, BankAccount.PK);
			ARReceipt receipt5 = CreateReceipt(ReceiptTypes.InterestReceived, 5M, BankAccount.PK);
			ARReceipt receipt6 = CreateReceipt(ReceiptTypes.MiscellaneousFees, 6M, BankAccount.PK);
			ARReceipt receipt7 = CreateReceipt(ReceiptTypes.MiscellaneousReceipt, 7M, BankAccount.PK);
			ARReceipt receipt8 = CreateReceipt(ReceiptTypes.PeriodicPayment, 8M, BankAccount.PK);
			ARReceipt receipt9 = CreateReceipt(ReceiptTypes.StampDuty, 9M, BankAccount.PK);

			DepositBatch depositBatchTransaction = CreateDepositBatch(BankAccount.PK);
			cash.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			cheque1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			creditCard.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			directCreditDepositTransaction1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			receipt1.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt2.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt3.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt4.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt5.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt6.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt7.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt8.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;
			receipt9.AH_ReceiptBatchNo = depositBatchTransaction.AH_ReceiptBatchNo;

			DocDepositBatch docDepositBatchObj = DocDepositBatch.New(depositBatchTransaction, Factory);

			AssertEquals(10, docDepositBatchObj.DirectCreditTransactions.Count);
			AssertEquals(245M, docDepositBatchObj.TotalDirectCreditAmount);
		}

		#region Implementation
		DepositBatch DepositBatchTransaction;
		AccBankAccount BankAccount;
		ARReceipt CreateReceipt(ZString receiptType, ZDecimal amount, ZGuid bankAccountPK)
		{
			var depositTransaction = Factory.New<ARReceipt>();
			depositTransaction.AH_ReceiptType = receiptType;
			depositTransaction.AH_TransactionType = TransactionTypes.Receipt;
			depositTransaction.AH_GB = GlbBranch.CurrentBranch.PK;
			depositTransaction.AH_LocalExTaxAmount = amount;
			depositTransaction.AH_OSTotalAmount = amount;
			depositTransaction.AH_AB = bankAccountPK;
			depositTransaction.AH_ExchangeRate = 1m;
			return depositTransaction;
		}

		DepositBatch CreateDepositBatch(ZGuid bankAccountPK)
		{
			DepositBatch depositBatchTransaction = Factory.New<DepositBatch>();
			depositBatchTransaction.AH_AB = bankAccountPK;
			depositBatchTransaction.AH_ExchangeRate = 1m;
			depositBatchTransaction.LoadTransactions(ZGuid.Empty);
			Factory.Save();
			return depositBatchTransaction;
		}

		AccBankAccount CreateDefaultBank()
		{
			AccBankAccount[] defaultBankAccounts = (AccBankAccount[])Factory.Load(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True));

			foreach (AccBankAccount account in defaultBankAccounts)
			{
				account.AB_IsDefaultReceiptBankAccount = ZBool.False;
			}

			var headerBisObj = Factory.NewWithValidTestData<AccGLHeader>();

			var bankAccountBisObj = Factory.New<AccBankAccount>();
			bankAccountBisObj.AB_BankName = "Test Bank Account";
			bankAccountBisObj.AB_BankAddress = "123 Address Test";
			bankAccountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccountBisObj.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankAccountBisObj.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			bankAccountBisObj.AB_AG = headerBisObj.PK;
			bankAccountBisObj.AB_Code = "ABCBANK";
			Factory.Save();
			return bankAccountBisObj;
		}

		protected override void SetUp()
		{
			BankAccount = CreateDefaultBank();
			DepositBatchTransaction = Factory.New<DepositBatch>();
			base.SetUp();
		}

		#endregion
	}
}
