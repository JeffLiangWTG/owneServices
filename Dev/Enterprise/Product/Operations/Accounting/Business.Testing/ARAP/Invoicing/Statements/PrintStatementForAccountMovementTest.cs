using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PrintStatementForAccountMovement))]
	public class PrintStatementForAccountMovementTest : PrintStatementTestBase
	{
		public override void TestTransactionHeaderCollectionCommonFilters()
		{
			SetupData();

			var statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.CurrencyNK = Creator.AUD.RX_Code;
			statement.IsMultipleCurrency = true;

			AssertEquals("Collection should contain 3 transactions", 3, statement.Transactions.Count);
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction3 should be in collection", statement.Transactions.Contains(transaction3));
			Assert("Transaction4 should be in collection", statement.Transactions.Contains(transaction4));

			AssertEquals("Collection should contain 3 transactions", 3, statement.TransactionForDueBuckets.Count);
			Assert("Transaction1 should be in collection", statement.TransactionForDueBuckets.Contains(transaction1));
			Assert("Transaction3 should be in collection", statement.TransactionForDueBuckets.Contains(transaction3));
			Assert("Transaction4 should be in collection", statement.TransactionForDueBuckets.Contains(transaction4));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.IsMultipleCurrency = false;
			statement.PostDateFrom = new ZDateTime(2015, 02, 12, 0, 0, 0);
			statement.PostDateTo = new ZDateTime(2015, 02, 14, 0, 0, 0);

			AssertEquals("Collection should contain 2 transactions", 2, statement.Transactions.Count);
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction3 should be in collection", statement.Transactions.Contains(transaction3));

			AssertEquals("Collection should contain 2 transactions", 2, statement.TransactionForDueBuckets.Count);
			Assert("Transaction1 should be in collection", statement.TransactionForDueBuckets.Contains(transaction1));
			Assert("Transaction3 should be in collection", statement.TransactionForDueBuckets.Contains(transaction3));

			statement = GetNewPrintStatement(Creator.AALSHI.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.IsMultipleCurrency = false;

			AssertEquals("Collection should contain 3 transactions", 3, statement.Transactions.Count);
			Assert("Transaction5 should be in collection", statement.Transactions.Contains(transaction5));
			Assert("Transaction6 should be in collection", statement.Transactions.Contains(transaction6));
			Assert("Transaction7 should be in collection", statement.Transactions.Contains(transaction7));

			AssertEquals("Collection should contain 3 transactions", 3, statement.TransactionForDueBuckets.Count);
			Assert("Transaction5 should be in collection", statement.TransactionForDueBuckets.Contains(transaction5));
			Assert("Transaction6 should be in collection", statement.TransactionForDueBuckets.Contains(transaction6));
			Assert("Transaction7 should be in collection", statement.TransactionForDueBuckets.Contains(transaction7));

			statement = GetNewPrintStatement(Creator.AALSHI.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.IsMultipleCurrency = false;
			statement.PostDateFrom = new ZDateTime(2015, 02, 16, 0, 0, 0);
			statement.PostDateTo = new ZDateTime(2015, 02, 16, 0, 0, 0);

			AssertEquals("Collection should contain 2 transactions", 2, statement.Transactions.Count);
			Assert("Transaction5 should be in collection", statement.Transactions.Contains(transaction6));
			Assert("Transaction5 should be in collection", statement.Transactions.Contains(transaction7));

			AssertEquals("Collection should contain 3 transaction", 3, statement.TransactionForDueBuckets.Count);
			Assert("Transaction5 should be in collection", statement.TransactionForDueBuckets.Contains(transaction5));
			Assert("Transaction6 should be in collection", statement.TransactionForDueBuckets.Contains(transaction6));
			Assert("Transaction7 should be in collection", statement.TransactionForDueBuckets.Contains(transaction7));
		}

		public void TestClosingBalance()
		{
			SetupData();

			var statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.CurrencyNK = Creator.AUD.RX_Code;
			statement.IsMultipleCurrency = true;
			AssertEquals("Closing Balance Should be the sum of (AH_Invoiceamount + AH_GSTAmount) of transaction1 + transaction3 + transaction4", -6.67M, statement.ClosingBalance);

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", Constants.StatementCollectionLetterType.StatementOfAccount) as PrintStatementForAccountMovement;
			statement.IsMultipleCurrency = false;
			AssertEquals("Closing Balance Should be the sum of AH_OSTotal of transaction1 + transaction3 + transaction4", -5M, statement.ClosingBalance);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintStatementForAccountMovement(Factory, GlbBranch.CurrentBranch, false, 0M);
		}

		protected override PrintStatement GetNewPrintStatement(ZGuid organisationPK, ZString currencyNK, ZString documentToPrint)
		{
			var statement = (PrintStatementForAccountMovement)GetNewBusinessObject();
			statement.OrganisationPK = organisationPK;
			statement.CurrencyNK = currencyNK;
			if (!documentToPrint.IsEmpty)
			{
				statement.DocumentToPrint = documentToPrint;
			}

			statement.PostDateFrom = ZDateTime.MinSmallDateTimeValue;
			statement.PostDateTo = ZDateTime.MaxSmallDateTimeValue.AddDays(-1); // EndOfDay of MaxSmallDateTimeValue will be invalid small datetime in AddTransactionHeaderSelect
			statement.GroupBy = "NON";
			statement.IsMultipleCurrency = false;
			statement.OpeningBalance = 0M;
			return statement;
		}

		void SetupData()
		{
			ZGuid anotherGLBCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_OH, Creator.AALSHI.PK);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, anotherGLBCompany);
			OrgCompanyData oldCompanyData = Factory.LoadTop1<OrgCompanyData>(filter);
			if (oldCompanyData != null)
			{
				oldCompanyData.Delete();
			}
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = anotherGLBCompany;
			Creator.AALSHI.CompanyData.OB_GC = anotherGLBCompany;
			Factory.Save();

			transaction1 = CreateCRDTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "CRD0001000", 200.0M, 0.75M, new ZDateTime(2015, 02, 12, 0, 0, 0));
			transaction2 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 0.75m, Creator.ABIGAS, new ZDateTime(2015, 02, 13, 0, 0, 0));
			transaction2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction2.AH_TransactionType = TransactionTypes.InvoiceBatch;

			transaction3 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV0001001", 150M, 0.75M, new ZDateTime(2015, 02, 14, 15, 25, 0));
			transaction4 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV00001002", 110M, 0.75M, new ZDateTime(2015, 02, 15, 0, 0, 0));
			transaction5 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001003", 110M, 0.75M, new ZDateTime(2015, 02, 15, 0, 0, 0));
			transaction6 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001004", 120M, 0.75M, new ZDateTime(2015, 02, 16, 0, 0, 0));
			transaction7 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001005", 120M, 0.75M, new ZDateTime(2015, 02, 16, 0, 0, 0));

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

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;

			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = cRDNumber;
			transaction.AH_PostDate = postDate;

			return transaction;
		}

		TransactionHeader transaction1;
		TransactionHeader transaction2;
		TransactionHeader transaction3;
		TransactionHeader transaction4;
		TransactionHeader transaction5;
		TransactionHeader transaction6;
		TransactionHeader transaction7;
	}
}
