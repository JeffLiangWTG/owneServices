using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GeneralLedgerDataProcessorTest : TestCaseWithFactory
	{
		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_ARINV()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			aRInvoice.Lines[0].AL_PostDate = ZDateTime.Now;
			aRInvoice.Lines[0].AL_ReverseDate = ZDateTime.Now;
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();

			AssertAccGeneralLedgerData(((INeedRow)aRInvoice.Lines[0]).Row, 6);
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_GLGJL()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var glJournal = TestObjectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)line1).Row, 1);
			AssertAccGeneralLedgerData(((INeedRow)line2).Row, 2);

			var sqlCheckAmount = "Select GLD_LocalDebitAmount, GLD_LocalCreditAmount From dbo.AccGeneralLedgerData";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlCheckAmount);
			AssertEquals(2, collection.Count);
			Assert(collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 250M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
			Assert(collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 250M));

			line1.UnsignedOSLineAmount = 300M;
			line2.UnsignedOSLineAmount = 300M;
			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)line1).Row, 2);
			AssertAccGeneralLedgerData(((INeedRow)line2).Row, 2);

			collection.Load(sqlCheckAmount);
			AssertEquals(2, collection.Count);
			Assert("Prevented duplicate data", collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 250M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
			Assert("Prevented duplicate data", collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 250M));

			var row1 = ((INeedRow)line1).Row;
			row1.SetModified();
			var row2 = ((INeedRow)line2).Row;
			row2.SetModified();

			AssertAccGeneralLedgerData(row1, 2);
			AssertAccGeneralLedgerData(row2, 2);

			collection.Load(sqlCheckAmount);
			AssertEquals(2, collection.Count);
			Assert("Delete original and insert new", collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 300M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
			Assert("Delete original and insert new", collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 300M));
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_DuplicateGLJournalLine()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var glJournal = TestObjectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			var dataRow1 = ((INeedRow)line1).Row;
			var dataRow2 = ((INeedRow)line1).Row;
			dataRow2.SetModified();
			dataRow2["AL_LineAmount"] = 100M;
			TestObjectCreator.MockNudgeGLDProcessData([dataRow1, dataRow2]);

			var sqlCheckAmount = "Select GLD_LocalDebitAmount, GLD_LocalCreditAmount, GLD_AL_TransactionLine From dbo.AccGeneralLedgerData";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlCheckAmount);
			AssertEquals(1, collection.Count);
			Assert(collection.All(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 100M));
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_GLAJL()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)line1).Row, 1);
			AssertAccGeneralLedgerData(((INeedRow)line2).Row, 2);

			var sqlCheckAmount = "Select GLD_LocalDebitAmount, GLD_LocalCreditAmount From dbo.AccGeneralLedgerData";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlCheckAmount);
			AssertEquals(2, collection.Count);
			Assert(collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 250M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
			Assert(collection.Any(x => decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 250M));
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_GLRJL()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var today = ZDateTime.Today;
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, today, today, today.AddDays(35));

			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			line.AL_Desc = "GL REVERSING JOURNAL";
			var clearLine = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			clearLine.AL_Desc = "Clearing line";

			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)line).Row, 2);

			var sqlCheckAmount = "Select GLD_LocalDebitAmount, GLD_LocalCreditAmount, GLD_AL_TransactionLine From dbo.AccGeneralLedgerData";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlCheckAmount);

			CombineAssertions(() =>
			{
				AssertEquals(2, collection.Count);
				Assert(collection.Any(x => Guid.Parse(x["GLD_AL_TransactionLine"].ToString()) == line.PK.ToGuid() && decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 10M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
				Assert(collection.Any(x => Guid.Parse(x["GLD_AL_TransactionLine"].ToString()) == line.PK.ToGuid() && decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 10M));
			});

			AssertAccGeneralLedgerData(((INeedRow)clearLine).Row, 4);
			collection.Load(sqlCheckAmount);
			CombineAssertions(() =>
			{
				AssertEquals(4, collection.Count);
				Assert(collection.Any(x => Guid.Parse(x["GLD_AL_TransactionLine"].ToString()) == clearLine.PK.ToGuid() && decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 10M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 0M));
				Assert(collection.Any(x => Guid.Parse(x["GLD_AL_TransactionLine"].ToString()) == clearLine.PK.ToGuid() && decimal.Parse(x["GLD_LocalDebitAmount"].ToString()) == 0M && decimal.Parse(x["GLD_LocalCreditAmount"].ToString()) == 10M));
			});
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_CBEXX()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			TestObjectCreator.AUDBankAccount.Factory.Save();

			var exchangeDifference = Factory.NewWithValidTestData<CashbookExchangeDiff>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			exchangeDifference.AH_Ledger = exchangeDifference.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : exchangeDifference.AH_Ledger;
			exchangeDifference.AH_InvoiceDate = ZDateTime.Now;
			exchangeDifference.AH_PostDate = ZDateTime.Now;
			exchangeDifference.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			exchangeDifference.AH_ExchangeRate = 0.1274m;
			exchangeDifference.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			exchangeDifference.AH_OSExTaxAmount = 0m;
			exchangeDifference.AH_InvoiceAmount = 20m;
			exchangeDifference.AH_AG = TestObjectCreator.CreateGLHeader().PK;
			exchangeDifference.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)exchangeDifference).Row, 2);
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_CBRCB()
		{
			var depositBatch = Factory.NewWithValidTestData<DepositBatch>();
			depositBatch.AH_TransactionNum = "00001580";
			depositBatch.AH_ReceiptBatchNo = "00001580";
			Factory.Save();

			AssertNoExceptionThrown(() => AssertAccGeneralLedgerData(((INeedRow)depositBatch).Row, 0));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_TaxGLMovement()
		{
			var taxGLMovement = TaxGLMovementGeneralLedgerDataLineCreatorTest.GetAccTaxGLMovementForTest(Factory, 110m, 110m);
			AssertAccGeneralLedgerData(((INeedRow)taxGLMovement).Row, 2);
		}

		void AssertAccGeneralLedgerData(DataRow gLDSource, int expectedRowCount)
		{
			TestObjectCreator.MockNudgeGLDProcessData([gLDSource]);

			using (var command = Db.Connection.Command($@"SELECT * FROM dbo.AccGeneralLedgerData"))
			{
				AssertEquals(expectedRowCount, DataUtils.GetDataTableFromCommand(command).Rows.Count);
			}
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataProcessorWithDataRow_EXXOVPDSC()
		{
			var pureGLDtable = "Truncate table dbo.AccGeneralLedgerData";
			CombineAssertions(() =>
			{
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Overpayment);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Discount);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
				AssertGeneralLedgerDataProcessor_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Discount);
				Db.Connection.ExecuteNonQuery(pureGLDtable);
			});
		}

		void AssertGeneralLedgerDataProcessor_EXXOVPDSC(ZString ledger, ZString transactionType)
		{
			if (ledger == LedgerTypes.AccountsPayable)
			{
				AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			}
			else if (ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			}

			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			TransactionHeader header;

			if (transactionType == TransactionTypes.ExchangeDifference && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APExchangeDifference>();
			}
			else if (transactionType == TransactionTypes.ExchangeDifference && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<ARExchangeDifference>();
			}
			else if (transactionType == TransactionTypes.Overpayment && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APOverpayment>();
			}
			else if (transactionType == TransactionTypes.Overpayment && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<AROverpayment>();
			}
			else if (transactionType == TransactionTypes.Discount && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APDiscount>();
			}
			else if (transactionType == TransactionTypes.Discount && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<ARDiscount>();
			}
			else
			{
				header = Factory.NewWithValidTestData<TransactionHeader>();
			}

			header.AH_Ledger = ledger;
			header.AH_TransactionType = transactionType;
			header.AH_InvoiceAmount = header.AH_OSTotal = 0;
			header.AH_PostDate = new ZDateTime(2023, 3, 1);
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			header.AH_PostToGL = "Y";
			header.AH_DueDate = new ZDateTime(2004, 10, 1);

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			header.AH_AG = glHeader.PK;

			Factory.Save();

			AssertAccGeneralLedgerData(((INeedRow)header).Row, 2);
		}

		public void TestGeneralLedgerDataProcessorWhenTransactionTypeIsNotSupoortedByLineCreator()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var header = Factory.NewWithValidTestData<APInvoice>();
			header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();

			AssertNoExceptionThrown("Skipping unsupported transaction types without NullReferenceException", () => TestObjectCreator.MockNudgeGLDProcessData([header]));
			AssertNullOrEmpty("No new errors should appear", ErrorReporter.LastMessageReported);
		}

		[TestDate(2023, 01, 02)]
		public void TestGeneralLedgerDataProcessorWithMutipleCompanies()
		{
			var newCompany = TestObjectCreator.CreateNewCompany("ABC");
			newCompany.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch = TestObjectCreator.CreateNewBranch(newCompany, "BRH");

			TestObjectCreator.CreateTestPeriods(new DateTime(2022, 07, 01));
			TestObjectCreator.CreateTestPeriodsForEntireYear(newCompany, 2023);

			var wip1 = TestObjectCreator.CreateWIP();
			var wip2 = TestObjectCreator.CreateWIP();
			wip2.AL_GC = newCompany.PK;
			Factory.Save();

			TestObjectCreator.MockNudgeGLDProcessData([wip1, wip2]);

			var glDataList = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 202301, 202301, 202307, 202307 }, glDataList.Select(glData => glData.GLD_PostPeriod));
		}

		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
