using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocStatementSummaryLine))]
	sealed class DocStatementSummaryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTotalOverdueByCurrencyFormatted()
		{
			line.TotalOverdueByCurrency = 0m;
			AssertEquals(ZString.Empty, line.TotalOverdueByCurrencyFormatted);

			line.TotalOverdueByCurrency = 53100.44m;
			AssertEquals("Overdue at statement date: 53,100.44 ERN", line.TotalOverdueByCurrencyFormatted);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "Overdue at statement date: 53.100,44 ERN", line.TotalOverdueByCurrencyFormatted);
			}
		}

		public void TestAddValuesFromTransaction()
		{
			ARInvoice header1 = Factory.NewWithValidTestData<ARInvoice>();
			header1.AH_InvoiceAmount = 500m;
			header1.AH_GSTAmount = 50m;
			header1.AH_OSTotal = 550m;
			header1.AH_OutstandingAmount = 550m;
			header1.AH_DueDate = ZDateTime.BrettsBirthday;
			DocTransactionHeader docHeader1 = DocTransactionHeader.New(header1, Factory);

			ARInvoice header2 = Factory.NewWithValidTestData<ARInvoice>();
			header2.AH_InvoiceAmount = 500m;
			header2.AH_GSTAmount = 50m;
			header2.AH_OSTotal = 550m;
			header2.AH_OutstandingAmount = 550m;
			header2.AH_DueDate = ZDateTime.Today;
			DocTransactionHeader docHeader2 = DocTransactionHeader.New(header2, Factory);

			line.AddValuesFromTransaction(docHeader1);
			AssertEquals(2, line.Count);
			AssertEquals(550m, line.Amount);
			AssertEquals(550m, line.Overdue);

			line.AddValuesFromTransaction(docHeader2);
			AssertEquals(3, line.Count);
			AssertEquals(1100m, line.Amount);
			AssertEquals(550m, line.Overdue);
		}

		public void TestCurrencyCode()
		{
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.RX_Code, line.CurrencyCode);
		}

		public void TestCurrencyName()
		{
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.RX_Desc, line.CurrencyName);
		}

		public void TestOrganisationCode()
		{
			AssertEquals("PLNTEXPRSS", line.OrganisationCode);
		}

		public void TestOrganisationName()
		{
			AssertEquals("Planet Express", line.OrganisationName);
		}

		public void TestDescriptionOne()
		{
			statement.IssueByTransactionBranch = true;
			AssertEquals(docHeader.Branch.BranchName, line.DescriptionOne);

			statement.IssueByTransactionBranch = false;
			statement.IssueByTransactionDepartment = true;
			AssertEquals(docHeader.Department.Desc, line.DescriptionOne);

			statement.IssueByTransactionDepartment = false;
			AssertEquals(docHeader.Organisation.Name, line.DescriptionOne);
		}

		public void TestDescriptionTwo()
		{
			AssertEquals(ZString.Empty, line.DescriptionTwo);

			statement.IssueByTransactionBranch = true;
			statement.IssueByTransactionDepartment = false;
			AssertEquals(ZString.Empty, line.DescriptionTwo);

			statement.IssueByTransactionDepartment = true;
			AssertEquals(docHeader.Department.Desc, line.DescriptionTwo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "PLNTEXPRSS";
			organisation.OH_FullName = "Planet Express";

			header = Factory.NewWithValidTestData<ARInvoice>();
			header.AH_OH = organisation.PK;

			statement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			statement.CurrencyNK = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

			docHeader = DocTransactionHeader.New(header, Factory);
			docStatement = DocStatement.New(statement, Factory);

			line = new DocStatementSummaryLine(docStatement, docHeader);
		}

		DocStatementSummaryLine line;
		OrgHeader organisation;
		ARInvoice header;
		DocTransactionHeader docHeader;
		PrintStatement statement;
		DocStatement docStatement;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocStatementSummaryLine(DocStatement.New(new PrintStatement(Factory, GlbBranch.CurrentBranch), Factory), DocTransactionHeader.New(Factory.NewWithValidTestData<ARInvoice>(), Factory));
		}

		#endregion
	}
}
