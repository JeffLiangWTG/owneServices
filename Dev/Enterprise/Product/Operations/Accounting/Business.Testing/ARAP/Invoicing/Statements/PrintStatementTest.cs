using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using StatementCollectionLetterType = Enterprise.Core.Constants.StatementCollectionLetterType;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PrintStatement))]
	public class PrintStatementTest : PrintStatementTestBase
	{
		public void TestBatchInvoicesExcludedWhenInvoiceLinesFullyPaid()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice invoiceOne = Factory.NewWithValidTestData<ARInvoice>();
			invoiceOne.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineOne = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineOne.AL_OSExTaxAmount = 10m;
			invoiceLineOne.AL_AG = Creator.GLHeader1.PK;
			invoiceOne.Lines.Add(invoiceLineOne);

			ARInvoice invoiceTwo = Factory.NewWithValidTestData<ARInvoice>();
			invoiceTwo.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineTwo = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineTwo.AL_OSExTaxAmount = 20m;
			invoiceLineTwo.AL_AG = Creator.GLHeader1.PK;
			invoiceTwo.Lines.Add(invoiceLineTwo);
			Factory.Save();

			InvoiceBatchHeader invoiceBatchHeaderOne = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatchHeaderOne.AH_OH = organisation.PK;
			invoiceBatchHeaderOne.Line.Add(invoiceOne);
			invoiceBatchHeaderOne.Line.Add(invoiceTwo);

			ARInvoice invoiceThree = Factory.NewWithValidTestData<ARInvoice>();
			invoiceThree.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineThree = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineThree.AL_OSExTaxAmount = 10m;
			invoiceLineThree.AL_AG = Creator.GLHeader1.PK;
			invoiceThree.Lines.Add(invoiceLineThree);

			ARInvoice invoiceFour = Factory.NewWithValidTestData<ARInvoice>();
			invoiceFour.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineFour = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineFour.AL_OSExTaxAmount = 20m;
			invoiceLineFour.AL_AG = Creator.GLHeader1.PK;
			invoiceFour.Lines.Add(invoiceLineFour);

			InvoiceBatchHeader invoiceBatchHeaderTwo = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatchHeaderTwo.AH_OH = organisation.PK;
			invoiceBatchHeaderTwo.Line.Add(invoiceThree);
			invoiceBatchHeaderTwo.Line.Add(invoiceFour);

			FullyPayInvoice(invoiceThree);
			FullyPayInvoice(invoiceFour);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);

			Assert("BatchInvoice1 should be present as it's not fully paid", statement.Transactions.Contains(invoiceBatchHeaderOne.PK));
			Assert("BatchInvoice2 should not be present as it is fully paid", !statement.Transactions.Contains(invoiceBatchHeaderTwo.PK));

			PayInvoice(invoiceOne, 10);
			Factory.Save();

			statement = GetNewPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			Assert("BatchInvoice1 should be present as it's only parly paid", statement.Transactions.Contains(invoiceBatchHeaderOne.PK));
			Assert("BatchInvoice2 should not be present as it is fully paid", !statement.Transactions.Contains(invoiceBatchHeaderTwo.PK));
		}

		public void TestSourceIdentifier()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var statement = GetNewPrintStatement(organisation.PK, Core.Constants.CurrencyCodes.Australia, StatementCollectionLetterType.StatementOfAccount);
			Assert(statement is ISourceIdentifierProvider);
			AssertEquals(organisation.PK, (statement as ISourceIdentifierProvider).SourceIdentifier);
		}

		public void TestInvoicesWithZeroAmountAreNotIncluded()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice dummyInvoice = Factory.NewWithValidTestData<ARInvoice>();
			dummyInvoice.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLine.AL_AG = Creator.GLHeader1.PK;
			invoiceLine.AL_AG = Creator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 10m;
			dummyInvoice.Lines.Add(invoiceLine);

			ARInvoice zeroInvoice = Factory.NewWithValidTestData<ARInvoice>();
			zeroInvoice.AH_OH = organisation.PK;
			invoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLine.AL_AG = Creator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 20m;
			zeroInvoice.Lines.Add(invoiceLine);
			invoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLine.AL_AG = Creator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = -20m;
			zeroInvoice.Lines.Add(invoiceLine);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			Assert("zeroInvoice should not be included", !statement.Transactions.Contains(zeroInvoice.PK));
		}

		void FullyPayInvoice(ARInvoice invoice)
		{
			PayInvoice(invoice, invoice.AH_InvoiceAmount + invoice.AH_GSTAmount);
		}

		void PayInvoice(ARInvoice invoice, decimal amount)
		{
			PayInvoice(invoice, amount, ZDate.Today);
		}

		void PayInvoice(ARInvoice invoice, decimal amount, ZDate matchDate)
		{
			AccTransactionMatchLink linkForInvoice = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkForInvoice.AP_Amount = amount;
			linkForInvoice.AP_MatchDate = matchDate;
			linkForInvoice.AP_AH = invoice.PK;
			invoice.AH_OutstandingAmount = invoice.AH_OutstandingAmount + invoice.AH_GSTAmount - amount;
			if (invoice.AH_OutstandingAmount == ZDecimal.Zero)
			{
				invoice.AH_FullyPaidDate = matchDate;
			}

			AccTransactionHeader payment = Factory.NewWithValidTestData<AccTransactionHeader>();
			payment.AH_InvoiceAmount = -amount;

			AccTransactionMatchLink linkForPayment = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkForPayment.AP_AH = payment.PK;
			linkForPayment.AP_Amount = -amount;
			linkForPayment.AP_MatchDate = matchDate;
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var statement = (PrintStatement)GetNewBusinessObject();
			string[] documentTitles =
			{
				StatementCollectionLetterType.StatementOfAccount,
				StatementCollectionLetterType.FirstReminder,
				StatementCollectionLetterType.SecondReminder,
				StatementCollectionLetterType.CollectionLetter,
				StatementCollectionLetterType.DemandLetter
			};
			string[] values =
			{
				"STATEMENT OF ACCOUNT", "REQUEST FOR IMMEDIATE PAYMENT", "2ND REQUEST FOR IMMEDIATE PAYMENT",
				"COLLECTION LETTER", "LETTER OF DEMAND"
			};

			for (var i = 0; i < documentTitles.Length; i++)
			{
				statement.DocumentToPrint = documentTitles[i];
				var titleCopyCountPair = statement.DocumentSupporter.GetDocumentTitlesForPivot("", null, null);
				AssertEquals(string.Format("GetDocumentTitlesForPivot should return {0}", values[i]), values[i], titleCopyCountPair.Title);
			}
		}

		#region TestProperties

		public void TestCurrencyNK()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			statement.CurrencyNK = "NNN";
			AssertEquals("Currency NK", "NNN", statement.CurrencyNK);
		}

		public void TestTransactionBranchPK()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			ZGuid transactionBranchGuid = ZGuid.NewZGuid();
			statement.TransactionBranchPK = transactionBranchGuid;
			AssertEquals("TransactionBranch PK", transactionBranchGuid, statement.TransactionBranchPK);
		}

		public void TestTransactionDepartmentPK()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			ZGuid transactionDepartmentGuid = ZGuid.NewZGuid();
			statement.TransactionDepartmentPK = transactionDepartmentGuid;
			AssertEquals("TransactionDepartment PK", transactionDepartmentGuid, statement.TransactionDepartmentPK);
		}

		public void TestCutOffDate()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			ZDateTime cutOffDate = ZDateTime.Today.Date;
			statement.CutOffDate = cutOffDate;
			AssertEquals("Cut Off Date", cutOffDate, statement.CutOffDate);
		}

		#endregion

		#region Other Tests

		[ExpectNoExceptions]
		public override void TestBizObjectFields()
		{
			//don't delete since it has to be a business object, but no fields are required
			//speak to Deb if you want to delete it.
			base.TestBizObjectFields();
		}

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", BusinessContext.Statement, ((PrintStatement)GetNewBusinessObject()).DocumentSupporter.BusinessContext);
		}

		public void TestGetDocBusinessObjects()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			DocumentWrapper[] wrapper = statement.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Statement, null);
			AssertEquals("No of Wrappers created", 1, wrapper.Length);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Enterprise.Core.Constants.DataContext.Statement is Supported", true, ((PrintStatement)GetNewBusinessObject()).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Statement)));
		}

		#endregion

		#region TransactionHeader Tests

		public override void TestTransactionHeaderCollectionCommonFilters()
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

			Creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			Creator.ABIGAS.OH_IsCreditor = true;

			Invoice transaction1 = CreateInvoice<ARInvoice>(Creator.ABIGAS);
			transaction1.AH_RX_NKTransactionCurrency = "USD";
			transaction1.AH_FullyPaidDate = ZDateTime.Empty;
			transaction1.AH_TransactionNum = "00001000";
			transaction1.AH_OutstandingAmount = 100M;
			transaction1.AH_GB = newBranch.PK;
			transaction1.Lines[0].AL_GB = newBranch.PK;

			Invoice transaction2 = CreateInvoice<ARInvoice>(Creator.ABIGAS);
			transaction2.AH_RX_NKTransactionCurrency = "USD";
			transaction2.AH_FullyPaidDate = ZDateTime.Empty;
			transaction2.AH_TransactionNum = "00001001";
			transaction2.AH_OutstandingAmount = 100M;
			var invoiceBatch = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatch.Line.Add(transaction2);
			invoiceBatch.AH_OH = transaction2.AH_OH;
			invoiceBatch.AH_RX_NKTransactionCurrency = transaction2.AH_RX_NKTransactionCurrency;
			invoiceBatch.AH_FullyPaidDate = ZDateTime.Now;

			Invoice transaction3 = CreateInvoice<APInvoice>(Creator.ABIGAS);
			transaction3.AH_RX_NKTransactionCurrency = "USD";
			transaction3.AH_FullyPaidDate = ZDateTime.Empty;
			transaction3.AH_TransactionNum = "00001002";
			transaction3.AH_OutstandingAmount = -110M;

			Invoice transaction4 = CreateInvoice<ARInvoice>(Creator.AALSHI);
			transaction4.AH_RX_NKTransactionCurrency = "USD";
			transaction4.AH_FullyPaidDate = ZDateTime.Empty;
			transaction4.AH_TransactionNum = "00001003";
			transaction4.AH_OutstandingAmount = 100M;

			Invoice transaction5 = CreateInvoice<ARInvoice>(Creator.ABIGAS);
			transaction5.AH_RX_NKTransactionCurrency = "USD";
			transaction5.AH_FullyPaidDate = ZDateTime.Empty;
			transaction5.AH_TransactionNum = "00001004";
			transaction5.AH_OutstandingAmount = 100M;

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.StatementOfAccount);
			AssertEquals("Collection should contain only one transaction", 2, statement.Transactions.Count);
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));
			Assert("Transaction5 should be in collection", statement.Transactions.Contains(transaction5));
		}

		public void TestTransactionHeaderCollectionWithCutOffPeriodOrOutstandingAmountGreaterThanSpecified()
		{
			ARInvoice arInv1 = (ARInvoice)CreateInvoice(Creator.ABIGAS, 1000m);
			arInv1.AH_PostDate = new ZDate(2012, 3, 11);
			PayInvoice(arInv1, 200m, new ZDate(2012, 4, 1));
			PayInvoice(arInv1, 800m, new ZDate(2012, 5, 1));

			ARInvoice arInv2 = (ARInvoice)CreateInvoice(Creator.ABIGAS, 500m);
			ARInvoice arInv3 = (ARInvoice)CreateInvoice(Creator.ABIGAS, 2000m);
			ARJournal arJournal = Factory.NewWithValidTestData<ARJournal>();
			arJournal.AH_OH = Creator.ABIGAS.PK;
			arJournal.AH_PostDate = new ZDate(2012, 1, 1);
			arJournal.AH_OSExTaxAmount = 500m;
			ARReceipt arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt.AH_OH = Creator.ABIGAS.PK;
			arReceipt.AH_PostDate = new ZDate(2012, 1, 1);
			arReceipt.AH_OSExTaxAmount = 500m;
			Factory.Save();

			var statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			statement.CutOffDate = ZDateTime.Empty;
			statement.EndOfPeriod = new ZDateTime(2012, 3, 31);
			var transactions = statement.Transactions;
			AssertEquals("CutOffPeriod is before part-paid date", 3, statement.Transactions.Count);
			AssertEquals("CutOffPeriod is before part-paid date", true, statement.Transactions.Contains(arInv1));
			AssertEquals("CutOffPeriod is before part-paid date", true, statement.Transactions.Contains(arJournal));
			AssertEquals("CutOffPeriod is before part-paid date", true, statement.Transactions.Contains(arReceipt));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			statement.CutOffDate = ZDateTime.Empty;
			statement.EndOfPeriod = new ZDateTime(2012, 4, 30);
			transactions = statement.Transactions;
			AssertEquals("CutOffPeriod is between part-paid date and fully-paid date", 3, statement.Transactions.Count);
			AssertEquals("CutOffPeriod is between part-paid date and fully-paid date", true, statement.Transactions.Contains(arInv1));
			AssertEquals("CutOffPeriod is between part-paid date and fully-paid date", true, statement.Transactions.Contains(arJournal));
			AssertEquals("CutOffPeriod is between part-paid date and fully-paid date", true, statement.Transactions.Contains(arReceipt));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			statement.CutOffDate = ZDateTime.Empty;
			statement.EndOfPeriod = new ZDateTime(2012, 5, 31);
			transactions = statement.Transactions;
			AssertEquals("CutOffPeriod is after fully-paid date", 2, statement.Transactions.Count);
			AssertEquals("CutOffPeriod is after fully-paid date", true, statement.Transactions.Contains(arJournal));
			AssertEquals("CutOffPeriod is after fully-paid date", true, statement.Transactions.Contains(arReceipt));

			ARInvoice arInv4 = (ARInvoice)CreateInvoice(Creator.ABIGAS, -1000m);
			arInv4.AH_PostDate = new ZDate(2012, 3, 11);
			PayInvoice(arInv4, -200m, new ZDate(2012, 4, 1));

			Factory.Save();

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			statement.CutOffDate = ZDateTime.Empty;
			statement.EndOfPeriod = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 200m;
			transactions = statement.Transactions;
			AssertEquals("CutOffPeriod is not specified but OutstandingAmount", 3, statement.Transactions.Count);
			AssertEquals("CutOffPeriod is not specified but OutstandingAmount", true, statement.Transactions.Contains(arInv2));
			AssertEquals("CutOffPeriod is not specified but OutstandingAmount", true, statement.Transactions.Contains(arInv3));
			AssertEquals("CutOffPeriod is not specified but OutstandingAmount", true, statement.Transactions.Contains(arJournal));
		}

		public void TestTransactionHeaderCollectionWithCurrencySpecified()
		{
			TransactionHeader aUDTransaction = CreateInvoice(Creator.ABIGAS);
			aUDTransaction.AH_RX_NKTransactionCurrency = "AUD";
			aUDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			aUDTransaction.AH_TransactionNum = "00001000";
			aUDTransaction.AH_OutstandingAmount = 100M;

			TransactionHeader uSDTransaction = CreateInvoice(Creator.ABIGAS);
			uSDTransaction.AH_RX_NKTransactionCurrency = "USD";
			uSDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			uSDTransaction.AH_TransactionNum = "00001001";
			uSDTransaction.AH_OutstandingAmount = 100M;

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", ZString.Empty);
			Assert("AUDTransaction should not be in collection", !statement.Transactions.Contains(aUDTransaction));
			Assert("USDTransaction should be in collection", statement.Transactions.Contains(uSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			Assert("AUDTransaction should be in collection", statement.Transactions.Contains(aUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
		}

		public void TestTransactionHeaderCollectionWithTransactionBranchSpecified()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;

			GlbBranch currentCompanyBranch = Factory.New<GlbBranch>();
			currentCompanyBranch.GB_GC = currentBranch.GB_GC;

			Factory.Save();

			TransactionHeader aUDTransaction = CreateInvoice(Creator.ABIGAS);
			aUDTransaction.AH_RX_NKTransactionCurrency = "AUD";
			aUDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			aUDTransaction.AH_TransactionNum = "00001000";
			aUDTransaction.AH_OutstandingAmount = 100M;
			aUDTransaction.AH_GB = currentBranch.PK;

			TransactionHeader otherAUDTransaction = CreateInvoice(Creator.ABIGAS);
			otherAUDTransaction.AH_RX_NKTransactionCurrency = "AUD";
			otherAUDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			otherAUDTransaction.AH_TransactionNum = "00001001";
			otherAUDTransaction.AH_OutstandingAmount = 100M;
			otherAUDTransaction.AH_GB = currentCompanyBranch.PK;

			TransactionHeader uSDTransaction = CreateInvoice(Creator.ABIGAS);
			uSDTransaction.AH_RX_NKTransactionCurrency = "USD";
			uSDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			uSDTransaction.AH_TransactionNum = "00001002";
			uSDTransaction.AH_OutstandingAmount = 100M;
			uSDTransaction.AH_GB = currentBranch.PK;

			TransactionHeader otherUSDTransaction = CreateInvoice(Creator.ABIGAS);
			otherUSDTransaction.AH_RX_NKTransactionCurrency = "USD";
			otherUSDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			otherUSDTransaction.AH_TransactionNum = "00001003";
			otherUSDTransaction.AH_OutstandingAmount = 100M;
			otherUSDTransaction.AH_GB = currentCompanyBranch.PK;

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", ZString.Empty);
			Assert("AUDTransaction should not be in collection", !statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should be in collection", statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should be in collection", statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", ZString.Empty);
			statement.TransactionBranchPK = currentCompanyBranch.PK;
			statement.IssueByTransactionBranch = true;
			Assert("AUDTransaction should not be in collection", !statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should be in collection", statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			Assert("AUDTransaction should be in collection", statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should be in collection", statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should not be in collection", !statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "AUD", ZString.Empty);
			statement.TransactionBranchPK = currentBranch.PK;
			statement.IssueByTransactionBranch = true;
			Assert("AUDTransaction should be in collection", statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should not be in collection", !statement.Transactions.Contains(otherUSDTransaction));
		}

		public void TestTransactionHeaderCollectionWithTransactionDepartmentSpecified()
		{
			GlbDepartment currentDepartment = GlbDepartment.CurrentDepartment;

			GlbDepartment currentCompanyDepartment = Factory.New<GlbDepartment>();

			Factory.Save();

			TransactionHeader aUDTransaction = CreateInvoice(Creator.ABIGAS);
			aUDTransaction.AH_RX_NKTransactionCurrency = Creator.AUD.RX_Code;
			aUDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			aUDTransaction.AH_TransactionNum = "00001000";
			aUDTransaction.AH_OutstandingAmount = 100M;
			aUDTransaction.AH_GE = currentDepartment.PK;

			TransactionHeader otherAUDTransaction = CreateInvoice(Creator.ABIGAS);
			otherAUDTransaction.AH_RX_NKTransactionCurrency = Creator.AUD.RX_Code;
			otherAUDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			otherAUDTransaction.AH_TransactionNum = "00001001";
			otherAUDTransaction.AH_OutstandingAmount = 100M;
			otherAUDTransaction.AH_GE = currentCompanyDepartment.PK;

			TransactionHeader uSDTransaction = CreateInvoice(Creator.ABIGAS);
			uSDTransaction.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			uSDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			uSDTransaction.AH_TransactionNum = "00001002";
			uSDTransaction.AH_OutstandingAmount = 100M;
			uSDTransaction.AH_GE = currentDepartment.PK;

			TransactionHeader otherUSDTransaction = CreateInvoice(Creator.ABIGAS);
			otherUSDTransaction.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			otherUSDTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			otherUSDTransaction.AH_TransactionNum = "00001003";
			otherUSDTransaction.AH_OutstandingAmount = 100M;
			otherUSDTransaction.AH_GE = currentCompanyDepartment.PK;

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, ZString.Empty);
			Assert("AUDTransaction should not be in collection", !statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should be in collection", statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should be in collection", statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, ZString.Empty);
			statement.TransactionDepartmentPK = currentCompanyDepartment.PK;
			statement.IssueByTransactionDepartment = true;
			Assert("AUDTransaction should not be in collection", !statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should be in collection", statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.AUD.RX_Code, ZString.Empty);
			Assert("AUDTransaction should be in collection", statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should be in collection", statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should not be in collection", !statement.Transactions.Contains(otherUSDTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.AUD.RX_Code, ZString.Empty);
			statement.TransactionDepartmentPK = currentDepartment.PK;
			statement.IssueByTransactionDepartment = true;
			Assert("AUDTransaction should be in collection", statement.Transactions.Contains(aUDTransaction));
			Assert("OtherAUDTransaction should not be in collection", !statement.Transactions.Contains(otherAUDTransaction));
			Assert("USDTransaction should not be in collection", !statement.Transactions.Contains(uSDTransaction));
			Assert("OtherUSDTransaction should not be in collection", !statement.Transactions.Contains(otherUSDTransaction));
		}

		public void TestTransactionHeaderCollectionWithInvoiceDateSpecified()
		{
			ZDateTime now = ZDateTime.Now.Date.AddDays(1);
			// Invoice Dates On Or Before

			AccTransactionHeader transaction1 = CreateInvoice(Creator.ABIGAS);
			transaction1.AH_RX_NKTransactionCurrency = "USD";
			transaction1.AH_InvoiceDate = now.AddDays(-5).AddDays(-1);
			transaction1.AH_FullyPaidDate = ZDateTime.Empty;
			transaction1.AH_TransactionNum = "00001000";
			transaction1.AH_OutstandingAmount = 100M;

			AccTransactionHeader transaction2 = CreateInvoice(Creator.ABIGAS);
			transaction2.AH_RX_NKTransactionCurrency = "USD";
			transaction2.AH_InvoiceDate = now.AddDays(-10).AddDays(-1);
			transaction2.AH_FullyPaidDate = ZDateTime.Empty;
			transaction2.AH_TransactionNum = "00001001";
			transaction2.AH_OutstandingAmount = 100M;

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.StatementOfAccount);
			statement.CutOffDate = now;

			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.StatementOfAccount);
			statement.CutOffDate = now.AddDays(-5);
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.StatementOfAccount);
			statement.CutOffDate = now.AddDays(-10);
			Assert("Transaction1 should not be in collection", !statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.StatementOfAccount);
			statement.CutOffDate = now.AddDays(-15);
			Assert("Transaction1 should not be in collection", !statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should not be in collection", !statement.Transactions.Contains(transaction2));
		}

		public void TestTransactionHeaderCollectionWithDueDateSpecified()
		{
			ZDateTime now = ZDateTime.Now;
			// Due Dates On Or Before

			AccTransactionHeader transaction1 = CreateInvoice(Creator.ABIGAS);
			transaction1.AH_RX_NKTransactionCurrency = "USD";
			transaction1.AH_InvoiceDate = now.AddDays(-30);
			transaction1.AH_DueDate = now.AddDays(-5).AddDays(-1);
			transaction1.AH_FullyPaidDate = ZDateTime.Empty;
			transaction1.AH_TransactionNum = "00001000";
			transaction1.AH_OutstandingAmount = 100M;

			AccTransactionHeader transaction2 = CreateInvoice(Creator.ABIGAS);
			transaction2.AH_RX_NKTransactionCurrency = "USD";
			transaction2.AH_InvoiceDate = now.AddDays(-30);
			transaction2.AH_DueDate = now.AddDays(-10).AddDays(-1);
			transaction2.AH_FullyPaidDate = ZDateTime.Empty;
			transaction2.AH_TransactionNum = "00001001";
			transaction2.AH_OutstandingAmount = 100M;

			now = now.Date.AddDays(1);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = now;
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = now.AddDays(-5);
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = now.AddDays(-10);
			Assert("Transaction1 should not be in collection", !statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, "USD", StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = now.AddDays(-15);
			Assert("Transaction1 should not be in collection", !statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should not be in collection", !statement.Transactions.Contains(transaction2));
		}

		public void TestTransactionHeaderCollectionWithAmountGreaterThanSpecified()
		{
			AccTransactionHeader transaction1 = CreateInvoice(Creator.ABIGAS, 200M);
			transaction1.AH_RX_NKTransactionCurrency = "USD";
			transaction1.AH_FullyPaidDate = ZDateTime.Empty;
			transaction1.AH_TransactionNum = "00001000";
			transaction1.AH_GSTAmount = 100M;
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, transaction1, 100M, ZDateTime.Now);

			AccTransactionHeader transaction2 = CreateInvoice(Creator.ABIGAS, -200M);
			transaction2.AH_RX_NKTransactionCurrency = "USD";
			transaction2.AH_FullyPaidDate = ZDateTime.Empty;
			transaction2.AH_TransactionNum = "00001001";
			transaction2.AH_GSTAmount = -100M;
			SetUpMatchLinkForTransaction(matchGroup, transaction2, -100M, ZDateTime.Now);

			AccTransactionHeader transaction3 = CreateInvoice(Creator.ABIGAS, 100M);
			transaction3.AH_RX_NKTransactionCurrency = "USD";
			transaction3.AH_FullyPaidDate = ZDateTime.Empty;
			transaction3.AH_TransactionNum = "00001002";
			transaction3.AH_GSTAmount = 100M;
			SetUpMatchLinkForTransaction(matchGroup, transaction3, 100M, ZDateTime.Now);
			transaction3.AH_FullyPaidDate = ZDateTime.Now;
			BalanceMatchGroup(matchGroup);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 0M;
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should be in collection", statement.Transactions.Contains(transaction2));
			Assert("Transaction3 should not be in collection", !statement.Transactions.Contains(transaction3));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 50M;
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should not be in collection", !statement.Transactions.Contains(transaction2));
			Assert("Transaction3 should not be in collection", !statement.Transactions.Contains(transaction3));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 100M;
			Assert("Transaction1 should be in collection", statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should not be in collection", !statement.Transactions.Contains(transaction2));
			Assert("Transaction3 should not be in collection", !statement.Transactions.Contains(transaction3));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 150M;
			Assert("Transaction1 should not be in collection", !statement.Transactions.Contains(transaction1));
			Assert("Transaction2 should not be in collection", !statement.Transactions.Contains(transaction2));
			Assert("Transaction3 should not be in collection", !statement.Transactions.Contains(transaction3));

			AccTransactionHeader transaction4 = CreateInvoice(Creator.ABIGAS, 500M);
			Factory.Save();

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.LocalCurrency.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 500M;
			statement.OrganisationPK = Creator.ABIGAS.PK;
			Assert("Transaction4 should be in collection", statement.Transactions.Contains(transaction4));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.LocalCurrency.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.OutStandingAmountGreaterThan = 501M;
			statement.OrganisationPK = Creator.ABIGAS.PK;
			Assert("Transaction4 should not be in collection", !statement.Transactions.Contains(transaction4));
		}

		public void TestTransactionHeaderCollectionWithIncludeTransactionsInActiveBatch()
		{
			var registry = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", true);
			registry.Add("ST1", (NoResString)"ST1 Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			AccTransactionHeader transaction = CreateInvoice(Creator.ABIGAS);
			transaction.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = "00001000";
			transaction.AH_OutstandingAmount = 100M;
			transaction.AH_InvoiceAmount = 100M;

			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 100m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";
			batch.ACB_Type = "STD";

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_OH_Debtor = Creator.ABIGAS.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;
			order.ACO_Amount = 100m;

			var orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = transaction.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;
			Factory.Save();

			var statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;

			Assert("transaction should be in collection", statement.Transactions.Contains(transaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.IncludeTransactionsInActiveBatch = false;

			Assert("transaction should not be in collection", !statement.Transactions.Contains(transaction));

			batch.ACB_Type = "ST1";
			Factory.Save();
			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.IncludeTransactionsInActiveBatch = true;

			Assert("transaction should not be in collection (ST1 Registry check is not ticked)", !statement.Transactions.Contains(transaction));

			batch.ACB_Type = "ST2";
			Factory.Save();
			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.IncludeTransactionsInActiveBatch = true;

			Assert("transaction should be in collection (ST2 Registry item was removed)", statement.Transactions.Contains(transaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.IncludeTransactionsInActiveBatch = false;

			Assert("transaction should not be in collection", !statement.Transactions.Contains(transaction));

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", true);
			registry.Add("ST1", (NoResString)"ST1 Desc", true);
			registry.Add("ST2", (NoResString)"ST2 Desc", true);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			batch.ACB_Type = "ST3";
			Factory.Save();
			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.IncludeTransactionsInActiveBatch = true;

			Assert("transaction should be in collection (ST3 Registry item was removed and there is no unticked entries)", statement.Transactions.Contains(transaction));
		}

		public void TestTransactionHeaderCollectionWithIsDisbursementSpecified()
		{
			List<AccTransactionHeader> disbursementTransactions = new List<AccTransactionHeader>(InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Length);
			foreach (string disbursementInvoiceType in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
			{
				AccTransactionHeader transaction = CreateInvoice(Creator.ABIGAS);
				transaction.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
				transaction.AH_FullyPaidDate = ZDateTime.Empty;
				transaction.AH_TransactionNum = "00001000";
				transaction.AH_OutstandingAmount = 100M;
				transaction.AH_InvoiceAmount = 100M;
				transaction.AH_TransactionCategory = disbursementInvoiceType;
				disbursementTransactions.Add(transaction);
			}

			AccTransactionHeader nonDisbursementTransaction = CreateInvoice(Creator.ABIGAS);
			nonDisbursementTransaction.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			nonDisbursementTransaction.AH_FullyPaidDate = ZDateTime.Empty;
			nonDisbursementTransaction.AH_TransactionNum = "00001001";
			nonDisbursementTransaction.AH_OutstandingAmount = -100M;
			nonDisbursementTransaction.AH_InvoiceAmount = -100M;
			nonDisbursementTransaction.AH_TransactionCategory = "";

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.DisbursementInvoicesOnly = false;
			foreach (AccTransactionHeader transaction in disbursementTransactions)
			{
				Assert(string.Format("Disbursement transaction '{0}' should be in collection", transaction.AH_TransactionCategory), statement.Transactions.Contains(transaction));
			}
			Assert("Non disbursement transaction should be in collection", statement.Transactions.Contains(nonDisbursementTransaction));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.USD.RX_Code, StatementCollectionLetterType.FirstReminder);
			statement.CutOffDate = ZDateTime.Empty;
			statement.DisbursementInvoicesOnly = true;
			foreach (AccTransactionHeader transaction in disbursementTransactions)
			{
				Assert(string.Format("Disbursement transaction '{0}' should be in collection", transaction.AH_TransactionCategory), statement.Transactions.Contains(transaction));
			}
			Assert("Non disbursement transaction should not be in collection", !statement.Transactions.Contains(nonDisbursementTransaction));
		}

		public void TestTransactionHeaderCollectionWithEndOfPeriodSpecified()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			AccTransactionHeader invoice1 = CreateInvoice(Creator.ABIGAS);
			invoice1.AH_PostDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2);
			invoice1.AH_GSTAmount = 100M;
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, invoice1, 50M, testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2));
			SetUpMatchLinkForTransaction(matchGroup, invoice1, 50M, testHelper.CurrentPeriod.AM_StartDate.AddDays(2));
			invoice1.AH_FullyPaidDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(2);

			AccTransactionHeader invoice2 = CreateInvoice(Creator.ABIGAS);
			invoice2.AH_PostDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2);
			invoice2.AH_GSTAmount = 100M;
			SetUpMatchLinkForTransaction(matchGroup, invoice2, 100M, testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2));
			invoice2.AH_FullyPaidDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2);

			AccTransactionHeader invoice3 = CreateInvoice(Creator.ABIGAS);
			invoice3.AH_PostDate = testHelper.PreviousOpenPeriod.AM_StartDate.AddDays(2);
			SetUpMatchLinkForTransaction(matchGroup, invoice3, 40M, testHelper.PreviousOpenPeriod.AM_StartDate.AddDays(2));
			invoice3.AH_OutstandingAmount = 60M;
			BalanceMatchGroup(matchGroup);

			AccTransactionHeader invoice4 = CreateInvoice(Creator.ABIGAS);
			invoice4.AH_PostDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(2);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.EndOfPeriod = testHelper.CurrentPeriod.AM_EndDate.Date;
			AssertEquals("Should be two transactions which are not fully paid by the specified EndOfPeriod", 2, statement.Transactions.Count);
			Assert("Invoice1 should not be in collection", !statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should not be in collection", !statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should be in collection", statement.Transactions.Contains(invoice4));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.EndOfPeriod = testHelper.PreviousOpenPeriod.AM_EndDate.Date;
			AssertEquals("Should be two transactions which are posted but not fully paid by the specified EndOfPeriod", 2, statement.Transactions.Count);
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should not be in collection", !statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should not be in collection", !statement.Transactions.Contains(invoice4));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.EndOfPeriod = testHelper.PreviousOpenPeriod.AM_EndDate.Date;
			statement.OutStandingAmountGreaterThan = 55M;
			AssertEquals("Should be one transaction posted but not fully paid by the specified EndOfPeriod with matching OutstandingAmount", 1, statement.Transactions.Count);
			Assert("Invoice1 should not be in collection", !statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should not be in collection", !statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should not be in collection", !statement.Transactions.Contains(invoice4));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.EndOfPeriod = testHelper.PreviousGLClosedPeriod.AM_EndDate.Date;
			AssertEquals("Should be one transaction which is only partially paid by the specified EndOfPeriod", 1, statement.Transactions.Count);
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should not be in collection", !statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should not be in collection", !statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should not be in collection", !statement.Transactions.Contains(invoice4));
		}

		public void TestTransactionHeaderCollectionWithIssueBySettlementGroupSpecified()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.ZECTRA.CompanyData.OB_IsDebtor = true;
			Creator.Agent.CompanyData.OB_IsDebtor = true;
			Creator.Creditor1.CompanyData.OB_IsDebtor = true;

			Creator.AALSHI.ARSettlementGroupPK = Creator.ABIGAS.PK;
			Creator.ZECTRA.ARSettlementGroupPK = Creator.ABIGAS.PK;

			Creator.Agent.APSettlementGroupPK = Creator.ABIGAS.PK;

			OrgRelatedParty nonCurrentCompanyRelatedParty = Creator.Creditor1.AllRelatedParties.AddNew();
			nonCurrentCompanyRelatedParty.PR_OH_Parent = Creator.Creditor1.PK;
			nonCurrentCompanyRelatedParty.PR_OH_RelatedParty = Creator.ABIGAS.PK;
			nonCurrentCompanyRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			nonCurrentCompanyRelatedParty.PR_GC = Creator.NonCurrentCompany.PK;

			AccTransactionHeader invoice1 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice2 = CreateInvoice(Creator.AALSHI);
			AccTransactionHeader invoice3 = CreateInvoice(Creator.ZECTRA);
			AccTransactionHeader invoice4 = CreateInvoice(Creator.Agent);
			AccTransactionHeader invoice5 = CreateInvoice(Creator.Creditor1);
			invoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice2.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice3.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice4.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice1.AH_GSTAmount = 10;
			invoice2.AH_GSTAmount = 10;
			invoice3.AH_GSTAmount = 10;
			invoice4.AH_GSTAmount = 10;
			invoice5.AH_GSTAmount = 10;

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, invoice1, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice2, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice3, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice4, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice5, 10, ZDateTime.Today.AddDays(-2));
			BalanceMatchGroup(matchGroup);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.IssueBySettlementGroup = false;
			statement.EndOfPeriod = ZDateTime.Now.AddDays(1);
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should not be in collection", !statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should not be in collection", !statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should not be in collection", !statement.Transactions.Contains(invoice4));
			Assert("Invoice5 should not be in collection", !statement.Transactions.Contains(invoice5));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.IssueBySettlementGroup = true;
			statement.EndOfPeriod = ZDateTime.Now.AddDays(1);
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should be in collection", statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
			Assert("Invoice4 should NOT be in collection", !statement.Transactions.Contains(invoice4));
			Assert("Invoice5 should NOT be in collection", !statement.Transactions.Contains(invoice5));
		}

		public void TestTransactionHeaderCollectionWithIssueByTransactionBranchSpecified()
		{
			GlbBranch currentCompanyBranch = Factory.New<GlbBranch>();
			currentCompanyBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			AccTransactionHeader invoice1 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice2 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice3 = CreateInvoice(Creator.ABIGAS);
			invoice2.AH_GB = currentCompanyBranch.PK;
			invoice3.AH_GB = currentCompanyBranch.PK;

			invoice1.AH_GSTAmount = 10;
			invoice2.AH_GSTAmount = 10;
			invoice3.AH_GSTAmount = 10;

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, invoice1, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice2, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice3, 10, ZDateTime.Today.AddDays(-2));
			BalanceMatchGroup(matchGroup);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.TransactionBranchPK = currentCompanyBranch.PK;
			statement.IssueByTransactionBranch = false;
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should be in collection", statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			statement.TransactionBranchPK = currentCompanyBranch.PK;
			statement.IssueByTransactionBranch = true;
			Assert("Invoice1 should not be in collection", !statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should be in collection", statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
		}

		public void TestTransactionHeaderCollectionWithIssueByTransactionDepartmentSpecified()
		{
			GlbDepartment currentCompanyDepartment = Factory.New<GlbDepartment>();

			AccTransactionHeader invoice1 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice2 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice3 = CreateInvoice(Creator.ABIGAS);
			invoice2.AH_GE = currentCompanyDepartment.PK;
			invoice3.AH_GE = currentCompanyDepartment.PK;

			invoice1.AH_GSTAmount = 10;
			invoice2.AH_GSTAmount = 10;
			invoice3.AH_GSTAmount = 10;

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, invoice1, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice2, 10, ZDateTime.Today.AddDays(-2));
			SetUpMatchLinkForTransaction(matchGroup, invoice3, 10, ZDateTime.Today.AddDays(-2));
			BalanceMatchGroup(matchGroup);

			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.LocalCurrency.RX_Code, StatementCollectionLetterType.StatementOfAccount);
			statement.TransactionDepartmentPK = currentCompanyDepartment.PK;
			statement.IssueByTransactionDepartment = false;
			Assert("Invoice1 should be in collection", statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should be in collection", statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));

			statement = GetNewPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.LocalCurrency.RX_Code, StatementCollectionLetterType.StatementOfAccount);
			statement.TransactionDepartmentPK = currentCompanyDepartment.PK;
			statement.IssueByTransactionDepartment = true;
			Assert("Invoice1 should not be in collection", !statement.Transactions.Contains(invoice1));
			Assert("Invoice2 should be in collection", statement.Transactions.Contains(invoice2));
			Assert("Invoice3 should be in collection", statement.Transactions.Contains(invoice3));
		}

		public void TestTransactionHeaderCollectionOrdering()
		{
			ZDateTime now = ZDateTime.Now;

			ARCreditNote creditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote1.AH_InvoiceDate = now.AddDays(-2);
			creditNote1.AH_OH = Creator.AALSHI.PK;
			creditNote1.AH_GB = GlbBranch.CurrentBranch.PK;
			InvoicingLineBase line = Creator.CreateInvoiceLine(creditNote1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARAdjustmentNote adjustmentNote1 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote1.AH_InvoiceDate = now.AddDays(-2);
			adjustmentNote1.AH_OH = Creator.AALSHI.PK;
			adjustmentNote1.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(adjustmentNote1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_InvoiceDate = now;
			invoice1.AH_OH = Creator.AALSHI.PK;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(invoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_InvoiceDate = now;
			invoice2.AH_OH = Creator.AALSHI.PK;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(invoice2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARCreditNote creditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote2.AH_InvoiceDate = now;
			creditNote2.AH_OH = Creator.AALSHI.PK;
			creditNote2.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(creditNote2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARCreditNote creditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote3.AH_InvoiceDate = now;
			creditNote3.AH_OH = Creator.AALSHI.PK;
			creditNote3.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(creditNote3, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARAdjustmentNote adjustmentNote2 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote2.AH_InvoiceDate = now;
			adjustmentNote2.AH_OH = Creator.AALSHI.PK;
			adjustmentNote2.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(adjustmentNote2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			ARAdjustmentNote adjustmentNote3 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote3.AH_InvoiceDate = now;
			adjustmentNote3.AH_OH = Creator.AALSHI.PK;
			adjustmentNote3.AH_GB = GlbBranch.CurrentBranch.PK;
			line = Creator.CreateInvoiceLine(adjustmentNote3, GlbCompany.CurrentCompany.LocalCurrency, 1m, 0m);
			line.AL_GSTVAT = 100M;
			line.AL_OSExTaxAmount = 100M;
			Factory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.AALSHI.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, StatementCollectionLetterType.StatementOfAccount);
			AssertEquals("Transaction collection should contain 8 transactions", 8, statement.Transactions.Count);
			AssertEquals("CreditNote1 should be on the first place", creditNote1, statement.Transactions[0]);
			AssertEquals("AdjustmentNote1 should be on the second place", adjustmentNote1, statement.Transactions[1]);
			AssertEquals("Invoice1 should be on the third place", invoice1, statement.Transactions[2]);
			AssertEquals("Invoice2 should be on the fourth place", invoice2, statement.Transactions[3]);
			AssertEquals("CreditNote2 should be on the fifth place", creditNote2, statement.Transactions[4]);
			AssertEquals("CreditNote3 should be on the sixth place", creditNote3, statement.Transactions[5]);
			AssertEquals("AdjustmentNote2 should be on the seventh place", adjustmentNote2, statement.Transactions[6]);
			AssertEquals("AdjustmentNote3 should be on the eighth place", adjustmentNote3, statement.Transactions[7]);
		}

		public void TestLoad1TransactionHeaderDbHits()
		{
			AssertTransactionHeaderDbHits(1, 1);
		}

		public void TestLoad5TransactionHeadersDbHits()
		{
			AssertTransactionHeaderDbHits(5, 1);
		}

		public void TestLoad6TransactionHeadersDbHits()
		{
			AssertTransactionHeaderDbHits(6, 2);
		}

		public void TestLoad10TransactionHeadersDbHits()
		{
			AssertTransactionHeaderDbHits(10, 2);
		}

		public void TestLoad11TransactionHeadersDbHits()
		{
			AssertTransactionHeaderDbHits(11, 3);
		}

		void AssertTransactionHeaderDbHits(int transactionCount, int hitCount)
		{
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			TestObjectCreator otherCreator = new TestObjectCreator(otherFactory);

			otherCreator.ABIGAS.CompanyData.OB_IsDebtor = true;
			otherCreator.ABIGAS.CompanyData.SetARTaxApplicable(false);

			for (int i = 0; i < transactionCount; i++)
			{
				ARInvoice invoice = (ARInvoice)otherCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1.0M);
				otherCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0M, 100M);
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
				invoice.AH_OH = otherCreator.ABIGAS.PK;
			}

			otherFactory.Save();

			PrintStatement statement = GetNewPrintStatement(Creator.ABIGAS.PK, Creator.AUD.RX_Code, StatementCollectionLetterType.StatementOfAccount);

			int hitsBefore = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals(string.Format("Collection should contain {0} transaction(s)", transactionCount), transactionCount, statement.Transactions.Count);
			int hitsAfter = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals(string.Format("Should be {0} Db Hit(s)", hitCount), hitCount, hitsAfter - hitsBefore);
		}

		#endregion

		#region Implementation

		protected AccTransactionMatchLink SetUpMatchLinkForTransaction(TransactionMatchLinkGroup matchGroup, AccTransactionHeader header, ZDecimal matchedAmount, ZDateTime matchedDate)
		{
			AccTransactionMatchLink matchLink = matchGroup.AddNew();

			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = matchedAmount;
			matchLink.AP_MatchDate = matchedDate;
			matchLink.AP_MatchGroupNum = "M000010001";

			return matchLink;
		}

		protected void BalanceMatchGroup(TransactionMatchLinkGroup matchGroup)
		{
			ZDecimal total = ZDecimal.Zero;
			foreach (TransactionMatchLink link in matchGroup)
			{
				total += link.AP_Amount;
			}
			if (total != ZDecimal.Zero)
			{
				AccTransactionHeader header = matchGroup.Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_InvoiceAmount = -total;

				TransactionMatchLink balanceLink = matchGroup.AddNew();
				balanceLink.AP_AH = header.PK;
				balanceLink.AP_Amount = -total;
				TestObjectCreator.SetupMatchLinkMatchDate(balanceLink);
			}
		}

		protected override PrintStatement GetNewPrintStatement(ZGuid organisationPK, ZString currencyNK, ZString documentToPrint)
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			statement.OrganisationPK = organisationPK;
			statement.CurrencyNK = currencyNK;
			if (!documentToPrint.IsEmpty)
			{
				statement.DocumentToPrint = documentToPrint;
			}

			statement.CutOffDate = ZDateTime.Today.AddDays(1);
			return statement;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintStatement(Factory, GlbBranch.CurrentBranch);
		}

		#endregion

	}
}
