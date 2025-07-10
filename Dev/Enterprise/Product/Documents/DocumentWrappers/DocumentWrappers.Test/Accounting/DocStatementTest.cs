using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocStatementTest : TestCaseWithFactory
	{
		public void TestGenericTransactions_InvoiceBatchHeader()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = organisation.PK;
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 10m;
			invoice1.Lines.Add(line);

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = organisation.PK;
			line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 20m;
			invoice2.Lines.Add(line);
			Factory.Save();

			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			batchHeader.AH_OH = organisation.PK;
			batchHeader.Line.Add(invoice1);
			batchHeader.Line.Add(invoice2);
			Factory.Save();

			SetUpPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals(1, statementWrapper.GenericTransactions.Count);
		}

		public void TestGenericTransactions()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = organisation.PK;
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 10m;
			invoice1.Lines.Add(line);

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = organisation.PK;
			line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 20m;
			invoice2.Lines.Add(line);
			Factory.Save();

			SetUpPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals(2, statementWrapper.GenericTransactions.Count);
		}

		public void TestGenericTransactionsSetEndOfStatementPeriodForCalculatingMatchedAmount()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_PostDate = new ZDateTime(2009, 05, 12);
			invoice1.AH_OH = organisation.PK;
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_OSExTaxAmount = 10m;
			line.AL_AG = Creator.GLHeader1.PK;
			invoice1.Lines.Add(line);

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_PostDate = new ZDateTime(2009, 06, 23);
			invoice2.AH_OH = organisation.PK;
			line = Factory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 20m;
			invoice2.Lines.Add(line);
			Factory.Save();

			SetUpPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Statement.EndOfPeriod = new ZDateTime(2011, 03, 31);
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals(2, statementWrapper.GenericTransactions.Count);

			foreach (DocGenericTransactionHeader x in statementWrapper.GenericTransactions)
			{
				AssertEquals("EndOfStatementPeriod should be set", new ZDateTime(2011, 03, 31), ((DocTransactionHeader)x.HeaderPlugIn).EndOfStatementPeriodForCalculatingMatchedAmount);
			}
		}

		[ExpectNoExceptions]
		public void TestCreatingNewPrintStatement()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DocStatement statementWrapper = DocStatement.New(Statement, factory);
		}

		public void TestIssueBySettlementGroup()
		{
			Statement.IssueBySettlementGroup = false;
			Assert(!StatementWrapper.IssueBySettlementGroup);

			Statement.IssueBySettlementGroup = true;
			Assert(StatementWrapper.IssueBySettlementGroup);
		}

		public void TestIssueByTransactionBranch()
		{
			Statement.IssueByTransactionBranch = false;
			Assert(!StatementWrapper.IssueByTransactionBranch);

			Statement.IssueByTransactionBranch = true;
			Assert(StatementWrapper.IssueByTransactionBranch);
		}

		public void TestIssueByTransactionDepartment()
		{
			Statement.IssueByTransactionDepartment = false;
			Assert(!StatementWrapper.IssueByTransactionDepartment);

			Statement.IssueByTransactionDepartment = true;
			Assert(StatementWrapper.IssueByTransactionDepartment);
		}

		public void TestBalanceForInvoiceBatches()
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ARInvoice invoiceOne = Factory.NewWithValidTestData<ARInvoice>();
			invoiceOne.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineOne = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineOne.AL_AG = Creator.GLHeader1.PK;
			invoiceLineOne.AL_OSExTaxAmount = 10m;
			invoiceOne.Lines.Add(invoiceLineOne);

			ARInvoice invoiceTwo = Factory.NewWithValidTestData<ARInvoice>();
			invoiceTwo.AH_OH = organisation.PK;
			ARInvoiceLine invoiceLineTwo = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineTwo.AL_AG = Creator.GLHeader1.PK;
			invoiceLineTwo.AL_OSExTaxAmount = 20m;
			invoiceTwo.Lines.Add(invoiceLineTwo);
			Factory.Save();

			InvoiceBatchHeader invoiceBatchHeaderOne = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatchHeaderOne.AH_OH = organisation.PK;
			invoiceBatchHeaderOne.Line.Add(invoiceOne);
			invoiceBatchHeaderOne.Line.Add(invoiceTwo);

			PayInvoice(invoiceOne, 10);
			Factory.Save();

			SetUpPrintStatement(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("BatchInvoice1 should be present as it's only parly paid", Statement.Transactions.Contains(invoiceBatchHeaderOne.PK));
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals("Should contain statementone as it's only partly paid", statementWrapper.Transactions.Count, 1);

			foreach (DocTransactionHeader header in statementWrapper.Transactions)
			{
				AssertEquals("Should be partly paid", 20m, header.Balance);
			}

			Statement.EndOfPeriod = ZDate.BrettsBirthday;
			statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals("Should contain statementone as its' fully unpaid at the given date", statementWrapper.Transactions.Count, 1);

			foreach (DocTransactionHeader header in statementWrapper.Transactions)
			{
				AssertEquals("Should be fully unpaid", 30m, header.Balance);
			}
		}

		void PayInvoice(ARInvoice invoice, decimal amount)
		{
			AccTransactionMatchLink linkForInvoice = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkForInvoice.AP_Amount = amount;
			linkForInvoice.AP_MatchDate = ZDate.Today;
			linkForInvoice.AP_AH = invoice.PK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = -amount;

			AccTransactionMatchLink linkToMatch = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkToMatch.AP_Amount = -amount;
			linkToMatch.AP_MatchDate = ZDate.Today;
			linkToMatch.AP_AH = header.PK;

			invoice.AH_OutstandingAmount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount - amount;
			if (invoice.AH_OutstandingAmount == ZDecimal.Zero)
			{
				invoice.AH_FullyPaidDate = ZDate.Today;
			}
		}

		public void TestOrganisation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var header = factory.LoadTop1<OrgHeader>(new ZQuery());
			DocStatement statementWrapper = DocStatement.New(Statement, factory);

			AssertNull("No oganisation selected", statementWrapper.Organisation);
			Statement.OrganisationPK = header.PK;
			AssertNotNull("Organisation is selected", statementWrapper.Organisation);
			AssertEquals("Is of type organisation", typeof(DocOrganisation), statementWrapper.Organisation.GetType());
		}

		public void TestAccountName()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var header = factory.LoadTop1<OrgHeader>(new ZQuery());
			DocStatement statementWrapper = DocStatement.New(Statement, factory);

			AssertEquals("AccountName", ZString.Empty, statementWrapper.AccountName);

			Statement.OrganisationPK = header.PK;
			((OrgHeader)((OrgHeaderSource)statementWrapper.Organisation.WrappedObject).WrappedObject).OH_FullName = "Planet Express";
			statementWrapper = DocStatement.New(Statement, factory);
			((OrgAddress)statementWrapper.Organisation.ARAddress.WrappedObject).OA_CompanyNameOverride = "";
			AssertEquals("AccountName", "Planet Express", statementWrapper.AccountName);
			((OrgAddress)statementWrapper.Organisation.ARAddress.WrappedObject).OA_CompanyNameOverride = "Martians are coming";
			AssertEquals("AccountName", "Planet Express", statementWrapper.AccountName);
		}

		public void TestOrganisationARAgreedPaymentMethod()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var header = factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgCompanyData companyData = header.CompanyData;
			companyData.OB_ARCreditAgreedPaymentMethod = "CHK";
			factory.Save();

			Statement.OrganisationPK = header.PK;
			DocStatement statementWrapper = DocStatement.New(Statement, factory);

			statementWrapper = DocStatement.New(Statement, factory);
			AssertEquals("OrganisationARAgreedPaymentMethod", "Business Check", statementWrapper.OrganisationARAgreedPaymentMethod);
		}

		public void TestOrganisationAPAgreedPaymentMethod()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var header = factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgCompanyData companyData = header.CompanyData;
			companyData.OB_APCreditAgreedPaymentMethod = "CHK";
			factory.Save();

			Statement.OrganisationPK = header.PK;
			DocStatement statementWrapper = DocStatement.New(Statement, factory);

			statementWrapper = DocStatement.New(Statement, factory);
			AssertEquals("OrganisationAPAgreedPaymentMethod", "Business Check", statementWrapper.OrganisationAPAgreedPaymentMethod);
		}

		public void TestCurrency()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var currency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			DocStatement statementWrapper = DocStatement.New(Statement, factory);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "VND";
			AssertNotNull("Currency is not blank", statementWrapper.Currency);
			AssertEquals("Is of type currency", typeof(DocCurrency), statementWrapper.Currency.GetType());
			AssertEquals("Currency should be the current login companys' currency", "VND", statementWrapper.Currency.Code);

			Statement.CurrencyNK = currency.RX_Code;
			AssertNotNull("Currency is not blank", statementWrapper.Currency);
			AssertEquals("Is of type currency", typeof(DocCurrency), statementWrapper.Currency.GetType());
		}

		public void TestBranch()
		{
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			AssertNotNull("Branch is not blank", statementWrapper.Branch);
			AssertEquals("Is of type branch", typeof(DocBranch), statementWrapper.Branch.GetType());
		}

		public void TestReceiptBankAccount()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var headerBisObj = factory.NewWithValidTestData<AccGLHeader>();

			var accountBisObj = factory.New<AccBankAccount>();
			accountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			accountBisObj.AB_RX_NKAccountCurrency = "USD";
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = headerBisObj.PK;
			accountBisObj.AB_Code = "ABCBANK";
			factory.Save();

			DocStatement statementWrapper = DocStatement.New(Statement, factory);
			Statement.CurrencyNK = "USD";
			AssertNotNull("Bank account is selected", statementWrapper.ReceiptBankAccount);
			AssertEquals("Is of type bank account", typeof(DocBankAccount), statementWrapper.ReceiptBankAccount.GetType());
		}

		public void TestCutOffDate()
		{
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			Statement.CutOffDate = ZDateTime.Empty;
			AssertEquals("Cut Off Date is empty", Env.Time.CurrentLocalDate, statementWrapper.CutOffDate);

			Statement.CutOffDate = Env.Time.CurrentLocalDate.AddDays(1);
			AssertEquals("Cut Off Date is not empty", Statement.CutOffDate.AddDays(-1), statementWrapper.CutOffDate);
		}

		public void TestTotalOverdueAmount()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 10000;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 20;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			Statement.CurrencyNK = currency.RX_Code;
			AssertEquals("Formatting for default country (Australia)", "10,000.00", statementWrapper.TotalOverdueAmount);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "10.000,00", statementWrapper.TotalOverdueAmount);
			}
		}

		public void TestTotalCurrentAmount()
		{
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 10;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 20000;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			Statement.CurrencyNK = currency.RX_Code;
			AssertEquals("Total current amount should match statement", "20,005.00", statementWrapper.TotalCurrentAmount);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "20.005,00", statementWrapper.TotalCurrentAmount);
			}
		}

		public void TestDisplayNotYetOutstandingAmountReturnCorrectValues()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, statementWrapper.DisplayNotYetOutstandingAmount);
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, statementWrapper.DisplayNotYetOutstandingAmount);
		}

		public void TestTotalCurrent()
		{
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			Statement.CurrencyNK = currency.RX_Code;
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			AssertEquals("Total current should be null if the TotalCurrentAmount is empty", string.Empty, statementWrapper.TotalCurrent);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 10;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 20000;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			AssertEquals("Total current amount should match statement", "Current at statement date: 20,005.00 " + statementWrapper.Currency.Code, statementWrapper.TotalCurrent);
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Total current amount should be null if the DisplayNotYetOutstandingAmountOnARStatementDocuments Registry item is not set", string.Empty, statementWrapper.TotalCurrent);
		}

		public void TestTotalOverdue()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			Statement.CurrencyNK = currency.RX_Code;
			AssertEquals("Total overdue amount should be null if the TotalOverdueAmount equal 0", string.Empty, statementWrapper.TotalOverdue);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 10000;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 20;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			AssertEquals("Total overdue amount should match statement", "Overdue at statement date: 10,000.00 " + statementWrapper.Currency.Code, statementWrapper.TotalOverdue);
		}

		public void TestStatementBalanceFormatted()
		{
			AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5.95;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 21595;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, ""));
			Statement.CurrencyNK = currency.RX_Code;
			AssertEquals(21600.95m, statementWrapper.StatementBalance);
			AssertEquals("21,600.95", statementWrapper.StatementBalanceFormatted);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "21.600,95", statementWrapper.StatementBalanceFormatted);
			}

			((RefCurrency)statementWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;
			AssertEquals("21,601", statementWrapper.StatementBalanceFormatted);
		}

		public void TestTotalDueAmount()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 10;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate;
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(1);
			receipt3.AH_OSTotal = 20;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			AssertEquals("Total due amount should match statement", "25.00", statementWrapper.TotalDueAmount);
		}

		public void TestTotalUnmatchedReceipts()
		{
			Statement.CurrencyNK = Creator.USD.RX_Code;
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			invoice1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			invoice1.AH_OSTotal = 80m;
			DocTransactionHeader invoice1Wrapper = DocTransactionHeader.New(invoice1, Factory);
			statementWrapper.Transactions.Add(invoice1Wrapper);

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			invoice2.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			invoice2.AH_OSTotal = 100m;
			DocTransactionHeader invoice2Wrapper = DocTransactionHeader.New(invoice2, Factory);
			statementWrapper.Transactions.Add(invoice2Wrapper);

			AssertEquals("Total of Receipts not Matched", ZString.Empty, statementWrapper.ReceiptsNotMatched);

			ARReceipt receipt1 = Factory.NewWithValidTestData<ARReceipt>();
			receipt1.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			receipt1.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt1.AH_OSTotal = 50000;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);

			AssertEquals("Total of Receipts not Matched", "Total Unallocated Receipts: 50,000.00 USD", statementWrapper.ReceiptsNotMatched);

			ARReceipt receipt2 = Factory.NewWithValidTestData<ARReceipt>();
			receipt2.AH_DueDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt2.AH_OSTotal = 70;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);

			AssertEquals("Total of Receipts not Matched", "Total Unallocated Receipts: 50,070.00 USD", statementWrapper.ReceiptsNotMatched);
		}

		public void TestStatementDisplayDate()
		{
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			Statement.StatementDisplayDate = ZDateTime.Empty;
			AssertEquals("Statement Display Date", Statement.StatementDisplayDate, statementWrapper.DisplayDate);

			Statement.StatementDisplayDate = Env.Time.CurrentLocalDate.AddDays(1);
			AssertEquals("Statement Display Date", Statement.StatementDisplayDate, statementWrapper.DisplayDate);

			Statement.StatementDisplayDate = Env.Time.CurrentLocalDate.AddDays(-1);
			AssertEquals("Statement Display Date", Statement.StatementDisplayDate, statementWrapper.DisplayDate);
		}

		public void TestTotalStatementAmount()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			var receipt1 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt1.AH_OSTotal = 10;
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);
			var receipt2 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt2.AH_OSTotal = 5;
			DocTransactionHeader receipt2Wrapper = DocTransactionHeader.New(receipt2, Factory);
			statementWrapper.Transactions.Add(receipt2Wrapper);
			var receipt3 = Factory.NewWithValidTestData<DirectReceipt>();
			receipt3.AH_OSTotal = 20;
			DocTransactionHeader receipt3Wrapper = DocTransactionHeader.New(receipt3, Factory);
			statementWrapper.Transactions.Add(receipt3Wrapper);
			AssertEquals("Total due amount should match statement", "35.00", statementWrapper.TotalStatementAmount);
		}

		public void TestStatementMailToAddress_SameCountry()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);

			var creator = new TestObjectCreator(Factory);
			DirectReceipt receipt1 = creator.CreateDirectReceipt(ZDateTime.Now, 5m, 0m, 5m, 0m);
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("Mail to Address Country", Core.Constants.CountryCodes.Australia, statementWrapper.Branch.MailToAddress.Country.Code);
			AssertEquals("Mail to Address on Statement (Same Country)", "EDI CUSTOMS BROKERS\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY\nAUSTRALIA", statementWrapper.MailToAddressWithCountry);
		}

		public void TestStatementMailToAddress_DifferentCountry()
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);

			var creator = new TestObjectCreator(Factory);
			DirectReceipt receipt1 = creator.CreateDirectReceipt(ZDateTime.Now, 5m, 0m, 5m, 0m);
			DocTransactionHeader receipt1Wrapper = DocTransactionHeader.New(receipt1, Factory);
			statementWrapper.Transactions.Add(receipt1Wrapper);

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "NZAKL";
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("Mail to Address Country", Core.Constants.CountryCodes.NewZealand, statementWrapper.Branch.MailToAddress.Country.Code);
			AssertEquals("Mail to Address on Statement (Different Country)", "EDI CUSTOMS BROKERS\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY\nNEW ZEALAND", statementWrapper.MailToAddressWithCountry);
		}

		public void TestDocumentTitle()
		{
			string statementDocumentTitle = "This is the document title for the Statement Letter";
			string firstReminderDocumentTitle = "This is the document title for the First Reminder Letter";
			string secondReminderDocumentTitle = "This is the document title for the Second Reminder Letter";
			string collectionLetterDocumentTitle = "This is the document title  for the Collection Letter";
			string demandLetterDocumentTitle = "This is the document title for the Demand Letter";

			AccountingMasterFilesRegistry.Instance.CollectionAndDemandFirstReminderDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, firstReminderDocumentTitle);
			AccountingMasterFilesRegistry.Instance.CollectionAndDemandSecondReminderDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, secondReminderDocumentTitle);
			AccountingMasterFilesRegistry.Instance.CollectionLetterDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionLetterDocumentTitle);
			AccountingMasterFilesRegistry.Instance.DemandLetterDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, demandLetterDocumentTitle);
			AccountingMasterFilesRegistry.Instance.StatementDocumentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statementDocumentTitle);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Document Title for Statement", statementDocumentTitle, statementWrapper.DocumentTitle);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Document Title for First Reminder", firstReminderDocumentTitle, statementWrapper.DocumentTitle);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.SecondReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Document Title for Second Reminder", secondReminderDocumentTitle, statementWrapper.DocumentTitle);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.CollectionLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Document Title for Collection Letter", collectionLetterDocumentTitle, statementWrapper.DocumentTitle);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.DemandLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Document Title for Demand Letter", demandLetterDocumentTitle, statementWrapper.DocumentTitle);
		}

		public void TestOpeningText()
		{
			string firstReminderOpeningText = "This is the opening text for the First Reminder Letter";
			string secondReminderOpeningText = "This is the opening text for the Second Reminder Letter";
			string collectionLetterOpeningText = "This is the opening text  for the Collection Letter";
			string demandLetterOpeningText = "This is the opening text for the Demand Letter";

			AccountingMasterFilesRegistry.Instance.CollectionAndDemandFirstReminderOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, firstReminderOpeningText);
			AccountingMasterFilesRegistry.Instance.CollectionAndDemandSecondReminderOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, secondReminderOpeningText);
			AccountingMasterFilesRegistry.Instance.CollectionLetterOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionLetterOpeningText);
			AccountingMasterFilesRegistry.Instance.DemandLetterOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, demandLetterOpeningText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("opening text for Statement", ZString.Empty, statementWrapper.OpeningText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("opening text for First Reminder", firstReminderOpeningText, statementWrapper.OpeningText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.SecondReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("opening text for Second Reminder", secondReminderOpeningText, statementWrapper.OpeningText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.CollectionLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("opening text for Collection Letter", collectionLetterOpeningText, statementWrapper.OpeningText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.DemandLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("opening text for Demand Letter", demandLetterOpeningText, statementWrapper.OpeningText);
		}

		public void TestPaymentRequestText()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Payment Request Text", "IMMEDIATE PAYMENT REQUESTED: ", statementWrapper.PaymentRequestText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Payment Request Text", "IMMEDIATE PAYMENT REQUESTED: ", statementWrapper.PaymentRequestText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.SecondReminder;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Payment Request Text", "IMMEDIATE PAYMENT REQUESTED: ", statementWrapper.PaymentRequestText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.CollectionLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Payment Request Text", "IMMEDIATE PAYMENT REQUIRED: ", statementWrapper.PaymentRequestText);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.DemandLetter;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertEquals("Payment Request Text", "IMMEDIATE PAYMENT REQUIRED: ", statementWrapper.PaymentRequestText);
		}

		public void TestStatementLogo()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			AssertEquals("Default logo", SystemDataRegistry.Instance.CompanyLogo.Value.Size, StatementWrapper.StatementLogo.Size);
			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(2, 2));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(2, 2), StatementWrapper.StatementLogo.Size);
			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Env.CurrentDepartment.PK, new Bitmap(5, 5));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(5, 5), StatementWrapper.StatementLogo.Size);
			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, new Bitmap(4, 4));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(4, 4), StatementWrapper.StatementLogo.Size);

			OrgHeader client = OrgHeader.New(Factory);
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var header = Factory.NewJobForTesting<JobHeader>();
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			ClientTariffAndLevelCollection collection = new ClientTariffAndLevelCollection();
			var element1 = collection.AddNew();
			element1.CodeList.AddPair("1", "Desc");
			element1.Code = "1";
			element1.Description = (NoResString)"Desc";
			element1.BrandName = "blah";
			element1.BrandEmailAddress = "blah@blah.com";
			element1.Image = new Bitmap(1, 1);
			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, client.PK.ToString());
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			StatementWrapper.SetTemplateConstants(constants);

			AssertEquals("Client logo", new Size(1, 1), StatementWrapper.StatementLogo.Size);
		}

		public void TestBalanceForInvoiceBatchHeaders()
		{
			AccTransactionHeader invoice1 = MakeAnInvoice("00001001");
			AccTransactionHeader invoice2 = MakeAnInvoice("00001002");
			AccTransactionHeader invoice3 = MakeAnInvoice("00001003");
			AccTransactionHeader invoice4 = MakeAnInvoice("00001004");
			Factory.Save();

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			Statement.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			StatementWrapper = DocStatement.New(Statement, Factory);

			AssertTransactionOutstandingAmount(invoice1, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(invoice2, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(invoice3, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(invoice4, StatementWrapper.Transactions, 60M);
			AssertEquals("Total Statement Amount", "240.00", StatementWrapper.TotalStatementAmount);

			InvoiceBatchHeader batchHeader1 = Factory.New<InvoiceBatchHeader>();
			InvoiceBatchHeader batchHeader2 = Factory.New<InvoiceBatchHeader>();
			batchHeader1.AH_OH = batchHeader2.AH_OH = Creator.ABIGAS.PK;

			invoice1.AH_AH_InvoiceStatement = ZGuid.Empty;
			invoice2.AH_AH_InvoiceStatement = batchHeader1.PK;
			invoice3.AH_AH_InvoiceStatement = batchHeader2.PK;
			invoice4.AH_AH_InvoiceStatement = batchHeader2.PK;
			Factory.Save();

			BusinessObjectFactory wrapperFactory = new BusinessObjectFactory();

			SetUpPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			StatementWrapper = DocStatement.New(Statement, wrapperFactory);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice1", invoice1, StatementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice2", invoice2, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice3", invoice3, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice4", invoice4, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("BatchHeader1", batchHeader1, StatementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("BatchHeader2", batchHeader2, StatementWrapper.Transactions, true);

			AssertTransactionOutstandingAmount(invoice1, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(batchHeader1, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(batchHeader2, StatementWrapper.Transactions, 120M);
			AssertEquals("Total Statement Amount", "240.00", StatementWrapper.TotalStatementAmount);
		}

		public void TestBalanceForInvoiceBatchHeadersWhereInvoicesHaveAlreadyBeenMatched()
		{
			AccTransactionHeader invoice1 = MakeAnInvoice("00001001");

			AccTransactionHeader invoice2 = MakeAnInvoice("00001002");
			SetUpMatchLinkForTransaction(invoice2, 60M, Env.Time.CurrentLocalDate.AddDays(-2)); // Fully Paid
			invoice2.AH_OutstandingAmount = 0m;
			invoice2.AH_GSTAmount = 0m;
			invoice2.AH_FullyPaidDate = Env.Time.CurrentLocalDate.AddDays(-2);

			AccTransactionHeader invoice3 = MakeAnInvoice("00001003");
			SetUpMatchLinkForTransaction(invoice3, 60M, Env.Time.CurrentLocalDate.AddDays(-2)); // Fully Paid
			invoice3.AH_OutstandingAmount = 0m;
			invoice3.AH_GSTAmount = 0m;
			invoice3.AH_FullyPaidDate = Env.Time.CurrentLocalDate.AddDays(-2);

			AccTransactionHeader invoice4 = MakeAnInvoice("00001004");

			Factory.Save();

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			Statement.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			StatementWrapper = DocStatement.New(Statement, Factory);

			AssertTransactionOutstandingAmount(invoice1, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(invoice4, StatementWrapper.Transactions, 60M);
			AssertEquals("Transactions.Count", 2, StatementWrapper.Transactions.Count);
			AssertEquals("Total Statement Amount", "120.00", StatementWrapper.TotalStatementAmount);

			InvoiceBatchHeader batchHeader1 = Factory.New<InvoiceBatchHeader>();
			InvoiceBatchHeader batchHeader2 = Factory.New<InvoiceBatchHeader>();
			batchHeader1.AH_OH = batchHeader2.AH_OH = Creator.ABIGAS.PK;

			batchHeader1.Line.Add(invoice2);
			batchHeader2.Line.Add(invoice3);
			batchHeader2.Line.Add(invoice4);

			batchHeader1.AH_OH = Creator.ABIGAS.PK;
			batchHeader1.AH_InvoiceAmount = invoice2.AH_InvoiceAmount + invoice2.AH_GSTAmount;
			batchHeader1.AH_OutstandingAmount = batchHeader1.AH_InvoiceAmount;

			batchHeader2.AH_OH = Creator.ABIGAS.PK;
			batchHeader2.AH_InvoiceAmount = invoice3.AH_InvoiceAmount + invoice3.AH_GSTAmount;
			batchHeader2.AH_InvoiceAmount += invoice4.AH_InvoiceAmount + invoice4.AH_GSTAmount;
			batchHeader2.AH_OutstandingAmount = batchHeader2.AH_InvoiceAmount;

			Factory.Save();

			BusinessObjectFactory wrapperFactory = new BusinessObjectFactory();

			SetUpPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			StatementWrapper = DocStatement.New(Statement, wrapperFactory);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice1", invoice1, StatementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice2", invoice2, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice3", invoice3, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice4", invoice4, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("BatchHeader1", batchHeader1, StatementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("BatchHeader2", batchHeader2, StatementWrapper.Transactions, true);

			AssertTransactionOutstandingAmount(invoice1, StatementWrapper.Transactions, 60M);
			AssertTransactionOutstandingAmount(batchHeader2, StatementWrapper.Transactions, 60M);
			AssertEquals("Transactions.Count", 2, StatementWrapper.Transactions.Count);
			AssertEquals("Total Statement Amount", "120.00", StatementWrapper.TotalStatementAmount);
		}

		[TestDate(2014, 11, 25)]
		public void TestStatementTotalOverdueAmount()
		{
			ZDateTime currentDate = Env.Time.CurrentLocalDate;
			ZDecimal toalAmount = 0;
			var statementWrapper = CreateStatementWrapperWithTransaction(currentDate.Date.AddDays(-1), false, 5, out toalAmount);
			AddTransactionHeader(currentDate.Date.AddDays(1), 25000m, statementWrapper);
			AddTransactionHeader(currentDate.Date, 15000m, statementWrapper);

			AssertEquals(50010.00m, toalAmount);

			AssertEquals("As Registry settings is off. No value should be displayed", string.Empty, statementWrapper.StatementTotalOverdueAmount);

			AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("5 Overdue Transaction, 2 Due Transaction", 7, statementWrapper.Transactions.Count);
			AssertEquals("As Registry settings is on. Value should be displayed which is 50,010.00 AUD", "50,010.00 AUD", statementWrapper.StatementTotalOverdueAmount);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "50.010,00 AUD", statementWrapper.StatementTotalOverdueAmount);
			}
		}

		[TestDate(2014, 11, 25)]
		public void TestStatementTotalDueAmount()
		{
			ZDateTime currentDate = Env.Time.CurrentLocalDate;
			ZDecimal toalAmount = 0;
			var statementWrapper = CreateStatementWrapperWithTransaction(currentDate.Date, true, 3, out toalAmount);
			AddTransactionHeader(currentDate.Date.AddDays(-2), 25000m, statementWrapper);
			AddTransactionHeader(currentDate.Date.AddDays(-1), 15000m, statementWrapper);

			AssertEquals(30003.00m, toalAmount);

			AssertEquals("As Registry settings is off. No value should be displayed", string.Empty, statementWrapper.StatementTotalOverdueAmount);

			AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("2 Overdue Transaction, 3 Due Transaction", 5, statementWrapper.Transactions.Count);
			AssertEquals("As Registry settings is on. Value should be displayed which is 30,003.00 AUD", "30,003.00 AUD", statementWrapper.StatementTotalDueAmount);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "30.003,00 AUD", statementWrapper.StatementTotalDueAmount);
			}
		}

		public void TestStatementTotalStatementAmount()
		{
			ZDateTime currentDate = Env.Time.CurrentLocalDate;
			ZDecimal toalAmount = 0;
			var statementWrapper = CreateStatementWrapperWithTransaction(DateTime.Today, true, 3, out toalAmount);
			AddTransactionHeader(currentDate.Date.AddDays(-2), 25000m, statementWrapper);
			AddTransactionHeader(currentDate.Date.AddDays(-1), 15000m, statementWrapper);

			AssertEquals(70003.00m, toalAmount + 40000m);

			AssertEquals("As Registry settings is off. No value should be displayed", string.Empty, statementWrapper.StatementTotalOverdueAmount);

			AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("2 Overdue Transaction, 3 Due Transaction", 5, statementWrapper.Transactions.Count);
			AssertEquals("As Registry settings is on. Value should be displayed which is 70,003.00 AUD", "70,003.00 AUD", statementWrapper.StatementTotalStatementAmount);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "70.003,00 AUD", statementWrapper.StatementTotalStatementAmount);
			}
		}

		[TestDate(2014, 11, 25)]
		public void TestOverdueBuckets()
		{
			ZDateTime currentDate = Env.Time.CurrentLocalDate;
			ZDecimal toal30DayOverDueAmount = 0;
			ZDecimal toal60DayOverDueAmount = 0;
			ZDecimal toal90DayOverDueAmount = 0;
			ZDecimal toal90PlusDayOverDueAmount = 0;

			//Create Wrapper
			var statementWrapper = CreateStatementWrapperWithTransaction(currentDate.Date, true, 0, out toal30DayOverDueAmount);

			//Create 1-30 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-1), false, 4, out toal30DayOverDueAmount);

			//Create 31-60 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-31), false, 3, out toal60DayOverDueAmount);

			//Create 61-90 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-61), false, 2, out toal90DayOverDueAmount);

			//Create 90+ days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-91), false, 1, out toal90PlusDayOverDueAmount);

			//Add few more
			AddTransactionHeader(currentDate, 25000m, statementWrapper);
			AddTransactionHeader(currentDate.AddDays(1), 15000m, statementWrapper);
			AddTransactionHeader(currentDate.AddDays(2), 5000m, statementWrapper);

			AssertEquals("10 Overdue Transaction, 3 Due Transaction", 13, statementWrapper.Transactions.Count);
			AssertEquals(145010.00m, toal30DayOverDueAmount + toal60DayOverDueAmount + toal90DayOverDueAmount + toal90PlusDayOverDueAmount + 45000m);

			AssertEquals("As Registry settings is off. No value should be displayed for Total30DaysOverdue", string.Empty, statementWrapper.Total30DaysOverdue);
			AssertEquals("As Registry settings is off. No value should be displayed for Total60DaysOverdue", string.Empty, statementWrapper.Total60DaysOverdue);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90DaysOverdue", string.Empty, statementWrapper.Total90DaysOverdue);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90PlusDaysOverdue", string.Empty, statementWrapper.Total90PlusDaysOverdue);

			AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("As Registry settings is on. Value should be displayed for Total30DaysOverdue which is 40,006.00 AUD", "40,006.00 AUD", statementWrapper.Total30DaysOverdue);
			AssertEquals("As Registry settings is on. Value should be displayed for Total60DaysOverdue which is 30,003.00 AUD", "30,003.00 AUD", statementWrapper.Total60DaysOverdue);
			AssertEquals("As Registry settings is on. Value should be displayed for Total90DaysOverdue which is 20,001.00 AUD", "20,001.00 AUD", statementWrapper.Total90DaysOverdue);
			AssertEquals("As Registry settings is on. Value should be displayed for Total90+DaysOverdue which is 10,000.00 AUD", "10,000.00 AUD", statementWrapper.Total90PlusDaysOverdue);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland for Total30DaysOverdue", "40.006,00 AUD", statementWrapper.Total30DaysOverdue);
				AssertEquals("Formatting for Iceland for Total60DaysOverdue", "30.003,00 AUD", statementWrapper.Total60DaysOverdue);
				AssertEquals("Formatting for Iceland for Total90DaysOverdue", "20.001,00 AUD", statementWrapper.Total90DaysOverdue);
				AssertEquals("Formatting for Iceland for Total90PlusDaysOverdue", "10.000,00 AUD", statementWrapper.Total90PlusDaysOverdue);
			}
		}

		public void TestOverDueBucketsVersion2()
		{
			ZDateTime currentDate = Env.Time.CurrentLocalDate;
			ZDecimal toalDue0To29 = 0;
			ZDecimal toalDue30To59 = 0;
			ZDecimal toalDue60To89 = 0;
			ZDecimal toalDue90To119 = 0;
			ZDecimal toalDue120To149 = 0;
			ZDecimal toalDue150Plus = 0;

			//Create Wrapper and Create 0-29 days overdue Transactions
			var statementWrapper = CreateStatementWrapperWithTransaction(currentDate.Date, false, 5, out toalDue0To29);

			//Create 30-59 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-30), false, 4, out toalDue30To59);

			//Create 60-89 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-60), false, 3, out toalDue60To89);

			//Create 90-119 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-90), false, 2, out toalDue90To119);

			//Create 120-149 days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-120), false, 1, out toalDue120To149);

			//Create 150+ days overdue Transactions
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, currentDate.Date.AddDays(-150), false, 1, out toalDue150Plus);

			//Add few more
			AddTransactionHeader(currentDate.AddDays(1), 15000m, statementWrapper);
			AddTransactionHeader(currentDate.AddDays(2), 5000m, statementWrapper);

			AssertEquals("16 Overdue Transaction, 2 Due Transaction", 18, statementWrapper.Transactions.Count);
			AssertEquals(180020.00m, toalDue0To29 + toalDue30To59 + toalDue60To89 + toalDue90To119 + toalDue120To149 + toalDue150Plus + 20000m);

			AssertEquals("As Registry settings is off. No value should be displayed for Total30DaysOverdue", string.Empty, statementWrapper.TotalDueBetween0To29thDay);
			AssertEquals("As Registry settings is off. No value should be displayed for Total60DaysOverdue", string.Empty, statementWrapper.TotalDueBetween30To59thDay);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90DaysOverdue", string.Empty, statementWrapper.TotalDueBetween60To89thDay);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90PlusDaysOverdue", string.Empty, statementWrapper.TotalDueBetween90To119thDay);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90PlusDaysOverdue", string.Empty, statementWrapper.TotalDueBetween120To149thDay);
			AssertEquals("As Registry settings is off. No value should be displayed for Total90PlusDaysOverdue", string.Empty, statementWrapper.TotalDue150PlusDay);

			AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("As Registry settings is on. Value should be displayed for TotalDueBetween0To29thDay which is 50,010.00 AUD", "50,010.00 AUD", statementWrapper.TotalDueBetween0To29thDay);
			AssertEquals("As Registry settings is on. Value should be displayed for TotalDueBetween30To59thDay which is 40,006.00 AUD", "40,006.00 AUD", statementWrapper.TotalDueBetween30To59thDay);
			AssertEquals("As Registry settings is on. Value should be displayed for TotalDueBetween60To89thDay which is 30,003.00 AUD", "30,003.00 AUD", statementWrapper.TotalDueBetween60To89thDay);
			AssertEquals("As Registry settings is on. Value should be displayed for TotalDueBetween90To119thDay which is 20,001.00 AUD", "20,001.00 AUD", statementWrapper.TotalDueBetween90To119thDay);
			AssertEquals("As Registry settings is on. Value should be displayed for TotalDueBetween120To149thDay which is 10,000.00 AUD", "10,000.00 AUD", statementWrapper.TotalDueBetween120To149thDay);
			AssertEquals("As Registry settings is on. Value should be displayed for TotalDue150PlusDay which is 10,000.00 AUD", "10,000.00 AUD", statementWrapper.TotalDue150PlusDay);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland for TotalDueBetween0To29thDay", "50.010,00 AUD", statementWrapper.TotalDueBetween0To29thDay);
				AssertEquals("Formatting for Iceland for TotalDueBetween30To59thDay", "40.006,00 AUD", statementWrapper.TotalDueBetween30To59thDay);
				AssertEquals("Formatting for Iceland for TotalDueBetween60To89thDay", "30.003,00 AUD", statementWrapper.TotalDueBetween60To89thDay);
				AssertEquals("Formatting for Iceland for TotalDueBetween90To119thDay", "20.001,00 AUD", statementWrapper.TotalDueBetween90To119thDay);
				AssertEquals("Formatting for Iceland for TotalDueBetween120To149thDay", "10.000,00 AUD", statementWrapper.TotalDueBetween120To149thDay);
				AssertEquals("Formatting for Iceland for TotalDue150PlusDay", "10.000,00 AUD", statementWrapper.TotalDue150PlusDay);
			}
		}

		DocStatementWithTransactionsExposed CreateStatementWrapperWithTransaction(ZDateTime baseDate, bool afterBasedate, int noOfTransaction, out ZDecimal totalAmount)
		{
			DocStatementWithTransactionsExposed statementWrapper = DocStatementWithTransactionsExposed.New(Statement, Factory);
			totalAmount = 0m;
			AddMultipleTransactionHeaderToStatementWrapper(statementWrapper, baseDate, afterBasedate, noOfTransaction, out totalAmount);
			Statement.CurrencyNK = Creator.AUD.RX_Code;
			return statementWrapper;
		}

		void AddMultipleTransactionHeaderToStatementWrapper(DocStatementWithTransactionsExposed statementWrapper, ZDateTime baseDate, bool afterBasedate, int noOfTransaction, out ZDecimal totalAmount)
		{
			totalAmount = 0m;

			if (afterBasedate)
			{
				for (int i = 0; i < noOfTransaction; i++)
				{
					AddTransactionHeader(baseDate.AddDays(i), 10000.00m + i, statementWrapper);
					totalAmount += (10000.00m + i);
				}
			}
			else
			{
				for (int i = 0; i < noOfTransaction; i++)
				{
					AddTransactionHeader(baseDate.AddDays((-1) * i), 10000.00m + i, statementWrapper);
					totalAmount += (10000.00m + i);
				}
			}
		}

		void AddTransactionHeader(ZDateTime dueDate, ZDecimal oSTotal, DocStatementWithTransactionsExposed statementWrapper)
		{
			var receipt = Factory.NewWithValidTestData<DirectReceipt>();
			receipt.AH_InvoiceDate = dueDate;
			receipt.AH_DueDate = dueDate;
			receipt.AH_OSTotal = oSTotal;
			var receiptWrapper = DocTransactionHeader.New(receipt, Factory);
			statementWrapper.Transactions.Add(receiptWrapper);
		}

		AccTransactionHeader MakeAnInvoice(ZString transactionNum)
		{
			AccTransactionHeader result = CreateInvoice(Creator.ABIGAS);
			SetUpMatchLinkForTransaction(result, 20M, Env.Time.CurrentLocalDate.AddDays(-1));
			SetUpMatchLinkForTransaction(result, 20M, Env.Time.CurrentLocalDate.AddDays(-2));
			result.AH_GSTAmount = 40M;
			result.AH_TransactionNum = transactionNum;
			return result;
		}

		#region DocTransactionHeader Tests

		public void TestDocTransactionsHaveCalculatedMatchedAmountPopulatedCorrectly_CurrentPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CurrencyNK = Creator.AUD.RX_Code;
			Invoice aRInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);

			aRInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInvoice.AH_PostDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_InvoiceDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_DueDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_OH = Creator.ABIGAS.PK;
			Creator.CreateInvoiceLine(aRInvoice, Creator.AUD, 1.0M, 100);
			aRInvoice.AH_RX_NKTransactionCurrency = Creator.AUD.RX_Code;

			AccTransactionMatchLink matchLinkForARInvoice = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
			matchLinkForARInvoice.AP_AH = aRInvoice.PK;
			matchLinkForARInvoice.AP_Amount = 40M;
			matchLinkForARInvoice.AP_MatchDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_OutstandingAmount = 70M;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = -40M;

			AccTransactionMatchLink linkToMatch = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = header.PK;
			linkToMatch.AP_Amount = -40M;
			linkToMatch.AP_MatchDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);

			Factory.Save();

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			DocTransactionHeader transactionHeaderWrapper = statementWrapper.Transactions[0];
			AssertEquals("TransactionWrapper.CalculatedMatchedAmount", 40M, transactionHeaderWrapper.CalculatedMatchedAmountForStatement);
		}

		public void TestDocTransactionsHaveCalculatedMatchedAmountPopulatedCorrectly_PastPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CurrencyNK = Creator.AUD.RX_Code;
			Invoice aRInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);

			aRInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInvoice.AH_PostDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_InvoiceDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_DueDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_OH = Creator.ABIGAS.PK;
			Creator.CreateInvoiceLine(aRInvoice, Creator.AUD, 1.0M, 100M);
			aRInvoice.AH_RX_NKTransactionCurrency = Creator.AUD.RX_Code;

			AccTransactionMatchLink firstMatchLinkForARInvoice = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
			firstMatchLinkForARInvoice.AP_AH = aRInvoice.PK;
			firstMatchLinkForARInvoice.AP_Amount = 40M;
			firstMatchLinkForARInvoice.AP_MatchDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(1);

			AccTransactionMatchLink secondMatchLinkForARInvoice = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
			secondMatchLinkForARInvoice.AP_AH = aRInvoice.PK;
			secondMatchLinkForARInvoice.AP_Amount = 50M;
			secondMatchLinkForARInvoice.AP_MatchDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(1);
			aRInvoice.AH_OutstandingAmount = 20M;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = -90M;

			AccTransactionMatchLink linkToMatch = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = header.PK;
			linkToMatch.AP_Amount = -90M;

			TestObjectCreator.SetupMatchLinkMatchDate(aRInvoice);

			Factory.Save();

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			Statement.EndOfPeriod = testHelper.PreviousGLClosedPeriod.AM_EndDate.Date.AddDays(1);
			DocStatement statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals("DocTransactionHeader Collection Count", 1, statementWrapper.Transactions.Count);
			DocTransactionHeader transactionHeaderWrapper = statementWrapper.Transactions[0];
			AssertEquals("TransactionWrapper.CalculatedMatchedAmount", 40M, transactionHeaderWrapper.CalculatedMatchedAmountForStatement);

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			Statement.EndOfPeriod = testHelper.CurrentPeriod.AM_EndDate.Date.AddDays(1);
			statementWrapper = DocStatement.New(Statement, Factory);

			AssertEquals("DocTransactionHeader Collection Count", 1, statementWrapper.Transactions.Count);
			transactionHeaderWrapper = statementWrapper.Transactions[0];
			AssertEquals("TransactionWrapper.CalculatedMatchedAmount", 90M, transactionHeaderWrapper.CalculatedMatchedAmountForStatement);
		}

		[TestDate(2016, 10, 15)]
		public void TestDocTransactionsHaveCalculatedMatchedAmountCorrectlyWithCuttOffPeriod_WhenTransactionIsMatchedAndPaidOnFirstDayOfNextPeriod()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CurrencyNK = Creator.AUD.RX_Code;
			Statement.OrganisationPK = Creator.ABIGAS.PK;

			Creator.CreateTestPeriodsForEntireYear(ZDateTime.Now.Year);
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var firstPeriod = periodCalculator.GetFirstPeriodForYear(ZDateTime.Now.Year);
			var secondPeriod = periodCalculator.GetNextPeriod(firstPeriod);

			var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV1", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
			invoice.AH_PostDate = periodCalculator.GetFirstDayForPeriod(firstPeriod).AddDays(1);
			invoice.AH_OH = Creator.ABIGAS.PK;
			//Fully pay invoice on the first day of second period
			var matchLink = Creator.CreateMatchLinkToPayARInvoice(invoice, periodCalculator.GetFirstDayForPeriod(secondPeriod));

			Factory.Save();

			//Generate Statement with first period as cut off period
			Statement.EndOfPeriod = periodCalculator.GetLastDayForPeriod(firstPeriod).Date.AddDays(1);
			AssertEquals("Pre-condition", Statement.EndOfPeriod, matchLink.AP_MatchDate);
			AssertMatchAmountAndBalance(Statement, 0m, 100m, 0m, -100m);

			//Generate Statement with second period as cut off period
			Statement.EndOfPeriod = periodCalculator.GetLastDayForPeriod(secondPeriod).Date.AddDays(1);
			AssertNotEquals("Pre-condition", Statement.EndOfPeriod, matchLink.AP_MatchDate);
			AssertMatchAmountAndBalance(Statement, 100m, 0m, -100m, 0m);
		}

		void AssertMatchAmountAndBalance(PrintStatement statement, ZDecimal expectedInvoiceMatchAmount, ZDecimal expectedInvoiceBalance, ZDecimal expectedReceiptMatchAmount, ZDecimal expectedReceiptBalance)
		{
			var statementWrapper = DocStatement.New(statement, Factory);
			AssertEquals("DocTransactionHeader Collection Count", 2, statementWrapper.Transactions.Count);
			var transactionHeaderWrapperForInvoice = statementWrapper.Transactions.Cast<DocTransactionHeader>().First(x => x.TransactionType == TransactionTypes.Invoice);
			AssertEquals("Invoice CalculatedMatchedAmount", expectedInvoiceMatchAmount, transactionHeaderWrapperForInvoice.CalculatedMatchedAmountForStatement);
			AssertEquals("Invoice Balance", expectedInvoiceBalance, transactionHeaderWrapperForInvoice.Balance);
			var transactionHeaderWrapperForReceipt = statementWrapper.Transactions.Cast<DocTransactionHeader>().First(x => x.TransactionType == TransactionTypes.Receipt);
			AssertEquals("Receipt CalculatedMatchedAmount", expectedReceiptMatchAmount, transactionHeaderWrapperForReceipt.CalculatedMatchedAmountForStatement);
			AssertEquals("Receipt Balance", expectedReceiptBalance, transactionHeaderWrapperForReceipt.Balance);
		}

		public void TestBizObjCollection_SettlementGroupHasOtherOrgsReferencing()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.ZECTRA.CompanyData.OB_IsDebtor = true;

			Creator.AALSHI.ARSettlementGroupPK = Creator.ABIGAS.PK;
			Creator.ZECTRA.ARSettlementGroupPK = Creator.ABIGAS.PK;

			AccTransactionHeader invoice1 = CreateInvoice(Creator.ABIGAS);
			AccTransactionHeader invoice2 = CreateInvoice(Creator.AALSHI);
			AccTransactionHeader invoice3 = CreateInvoice(Creator.ZECTRA);

			SetUpMatchLinkForTransaction(invoice1, 10, Env.Time.CurrentLocalDate.AddDays(-2));
			SetUpMatchLinkForTransaction(invoice2, 10, Env.Time.CurrentLocalDate.AddDays(-2));
			SetUpMatchLinkForTransaction(invoice3, 10, Env.Time.CurrentLocalDate.AddDays(-2));

			invoice1.AH_GSTAmount = 10M;
			invoice2.AH_GSTAmount = 10M;
			invoice3.AH_GSTAmount = 10M;

			Factory.Save();

			SetUpPrintStatement(Creator.AALSHI.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Statement.EndOfPeriod = ZDateTime.Now.AddDays(1);

			DocStatement statementWrapper = DocStatement.New(Statement, Factory);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice1", invoice1, statementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice2", invoice2, statementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice3", invoice3, statementWrapper.Transactions, false);
			AssertTransactionOutstandingAmount(invoice2, statementWrapper.Transactions, 90M);

			SetUpPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Statement.EndOfPeriod = ZDateTime.Now.AddDays(1);
			Statement.IssueBySettlementGroup = false;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice1", invoice1, statementWrapper.Transactions, true);
			AssertTransactionOutstandingAmount(invoice1, statementWrapper.Transactions, 90M);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice2", invoice2, statementWrapper.Transactions, false);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice3", invoice3, statementWrapper.Transactions, false);

			SetUpPrintStatement(Creator.ABIGAS.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Statement.EndOfPeriod = ZDateTime.Now.AddDays(1);
			Statement.IssueBySettlementGroup = true;
			statementWrapper = DocStatement.New(Statement, Factory);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice1", invoice1, statementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice2", invoice2, statementWrapper.Transactions, true);
			AssertTransactionHeaderCollectionContainsTransaction("Invoice3", invoice3, statementWrapper.Transactions, true);

			AssertTransactionOutstandingAmount(invoice1, statementWrapper.Transactions, 90M);
			AssertTransactionOutstandingAmount(invoice2, statementWrapper.Transactions, 90M);
			AssertTransactionOutstandingAmount(invoice3, statementWrapper.Transactions, 90M);
		}

		public void TestTransactionLinesCollectionNotLoaded()
		{
			AccTransactionHeader invoice1 = MakeAnInvoice("00001001");
			AccTransactionHeader invoice2 = MakeAnInvoice("00001002");
			AccTransactionHeader invoice3 = MakeAnInvoice("00001003");
			AccTransactionHeader invoice4 = MakeAnInvoice("00001004");

			Statement.OrganisationPK = Creator.ABIGAS.PK;
			Statement.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			InvoiceBatchHeader batchHeader1 = Factory.New<InvoiceBatchHeader>();
			InvoiceBatchHeader batchHeader2 = Factory.New<InvoiceBatchHeader>();

			invoice1.AH_AH_InvoiceStatement = ZGuid.Empty;
			invoice2.AH_AH_InvoiceStatement = batchHeader1.PK;
			invoice3.AH_AH_InvoiceStatement = batchHeader2.PK;
			invoice4.AH_AH_InvoiceStatement = batchHeader2.PK;
			Factory.Save();

			BusinessObjectFactory wrapperFactory = new BusinessObjectFactory();
			StatementWrapper = DocStatement.New(Statement, wrapperFactory);
			int count = StatementWrapper.Transactions.Count; // to make getter fetch something

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertMaxDbHits(6, wrapperFactory);
		}

		AccTransactionHeader CreateInvoice(OrgHeader orgToSetup)
		{
			orgToSetup.CompanyData.OB_IsDebtor = true;
			orgToSetup.CompanyData.SetARTaxApplicable(false);

			Invoice invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1.0M);
			Creator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0M, 100M);
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_OH = orgToSetup.PK;
			return invoice;
		}

		AccTransactionMatchLink SetUpMatchLinkForTransaction(AccTransactionHeader header1, ZDecimal matchedAmount, ZDateTime matchedDate)
		{
			AccTransactionMatchLink matchLink = ((IMatching)header1).CurrentMatchGroup.AddNew();

			matchLink.AP_AH = header1.PK;
			matchLink.AP_Amount = matchedAmount;
			matchLink.AP_MatchDate = matchedDate;
			matchLink.AP_MatchGroupNum = "M000010001";

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = -matchedAmount;

			AccTransactionMatchLink link = ((IMatching)header1).CurrentMatchGroup.AddNew();
			link.AP_AH = header.PK;
			link.AP_Amount = -matchedAmount;
			link.AP_MatchDate = matchedDate;
			link.AP_MatchGroupNum = "M000010001";

			return matchLink;
		}

		void AssertTransactionHeaderCollectionContainsTransaction(ZString transactionToFindAlias, AccTransactionHeader transactionToFind, DocTransactionHeaderCollection collection, bool expectedResult)
		{
			bool found = false;

			foreach (DocTransactionHeader transactionDocWrapper in collection)
			{
				AccTransactionHeader currentTransaction = (AccTransactionHeader)transactionDocWrapper.WrappedObject;

				if (currentTransaction.PK == transactionToFind.PK)
				{
					found = true;
					break;
				}
			}

			AssertEquals("Transaction " + transactionToFindAlias + " in DocTransactionHeaderCollection", expectedResult, found);
		}

		void AssertTransactionOutstandingAmount(AccTransactionHeader transactionToFind, DocTransactionHeaderCollection collection, ZDecimal outstandingAmount)
		{
			bool found = false;

			foreach (DocTransactionHeader transactionDocWrapper in collection)
			{
				AccTransactionHeader currentTransaction = (AccTransactionHeader)transactionDocWrapper.WrappedObject;

				if (currentTransaction.PK == transactionToFind.PK)
				{
					found = true;
					AssertEquals("OutstandingAmount for Invoice " + currentTransaction.AH_TransactionNum, outstandingAmount, transactionDocWrapper.Balance);
				}
			}

			AssertEquals("Transaction " + transactionToFind.AH_TransactionNum + " in DocTransactionHeaderCollection", true, found);
		}

		#endregion

		class DocStatementWithTransactionsExposed : DocStatement
		{
			public DocStatementWithTransactionsExposed(PrintStatement printStatement, BusinessObjectFactory factoryToWrap)
				: base(printStatement, factoryToWrap)
			{
			}

			public static new DocStatementWithTransactionsExposed New(PrintStatement printStatement, BusinessObjectFactory factoryToWrap)
			{
				return (printStatement == null) ? null : new DocStatementWithTransactionsExposed(printStatement, factoryToWrap);
			}

			public void ResetfTransactionsForTest()
			{
				fTransactions = null;
			}

			public override DocTransactionHeaderCollection Transactions
			{
				get
				{
					if (fTransactions == null)
					{
						fTransactions = new DocTransactionHeaderCollection(Factory);
					}
					return fTransactions;
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			SetUpPrintStatement(ZGuid.Empty, ZString.Empty);
			StatementWrapper = DocStatement.New(Statement, Factory);
			base.SetUp();
			CurrentCurrencySymbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
		}
		string CurrentCurrencySymbol;

		protected override void TearDown()
		{
			AssertEquals("Currency symbol has changed during test and not been reset", CurrentCurrencySymbol, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol);
			base.TearDown();
		}

		PrintStatement GetAndSetUpPrintStatement(ZGuid organisationPK, ZString currencyNK, BusinessObjectFactory specificFactory)
		{
			PrintStatement newStatement = new PrintStatement(specificFactory, GlbBranch.CurrentBranch);
			newStatement.CutOffDate = Env.Time.CurrentLocalDate.AddDays(1);
			if (!organisationPK.IsEmpty)
			{
				newStatement.OrganisationPK = organisationPK;
			}

			if (!currencyNK.IsEmpty)
			{
				newStatement.CurrencyNK = currencyNK;
			}

			return newStatement;
		}

		void SetUpPrintStatement(ZGuid organisationPK, ZString currencyNK)
		{
			Statement = GetAndSetUpPrintStatement(organisationPK, currencyNK, Factory);
		}

		PrintStatement Statement;
		DocStatement StatementWrapper;

		TestObjectCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new TestObjectCreator(Factory);
				}

				return fCreator;
			}
		}

		TestObjectCreator fCreator;

		#endregion

	}
}
