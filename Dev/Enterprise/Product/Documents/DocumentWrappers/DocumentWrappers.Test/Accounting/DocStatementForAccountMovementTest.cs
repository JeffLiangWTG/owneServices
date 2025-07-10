using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocStatementForAccountMovementTest : TestCaseWithFactory
	{
		[DisableZeroExchangeRateOverriding]
		public void TestTransactions()
		{
			SetupTransactions();

			var statement = SetUpPrintStatement(Creator.AALSHI.PK, Creator.AUD.RX_Code, true);
			statement.PostDateFrom = new ZDateTime(2015, 02, 17, 0, 0, 0);
			statement.PostDateTo = new ZDateTime(2015, 02, 18, 0, 0, 0);

			var statementWrapper = DocStatementForAccountMovement.New(statement, Factory) as DocStatementForAccountMovement;
			AssertEquals("Transaction Count", 2, statementWrapper.Transactions.Count);
			AssertEquals("Transaction Count", 3, statementWrapper.TransactionsForDueBuckets.Count);

			statement = SetUpPrintStatement(Creator.AALSHI.PK, Creator.AUD.RX_Code, true);
			statement.PostDateFrom = new ZDateTime(2015, 02, 15, 0, 0, 0);
			statement.PostDateTo = new ZDateTime(2015, 02, 18, 0, 0, 0);

			statementWrapper = DocStatementForAccountMovement.New(statement, Factory) as DocStatementForAccountMovement;
			AssertEquals("Transaction Count", 3, statementWrapper.Transactions.Count);
			AssertEquals("Transaction Count", 3, statementWrapper.TransactionsForDueBuckets.Count);

			statement = SetUpPrintStatement(Creator.AALSHI.PK, Creator.AUD.RX_Code, true);
			statement.PostDateFrom = new ZDateTime(2015, 02, 19, 0, 0, 0);
			statement.PostDateTo = new ZDateTime(2015, 02, 19, 0, 0, 0);

			statementWrapper = DocStatementForAccountMovement.New(statement, Factory) as DocStatementForAccountMovement;
			AssertEquals("Transaction Count", 1, statementWrapper.Transactions.Count);
			AssertEquals("Transaction Count", 4, statementWrapper.TransactionsForDueBuckets.Count);
		}

		public void TestCurrencyForAttachedInvoicesWhenAccountMovement()
		{
			var transactionAUD1 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.AUD, "INV0001011", 150M, 0.75M, new ZDateTime(2016, 12, 14, 0, 0, 0));
			var transactionAUD2 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.AUD, "INV0001012", 140M, 0.75M, new ZDateTime(2016, 12, 14, 0, 0, 0));
			var transactionUSD = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV00001013", 110M, 0.75M, new ZDateTime(2016, 12, 16, 0, 0, 0));
			Factory.Save();

			var statement1 = SetUpPrintStatement(Creator.ABIGAS.PK, Creator.AUD.RX_Code, true);
			statement1.PostDateFrom = new ZDateTime(2016, 12, 14, 0, 0, 0);
			statement1.PostDateTo = new ZDateTime(2016, 12, 16, 0, 0, 0);
			var statementWrapper1 = DocStatementForAccountMovement.New(statement1, Factory) as DocStatementForAccountMovement;
			AssertEquals("Transaction Count", 3, statementWrapper1.Transactions.Count);
			AssertEquals("The currency of attached invoice should not changed", transactionAUD1.AH_RX_NKTransactionCurrency, statementWrapper1.Transactions[0].Currency.Code);
			AssertEquals("The currency of attached invoice should not changed", transactionAUD2.AH_RX_NKTransactionCurrency, statementWrapper1.Transactions[1].Currency.Code);
			AssertEquals("The currency of attached invoice should not changed", transactionUSD.AH_RX_NKTransactionCurrency, statementWrapper1.Transactions[2].Currency.Code);

			var statement2 = SetUpPrintStatement(Creator.ABIGAS.PK, Creator.EUR.RX_Code, true);
			statement2.PostDateFrom = new ZDateTime(2016, 12, 14, 0, 0, 0);
			statement2.PostDateTo = new ZDateTime(2016, 12, 16, 0, 0, 0);
			var statementWrapper2 = DocStatementForAccountMovement.New(statement2, Factory) as DocStatementForAccountMovement;
			AssertEquals("Transaction Count", 3, statementWrapper2.Transactions.Count);
			AssertEquals("The currency of attached invoice should not changed", transactionAUD1.AH_RX_NKTransactionCurrency, statementWrapper2.Transactions[0].Currency.Code);
			AssertEquals("The currency of attached invoice should not changed", transactionAUD2.AH_RX_NKTransactionCurrency, statementWrapper2.Transactions[1].Currency.Code);
			AssertEquals("The currency of attached invoice should not changed", transactionUSD.AH_RX_NKTransactionCurrency, statementWrapper2.Transactions[2].Currency.Code);
		}

		PrintStatementForAccountMovement SetUpPrintStatement(ZGuid organisationPK, ZString currencyNK, bool isMultipleCurrency)
		{
			var newStatement = new PrintStatementForAccountMovement(Factory, GlbBranch.CurrentBranch, isMultipleCurrency);
			if (!organisationPK.IsEmpty)
			{
				newStatement.OrganisationPK = organisationPK;
			}

			if (!currencyNK.IsEmpty)
			{
				newStatement.CurrencyNK = currencyNK;
			}

			newStatement.IsMultipleCurrency = isMultipleCurrency;
			return newStatement;
		}

		void SetupTransactions()
		{
			ZGuid anotherGLBCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_OH, Creator.AALSHI.PK);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, anotherGLBCompany);
			OrgCompanyData oldCompanyData = Factory.LoadTop1<OrgCompanyData>(filter);
			oldCompanyData.Delete();
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = anotherGLBCompany;
			Creator.AALSHI.CompanyData.OB_GC = anotherGLBCompany;
			Factory.Save();

			CreateCRDTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "CRD0001000", 200.0M, 0.75M, new ZDateTime(2015, 02, 12, 0, 0, 0));

			transaction2 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 0.75m, Creator.ABIGAS, new ZDateTime(2015, 02, 13, 0, 0, 0));
			transaction2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction2.AH_TransactionType = TransactionTypes.InvoiceBatch;

			CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV0001001", 150M, 0.75M, new ZDateTime(2015, 02, 14, 15, 25, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV00001002", 110M, 0.75M, new ZDateTime(2015, 02, 15, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001003", 110M, 0.75M, new ZDateTime(2015, 02, 16, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001004", 120M, 0.75M, new ZDateTime(2015, 02, 17, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001005", 120M, 0.75M, new ZDateTime(2015, 02, 18, 0, 0, 0));
			Creator.CreateARReceipt(0.75M, 250M, new ZDateTime(2015, 02, 19, 0, 0, 0), new ZDateTime(2015, 02, 18, 0, 0, 0), Creator.AALSHI.PK, Creator.AUDBankAccount.PK);

			Factory.Save();
		}

		InvoicingBase CreateINVTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString invoiceNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNumber, currency, exchangeRate, invoiceAmount * exchangeRate, 0M, invoiceAmount, 0M, debtor, Creator.FRT.PK, postDate, postDate.AddDays(-2), postDate.AddDays(-5), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_PostDate = postDate;
			transaction.AH_TransactionNum = invoiceNumber;

			return transaction;
		}

		ARCreditNote CreateCRDTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString cRDNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateARCreditNoteWithLine(cRDNumber, debtor, Creator.USD, exchangeRate, "Credit Note Line 1", newJob, Creator.FRT, invoiceAmount, new ZDateTime(2015, 02, 12, 0, 0, 0), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = cRDNumber;
			transaction.AH_PostDate = postDate;

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;

			return transaction;
		}

		TransactionHeader transaction2;

		TestObjectCreator Creator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;
	}
}
