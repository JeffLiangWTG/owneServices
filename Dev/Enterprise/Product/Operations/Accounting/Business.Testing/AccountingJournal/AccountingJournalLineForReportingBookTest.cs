using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingJournalLineForReportingBook))]
	public class AccountingJournalLineForReportingBookTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPublicProperties()
		{
			var accountingJournalLineForReportingBook = (AccountingJournalLineForReportingBook)GetNewBusinessObject();
			AssertEquals("111", accountingJournalLineForReportingBook.AlternateGLAccount);
			AssertEquals("1110", accountingJournalLineForReportingBook.ParentAccountNum);
			AssertEquals("AU1", accountingJournalLineForReportingBook.BranchCode);
			AssertEquals("BRN", accountingJournalLineForReportingBook.DeptCode);
			AssertEquals((ZDecimal)12, accountingJournalLineForReportingBook.LocalDebitAmount);
			AssertEquals(ZDecimal.Zero, accountingJournalLineForReportingBook.LocalCreditAmount);
			AssertEquals((ZDecimal)1.5, accountingJournalLineForReportingBook.GLAmountOSDebit);
			AssertEquals(ZDecimal.Zero, accountingJournalLineForReportingBook.GLAmountOSCredit);
			AssertEquals(new ZDateTime(2024, 8, 12), accountingJournalLineForReportingBook.GlPostDate);
			AssertEquals("CNY", accountingJournalLineForReportingBook.CurrencyCode);
			AssertEquals("202408", accountingJournalLineForReportingBook.ReportingBookPeriod);
			AssertEquals("desc", accountingJournalLineForReportingBook.AlternateGLAccountDesc);

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				AssertEquals("desc ORG[ORG] OCG[OCG]", accountingJournalLineForReportingBook.AlternateGLAccountDesc);
			}
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Creator.CreateAlternateChart("MGT", "chart");
			Factory.Save();
			var reportingBook = Creator.CreateReportingBook("TRR", "book", chart.PK, "");
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
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("BranchCode", typeof(string));
			dataTable.Columns.Add("DepartmentCode", typeof(string));
			dataTable.Columns.Add("GLAmountLocalDebit", typeof(decimal));
			dataTable.Columns.Add("GLAmountOSDebit", typeof(decimal));
			dataTable.Columns.Add("GLAmountLocalCredit", typeof(decimal));
			dataTable.Columns.Add("GLAmountOSCredit", typeof(decimal));
			dataTable.Columns.Add("PostDate", typeof(DateTime));
			dataTable.Columns.Add("Currency", typeof(string));
			dataTable.Columns.Add("PostPeriod", typeof(int));
			dataTable.Columns.Add("OriginalAttribute_ORG", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_OCG", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_LFE", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_LFO", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_TIC", typeof(string));
			dataTable.Columns.Add("OriginalAttribute_SPR", typeof(string));
			return dataTable;
		}

		void PopulateDataRow(DataRow row)
		{
			row["AlternateGLAccountDescription"] = "desc";
			row["AlternateGLAccount"] = "111";
			row["GLAccountNum"] = "1110";
			row["BranchCode"] = "AU1";
			row["DepartmentCode"] = "BRN";
			row["GLAmountLocalDebit"] = 12m;
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountOSDebit"] = 1.5m;
			row["GLAmountOSCredit"] = decimal.Zero;
			row["PostDate"] = new DateTime(2024, 8, 12);
			row["Currency"] = "CNY";
			row["PostPeriod"] = 202408;
			row["OriginalAttribute_ORG"] = "ORG";
			row["OriginalAttribute_OCG"] = "OCG";
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = Creator.CC1.PK;
			transactionLine.AL_LineAmount = 200M;
			reportingBook = CreateReportingBook();
			var dataTable = CreateGeneralLedgerTransactionData();
			var row = dataTable.NewRow();
			PopulateDataRow(row);
			return new AccountingJournalLineForReportingBook(row, reportingBook, transactionLine);
		}

		protected TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}

		TestObjectCreator creator;
		AccReportingBook reportingBook;
	}
}
