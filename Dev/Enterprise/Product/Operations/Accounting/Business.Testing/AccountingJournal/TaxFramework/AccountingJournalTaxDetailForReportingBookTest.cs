using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingJournalTaxDetailForReportingBook))]
	public class AccountingJournalTaxDetailForReportingBookTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateMethod()
		{
			var glMovementDetailsList = new List<IGLMovementDetails>();
			var glMovementDetailsMock1 = new Mock<IGLMovementDetails>();
			var glMovementDetailsMock2 = new Mock<IGLMovementDetails>();

			glMovementDetailsList.Add(glMovementDetailsMock1.Object);
			glMovementDetailsList.Add(glMovementDetailsMock2.Object);

			var result = AccountingJournalTaxDetailForReportingBook.Create(Factory, glMovementDetailsList, Factory.New<AccReportingBook>(), Array.Empty<DataRow>());

			AssertEquals("2 items retuned from the Create method", 2, result.Count);
			AssertEquals("The result is of type List<AccountingJournalTaxDetail>",  typeof(List<AccountingJournalTaxDetail>), result.GetType());
		}

		public void TestAccountingJournalTaxDetailMapping()
		{
			var table = CreateGeneralLedgerTransactionData();
			var reportingBook = CreateReportingBook();
			var row = table.NewRow();
			row["OriginalAccount"] = (ZString)"111";
			row["PostPeriod"] = (ZInt)202401;
			row["AlternateGLAccountDescription"] = (ZString)"a";
			row["OriginalAttribute_LFO"] = (ZString)"LOC";
			row["GLAccountNum"] = (ZString)"1010.10.10";
			var rows = new DataRow[] { row };
			var glMovementList = new List<IGLMovementDetails>();
			IGLMovementDetails glMovementDetails = new GLMovementDetails();
			glMovementDetails.GLAccount = "1010.10.10";
			glMovementList.Add(glMovementDetails);
			var accountingJournalTaxDetailForReportingBook = AccountingJournalTaxDetailForReportingBook.Create(Factory, glMovementList, reportingBook, rows);
			AssertEquals("202401", accountingJournalTaxDetailForReportingBook[0].PostPeriod);
			AssertEquals("111", accountingJournalTaxDetailForReportingBook[0].AlternateAccountNum);
			AssertEquals("a", accountingJournalTaxDetailForReportingBook[0].AlternateAccountDesc);

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				AssertEquals("a LFO[LOC]", accountingJournalTaxDetailForReportingBook[0].AlternateAccountDesc);
			}
		}

		AccReportingBook CreateReportingBook()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT", "chart");
			Factory.Save();
			var reportingBook = creator.CreateReportingBook("TRR", "book", chart.PK, "");
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
			dataTable.Columns.Add("OriginalAccount", typeof(ZString));
			dataTable.Columns.Add("PostPeriod", typeof(ZInt));
			dataTable.Columns.Add("OriginalAttribute_LFO", typeof(ZString));
			dataTable.Columns.Add("OriginalAttribute_ORG", typeof(ZString));
			dataTable.Columns.Add("OriginalAttribute_OCG", typeof(ZString));
			dataTable.Columns.Add("OriginalAttribute_LFE", typeof(ZString));
			dataTable.Columns.Add("OriginalAttribute_TIC", typeof(ZString));
			dataTable.Columns.Add("OriginalAttribute_SPR", typeof(ZString));
			return dataTable;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			reportingBook = CreateReportingBook();
			var dataTable = CreateGeneralLedgerTransactionData();
			var row = dataTable.NewRow();
			row["OriginalAccount"] = (ZString)"111";
			row["PostPeriod"] = (ZInt)202401;
			row["AlternateGLAccountDescription"] = (ZString)"a";
			row["OriginalAttribute_LFO"] = (ZString)"LOC";
			row["GLAccountNum"] = (ZString)"1010.10.10";
			var rows = new DataRow[] { row };
			var glMovementDetailsList = new List<IGLMovementDetails>();
			var glMovementDetailsMock1 = new Mock<IGLMovementDetails>();

			glMovementDetailsList.Add(glMovementDetailsMock1.Object);

			return AccountingJournalTaxDetailForReportingBook.Create(Factory, glMovementDetailsList, reportingBook, rows)[0];
		}

		AccReportingBook reportingBook;
	}
}
