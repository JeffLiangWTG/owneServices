using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocStatementSummaryTest : TestCaseWithFactory
	{
		public void TestDocumentTitle()
		{
			AssertEquals("STATEMENT OF ACCOUNT SUMMARY", docSummary.DocumentTitle);
		}

		public void TestTaxId()
		{
			AssertEquals("TAX #: 41 065 894 724", docSummary.TaxId);
		}

		public void TestAROrgAddressPK()
		{
			DocOrganisation docOrg = DocOrganisation.New(organisation, Factory);
			AssertEquals(docOrg.ARAddress.OrgAddress.PK, docSummary.AROrgAddressPK);
		}

		public void TestOrganisationCode()
		{
			AssertEquals(organisation.OH_Code, docSummary.OrganisationCode);
		}

		public void TestOrganisationName()
		{
			AssertEquals(organisation.OH_FullName, docSummary.OrganisationName);
		}

		public void TestSTDTerms()
		{
			AssertEquals("Cash on Delivery", docSummary.STDTerms);
		}

		public void TestDSBTerms()
		{
			AssertEquals("Cash on Delivery", docSummary.DSBTerms);
		}

		public void TestDisplayDate()
		{
			statement.StatementDisplayDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, docSummary.DisplayDate);
		}

		public void TestShouldHideCompanyName()
		{
			statement.IssueBySettlementGroup = false;
			statement.IssueByTransactionBranch = true;
			statement.IssueByTransactionDepartment = true;
			Assert(docSummary.ShouldHideCompanyName);

			statement.IssueBySettlementGroup = true;
			statement.IssueByTransactionBranch = false;
			statement.IssueByTransactionDepartment = false;
			Assert(docSummary.ShouldHideCompanyName);

			statement.IssueBySettlementGroup = true;
			statement.IssueByTransactionBranch = true;
			statement.IssueByTransactionDepartment = false;
			Assert(!docSummary.ShouldHideCompanyName);

			statement.IssueBySettlementGroup = true;
			statement.IssueByTransactionBranch = false;
			statement.IssueByTransactionDepartment = true;
			Assert(!docSummary.ShouldHideCompanyName);
		}

		public void TestLines()
		{
			PrintStatement statement2 = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			statement2.CurrencyNK = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD")).RX_Code;
			printStatements.Add(statement2.PK.ToString(), statement2);

			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.OH_Code = "CRSTYCRB";
			organisation2.OH_FullName = "Crasty Crab";

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = organisation.PK;
			invoice1.AH_InvoiceAmount = 500m;
			invoice1.AH_GSTAmount = 50m;
			invoice1.AH_OSTotal = 550m;
			invoice1.AH_OutstandingAmount = 550m;
			invoice1.AH_DueDate = ZDateTime.BrettsBirthday;

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = organisation2.PK;
			invoice2.AH_InvoiceAmount = 1500m;
			invoice2.AH_GSTAmount = 150m;
			invoice2.AH_OSTotal = 1650m;
			invoice2.AH_OutstandingAmount = 1650m;
			invoice2.AH_DueDate = ZDateTime.Today;

			ARInvoice invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_OH = organisation.PK;
			invoice3.AH_InvoiceAmount = 2500m;
			invoice3.AH_GSTAmount = 250m;
			invoice3.AH_OSTotal = 2750m;
			invoice3.AH_OutstandingAmount = 2750m;
			invoice3.AH_DueDate = ZDateTime.BrettsBirthday;

			ARInvoice invoice4 = Factory.NewWithValidTestData<ARInvoice>();
			invoice4.AH_OH = organisation2.PK;
			invoice4.AH_InvoiceAmount = 3500m;
			invoice4.AH_GSTAmount = 350m;
			invoice4.AH_OSTotal = 3850m;
			invoice4.AH_OutstandingAmount = 3850m;
			invoice4.AH_DueDate = ZDateTime.BrettsBirthday;

			ARInvoice invoice5 = Factory.NewWithValidTestData<ARInvoice>();
			invoice5.AH_OH = organisation2.PK;
			invoice5.AH_InvoiceAmount = 4500m;
			invoice5.AH_GSTAmount = 450m;
			invoice5.AH_OSTotal = 4950m;
			invoice5.AH_OutstandingAmount = 4950m;
			invoice5.AH_DueDate = ZDateTime.BrettsBirthday;

			statement.Transactions.Add(invoice1);
			statement.Transactions.Add(invoice2);
			statement.Transactions.Add(invoice3);
			statement.Transactions.Add(invoice4);
			statement2.Transactions.Add(invoice5);

			docSummary.Lines.Sort("CurrencyCode");
			AssertEquals(3, docSummary.Lines.Count);
			AssertEquals("Crasty Crab", docSummary.Lines[0].DescriptionOne);
			AssertEquals("Planet Express", docSummary.Lines[1].DescriptionOne);
			AssertEquals("Crasty Crab", docSummary.Lines[2].DescriptionOne);
			AssertEquals(2, docSummary.Lines[0].Count);
			AssertEquals(2, docSummary.Lines[1].Count);
			AssertEquals(1, docSummary.Lines[2].Count);
			AssertEquals(5500m, docSummary.Lines[0].Amount);
			AssertEquals(3300m, docSummary.Lines[1].Amount);
			AssertEquals(4950m, docSummary.Lines[2].Amount);
			AssertEquals(3850m, docSummary.Lines[0].Overdue);
			AssertEquals(3300m, docSummary.Lines[1].Overdue);
			AssertEquals(4950m, docSummary.Lines[2].Overdue);
			AssertEquals(7150m, docSummary.Lines[0].TotalOverdueByCurrency);
			AssertEquals(7150m, docSummary.Lines[1].TotalOverdueByCurrency);
			AssertEquals(4950m, docSummary.Lines[2].TotalOverdueByCurrency);
		}

		protected override void SetUp()
		{
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "PLNTEXPRSS";
			organisation.OH_FullName = "Planet Express";
			statement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			statement.CurrencyNK = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			statement.OrganisationPK = organisation.PK;
			printStatements = new Dictionary<string, PrintStatement>();
			printStatements.Add(statement.PK.ToString(), statement);
			summary = new PrintSummary(Factory, printStatements);
			docSummary = DocStatementSummary.New(summary, Factory);
			base.SetUp();
		}

		DocStatementSummary docSummary;
		PrintSummary summary;
		PrintStatement statement;
		OrgHeader organisation;
		Dictionary<string, PrintStatement> printStatements;
	}
}
