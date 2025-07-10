using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccountingJournalLineForReportingBook))]
	sealed class DocAccountingJournalLineForReportingBookTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFieldValues()
		{
			var lineForReportingBook = GetNewBusinessObject() as DocAccountingJournalLineForReportingBook;
			AssertEquals("desc", lineForReportingBook.Description);
			AssertEquals("DEM", lineForReportingBook.Branch.Code);
			AssertEquals("CNY", lineForReportingBook.Currency.Code);
			AssertEquals("BRN", lineForReportingBook.Department.Code);
			AssertEquals("", lineForReportingBook.MultiSubAccountTypeCode);
			AssertEquals("RRR", lineForReportingBook.RevRecognitionType);
			AssertEquals("Accrual", lineForReportingBook.TaxBasis);
			AssertEquals("ERN", lineForReportingBook.TransactionHeaderCurrency);
			AssertEquals("202308", lineForReportingBook.PostPeriod);
			AssertEquals("1.50 DR", lineForReportingBook.ForeignCurrencyEquivalent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var glJournal = objectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			var line = objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, objectCreator.GLHeader1.PK);
			line.AL_RevRecognitionType = "RRR";
			objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, objectCreator.GLHeader2.PK);
			Factory.Save();
			var table = CreateGeneralLedgerTransactionData();
			PopulateRow(table, glJournal.PK.ToGuid(), line.PK.ToGuid());

			var accountingJournal = new GLAccountingJournal(glJournal, Factory.GetCachedReadOnlyFactory(), CreateReportingBook(), table);

			return DocAccountingJournalLineForReportingBook.New(accountingJournal.Lines.First(), Factory);
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_Code = "TRR";
			reportingBook.ARB_Description = "Description";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			Factory.Save();
			return reportingBook;
		}

		DataTable CreateGeneralLedgerTransactionData()
		{
			var dataTable = new DataTable("GeneralLedgerTransactionData");
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add("AlternateGLAccountDescription", typeof(string));
			dataTable.Columns.Add("AlternateGLAccount", typeof(string));
			dataTable.Columns.Add("GLAccountNum", typeof(string));
			dataTable.Columns.Add("GLAccountType", typeof(string));
			dataTable.Columns.Add("GLType", typeof(string));
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("BranchCode", typeof(string));
			dataTable.Columns.Add("DepartmentCode", typeof(string));
			dataTable.Columns.Add("GLAmountLocalDebit", typeof(decimal));
			dataTable.Columns.Add("GLAmountOSDebit", typeof(decimal));
			dataTable.Columns.Add("GLAmountLocalCredit", typeof(decimal));
			dataTable.Columns.Add("GLAmountOSCredit", typeof(decimal));
			dataTable.Columns.Add("GLAmountLocalBalance", typeof(decimal));
			dataTable.Columns.Add("PostDate", typeof(DateTime));
			dataTable.Columns.Add("Currency", typeof(string));
			dataTable.Columns.Add("PostPeriod", typeof(int));
			dataTable.Columns.Add("TaxGLMovementKey", typeof(int));
			dataTable.Columns.Add("OriginalAttribute_ORG", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_OCG", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_LFE", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_LFO", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_TIC", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_SPR", typeof(string));
			return dataTable;
		}

		DataRow PopulateRow(DataTable table, Guid headerID, Guid lineID)
		{
			var row = table.Rows.Add();
			row["AlternateGLAccount"] = "101";
			row["GLAccountNum"] = "1010.11.11";
			row["AlternateGLAccountDescription"] = "desc";
			row["AlternateGLAccount"] = "111";
			row["GLAccountNum"] = "1110";
			row["BranchCode"] = "DEM";
			row["DepartmentCode"] = "BRN";
			row["GLAmountLocalDebit"] = 12m;
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountLocalBalance"] = 12m;
			row["GLAmountOSDebit"] = 1.5m;
			row["GLAmountOSCredit"] = decimal.Zero;
			row["PostDate"] = new DateTime(2024, 8, 12);
			row["Currency"] = "CNY";
			row["PostPeriod"] = 202308;
			row["OriginalAttribute_ORG"] = "ORG";
			row["OriginalAttribute_OCG"] = "OCG";
			row["GLType"] = "OCG";
			row["GLAccountType"] = "THG";
			row["TransactionHeaderID"] = headerID;
			row["TransactionLineID"] = lineID;
			return row;
		}
	}
}
