using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccountingJournalLine))]
	sealed class DocAccountingJournalLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var aj = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());

			return DocAccountingJournalLine.New(aj.Lines.First(), Factory);
		}

		public void TestLocalCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var docAccountingJournalLine = (DocAccountingJournalLine)GetNewBusinessObject();
				AssertEquals(Core.Constants.CurrencyCodes.Australia, docAccountingJournalLine.LocalCurrency.Code);
			}
		}

		public void TestDescription()
		{
			Action<DataRow> setAction = (row) => row["AlternateGLAccountDescription"] = "a";
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("a", docAccountingJournalLine.Description);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("GENERAL LEDGER JOURNAL", docAccountingJournalLine.Description);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestDescriptionOne()
		{
			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row = table.NewRow();

			var line = new BusinessObjectFactory().New<AccTransactionLines>();
			line.AL_Desc = "desc";
			var accountingJournalLine = new AccountingJournalLineForReportingBook(row, reportingBook, line);
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLine, Factory);
			AssertEquals("desc", docAccountingJournalLine.LineSupporter.GetDescriptionOne());

			line.AL_Desc = "line desc";
			AssertEquals("line desc", docAccountingJournalLine.LineSupporter.GetDescriptionOne());
		}

		public void TestPostDate()
		{
			Action<DataRow> setAction = (row) => row["PostDate"] = new DateTime(2024, 8, 12);
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals(new DateTime(2024, 8, 12), docAccountingJournalLine.PostDate);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals(ZDateTime.Today.ToDateTime(), docAccountingJournalLine.PostDate);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestPostPeriod()
		{
			Action<DataRow> setAction = (row) => row["PostPeriod"] = 202408;
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("202408", docAccountingJournalLine.PostPeriod);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("0", docAccountingJournalLine.PostPeriod);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestForeignCurrencyEquivalent()
		{
			Action<DataRow> setAction = (row) => row["GLAmountOSDebit"] = 20.08m;
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("20.08 DR", docAccountingJournalLine.ForeignCurrencyEquivalent);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("", docAccountingJournalLine.ForeignCurrencyEquivalent);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestAlternateGLAccount()
		{
			Action<DataRow> setAction = (row) => row["AlternateGLAccount"] = "101";
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("101", docAccountingJournalLine.AlternateGLAccount);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("", docAccountingJournalLine.AlternateGLAccount);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestParentAccountNum()
		{
			Action<DataRow> setAction = (row) => row["GLAccountNum"] = "1010.10.10";
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("1010.10.10", docAccountingJournalLine.ParentAccountNum);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("", docAccountingJournalLine.ParentAccountNum);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestDebitAmount()
		{
			Action<DataRow> setAction = (row) => row["GLAmountLocalDebit"] = 20.08m;
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("20.08", docAccountingJournalLine.DebitAmount);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("250.00", docAccountingJournalLine.DebitAmount);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestCreditAmount()
		{
			Action<DataRow> setAction = (row) => row["GLAmountLocalCredit"] = 20.08m;
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("20.08", docAccountingJournalLine.CreditAmount);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("", docAccountingJournalLine.CreditAmount);
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestDebitAmountForMultipleReportingBookAccountingJournalLines()
		{
			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row = table.NewRow();
			row["Currency"] = "CNY";
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountLocalDebit"] = (decimal)2.8;
			row["GLAmountLocalBalance"] = (decimal)2.8;
			row["GLAccountNum"] = "1010.78.78";

			var accountingJournalLine1 = new AccountingJournalLineForReportingBook(row, reportingBook, Factory.New<AccTransactionLines>());
			var accountingJournalLine2 = new AccountingJournalLineForReportingBook(row, reportingBook, Factory.New<AccTransactionLines>());
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLine1, Factory);
			Assert(docAccountingJournalLine.AddAccountingJournalLine(accountingJournalLine2));
			AssertEquals("5.60", docAccountingJournalLine.DebitAmount);

			row["GLAmountLocalCredit"] = (decimal)2.8;
			row["GLAmountLocalDebit"] = decimal.Zero;
			row["GLAmountLocalBalance"] = (decimal)-2.8;

			AssertEquals("", docAccountingJournalLine.DebitAmount);
		}

		public void TestCreditAmountForMultipleReportingBookAccountingJournalLines()
		{
			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row = table.NewRow();
			row["Currency"] = "CNY";
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountLocalDebit"] = (decimal)2.8;
			row["GLAmountLocalBalance"] = (decimal)2.8;
			row["GLAccountNum"] = "1010.78.78";

			var accountingJournalLine1 = new AccountingJournalLineForReportingBook(row, reportingBook, Factory.New<AccTransactionLines>());
			var accountingJournalLine2 = new AccountingJournalLineForReportingBook(row, reportingBook, Factory.New<AccTransactionLines>());
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLine1, Factory);
			Assert(docAccountingJournalLine.AddAccountingJournalLine(accountingJournalLine2));
			AssertEquals("", docAccountingJournalLine.CreditAmount);

			row["GLAmountLocalCredit"] = (decimal)2.8;
			row["GLAmountLocalDebit"] = decimal.Zero;
			row["GLAmountLocalBalance"] = (decimal)-2.8;

			AssertEquals("5.60", docAccountingJournalLine.CreditAmount);
		}

		public void TestOSUnsignedAmount()
		{
			Action<DataRow> setAction = (row) => row["GLAmountOSDebit"] = 20.08m;
			Action<DocAccountingJournalLine> assertReportBook = (docAccountingJournalLine) => AssertEquals("20.08", docAccountingJournalLine.OSUnsignedAmount);
			Action<DocAccountingJournalLine> assertNonReportingBook = (docAccountingJournalLine) => AssertEquals("250.00", docAccountingJournalLine.OSUnsignedAmount);
			TestField(setAction, assertReportBook, assertNonReportingBook);

			setAction = (row) => row["GLAmountOSCredit"] = (decimal)20.08;
			assertReportBook = (docAccountingJournalLine) => AssertEquals("20.08", docAccountingJournalLine.OSUnsignedAmount);
			assertNonReportingBook = null;
			TestField(setAction, assertReportBook, assertNonReportingBook);
		}

		public void TestJournalEntriesNumber()
		{
			var generalLedgerData = Factory.New<AccGeneralLedgerData>();
			generalLedgerData.GLD_JournalEntriesNumber = "123";
			var accountingJournalLineForGeneralLedgerData = new AccountingJournalLineForGeneralLedgerData(generalLedgerData);
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLineForGeneralLedgerData, Factory);

			AssertEquals("123", docAccountingJournalLine.JournalEntriesNumber);
		}

		public void TestIsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions");
			var factory = new BusinessObjectFactory();
			var generalLedgerData = factory.New<AccGeneralLedgerData>();

			var accountingJournalLineForGeneralLedgerData = new AccountingJournalLineForGeneralLedgerData(generalLedgerData);
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLineForGeneralLedgerData, Factory);

			AssertEquals(true, docAccountingJournalLine.IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn);
		}

		public void TestSetDebitCreditSign()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions");

			var generalLedgerData = Factory.New<AccGeneralLedgerData>();
			generalLedgerData.GLD_LocalCreditAmount = 1000m;
			generalLedgerData.GLD_LocalDebitAmount = 0m;
			var accountingJournalLineForGeneralLedgerData = new AccountingJournalLineForGeneralLedgerData(generalLedgerData);
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLineForGeneralLedgerData, Factory);

			AssertEquals(DebitCreditDataEntry.CR, accountingJournalLineForGeneralLedgerData.DebitCreditSign);
			AssertEquals(DebitCreditDataEntry.CR, docAccountingJournalLine.DebitCreditSign);
		}

		void InsertGeneralLedgerDataRegistry(Guid currentCompanyPK, String sdName)
		{
			var sql =
				@"
				DELETE FROM dbo.StmData WHERE SD_Name = @sdName
				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_PreserveTestValue, SD_BinaryValue, SD_SystemCreateTimeUtc)
				VALUES (newid(), @SdName, @CompanyPK, 0, @BinaryVal, GETUTCDATE())";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@BinaryVal", SqlDbType.Binary, System.Text.Encoding.Unicode.GetBytes("true"));
				command.AddParameter("@SdName", SqlDbType.VarChar, sdName);
				command.ExecuteNonQuery();
			}
		}

		void TestField(Action<DataRow> setAction, Action<DocAccountingJournalLine> assertReportBook, Action<DocAccountingJournalLine> assertNonReportingBook)
		{
			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row = table.NewRow();
			row["Currency"] = "CNY";
			row["GLAmountLocalDebit"] = decimal.Zero;
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountOSCredit"] = decimal.Zero;
			row["GLAmountOSDebit"] = decimal.Zero;
			setAction.Invoke(row);
			var accountingJournalLine = new AccountingJournalLineForReportingBook(row, reportingBook, new BusinessObjectFactory().New<AccTransactionLines>());
			var docAccountingJournalLine = DocAccountingJournalLine.New(accountingJournalLine, Factory);
			assertReportBook.Invoke(docAccountingJournalLine);

			docAccountingJournalLine = (DocAccountingJournalLine)GetNewBusinessObject();
			if (assertNonReportingBook != null)
			{
				assertNonReportingBook.Invoke(docAccountingJournalLine);
			}
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
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

		public void TestLineForMultiSubAccountTypeCode()
		{
			var objectCreator = new TestObjectCreator(new BusinessObjectFactory());
			objectCreator.CreateGLHeaderSubAccount(objectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			objectCreator.Factory.Save();

			var glJournal = objectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			objectCreator.CreateGLJournalLine(glJournal, 500M, DebitCredit.CR, objectCreator.GLHeader2.PK);
			var line1 = objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, objectCreator.GLHeader1.PK);
			var line2 = objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, objectCreator.GLHeader1.PK);
			line1.AL_Calc_FirstSubClassParentId = objectCreator.ABIGAS.PK;
			line2.AL_Calc_FirstSubClassParentId = objectCreator.AALSHI.PK;
			objectCreator.Factory.Save();
			var aj = DocAccountingJournal.New(new DummyAccountingJournal(glJournal, ReadonlyFactory), ReadonlyFactory);
			AssertEquals(3, aj.LinesWithNonZeroAmount.Count);
			AssertNullOrEmpty(aj.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().FirstOrDefault(x => x.GLAccount.AccountNumber == objectCreator.GLHeader2.AG_AccountNum).MultiSubAccountTypeCode);
			AssertNotNull(aj.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Where(x => x.GLAccount.AccountNumber == objectCreator.GLHeader1.AG_AccountNum && x.MultiSubAccountTypeCode.Equals("ORG: ABIGAS")));
			AssertNotNull(aj.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Where(x => x.GLAccount.AccountNumber == objectCreator.GLHeader1.AG_AccountNum && x.MultiSubAccountTypeCode.Equals("ORG: AALSHI")));

			line2.AL_Calc_FirstSubClassParentId = objectCreator.ABIGAS.PK;
			objectCreator.Factory.Save();
			aj = DocAccountingJournal.New(new DummyAccountingJournal(glJournal, ReadonlyFactory), ReadonlyFactory);
			AssertEquals(2, aj.LinesWithNonZeroAmount.Count);
			AssertNullOrEmpty(aj.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().FirstOrDefault(x => x.GLAccount.AccountNumber == objectCreator.GLHeader2.AG_AccountNum).MultiSubAccountTypeCode);
			AssertEquals(1, aj.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.GLAccount.AccountNumber == objectCreator.GLHeader1.AG_AccountNum && x.MultiSubAccountTypeCode.Equals("ORG: ABIGAS")));
		}

		public void TestReportingBookLinesForRollup()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob(objectCreator.LocalClient, 1.0m, objectCreator.Agent, 1.0m);
			var apInvoice = objectCreator.CreateAPInvoice<APInvoice>("AP100001", objectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, objectCreator.Creditor1);
			apInvoice.AH_AB = objectCreator.AUDBankAccount.PK;
			apInvoice.AH_GB_TaxBranch = objectCreator.NonCurrentBranch.PK;
			apInvoice.Lines.RemoveAndDeleteAll();
			var line1 = objectCreator.CreateAPInvoiceLine(apInvoice, job, objectCreator.CC1, objectCreator.AUD, 1.0m, "AP Line 001", 250m);
			var line2 = objectCreator.CreateAPInvoiceLine(apInvoice, job, objectCreator.CC1, objectCreator.AUD, 1.0m, "AP Line 002", 260m);
			var charge1 = objectCreator.CreateJobCharge(line1, job, objectCreator.CC1);
			charge1.JR_GB_CostTaxBranch = objectCreator.NonCurrentBranch.PK;
			var charge2 = objectCreator.CreateJobCharge(line2, job, objectCreator.CC1);
			charge2.JR_GB_CostTaxBranch = objectCreator.NonCurrentBranch.PK;
			Factory.Save();

			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row1 = PopulateRow(table, apInvoice.PK.ToGuid(), line1.PK.ToGuid());
			row1["GLAccountNum"] = (ZString)"1010.11.11";

			var row2 = PopulateRow(table, apInvoice.PK.ToGuid(), line2.PK.ToGuid());
			row2["GLAccountNum"] = (ZString)"1010.11.12";

			var row3 = PopulateRow(table, apInvoice.PK.ToGuid(), line2.PK.ToGuid());
			row3["GLAccountNum"] = (ZString)"1010.11.13";
			row3["OriginalAttribute_OCG"] = "OUG";

			var row4 = PopulateRow(table, apInvoice.PK.ToGuid(), line2.PK.ToGuid());
			row4["GLAccountNum"] = (ZString)"1010.11.13";
			row4["OriginalAttribute_OCG"] = "ORG";

			var journal = DocAccountingJournal.New(new ARAPAccountingJournal(apInvoice, ReadonlyFactory, reportingBook, table), ReadonlyFactory);
			AssertEquals(1, journal.LinesWithNonZeroAmount.Count);
			AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.DebitAmount == "48.00"));

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.DisplayParentAccount = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				journal = DocAccountingJournal.New(new ARAPAccountingJournal(apInvoice, ReadonlyFactory, reportingBook, table), ReadonlyFactory);
				AssertEquals("Roll up by ParentAccount", 3, journal.LinesWithNonZeroAmount.Count);
				AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.ParentAccountNum == "1010.11.13" && x.DebitAmount == "24.00"));
				AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.ParentAccountNum == "1010.11.11" && x.DebitAmount == "12.00"));
			}

			reportingBookAccountingJournalPrintOption.DisplayParentAccount = false;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				journal = DocAccountingJournal.New(new ARAPAccountingJournal(apInvoice, ReadonlyFactory, reportingBook, table), ReadonlyFactory);
				AssertEquals("Roll up by attributes", 3, journal.LinesWithNonZeroAmount.Count);
				AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.Description == "desc ORG[ORG] OCG[ORG]"));
				AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.Description == "desc ORG[ORG] OCG[OUG]"));
				AssertEquals(1, journal.LinesWithNonZeroAmount.Cast<DocGenericTransactionLine>().Count(x => x.Description == "desc ORG[ORG] OCG[OCG]"));
			}
		}

		public void TestNoRollUpForGLJournals()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var glJournal = objectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			var line1 = objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, objectCreator.GLHeader1.PK);
			var line2 = objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, objectCreator.GLHeader1.PK);
			line1.AL_Calc_FirstSubClassParentId = objectCreator.ABIGAS.PK;
			line2.AL_Calc_FirstSubClassParentId = objectCreator.AALSHI.PK;
			Factory.Save();

			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			PopulateRow(table, glJournal.PK.ToGuid(), line1.PK.ToGuid());
			PopulateRow(table, glJournal.PK.ToGuid(), line1.PK.ToGuid());
			PopulateRow(table, glJournal.PK.ToGuid(), line1.PK.ToGuid());
			PopulateRow(table, glJournal.PK.ToGuid(), line2.PK.ToGuid());

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.DisplayParentAccount = true;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				var journal = DocAccountingJournal.New(new GLAccountingJournal(glJournal, ReadonlyFactory, reportingBook, table), ReadonlyFactory);
				AssertEquals("No Roll up For GL Journals", 4, journal.LinesWithNonZeroAmount.Count);
			}
		}

		DataRow PopulateRow(DataTable table, Guid headerID, Guid lineID)
		{
			var row = table.Rows.Add();
			row["AlternateGLAccount"] = "101";
			row["GLAccountNum"] = "1010.11.11";
			row["AlternateGLAccountDescription"] = "desc";
			row["AlternateGLAccount"] = "111";
			row["GLAccountNum"] = "1110";
			row["BranchCode"] = "AU1";
			row["DepartmentCode"] = "BRN";
			row["GLAmountLocalDebit"] = 12m;
			row["GLAmountLocalCredit"] = decimal.Zero;
			row["GLAmountLocalBalance"] = 12m;
			row["GLAmountOSDebit"] = 1.5m;
			row["GLAmountOSCredit"] = decimal.Zero;
			row["PostDate"] = new DateTime(2024, 8, 12);
			row["Currency"] = "CNY";
			row["PostPeriod"] = 202408;
			row["OriginalAttribute_ORG"] = "ORG";
			row["OriginalAttribute_OCG"] = "OCG";
			row["GLType"] = "OCG";
			row["GLAccountType"] = "THG";
			row["TransactionHeaderID"] = headerID;
			row["TransactionLineID"] = lineID;
			return row;
		}

		ReadOnlyBusinessObjectFactory ReadonlyFactory
		{
			get { return readonlyFactory ?? (readonlyFactory = Factory.GetCachedReadOnlyFactory()); }
		}
		ReadOnlyBusinessObjectFactory readonlyFactory;
	}
}
