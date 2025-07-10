using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalForTest))]
	public partial class GLJournal_InnerTest : TransactionHeaderWithLinesTest
	{
		#region Not Applicable Test Cases

		public override void TestRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine()
		{
			Assert("Not Job related transaction", true);
		}

		protected override bool IsJobRelatedTransaction => false;

		public new void TestEDUAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		public new void TestGSTAndQSTBasedOnQCTAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		public new void TestOTO6AmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		public new void TestQSTAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		public new void TestRETAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		public void TestPeriodPK()
		{
			var creator = new TestObjectCreator(Factory);

			var journal = creator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			creator.CreateGLJournalLine(journal, 10, DebitCredit.CR, creator.ExchangeGainLossControlAccount.PK);
			creator.CreateGLJournalLine(journal, 10, DebitCredit.DR, creator.ExchangeGainLossAdjustmentAccount.PK);

			var newID = Guid.NewGuid();
			journal.PeriodPK = newID;

			Factory.Save();

			AssertEquals(journal.PeriodPK, newID);
		}

		public void TestIsPeriodExists()
		{
			var creator = new TestObjectCreator(Factory);

			var journal = creator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			creator.CreateGLJournalLine(journal, 10, DebitCredit.CR, creator.ExchangeGainLossControlAccount.PK);
			creator.CreateGLJournalLine(journal, 10, DebitCredit.DR, creator.ExchangeGainLossAdjustmentAccount.PK);

			journal.PeriodPK = ZGuid.Empty;
			Assert(!journal.IsPeriodExists);

			var newID = Guid.NewGuid();
			journal.PeriodPK = newID;
			Assert(!journal.IsPeriodExists);

			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			journal.PeriodPK = period.PK;
			Assert(journal.IsPeriodExists);

			Factory.Save();
		}

		public void TestAddOneEditLogOfAccTransactionHeaderWhenEditGLJournal()
		{
			Factory.Save();

			var creator = new TestObjectCreator(Factory);

			var journal = creator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			creator.CreateGLJournalLine(journal, 10, DebitCredit.CR, creator.ExchangeGainLossControlAccount.PK);
			creator.CreateGLJournalLine(journal, 10, DebitCredit.DR, creator.ExchangeGainLossAdjustmentAccount.PK);

			DateTime highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeAdd = GetEDTStmALogAfterSL_PostedTime(highWaterMark);
			Factory.Save();
			StmALog[] logsAfterSave = GetEDTStmALogAfterSL_PostedTime(highWaterMark);
			AssertEquals("There should be 1 logs added.", 1, logsAfterSave.Length - logsBeforeAdd.Length);

			journal.AH_Desc = "Changed";
			journal.GLJournalLines[0].UnsignedOSLineAmount = 20;
			journal.GLJournalLines[1].UnsignedOSLineAmount = 20;

			highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark);
			Factory.Save();
			StmALog[] logsAfterEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark);

			AssertEquals("There should be 1 logs added.", 1, logsAfterEdit.Length - logsBeforeEdit.Length);
			var stmALogAccTransactionHeader = GetLogByTable(logsAfterEdit, "AccTransactionHeader");
			AssertEquals("There should be 1 acctransactionheader edit log.", 1, stmALogAccTransactionHeader.Length);
			AssertEquals("AccTransactionHeader Should be edit.", "EDT", stmALogAccTransactionHeader[0].SL_SE_NKEvent);
		}

		StmALog[] GetEDTStmALogAfterSL_PostedTime(DateTime sL_PostedTimeUtc)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, sL_PostedTimeUtc);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new string[] { "EDT", "ADD" });
			query.AddToFilter(StmALogSchema.SL_Table, SQLComparisonOperator.NotEqual, "STMALog");
			return Factory.Load<StmALog>(query);
		}

		StmALog[] GetLogByTable(StmALog[] logs, string table)
		{
			var logsByTable = (from log in logs
							   where log.SL_Table == table
							   select log).ToArray();

			return logsByTable.ToArray();
		}

		public void TestPostDatePeriod()
		{
			var now = ZDateTime.Now;
			TestObjectCreator.CreateTestPeriods(now.Date);

			var standardJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, now, now, null);
			AssertPostDatePeriod(standardJournal);
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertPostDatePeriod(standardJournal);
			}

			var noteJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, now, now, null);
			AssertPostDatePeriod(noteJournal);

			void AssertPostDatePeriod(GLJournal header)
			{
				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var expectedDate = header.AH_PostDate;
				AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), header.AH_PostDatePeriod);

				expectedDate = now.AddMonths(3);
				header.AH_PostDate = expectedDate;
				AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), header.AH_PostDatePeriod);
			}
		}

		public void TestDueDatePeriod()
		{
			var now = ZDateTime.Now;
			TestObjectCreator.CreateTestPeriods(now.Date);

			var standardJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, now, now);
			AssertDueDatePeriod(standardJournal);

			var reverseJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, now, now, now.AddMonths(1));
			AssertDueDatePeriod(reverseJournal);
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertDueDatePeriod(reverseJournal);
			}

			void AssertDueDatePeriod(GLJournal header)
			{
				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var expectedDate = header.AH_DueDate;
				AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), header.AH_DueDatePeriod);

				expectedDate = now.AddMonths(3);
				header.AH_DueDate = expectedDate;
				AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), header.AH_DueDatePeriod);
			}
		}

		public void TestSetApprovalRequestJournalNumberOnSaving_WhenRequestInJournalFactory()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var request = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			request.Initialize(journal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request.Factory.Save();

			AssertEquals("Precondition: request journal number", "", request.PostingDetails.Journal.AH_TransactionNum);
			AssertEquals("Precondition: Ask request status to load request in journal factory", request.XP_ApprovalStatus, journal.ApprovalRequestStatus);
			AssertNotNull("Precondition: request in journal factory", Factory.LoadTop1<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, request.PK) { FetchOnlyFromLocalCache = true }));

			request.PrepareToPost(journal.PK, "");
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			BusinessObjectFactory.SaveTogether(request.Factory, Factory);
			AssertNotEquals("Journal Number", "", journal.AH_TransactionNum);
			var requestInAnotherFactory = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(request.PK);
			AssertEquals("request journal number", journal.AH_TransactionNum, requestInAnotherFactory.PostingDetails.Journal.AH_TransactionNum);
		}

		public void TestSetApprovalRequestJournalNumberOnSaving_WhenRequestNotInJournalFactory()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var request = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			request.Initialize(journal);

			AssertEquals("Precondition: request journal number", "", request.PostingDetails.Journal.AH_TransactionNum);
			AssertNull("Precondition: request NOT in journal factory", Factory.LoadTop1<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, request.PK) { FetchOnlyFromLocalCache = true }));

			request.PrepareToPost(journal.PK, "");
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			BusinessObjectFactory.SaveTogether(request.Factory, Factory);
			AssertNotEquals("Journal Number", "", journal.AH_TransactionNum);
			var requestInAnotherFactory = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(request.PK);
			AssertEquals("request journal number", journal.AH_TransactionNum, requestInAnotherFactory.PostingDetails.Journal.AH_TransactionNum);
		}

		public void TestApprovalRequestStatus()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var request1 = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			request1.Initialize(journal);
			var expectedCreateTime1 = ZDateTime.Today.AddDays(-2);
			request1.XP_SystemCreateTimeUtc = expectedCreateTime1;
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			AssertEquals("Show status only for saved requests", "", journal.ApprovalRequestStatus);
			request1.Factory.Save();
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, journal.ApprovalRequestStatus);

			var requestToDelete = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			requestToDelete.Initialize(journal);
			requestToDelete.XP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			requestToDelete.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			AssertEquals("Show status only for saved requests", Constants.GenApprovalRequestApprovalStatus.Cancelled, journal.ApprovalRequestStatus);
			requestToDelete.Factory.Save();
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, journal.ApprovalRequestStatus);

			var request2 = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			request2.Initialize(journal);
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var expectedCreateTime2 = ZDateTime.Today.AddDays(-5);
			request2.XP_SystemCreateTimeUtc = expectedCreateTime2;
			AssertEquals("Show status only for saved requests", Constants.GenApprovalRequestApprovalStatus.Approved, journal.ApprovalRequestStatus);
			request2.Factory.Save();
			AssertEquals("Precondition: request1.XP_SystemCreateTimeUtc", expectedCreateTime1, request1.XP_SystemCreateTimeUtc);
			AssertEquals("Precondition: request2.XP_SystemCreateTimeUtc", expectedCreateTime2, request2.XP_SystemCreateTimeUtc);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, journal.ApprovalRequestStatus);

			request2.PrepareToPost(journal.PK, "");
			BusinessObjectFactory.SaveTogether(request2.Factory, Factory);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var journalInNewFactory = newFactory.Load<GLJournal>(journal.PK);
			var requestInNewFactory = Factory.New<GLJournalApprovalRequest>();
			requestInNewFactory.Initialize(journalInNewFactory);
			var expectedCreateTimeInNewFactory = ZDateTime.Today;
			requestInNewFactory.XP_SystemCreateTimeUtc = expectedCreateTimeInNewFactory;
			requestInNewFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			newFactory.Save();
			AssertEquals("Precondition: requestInNewFactory.XP_SystemCreateTimeUtc", expectedCreateTimeInNewFactory, requestInNewFactory.XP_SystemCreateTimeUtc);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, journal.ApprovalRequestStatus);
		}

		public void TestIsEliminationJournal()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			journal.AH_TransactionCategory = "";
			Assert("Journal is not elimination journal, as no elimination presentaion category is setup.", !journal.IsEliminationJournal);

			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CA1";
			category1.Description = (NoResString)"Category 1";
			var category2 = list.AddNew();
			category2.Code = "CA2";
			category2.Description = (NoResString)"Category 2";
			category2.Bool = true; // active
			category2.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(2, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			journal.AH_TransactionCategory = category1.Code;
			Assert("Journal is not elimination journal", !journal.IsEliminationJournal);

			journal.AH_TransactionCategory = category2.Code;
			Assert("Journal is elimination journal", journal.IsEliminationJournal);
		}

		public void TestJournalIsReadonlyIfLinkedToConsolidationBatch()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			Factory.Save();
			ZGuid journalPk = journal.PK;

			Assert(!journal.ReadOnly);

			var group = Factory.NewWithValidTestData<AccConsolidationGroup>();
			var company = GlbCompany.CurrentCompany;
			var period = Factory.NewWithValidTestData<AccPeriodManagement>();

			var consolidationBatch = Factory.New<AccConsolidationBatch>();
			consolidationBatch.YB_AH_EliminationJournal = journalPk;
			consolidationBatch.YB_YR_ConsolidationGroup = group.PK;
			consolidationBatch.YB_GC_Company = company.PK;
			consolidationBatch.YB_AM_Period = period.PK;

			Factory.Save();

			journal = new BusinessObjectFactory().Load<GLJournal>(journalPk);
			Assert(journal.ReadOnly);
		}

		public void TestObsoleteJournalCategoriesAreAddedToLookup()
		{
			Journal.AH_TransactionCategory = "XYZ";

			Journal.Factory.Save();
			Header = new BusinessObjectFactory().Load<GLJournalForTest>(Journal.PK);
			Assert(Journal.TransactionCategory_List.ContainsCode("XYZ"));
			AssertEquals("Obsolete Journal Category", Journal.TransactionCategory_List.GetDescriptionFromCode("XYZ"));
		}

		public void TestCopyJournal()
		{
			var transactionType = TransactionTypes.GLReversingJournal;
			var description = "Test Description";
			var dueDate = new ZDateTime(2004, 2, 28);
			var currencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var postDate = new ZDateTime(2004, 1, 30);

			var lineDesc = "Line Description";
			var lineAmount = 300.00m;
			var gLAccount = TestCaseHelper.GetFirstPKFromTable(AccGLHeader.Schema.TableName);

			var journalToCopy = SetupJournalAndLinesForCopy(transactionType, "12345678", postDate, dueDate, lineAmount);

			journalToCopy.AH_Desc = description;
			journalToCopy.AH_OSExTaxAmount = 0m;
			journalToCopy.AH_ExchangeRate = 1m;

			journalToCopy.GLJournalLines[0].AL_Desc = lineDesc;
			journalToCopy.GLJournalLines[1].AL_Desc = lineDesc;

			Factory.Save();

			var journalCopy = (GLJournal)((ITemplateCopyable)journalToCopy).TemplateCopy();

			var currPeriodEndDate = CurrentPeriod.AM_EndDate.Date;

			AssertEquals("Ledger should be GL", LedgerTypes.General, journalCopy.AH_Ledger);
			AssertEquals("Transaction Type should be RJL", transactionType, journalCopy.AH_TransactionType);
			AssertEquals("Transaction Number should be blank", ZString.Empty, journalCopy.AH_TransactionNum);
			AssertEquals("Transaction Count should be default", (ZByte)1, journalCopy.AH_TransactionCount);
			AssertEquals("Transaction reference should be empty", ZString.Empty, journalCopy.AH_TransactionReference);
			AssertEquals("Description should copy over", description, journalCopy.AH_Desc);
			AssertEquals("Invoice Date should be today's date", ZDateTime.Today.Date, journalCopy.AH_InvoiceDate.Date);
			AssertEquals("Due Date should be empty", ZDateTime.Empty, journalCopy.AH_DueDate.Date);
			AssertEquals("Transaction Category should be same as original", "ALL", journalCopy.AH_TransactionCategory);
			AssertEquals("OS Ex Tax Amount should be same as original journal", 0m, journalCopy.AH_OSExTaxAmount);
			AssertEquals("Local Amount should be same as original journal", 0m, journalCopy.AH_LocalExTaxAmount);
			AssertEquals("Currency should be same as original journal", currencyNK, journalCopy.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be same as original journal", 1m, journalCopy.AH_ExchangeRate);
			AssertEquals("Post Date should be end of current period", currPeriodEndDate, journalCopy.AH_PostDate.Date);
			AssertEquals("Is Disbursement should be N", ZBool.False, journalCopy.AH_IsDisbursementCalc);
			AssertEquals("Cheque or Reference should be empty", ZString.Empty, journalCopy.AH_ChequeOrReference);
			AssertEquals("Receipt Type should be empty", ZString.Empty, journalCopy.AH_ReceiptType);
			AssertEquals("Cheque Drawer should be empty", ZString.Empty, journalCopy.AH_ChequeDrawer);
			AssertEquals("Drawer Bank should be empty", ZString.Empty, journalCopy.AH_DrawerBank);
			AssertEquals("Drawer Branch should be empty", ZString.Empty, journalCopy.AH_DrawerBranch);
			AssertEquals("Bank Account should be empty", ZGuid.Empty, journalCopy.AH_AB);
			AssertEquals("Organisation field should be empty", ZGuid.Empty, journalCopy.AH_OH);
			AssertEquals("Job field should be empty", ZGuid.Empty, journalCopy.AH_JH);
			AssertEquals("Branch field should be same as original", GlbBranch.CurrentBranch.PK, journalCopy.AH_GB);
			AssertEquals("Department field should be same as original", GlbDepartment.CurrentDepartment.PK, journalCopy.AH_GE);
			AssertEquals("GL Header field (on journal header) should be empty", ZGuid.Empty, journalCopy.AH_AG);
			AssertEquals("Invoice Approved field should be N", ZBool.False, journalCopy.AH_InvoiceApproved);
			AssertEquals("Consolidated Invoice Reference field should be empty", ZString.Empty, journalCopy.AH_ConsolidatedInvoiceRef);
			AssertEquals("Fully Paid Date should be null", ZDateTime.Empty, journalCopy.AH_FullyPaidDate);
			AssertEquals("Invoice Printed should be N", ZBool.False, journalCopy.AH_InvoicePrinted);
			AssertEquals("Is Cancelled should be N", ZBool.False, journalCopy.AH_IsCancelled);
			AssertEquals("Is Cleared in Cash Book should be empty", ZDateTime.Empty, journalCopy.AH_DateClearedInCashbook);
			AssertEquals("Not allocated should be N", ZBool.False, journalCopy.AH_NotAllocated);
			AssertEquals("Outstanding Amount should be 0", 0.0m, journalCopy.AH_OutstandingAmount);
			AssertEquals("Post 1 should be N", ZBool.False, journalCopy.AH_POST1);
			AssertEquals("Post 2 should be N", ZBool.False, journalCopy.AH_POST2);
			AssertEquals("Post 3 should be N", ZBool.False, journalCopy.AH_POST3);
			AssertEquals("Post 4 should be N", ZBool.False, journalCopy.AH_POST4);
			AssertEquals("Posted To EFT should be N", ZBool.False, journalCopy.AH_PostedToEFT);
			AssertEquals("Posted To GL should be N", "N", journalCopy.AH_PostToGL);
			AssertEquals("Receipt Batch Num should be empty", ZString.Empty, journalCopy.AH_ReceiptBatchNo);
			AssertEquals("Transaction Belongs To Group should be empty", ZGuid.Empty, journalCopy.AH_TransactionBelongsToGroup);
			AssertEquals("Invoice Term should be empty", ZString.Empty, journalCopy.AH_InvoiceTerm);
			AssertEquals("Invoice Term Days should be 0", (ZByte)0, journalCopy.AH_InvoiceTermDays);

			AssertEquals("Should be 2 Lines added", 2, journalCopy.GLJournalLines.Count);
			var jnlCopyLine1 = journalCopy.GLJournalLines[0];
			var jnlCopyLine2 = journalCopy.GLJournalLines[1];

			AssertEquals("Line Type should be transaction type for Row 1", transactionType, jnlCopyLine1.AL_LineType);
			AssertEquals("Line Type should be transaction type for Row 2", transactionType, jnlCopyLine2.AL_LineType);

			AssertEquals("Line Description should be same as original Line", lineDesc, jnlCopyLine1.AL_Desc);
			AssertEquals("Line Description should be same as original Line", lineDesc, jnlCopyLine2.AL_Desc);

			AssertEquals("Line Amount should be same as original Line", lineAmount, jnlCopyLine1.AL_OSExTaxAmount);
			AssertEquals("Line Amount should be same as original Line", -lineAmount, jnlCopyLine2.AL_OSExTaxAmount);

			AssertEquals("Line Amount should be same as original Line", lineAmount, jnlCopyLine1.AL_LocalExTaxAmount);
			AssertEquals("Line Amount should be same as original Line", -lineAmount, jnlCopyLine2.AL_LocalExTaxAmount);

			AssertEquals("Currency should be same as original line", currencyNK, jnlCopyLine1.AL_RX_NKTransactionCurrency);
			AssertEquals("Currency should be same as original line", currencyNK, jnlCopyLine2.AL_RX_NKTransactionCurrency);

			AssertEquals("Post Date should be end of current period", currPeriodEndDate, jnlCopyLine1.AL_PostDate.Date);
			AssertEquals("Post Date should be end of current period", currPeriodEndDate, jnlCopyLine2.AL_PostDate.Date);

			AssertEquals("Post to GL should be N", "N", jnlCopyLine1.AL_PostToGL);
			AssertEquals("Post to GL should be N", "N", jnlCopyLine2.AL_PostToGL);

			AssertEquals("Reverse Date should be empty", ZDateTime.Empty, jnlCopyLine1.AL_ReverseDate);
			AssertEquals("Reverse Date should be empty", ZDateTime.Empty, jnlCopyLine2.AL_ReverseDate);

			AssertEquals("Reverse to GL should be N", "N", jnlCopyLine1.AL_ReverseToGL);
			AssertEquals("Reverse to GL should be N", "N", jnlCopyLine2.AL_ReverseToGL);

			AssertEquals("Header FK should be copy PK", journalCopy.PK, jnlCopyLine1.AL_AH);
			AssertEquals("Header FK should be copy PK", journalCopy.PK, jnlCopyLine2.AL_AH);

			AssertEquals("Line GL Header should be same as original line", gLAccount, jnlCopyLine1.AL_AG);
			AssertEquals("Line GL Header should be same as original line", gLAccount, jnlCopyLine2.AL_AG);
		}

		public void TestCopyJournal_PostDateEnabled()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var postDate = new ZDateTime(2020, 1, 22);
				var currPeriodEndDate = CurrentPeriod.AM_EndDate.Date;
				var dueDate = new ZDateTime(2020, 2, 22);
				var currPeriodStartDate = CurrentPeriod.AM_StartDate.Date;

				var journalToCopy = SetupJournalAndLinesForCopy(TransactionTypes.GLStandardJournal, "12345678", postDate, dueDate, 10);
				var revJournalToCopy = SetupJournalAndLinesForCopy(TransactionTypes.GLReversingJournal, "87654321", postDate, dueDate, 10);

				Factory.Save();

				const string originalMsg = "Post Date should be as the original";
				const string notEndPeriodMsg = "Post Date should NOT be end of current period";
				const string nullDueDateMsg = "Due Date should be empty";
				const string notStartPeriodMsg = "Due Date should NOT be start of current period";

				var journalCopy = (GLJournal)((ITemplateCopyable)journalToCopy).TemplateCopy();

				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, journalCopy.AH_PostDate.Date);
				AssertEquals(originalMsg, postDate, journalCopy.AH_PostDate.Date);

				var firstLine = journalCopy.GLJournalLines[0];
				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, firstLine.AL_PostDate.Date);
				AssertEquals(originalMsg, postDate, firstLine.AL_PostDate.Date);
				var secLine = journalCopy.GLJournalLines[1];
				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, secLine.AL_PostDate.Date);
				AssertEquals(originalMsg, postDate, secLine.AL_PostDate.Date);

				var revJournalCopy = (GLJournal)((ITemplateCopyable)revJournalToCopy).TemplateCopy();

				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, revJournalCopy.AH_PostDate.Date);
				AssertEquals(originalMsg, postDate, revJournalCopy.AH_PostDate.Date);
				AssertNotEquals(notStartPeriodMsg, currPeriodStartDate, revJournalCopy.AH_DueDate.Date);
				AssertEquals(nullDueDateMsg, ZDate.Empty, revJournalCopy.AH_DueDate.Date);

				firstLine = revJournalCopy.GLJournalLines[0];
				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, firstLine.AL_PostDate.Date);
				AssertEquals(originalMsg, postDate, firstLine.AL_PostDate.Date);
				AssertNotEquals(notStartPeriodMsg, currPeriodStartDate, firstLine.AL_ReverseDate.Date);
				AssertEquals(nullDueDateMsg, ZDate.Empty, firstLine.AL_ReverseDate.Date);
				secLine = revJournalCopy.GLJournalLines[1];
				AssertNotEquals(notEndPeriodMsg, currPeriodEndDate, secLine.AL_PostDate.Date);
				AssertEquals(originalMsg, postDate, secLine.AL_PostDate.Date);
				AssertNotEquals(notStartPeriodMsg, currPeriodStartDate, secLine.AL_ReverseDate.Date);
				AssertEquals(nullDueDateMsg, ZDate.Empty, secLine.AL_ReverseDate.Date);
			}
		}

		GLJournal SetupJournalAndLinesForCopy(string transType, string transNum, ZDateTime postDate, ZDateTime dueDate, ZDecimal lineAmount)
		{
			var journalToCopy = TestObjectCreator.CreateGLJournal(transType, ZDateTime.Now, postDate, dueDate);

			journalToCopy.AH_TransactionNum = transNum;
			journalToCopy.AH_TransactionCategory = "ALL";
			journalToCopy.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			journalToCopy.AH_GB = GlbBranch.CurrentBranch.PK;
			journalToCopy.AH_GE = GlbDepartment.CurrentDepartment.PK;

			var gLAccount = TestCaseHelper.GetFirstPKFromTable(AccGLHeader.Schema.TableName);
			TestObjectCreator.CreateGLJournalLine(journalToCopy, lineAmount, DebitCredit.DR, gLAccount);
			journalToCopy.Balance();
			journalToCopy.GLJournalLines[1].AL_AG = gLAccount;

			return journalToCopy;
		}

		public void TestSetInvoiceDateOnSave()
		{
			SetupForSave();
			Journal.AH_InvoiceDate = ZDateTime.Empty;
			Journal.Factory.Save();
			AssertEquals("Invoice Date", ZDateTime.Now.Date, Journal.AH_InvoiceDate.Date);
		}

		public void TestSetGLJournalDefaultValues()
		{
			AssertEquals("Invoice Date", Env.Time.CurrentLocalDateTime.Date, Journal.AH_InvoiceDate.Date);
			AssertEquals("TransactionType", TransactionTypes.GLStandardJournal, Journal.AH_TransactionType);
			AssertDescription();
		}

		protected virtual void AssertDescription()
		{
			AssertEquals("Description", "GENERAL LEDGER JOURNAL", Journal.AH_Desc);
		}

		public void TestBalancingDebitBalance()
		{
			GLJournalLine newLine = TestJournal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = 200.00m;
			newLine.DebitCreditSign = nameof(DebitCredit.DR);

			Assert("Should not balance", !TestJournal.IsBalanced);
			TestJournal.Balance();
			Assert("Should now balance", TestJournal.IsBalanced);
			AssertEquals("Unsigned amount should be 200", 200.00m, TestJournal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals("Sign should now be CR", DebitCreditDataEntry.CR, TestJournal.GLJournalLines[1].DebitCreditSign.ToString());
		}

		public virtual void TestBalancingDebitBalanceWithForeignCurrency()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			GLJournalLine newLine = TestJournal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			newLine.UnsignedOSLineAmount = 200.00m;
			newLine.AL_ExchangeRate = 0.90M;
			newLine.DebitCreditSign = nameof(DebitCredit.DR);

			Assert("Should not balance", !TestJournal.IsBalanced);
			TestJournal.Balance();
			Assert("Should now balance", TestJournal.IsBalanced);
			AssertEquals("Should be balanced to balancing account", TestJournal.BalancingAccount.Value, TestJournal.GLJournalLines[1].AL_AG);
			AssertEquals("This would be a local currency line", TestObjectCreator.AUD.RX_Code, TestJournal.GLJournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals("Unsigned OS amount should be 222.22", 222.22M, TestJournal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals("Unsigned LOCAL amount should be 222.22", 222.22M, TestJournal.GLJournalLines[1].UnsignedLocalLineAmount);
			AssertEquals("Sign should now be CR", DebitCreditDataEntry.CR, TestJournal.GLJournalLines[1].DebitCreditSign.ToString());
		}

		public void TestBalancingCreditBalance()
		{
			GLJournalLine newLine = TestJournal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = 200.00m;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			AssertEquals("Total should be negative 200", -200.00m, TestJournal.AH_OSExTaxAmount);
			Assert("Should not balance", !TestJournal.IsBalanced);
			TestJournal.Balance();
			Assert("Should now balance", TestJournal.IsBalanced);
			AssertEquals("Unsigned amount should be 200", 200.00m, TestJournal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals("Sign should now be DR", DebitCreditDataEntry.DR, TestJournal.GLJournalLines[1].DebitCreditSign.ToString());
		}

		public virtual void TestBalancingCreditBalanceForeignCurrency()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			GLJournalLine newLine = TestJournal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			newLine.UnsignedOSLineAmount = 200.00M;
			newLine.AL_ExchangeRate = 0.90M;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			Assert("Should not balance", !TestJournal.IsBalanced);
			TestJournal.Balance();
			Assert("Should now balance", TestJournal.IsBalanced);
			AssertEquals("Should be balanced to balancing account", TestJournal.BalancingAccount.Value, TestJournal.GLJournalLines[1].AL_AG);
			AssertEquals("This would be a local currency line", TestObjectCreator.AUD.RX_Code, TestJournal.GLJournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals("Unsigned OS amount should be 222.22", 222.22M, TestJournal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals("Unsigned Local amount should be 222.22", 222.22M, TestJournal.GLJournalLines[1].UnsignedLocalLineAmount);
			AssertEquals("Sign should now be DR", DebitCreditDataEntry.DR, TestJournal.GLJournalLines[1].DebitCreditSign.ToString());
		}

		[TestDate(2014, 3, 20)]
		public void TestChangingPostInPeriodChangesDefaultExchangeRate_ForExistingLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);

			ZInt feb = 201402;

			ZDateTime febStartDate = new ZDateTime(2014, 2, 1);
			ZDateTime febEndDate = new ZDateTime(2014, 2, 28);

			ZInt march = 201403;

			ZDateTime marchStartDate = new ZDateTime(2014, 3, 1);
			ZDateTime marchEndDate = new ZDateTime(2014, 3, 31);

			ZDecimal usdFebExhangeRate = 0.91M;
			ZDecimal usdMarchExhangeRate = 0.85M;
			ZDecimal eurFebExchangeRate = 0.65M;

			periodHelper.SetupSinglePeriod(feb, febStartDate, febEndDate);
			periodHelper.SetupSinglePeriod(march, marchStartDate, marchEndDate);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.PeriodEndRate, usdFebExhangeRate, febStartDate, febEndDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.PeriodEndRate, usdMarchExhangeRate, marchStartDate, marchEndDate);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.PeriodEndRate, eurFebExchangeRate, febStartDate, febEndDate);

			TestJournal.AH_PostDate = ZDateTime.Empty;
			TestJournal.PostPeriod = feb;

			var line = TestJournal.GLJournalLines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("line exchange rate should be of Feb's", usdFebExhangeRate, line.AL_ExchangeRate);

			var line2 = TestJournal.GLJournalLines.AddNew();
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.EUR.RX_Code;
			AssertEquals("line exchange rate should be of Feb's", eurFebExchangeRate, line2.AL_ExchangeRate);

			TestJournal.PostPeriod = march;
			AssertEquals("line exchange rate should be of March's", usdMarchExhangeRate, line.AL_ExchangeRate);
			AssertEquals("line exchange rate should be of March's", 0M, line2.AL_ExchangeRate);

			line.AL_ExchangeRate = 0.89M;

			TestJournal.PostPeriod = feb;
			AssertEquals("line exchange rate should not be updated to default to Febs PER as user has manually changed the value of exchange rate.", 0.89M, line.AL_ExchangeRate);
			AssertEquals("line exchange rate should be of Feb's", eurFebExchangeRate, line2.AL_ExchangeRate);
		}

		public virtual void TestBalancingRowUsesClearingAccount()
		{
			Guid fakeClearingAccount = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fakeClearingAccount);

			GLJournalLine newLine = TestJournal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = 200.00m;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			TestJournal.Balance();
			AssertEquals("Balancing row should have GL Journal Clearing Account", fakeClearingAccount, TestJournal.GLJournalLines[1].AL_AG);
		}

		public void TestValidateAH_Desc()
		{
			TestJournal.AH_Desc = string.Empty;
			Assert("Should be errors on Description", TestJournal.AH_DescInfo.HasErrors());
			TestJournal.AH_Desc = "Description";
			Assert("Should now be no errors on Description", !TestJournal.AH_DescInfo.HasErrors());
		}

		public void TestValidateAH_TransactionType()
		{
			string invalidInput = "ABC";
			Journal.AH_TransactionType = invalidInput;
			Assert("Should be errors on Journal Type", Journal.AH_TransactionTypeInfo.HasErrors());
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			Assert("Should be no errors on Journal Type", !Journal.AH_TransactionTypeInfo.HasErrors());
			Journal.AH_TransactionType = string.Empty;
			Assert("Should be errors for empty input", Journal.AH_TransactionTypeInfo.HasErrors());
		}

		public virtual void TestChangeOfJournalType()
		{
			const string periodNotVisibleMsg = "Reverse Period should not be visible";
			const string periodVisibleMsg = "Reverse Period should be visible";
			const string dateNotVisibleMsg = "Reverse Date should not be visible";
			const string dateVisibleMsg = "Reverse Date should be visible";

			Assert(periodNotVisibleMsg, Journal.AgePeriodInfo.ReadOnly);
			Assert(dateNotVisibleMsg, Journal.AH_DueDateInfo.ReadOnly);

			Journal.AgePeriod = 200312;
			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			Assert(periodVisibleMsg, !Journal.AgePeriodInfo.ReadOnly);
			Assert(dateVisibleMsg, !Journal.AH_DueDateInfo.ReadOnly);

			Journal.AgePeriod = 200312;
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			AssertEquals("Age Period should reset to zero when Age Period is set to ReadOnly", 0, Journal.AgePeriod);
			Assert(periodNotVisibleMsg, Journal.AgePeriodInfo.ReadOnly);
			Assert(dateNotVisibleMsg, Journal.AH_DueDateInfo.ReadOnly);

			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Assert(periodVisibleMsg, !Journal.AgePeriodInfo.ReadOnly);
			Assert(dateNotVisibleMsg, Journal.AH_DueDateInfo.ReadOnly);

			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			Assert(periodVisibleMsg, !Journal.AgePeriodInfo.ReadOnly);
			Assert(dateVisibleMsg, !Journal.AH_DueDateInfo.ReadOnly);
		}

		public void TestJournalTotalCalculation()
		{
			AddLine(50.00m, DebitCredit.DR);
			AssertEquals("Total should be positive 50", 50m, Journal.AH_OSExTaxAmount);
			AddLine(50.00m, DebitCredit.CR);
			AssertEquals("Total should be Zero", 0m, Journal.AH_OSExTaxAmount);
			AddLine(100.00m, DebitCredit.CR);
			AssertEquals("Total should be negative 100", -100m, Journal.AH_OSExTaxAmount);
			AddLine(100.00m, DebitCredit.DR);
			AssertEquals("Total should be Zero", 0m, Journal.AH_OSExTaxAmount);
		}

		public virtual void TestDefaultDescriptionOnChangeOfJournalType()
		{
			CodeDescriptionPairList journalTypeList = new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes);
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			string expectedDesc = "GENERAL LEDGER JOURNAL";
			AssertEquals("Description", expectedDesc, Journal.AH_Desc);
			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			expectedDesc = "GL " + journalTypeList.GetDescriptionFromCode(TransactionTypes.GLAutoJournal).ToUpper();
			AssertEquals("Description after not changing from default", expectedDesc, Journal.AH_Desc);
		}

		[TestDate(2020, 8, 20, 11, 30, 0)]
		public override void TestPostDateImplementation()
		{
			var journal = Factory.New<GLJournal>();
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;
			const string defaultMsg = "Post Date should default to last day of period";
			const string periodMsg = "Post Date should be last day of period when setting post date";

			AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);

			var now = ZDateTime.Today.Date;
			journal.AH_PostDate = now;
			AssertEquals(periodMsg, periodEndDate, journal.AH_PostDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				const string notPeriodMsg = "Post Date should NOT be last day of period when setting post date";

				journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = now;
				AssertNotEquals("GJL " + notPeriodMsg + ", if EnablePostDateGLJournal = YES", periodEndDate, journal.AH_PostDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = now;
				AssertNotEquals("RJL " + notPeriodMsg + ", if EnablePostDateGLJournal = YES", periodEndDate, journal.AH_PostDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = now;
				AssertNotEquals("NJL " + notPeriodMsg + ", if EnablePostDateGLJournal = YES", periodEndDate, journal.AH_PostDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = now;
				AssertEquals("AJL " + periodMsg + ", if EnablePostDateGLJournal = YES", periodEndDate, journal.AH_PostDate.Date);
			}
		}

		[TestDate(2020, 8, 20, 11, 30, 0)]
		public void TestDueDateImplementation()
		{
			var journal = Factory.New<GLJournal>();

			var periodStartDate = CurrentPeriod.AM_StartDate.Date;
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;
			const string defaultMsg = "Due Date should default to empty";
			const string startPeriodMsg = "Due Date should be first day of period when setting due date";
			const string endPeriodMsg = "Due Date should be last day of period when setting due date";
			var now = ZDateTime.Today.Date;

			AssertEquals(defaultMsg, ZDate.Empty, journal.AH_DueDate.Date);

			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			journal.AH_DueDate = now;
			AssertEquals("RJL " + startPeriodMsg, periodStartDate, journal.AH_DueDate.Date);

			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			AssertEquals(periodStartDate, journal.AH_DueDate.Date);
			journal.AH_DueDate = now;
			AssertEquals("AJL " + endPeriodMsg, periodEndDate, journal.AH_DueDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				const string notPeriodMsg = "Due Date should NOT be first day of period when setting due date";

				journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(defaultMsg, ZDate.Empty, journal.AH_DueDate.Date);
				journal.AH_DueDate = now;
				AssertEquals("GJL " + notPeriodMsg, now, journal.AH_DueDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				AssertEquals(now, journal.AH_DueDate.Date);
				journal.AH_DueDate = now;
				AssertNotEquals("RJL " + notPeriodMsg + ", if EnablePostDateGLJournal = YES", periodStartDate, journal.AH_DueDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(defaultMsg, ZDate.Empty, journal.AH_DueDate.Date);
				journal.AH_DueDate = now;
				AssertEquals("NJL " + notPeriodMsg, now, journal.AH_DueDate.Date);

				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				AssertEquals(now, journal.AH_DueDate.Date);
				journal.AH_DueDate = now;
				AssertEquals("AJL " + endPeriodMsg + ", if EnablePostDateGLJournal = YES", periodEndDate, journal.AH_DueDate.Date);
			}
		}

		[TestDate(2020, 8, 20)]
		public void TestPostDateAndDueDateAlwaysEqualStartOrEndDateOfPeriod()
		{
			var dateWithNoPeriod = new ZDateTime(2079, 1, 1);
			var endDateWithNoPeriod = dateWithNoPeriod.AddMonths(1);

			var periodStart = CurrentPeriod.AM_StartDate;
			var periodStartDate = CurrentPeriod.AM_StartDate.Date;
			var periodEnd = CurrentPeriod.AM_EndDate;
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;
			var nextPeriodStart = FuturePeriod.AM_StartDate;
			var nextPeriodStartDate = FuturePeriod.AM_StartDate.Date;
			var nextPeriodEnd = FuturePeriod.AM_EndDate;
			var nextPeriodEndDate = FuturePeriod.AM_EndDate.Date;

			const string defaultMsg = "Post Date should default to last day of period";
			const string alwaysMsg = "Post Date should always equal last day of period";
			const string defaultDueMsg = "Due Date should default to null";
			const string noPeriodMsg = "For dates with no period, expected behaviour is to leave it be.";

			GLJournal journal = Factory.New<GLJournal>();
			AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
			AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

			var today = ZDateTime.Today;
			var nextMonth = today.AddMonths(1);

			// GJL
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;

			journal.AH_PostDate = dateWithNoPeriod;
			AssertEquals(noPeriodMsg, dateWithNoPeriod, journal.AH_PostDate);

			string message = "GJL " + alwaysMsg;
			journal.AH_PostDate = today;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodStart;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodEnd;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

				message = "GJL Post Date should NOT necessary equal last day of period, if EnablePostDateGLJournal = YES";
				journal.AH_PostDate = today;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodStart;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodEnd;
				AssertEquals(periodEndDate, journal.AH_PostDate.Date);
			}

			// AJL
			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
			AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

			journal.AH_PostDate = dateWithNoPeriod;
			AssertEquals(noPeriodMsg, dateWithNoPeriod, journal.AH_PostDate);
			journal.AH_DueDate = endDateWithNoPeriod;
			AssertEquals(noPeriodMsg, endDateWithNoPeriod, journal.AH_DueDate);

			message = "AJL " + alwaysMsg;
			journal.AH_PostDate = today;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodStart;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodEnd;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);

			message = "AJL End Date should always equal last day of period";
			journal.AH_DueDate = nextMonth;
			AssertEquals(message, nextPeriodEndDate, journal.AH_DueDate.Date);
			journal.AH_DueDate = nextPeriodStart;
			AssertEquals(message, nextPeriodEndDate, journal.AH_DueDate.Date);
			journal.AH_DueDate = nextPeriodEnd;
			AssertEquals(message, nextPeriodEndDate, journal.AH_DueDate.Date);

			// NJL
			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
			AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

			message = "NJL " + alwaysMsg;
			journal.AH_PostDate = today;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodStart;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodEnd;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

				message = "NJL Post Date should NOT necessary equal last day of period, if EnablePostDateGLJournal = YES";
				journal.AH_PostDate = today;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodStart;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodEnd;
				AssertEquals(periodEndDate, journal.AH_PostDate.Date);
			}

			// RJL
			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
			AssertEquals(defaultDueMsg, ZDate.Empty, journal.AH_DueDate.Date);

			journal.AH_PostDate = dateWithNoPeriod;
			AssertEquals(noPeriodMsg, dateWithNoPeriod, journal.AH_PostDate);
			journal.AH_DueDate = endDateWithNoPeriod;
			AssertEquals(noPeriodMsg, endDateWithNoPeriod, journal.AH_DueDate);

			message = "RJL " + alwaysMsg;
			journal.AH_PostDate = today;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodStart;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);
			journal.AH_PostDate = periodEnd;
			AssertEquals(message, periodEndDate, journal.AH_PostDate.Date);

			message = "RJL End Date should always equal first day of period";
			journal.AH_DueDate = nextMonth;
			AssertEquals(message, nextPeriodStartDate, journal.AH_DueDate.Date);
			journal.AH_DueDate = nextPeriodStart;
			AssertEquals(message, nextPeriodStartDate, journal.AH_DueDate.Date);
			journal.AH_DueDate = nextPeriodEnd;
			AssertEquals(message, nextPeriodStartDate, journal.AH_DueDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				AssertEquals(defaultMsg, periodEndDate, journal.AH_PostDate.Date);
				AssertEquals(defaultDueMsg, nextPeriodStartDate, journal.AH_DueDate.Date);

				message = "RJL Post Date should NOT necessary equal last day of period, if EnablePostDateGLJournal = YES";
				journal.AH_PostDate = today;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodStart;
				AssertNotEquals(message, periodEndDate, journal.AH_PostDate.Date);
				journal.AH_PostDate = periodEnd;
				AssertEquals(periodEndDate, journal.AH_PostDate.Date);

				message = "RJL End Date should NOT necessary equal first day of period, if EnablePostDateGLJournal = YES";
				journal.AH_DueDate = nextMonth;
				AssertNotEquals(message, periodStartDate, journal.AH_DueDate.Date);
				journal.AH_DueDate = periodStart;
				AssertEquals(periodStartDate, journal.AH_DueDate.Date);
				journal.AH_DueDate = periodEnd;
				AssertNotEquals(message, periodStartDate, journal.AH_DueDate.Date);
			}
		}

		public void TestSaving_GLStandardJournal()
		{
			var period = CurrentPeriod.AM_Period;
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;

			Journal.PostPeriod = period;
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;

			var today = ZDateTime.Today;
			Journal.Factory.Save();

			AssertEquals("Invoice Date should be todays date/time", today.Date, Journal.AH_InvoiceDate.Date);
			AssertEquals("OS Total should be equal to invoice amount", Journal.AH_LocalExTaxAmount, Journal.AH_OSTotalAmount);
			AssertEquals("Post Date should be last day of period", periodEndDate, Journal.AH_PostDate.Date);
			AssertEquals("Age Date should be empty", ZDateTime.Empty, Journal.AH_DueDate);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Journal.AH_PostDate = Journal.AH_PostDate.AddDays(-1);
				AssertNotEquals("Post Date should NOT be last day of period, if EnablePostDateGLJournal = YES", periodEndDate, Journal.AH_PostDate.Date);
			}
			Journal.AH_PostDate = Journal.AH_PostDate;
			AssertEquals("Post Date should be last day of period when set with EnablePostDateGLJournal = NO", periodEndDate, Journal.AH_PostDate.Date);
		}

		public void TestSaving_GLReversingJournal()
		{
			var period = CurrentPeriod.AM_Period;
			var futurePeriod = FuturePeriod.AM_Period;
			var periodStartDate = FuturePeriod.AM_StartDate.Date;
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;

			Journal.PostPeriod = period;
			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;

			var today = ZDateTime.Today;
			Journal.AgePeriod = futurePeriod;
			Journal.Factory.Save();

			AssertEquals("Invoice Date should be todays date/time", today.Date, Journal.AH_InvoiceDate.Date);
			AssertEquals("OS Total should be equal to invoice amount", Journal.AH_LocalExTaxAmount, Journal.AH_OSTotalAmount);
			AssertEquals("Post Date should be last day of period", periodEndDate, Journal.AH_PostDate.Date);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Journal.AH_PostDate = Journal.AH_PostDate.AddDays(-1);
				AssertNotEquals("Post Date should NOT be last day of period, if EnablePostDateGLJournal = YES", periodEndDate, Journal.AH_PostDate.Date);
				Journal.AH_DueDate = Journal.AH_DueDate.AddDays(1);
				AssertNotEquals("Due Date should NOT be first day of period, if EnablePostDateGLJournal = YES", periodStartDate, Journal.AH_DueDate.Date);
			}
			Journal.AH_PostDate = Journal.AH_PostDate;
			AssertEquals("Post Date should be last day of period when set with EnablePostDateGLJournal = NO", periodEndDate, Journal.AH_PostDate.Date);
			Journal.AH_DueDate = Journal.AH_DueDate;
			AssertEquals("Age Date should be first day of period when set with EnablePostDateGLJournal = NO", periodStartDate, Journal.AH_DueDate.Date);

			Journal.Factory.Save();

			AssertEquals("Post Date should be last day of period", periodEndDate, Journal.AH_PostDate.Date);
			AssertEquals("Age Date should be first day of period", periodStartDate, Journal.AH_DueDate.Date);
		}

		public void TestSaving_GLAutoJournal()
		{
			var period = CurrentPeriod.AM_Period;
			var periodEndDate = CurrentPeriod.AM_EndDate.Date;

			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.PostPeriod = period;
			Journal.AgePeriod = period;

			var today = ZDateTime.Today;
			Journal.Factory.Save();

			AssertEquals("Invoice Date should be todays date/time", today.Date, Journal.AH_InvoiceDate.Date);
			AssertEquals("OS Total should be equal to invoice amount", Journal.AH_LocalExTaxAmount, Journal.AH_OSTotalAmount);
			AssertEquals("Post Date should be last day of period", periodEndDate, Journal.AH_PostDate.Date);
			AssertEquals("Age Date should be last day of period", periodEndDate, Journal.AH_DueDate.Date);
		}

		public override void TestTransactionNumberOnSave()
		{
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			var nextJournalNumber = Env.NumberFountains.GLJournal.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);
			Journal.Factory.Save();

			AssertEquals("Transaction number should be same as original", nextJournalNumber, Journal.AH_TransactionNum);
		}

		public void TestAgePeriodValidation()
		{
			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.PostPeriod = PeriodManagementTestHelper.CurrentPeriod.AM_Period;
			Journal.AgePeriod = PeriodManagementTestHelper.FuturePeriod.AM_Period;
			var periodInfo = Journal.AgePeriodInfo;
			Assert("Age period should not have errors", !periodInfo.HasErrors());

			Journal.AgePeriod = PeriodManagementTestHelper.InvalidPeriodInt;
			Assert("Age Period is invalid and should have errors", periodInfo.HasErrors());
			string expectedError = "This period is invalid. Please go to Period Management to setup periods";
			AssertEquals("Age Period Error", expectedError, periodInfo.GetErrors().GetFirstMessage());

			Journal.AgePeriod = PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period;
			Assert("Age Period is less than PostPeriod and should have errors", periodInfo.HasErrors());
			expectedError = "Reverse/Ending Period must be greater than post period";
			AssertEquals("Reverse/Ending Period is less than post period and should be in error", expectedError, periodInfo.GetErrors().GetFirstMessage());

			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			Journal.AgePeriod = 0;
			Assert("Should not validate age period for standard journals", !periodInfo.HasErrors());

			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			Journal.PostPeriod = PeriodManagementTestHelper.CurrentPeriod.AM_Period;
			Journal.AgePeriod = PeriodManagementTestHelper.FuturePeriod.AM_Period;
			Assert("Age period should not have errors", !periodInfo.HasErrors());
			var dateInfo = Journal.AH_DueDateInfo;
			Assert("Age date should not have errors", !dateInfo.HasErrors());

			Journal.AgePeriod = PeriodManagementTestHelper.InvalidPeriodInt;
			Assert("Age Period is invalid and should have errors", periodInfo.HasErrors());
			Assert("Age Date is invalid and should have errors", dateInfo.HasErrors());
			expectedError = "This period is invalid. Please go to Period Management to setup periods";
			AssertEquals("Age Period Error", expectedError, periodInfo.GetErrors().GetFirstMessage());
			AssertCollectionContains("Age Date Error", dateInfo.GetErrors(), err => err.Message == expectedError);

			Journal.AgePeriod = PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period;
			Assert("Age Period is less than PostPeriod and should have errors", periodInfo.HasErrors());
			Assert("Age Date is less than PostPeriod and should have errors", dateInfo.HasErrors());
			expectedError = "Reverse/Ending Period must be greater than post period";
			AssertEquals("Reverse/Ending Period is less than post period and should be in error", expectedError, periodInfo.GetErrors().GetFirstMessage());
			AssertCollectionContains("Reverse/Ending Date is less than Post Date and should be in error", dateInfo.GetErrors(), err => err.Message == expectedError);
		}

		public void TestUpdatingInvalidAgePeriodToValidShowsError()
		{
			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.AgePeriod = 202002;
			Journal.PostPeriod = 202001;
			Assert("Age Period should have errors", Journal.AgePeriodInfo.HasErrors());
			Assert("Precondition: DueDate should be empty", Journal.AH_DueDate.IsEmpty);

			PeriodManagementTestHelper.PostPeriodsForEntireYear(2020);
			Journal.Validation.ValidateAll();

			Assert("Age Period should have an error", Journal.AgePeriodInfo.HasErrors());
			AssertHasError(Journal.AgePeriodInfo, GLJournalValidation.PeriodCreatedConcurrentError(Journal.AgePeriod, Journal.AgePeriodInfo.HumanReadableName, Journal.AH_DueDateInfo.HumanReadableName));
		}

		[TestDate(2019, 06, 01)]
		public void TestUpdatingInvalidPostPeriodToValidShowsError()
		{
			Assert("Pre-condition: User should have Post Period Update Rights allowed", Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed);
			ToggleSetCanUserPostToPreviousPeriods(true);

			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.AgePeriod = 202002;
			Journal.PostPeriod = 202001;
			Assert("Post Period should have errors", Journal.PostPeriodInfo.HasErrors());
			Assert("Post Date should be empty", Journal.AH_PostDate.IsEmpty);

			PeriodManagementTestHelper.PostPeriodsForEntireYear(2020);
			Journal.Validation.ValidateAll();

			Assert("Post Period should have errors", Journal.PostPeriodInfo.HasErrors());
			AssertHasError(Journal.PostPeriodInfo, GLJournalValidation.PeriodCreatedConcurrentError(Journal.PostPeriod, Journal.PostPeriodInfo.HumanReadableName, Journal.AH_PostDateInfo.HumanReadableName));
		}

		public void TestPostPeriodValidation()
		{
			var originalGeneralLedgerPostToPreviousOpenPeriodAllows = Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed;
			try
			{
				Journal.PostPeriod = CurrentPeriod.AM_Period;
				var periodInfo = Journal.PostPeriodInfo;
				Assert("Post Period should not have errors", !periodInfo.HasErrors());
				var dateInfo = Journal.AH_PostDateInfo;
				Assert("Post Date should not have errors", !dateInfo.HasErrors());

				Journal.PostPeriod = 123456;
				Assert("Post period invalid and should now have errors", periodInfo.HasErrors());
				Assert("Post date invalid and should now have errors", dateInfo.HasErrors());
				string expectedError = "This period is invalid. Please go to Period Management to setup periods";
				AssertEquals("Invalid Period Error:", expectedError, periodInfo.GetErrors().GetFirstMessage());
				AssertCollectionContains("Invalid Period Date Error", dateInfo.GetErrors(), err => err.Message == expectedError);

				Journal.PostPeriod = PreviousGLClosedPeriod.AM_Period;
				Assert("Post period closed and should now have errors", periodInfo.HasErrors());
				Assert("Post period closed and Date should now have errors", dateInfo.HasErrors());
				expectedError = "The General Ledger is closed for 999995 period.  Only Presentation Journals can now be created. A Presentation Category must be assigned.";
				AssertEquals("Closed Period Error:", expectedError, periodInfo.GetErrors().GetFirstMessage());
				AssertCollectionContains("Closed Period Date Error", dateInfo.GetErrors(), err => err.Message == expectedError);

				ToggleSetCanUserPostToPreviousPeriods(true);
				Journal.PostPeriod = PreviousOpenPeriod.AM_Period;
				Journal.AH_PostDate = Journal.AH_PostDate; //refresh
				Assert("Post Period should not have errors", !periodInfo.HasErrors());
				Assert("Post Date should not have errors", !dateInfo.HasErrors());

				ToggleSetCanUserPostToPreviousPeriods(false);
				Journal.PostPeriod = 0;
				Journal.PostPeriod = PreviousOpenPeriod.AM_Period;
				Assert("Post Period is previous open period, rights are declined and should error", periodInfo.HasErrors());
				Assert("Post Date is in previous open period, rights are declined and should error", dateInfo.HasErrors());
				expectedError = "You do not have sufficient rights to post to previous periods";
				AssertEquals("Post Period is Previous open period but rights are disallowed", expectedError, periodInfo.GetErrors().GetFirstMessage());
				AssertCollectionContains("Post Date is in Previous open period but rights are disallowed", dateInfo.GetErrors(), err => err.Message == expectedError);
			}
			finally
			{
				Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed = originalGeneralLedgerPostToPreviousOpenPeriodAllows;
			}
		}

		protected virtual void ToggleSetCanUserPostToPreviousPeriods(bool flag)
		{
			((GLJournalForTest)Journal).SetCanUserPostToPreviousPeriods(flag);
		}

		public override void TestPostingToPriorNonClosedPeriodGivesWarning()
		{
			Assert("should not have errors to start off with", !Journal.AH_PostDateInfo.HasErrors());
			Journal.AH_PostDate = PreviousOpenPeriod.AM_StartDate;
			Assert("Should not have any warnings", !Journal.AH_PostDateInfo.HasWarnings());
		}

		public void TestLoadLinesInCorrectSequence()
		{
			SetupForSave();

			((GLJournalLine)Journal.Lines[0]).UnsignedOSLineAmount = 200.00m;
			((GLJournalLine)Journal.Lines[0]).DebitCreditSign = nameof(DebitCredit.DR);

			((GLJournalLine)Journal.Lines[1]).UnsignedOSLineAmount = 150.00m;
			((GLJournalLine)Journal.Lines[1]).DebitCreditSign = nameof(DebitCredit.CR);

			AddLine(50, DebitCredit.CR);

			Factory.Save();

			BusinessObjectFactory newFactoryToTestLoad = new BusinessObjectFactory();
			GLJournal loadedJournal = Factory.Load<GLJournal>(Journal.PK);

			GLJournalLine line1 = newFactoryToTestLoad.Load<GLJournalLine>(Journal.GLJournalLines[0].PK);
			GLJournalLine line2 = newFactoryToTestLoad.Load<GLJournalLine>(Journal.GLJournalLines[1].PK);
			GLJournalLine line3 = newFactoryToTestLoad.Load<GLJournalLine>(Journal.GLJournalLines[2].PK);

			// Reset the sequence of the already saved lines

			line1.AL_Sequence = (ZShort)3;
			line2.AL_Sequence = (ZShort)1;
			line3.AL_Sequence = (ZShort)2;

			newFactoryToTestLoad.Save();

			newFactoryToTestLoad = new BusinessObjectFactory();

			ReLoadHeaderForTest();

			AssertEquals("Line 1 absolute amount", 150m, Journal.GLJournalLines[0].UnsignedOSLineAmount);
			AssertEquals("Line 2 absolute amount", 50m, Journal.GLJournalLines[1].UnsignedOSLineAmount);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			Header.AH_TransactionType = TransactionTypes.GLStandardJournal;
			AddLine(200.00m, DebitCredit.DR);
			AddLine(200.00m, DebitCredit.CR);
			Header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			SetupForSave();

			Header.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			GLJournal loadedHeader = (GLJournal)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Header.PK);

			AssertEquals("Total local amount fields should be zero on load", 0m, Header.AH_LocalExTaxAmount);
			AssertEquals("Total local amount fields should be zero on load", 0m, Header.AH_LocalTotalAmount);
			AssertEquals("Total OS amount fields should be zero on load", 0m, Header.AH_OSExTaxAmount);
			AssertEquals("Total OS amount fields should be zero on load", 0m, Header.AH_OSTotalAmount);
		}

		public void TestPostDateValidationAllowsForwardPosting()
		{
			Journal.AH_PostDate = FuturePeriod.AM_EndDate;
			Assert("There should be no errors when setting the post date to a forward date",
				!Journal.AH_PostDateInfo.HasErrors());
		}

		public void TestGLJournalTypeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var testJournal = Factory.New<GLJournal>();
				AssertArrayEqualsByElements("Transaction type list for China", new string[] { "GJL", "NJL" }, testJournal.TransactionType_List.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var testJournalAust = Factory.New<GLJournal>();
				AssertArrayEqualsByElements("Transaction type list", new string[] { "AJL", "RJL", "GJL", "NJL" }, testJournalAust.TransactionType_List.GetAllCodes());
			}
		}

		public void TestGLJournalTypeListFromProvider()
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair("GGL", "Standard Journal"));
			result.Add(new CodeDescriptionPair("NJY", "Note Journal"));

			var mockGlJournalTypesProvider = new Mock<IGLJournalTypesProvider>();
			mockGlJournalTypesProvider.Setup(x => x.GetGLJournalTypes()).Returns(result);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IGLJournalTypesProvider>>().Setup(x => x.Get()).Returns(mockGlJournalTypesProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				var testJournal = Factory.New<GLJournal>();
				AssertArrayEqualsByElements("Transaction type list", new string[] { "GGL", "NJY" }, testJournal.TransactionType_List.GetAllCodes());
			}

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(value: null);
			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				var testJournal = Factory.New<GLJournal>();
				AssertArrayEqualsByElements("Transaction type list", new string[] { "AJL", "RJL", "GJL", "NJL" }, testJournal.TransactionType_List.GetAllCodes());
			}
		}

		public override void TestPostDateOnLinesSameAsHeader()
		{
			var postDate = PeriodManagementTestHelper.PreviousOpenPeriod.AM_EndDate;
			Assert("Precondition: Initial post date on line not equal header date", postDate != DependentLine1.AL_PostDate);
			Assert("Precondition: Initial post date on line not equal header date", postDate != DependentLine2.AL_PostDate);

			HeaderWithLines.AH_PostDate = postDate;

			AssertEquals("both lines should have the correct post date", postDate, DependentLine1.AL_PostDate);
			AssertEquals("both lines should have the correct post date", postDate, DependentLine2.AL_PostDate);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var postDate2 = new ZDateTime(postDate.Year, postDate.Month, 20);
				HeaderWithLines.AH_PostDate = postDate2;

				AssertEquals("both lines should have the correct post date", postDate2, DependentLine1.AL_PostDate);
				AssertEquals("both lines should have the correct post date", postDate2, DependentLine2.AL_PostDate);
			}
		}

		public void TestDueDateOnLinesSameAsHeader()
		{
			var dueDate = PeriodManagementTestHelper.FuturePeriod.AM_StartDate;
			Assert("Precondition: Initial due date on line not equal header date", dueDate != DependentLine1.AL_ReverseDate);
			Assert("Precondition: Initial due date on line not equal header date", dueDate != DependentLine2.AL_ReverseDate);

			HeaderWithLines.AH_DueDate = dueDate;

			AssertEquals("both lines should have the correct due date", dueDate, DependentLine1.AL_ReverseDate);
			AssertEquals("both lines should have the correct due date", dueDate, DependentLine2.AL_ReverseDate);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dueDate2 = new ZDateTime(dueDate.Year, dueDate.Month, 20);
				HeaderWithLines.AH_DueDate = dueDate2;

				AssertEquals("both lines should have the correct due date", dueDate2, DependentLine1.AL_ReverseDate);
				AssertEquals("both lines should have the correct due date", dueDate2, DependentLine2.AL_ReverseDate);
			}
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "GLJ", ((IDocManagerSupport)Factory.New<GLJournal>()).DocManagerInfo.DocManagerCode);
		}

		[TestDate(2016, 1, 4)]
		public void TestSavingProcessWithEdocs()
		{
			Factory.Save();

			GLJournal journal = null;

			Action createGLJournal = () =>
			{
				journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			};

			Action<int, string> assertEdoc = (count, name) =>
			{
				var newFactory1 = new BusinessObjectFactory();
				newFactory1.RefreshEnabled = false;
				var journalInNewFactory = newFactory1.Load<GLJournal>(journal.PK);
				var eDocs = journalInNewFactory.DocManagerInfo.AllEDocs.Cast<IeDoc>().ToArray();
				AssertEquals(count, eDocs.Length);

				var fileName = string.Format(name, journal.AH_TransactionNum);
				var eDoc = eDocs.First(x => x.FileName == fileName);
				AssertNotNull(eDoc);
			};

			//1.	New journal
			//a.	Create new journal – only document added on saving should be saved.
			createGLJournal();
			Factory.Save();
			assertEdoc(1, "GL {0}_BNE_E_4012016 120000 AM.pdf");

			//b.	Create new journal. Add eDoc – new document added on saving should be saved together with the document added before.
			createGLJournal();
			journal.DocManagerInfo.AddFileOrDocument(new byte[] { 1 }, "test 1.b", "INV");
			Factory.Save();
			assertEdoc(2, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(2, "test 1.b");

			//2.	Existing Journal
			createGLJournal();
			Factory.Save();
			assertEdoc(1, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			//a.	Add eDoc and don’t change journal – added document should be saved.
			journal.DocManagerInfo.AddFileOrDocument(new byte[] { 1 }, "test 2.a", "INV");
			Factory.Save();
			assertEdoc(2, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(2, "test 2.a");

			//b.	Add eDoc and change journal – new document added on saving should be saved together with the document added before.5
			journal.DocManagerInfo.AddFileOrDocument(new byte[] { 2 }, "test 2.b", "INV");
			journal.AH_PostDate = journal.AH_PostDate.AddMinutes(1);
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				Factory.Save();
			}
			assertEdoc(4, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(4, "GL {0}_BNE_E_4012016 120000 AM[2].pdf");
			assertEdoc(4, "test 2.a");
			assertEdoc(4, "test 2.b");

			//c.	Change journal and don’t add anything in eDocs – only document added on saving should be saved.
			journal.AH_PostDate = journal.AH_PostDate.AddMinutes(1);
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				Factory.Save();
			}
			assertEdoc(5, "GL 00001003_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(5, "GL 00001003_BNE_E_4012016 120000 AM[2].pdf");
			assertEdoc(5, "GL 00001003_BNE_E_4012016 120000 AM[3].pdf");
			assertEdoc(5, "test 2.a");
			assertEdoc(5, "test 2.b");

			//3. need to delete added PDF from eDocs in case on unsuccessful saving. Otherwise next saving will add new document.
			journal.AH_PostDate = journal.AH_PostDate.AddMinutes(1);

			var newFactory2 = new BusinessObjectFactory();
			newFactory2.RefreshEnabled = false;
			var journalInnNewFactory2 = newFactory2.Load<GLJournal>(journal.PK);
			journalInnNewFactory2.AH_PostDate = journalInnNewFactory2.AH_PostDate.AddMinutes(10);
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(newFactory2))
			{
				newFactory2.Save();
			}

			Exception exceptionOnSaving = null;
			try
			{
				AssertEquals(5, journal.DocManagerInfo.AllEDocs.Count);
				using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
				{
					Factory.Save();
				}
			}
			catch (Exception ex)
			{
				exceptionOnSaving = ex;
			}

			AssertNotNull(exceptionOnSaving);
			AssertEquals(true, journal.IsInDatabase);
			AssertEquals(true, journal.HasChanges);
			AssertEquals(5, journal.DocManagerInfo.AllEDocs.Count);
			journal.CancelChanges();
			journal.Reload();

			//4.	Existing Journal
			createGLJournal();
			Factory.Save();
			assertEdoc(1, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			//a.	change lines , 1 more eDoc should be added.
			(journal.Lines[0] as GLJournalLine).UnsignedOSLineAmount++;
			(journal.Lines[1] as GLJournalLine).UnsignedOSLineAmount++;
			Factory.Save();
			assertEdoc(2, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(2, "GL {0}_BNE_E_4012016 120000 AM[2].pdf");

			//b. change lines again, 1 more eDoc should be added.
			(journal.Lines[0] as GLJournalLine).UnsignedOSLineAmount++;
			(journal.Lines[1] as GLJournalLine).UnsignedOSLineAmount++;
			Factory.Save();
			assertEdoc(3, "GL {0}_BNE_E_4012016 120000 AM.pdf");
			assertEdoc(3, "GL {0}_BNE_E_4012016 120000 AM[2].pdf");
			assertEdoc(3, "GL {0}_BNE_E_4012016 120000 AM[3].pdf");
		}

		public void TestDoesHeaderAndLinesHavePersistentChanges()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Assert("Journal is not in database", !journal.IsInDatabase);
			AssertEquals(true, journal.DoesHeaderAndLinesHavePersistentChanges);

			Factory.Save();
			Assert("Journal is in database", journal.IsInDatabase);

			var originalDescription = journal.AH_Desc;
			journal.AH_Desc = "test description";
			journal.HasChanges = false;
			AssertEquals("Persistent change in Header Description", true, journal.DoesHeaderAndLinesHavePersistentChanges);
			journal.AH_Desc = originalDescription;
			journal.HasChanges = false;
			AssertEquals("No Persistent change in Header Description", false, journal.DoesHeaderAndLinesHavePersistentChanges);

			var originalLineAmount = journal.Lines[1].AL_LineAmount;
			journal.Lines[1].AL_LineAmount = 25;
			journal.HasChanges = false;
			AssertEquals("Persistent change in Line Amount", true, journal.DoesHeaderAndLinesHavePersistentChanges);
			journal.Lines[1].AL_LineAmount = originalLineAmount;
			journal.HasChanges = false;
			AssertEquals("No Persistent change in Line Amount", false, journal.DoesHeaderAndLinesHavePersistentChanges);

			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 20, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 20, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			AssertEquals("Persistent change in Lines", true, journal.DoesHeaderAndLinesHavePersistentChanges);

			Factory.Save();

			journal.Lines.Remove(line1);
			journal.Lines.Remove(line2);
			AssertEquals("Persistent change in Lines", true, journal.DoesHeaderAndLinesHavePersistentChanges);

			Factory.Save();
			AssertEquals("No Persistent change in Journal after saving", false, journal.DoesHeaderAndLinesHavePersistentChanges);
		}

		public void TestShouldCreateAutoLogIfOnlyChildrenHaveChanges()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.IsTopLevel = true;
			Factory.Save();

			var originalDesc = journal.AH_Desc;
			journal.AH_Desc = "test description";
			AssertEquals("Should create auto log as header has changes", true, ((IAutoAdminLogTarget)journal).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
			AssertEquals("Should update audit columns as header has changes", true, ((IUpdateAuditFields)journal).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);

			journal.AH_Desc = originalDesc;
			AssertEquals("Should not create auto log as journal have no changes", false, ((IAutoAdminLogTarget)journal).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
			AssertEquals("Should not update audit columns as journal have no changes", false, ((IUpdateAuditFields)journal).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);

			var originalLineAmount = journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount;
			journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount = originalLineAmount + 10m;
			AssertEquals("Should create auto log as children have changes", true, ((IAutoAdminLogTarget)journal).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
			AssertEquals("Should update audit columns as children have changes", true, ((IUpdateAuditFields)journal).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
			journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount = originalLineAmount;
			AssertEquals("Should not create auto log as journal have no changes", false, ((IAutoAdminLogTarget)journal).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
			AssertEquals("Should not update audit columns as journal have no changes", false, ((IUpdateAuditFields)journal).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		public override void TestGLJournalGetValidValidationWhileAH_PostDateHasChanges()
		{
			(Header as GLJournalForTest)?.SetCanUserPostToPreviousPeriods(true);

			base.TestGLJournalGetValidValidationWhileAH_PostDateHasChanges();
		}

		public void TestCreateApprovedJournalApprovalRequest()
		{
			Journal.PostPeriod = CurrentPeriod.AM_Period;
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			Journal.PeriodPK = CurrentPeriod.PK;

			Journal.Factory.Save();

			var factory = new BusinessObjectFactory();
			var journal = factory.Load<GLJournal>(Journal.PK);
			AssertEquals(1, journal.Approvals.Count);
			var approvalRequest = journal.Approvals[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest.XP_ApprovalStatus);
			AssertEquals(journal.AH_GB, approvalRequest.XP_GB_RequestingBranch);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalRequest.XP_GS_NKApprovingUser1);
			AssertEquals(ZDateTime.Now.Date, approvalRequest.XP_ApprovalDate.Date);
		}

		[TestDate(2024, 10, 08)]
		public void TestSetPostPeriodSuccessfullyWithoutGLAccount()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2024);

			var glJournalTypeList = Journal.TransactionType_List;
			foreach (ICodeDescription glJournalTypePair in glJournalTypeList)
			{
				var journal = TestObjectCreator.CreateGLJournal(glJournalTypePair.Code, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				var line = (GLJournalLine)journal.Lines.AddNew();
				line.AL_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
				line.AL_ExchangeRate = 5M;
				line.UnsignedLocalLineAmount = 10M;

				AssertNoExceptionThrown($"Failed to set PostPeriod in {glJournalTypePair.Code}",
					() => journal.PostPeriod = 202409);

				var validation = new GLJournalValidation(journal);
				validation.ValidateAll();
				AssertEquals($"{glJournalTypePair.Code} should have only one error when GL account is empty", 1, journal.GetErrors().Count());
				AssertHasError(line.AL_AGInfo, "Please enter a GL Post To Account.");
			}
		}

		#region Reversing for China

		protected override void AssertReversedAmountsCorrectlyNegated(TransactionHeader reversingHeader)
		{
			// For GL Journal the header amounts should be 0
			AssertEquals("Reverse OS Ex Tax Amount", 0m, reversingHeader.AH_OSExTaxAmount);
			AssertEquals("Reverse OS Tax Amount", 0m, reversingHeader.AH_OSTaxAmount);
			AssertEquals("Reverse Local Ex Tax Amount", 0m, reversingHeader.AH_LocalExTaxAmount);
			AssertEquals("Reverse Local Tax Amount", 0m, reversingHeader.AH_LocalTaxAmount);
			AssertEquals("Reverse Local Invoice Amount - DB Field", 0m, reversingHeader.AH_InvoiceAmount);
			AssertEquals("Reverse Local GST Amount - DB Field", 0m, reversingHeader.AH_GSTAmount);
			AssertEquals("Reverse OS Total Amount - DB Field", 0m, reversingHeader.AH_OSTotal);
		}

		protected override void AssertReverseTransactionDueDateValue(TransactionHeader reversingHeader)
		{
			Assert("Due date should be empty", reversingHeader.AH_DueDate.IsEmpty);
		}

		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			base.SetupHeaderForReversing(aH_OSExTaxAmount, aH_OSTaxAmount);
			Header.AH_OH = ZGuid.Empty;
			Header.AH_JH = ZGuid.Empty;
			Header.AH_AB = ZGuid.Empty;
			Header.AH_TransactionCategory = "";
			Header.AH_CashBasisGSTIndicator = false;
			Header.AH_ChequeDrawer = ZString.Empty;
			Header.AH_ChequeOrReference = ZString.Empty;
			Header.AH_DrawerBank = ZString.Empty;
			Header.AH_DrawerBranch = ZString.Empty;
			Header.AH_InvoiceTerm = ZString.Empty;
			Header.AH_InvoiceTermDays = 0;
			Header.AH_ReceiptType = ZString.Empty;
		}

		public void TestSuspendValidationWhileGeneratingReverseTransaction()
		{
			AccGLHeader gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionNum = "00001111";
			journal.PostPeriod = PreviousOpenPeriod.AM_Period;
			GLJournalLine line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 34m;
			line1.AL_LocalExTaxAmount = 34m;

			GLJournalLine line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -34m;
			line2.AL_LocalExTaxAmount = -34m;

			GLJournalLine line3 = (GLJournalLine)journal.Lines.AddNew();
			line3.AL_AG = gLAccount1.PK;
			line3.AL_OSExTaxAmount = 0m;
			line3.AL_LocalExTaxAmount = 0m;

			GLJournalLine line4 = (GLJournalLine)journal.Lines.AddNew();
			line4.AL_AG = gLAccount2.PK;
			line4.AL_OSExTaxAmount = 0m;
			line4.AL_LocalExTaxAmount = 0m;

			Factory.Save();

			journal.GenerateReverseTransaction(true);
			var lines = (journal.ReverseTransaction as GLJournal).Lines.Cast<GLJournalLine>().Where(x => x.UnsignedLocalLineAmount == 0).ToArray();
			AssertEquals(2, lines.Length);

			AssertNoErrors(lines[0].UnsignedLocalLineAmountInfo);
			AssertNoErrors(lines[0].UnsignedOSLineAmountInfo);

			AssertNoErrors(lines[1].UnsignedLocalLineAmountInfo);
			AssertNoErrors(lines[1].UnsignedOSLineAmountInfo);
		}

		public override void TestGenerateReverseTransaction()
		{
			AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(Factory, GlbCompany.CurrentCompany);
			AccGLHeader gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionNum = "00001111";
			journal.PostPeriod = PreviousOpenPeriod.AM_Period;
			GLJournalLine line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 34m;
			line1.AL_LocalExTaxAmount = 34m;

			GLJournalLine line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -34m;
			line2.AL_LocalExTaxAmount = -34m;

			Factory.Save();

			journal.GenerateReverseTransaction(true);
			GLJournal reversingJournal = ((IReversing)journal).ReverseTransaction as GLJournal;
			Assert("Transaction number on the reversing journal should be empty", reversingJournal.AH_TransactionNum.IsEmpty);
			AssertEquals("Post Period should be current period", calculator.GetPeriodFromDate(ZDateTime.Today), reversingJournal.PostPeriod);
			AssertEquals("The reversing journal should have 2 lines", 2, reversingJournal.Lines.Count);
			GLJournalLine reversingLine1 = (GLJournalLine)reversingJournal.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AG, gLAccount1.PK))[0];
			AssertEquals("Reversing Line should have amount = -34", -34m, reversingLine1.AL_OSExTaxAmount);
			AssertEquals("Reversing Line should have local amount = -34", -34m, reversingLine1.AL_LocalExTaxAmount);
		}

		#endregion

		#region ReadOnly State Tests

		public void TestReadOnlyFields()
		{
			Assert("Journal Total Field (i.e. InvoiceAmount) should be readonly always", Journal.AH_OSExTaxAmountInfo.ReadOnly);
			Assert("Journal Number Field should always be readonly", Journal.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestAH_TransactionType_ReadOnly()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			AssertEquals("AH_TransactionType not read only by default", false, journal.AH_TransactionTypeInfo.ReadOnly);

			Factory.Save();
			AssertEquals("AH_TransactionType read only for saved journals.", true, journal.AH_TransactionTypeInfo.ReadOnly);
		}

		public void TestAH_TransactionCategory_ReadOnly()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			AssertEquals("AH_TransactionCategory not read only by default", false, journal.AH_TransactionCategoryInfo.ReadOnly);

			Factory.Save();
			AssertEquals("AH_TransactionCategory read only for saved journals.", true, journal.AH_TransactionCategoryInfo.ReadOnly);
		}

		public override void TestAH_PostDate_ReadOnly()
		{
			base.TestAH_PostDate_ReadOnly();

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var journal = Factory.NewWithValidTestData<GLJournal>();
				AssertEquals("AH_PostDate must be read only as PostPeriod", journal.PostPeriodInfo.ReadOnly, journal.AH_PostDateInfo.ReadOnly);
				Assert(!journal.AH_PostDateInfo.ReadOnly);

				var noteJournal = Factory.NewWithValidTestData<GLJournal>();
				noteJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals("AH_PostDate must be read only as PostPeriod", noteJournal.PostPeriodInfo.ReadOnly, noteJournal.AH_PostDateInfo.ReadOnly);
				Assert(!noteJournal.AH_PostDateInfo.ReadOnly);

				var revJournal = Factory.NewWithValidTestData<GLJournal>();
				revJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				AssertEquals("AH_PostDate must be read only as PostPeriod", revJournal.PostPeriodInfo.ReadOnly, revJournal.AH_PostDateInfo.ReadOnly);
				Assert(!revJournal.AH_PostDateInfo.ReadOnly);
			}
		}

		public void TestAH_DueDate_ReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var revJournal = Factory.NewWithValidTestData<GLJournal>();
				revJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				AssertEquals("AH_DueDate must be read only as AgePeriod", revJournal.AgePeriodInfo.ReadOnly, revJournal.AH_DueDateInfo.ReadOnly);
				Assert(!revJournal.AH_DueDateInfo.ReadOnly);
			}
		}

		public void TestGLModifyPresentationField()
		{
			var creator = new TestObjectCreator(Factory);
			var gLJournal = creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDate.Today);

			Environment.Env.Security.GeneralLedgerModifyPresentationField.IsAllowed = true;
			AssertEquals("Should not be read only", false, gLJournal.AH_TransactionCategory_ReadOnly);

			Environment.Env.Security.GeneralLedgerModifyPresentationField.IsAllowed = false;
			AssertEquals("Should be read only", true, gLJournal.AH_TransactionCategory_ReadOnly);
		}

		public void TestGLAwaitingApprovalModifyPresentationField()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var request = new BusinessObjectFactory().New<GLJournalApprovalRequest>();
			request.Initialize(journal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request.Factory.Save();

			Environment.Env.Security.GLJournalApprovalModifyPresentationField.IsAllowed = true;
			AssertEquals("Should not be read only", false, journal.AH_TransactionCategory_ReadOnly);

			Environment.Env.Security.GLJournalApprovalModifyPresentationField.IsAllowed = false;
			AssertEquals("Should be read only", true, journal.AH_TransactionCategory_ReadOnly);
		}

		[UseSnapshotProtection]
		public void TestPostPeriodReadOnlyOnSavingForChina()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			try
			{
				GLJournal testJournal = Factory.NewWithValidTestData<GLJournal>();
				Assert("Post Period should not be read-only.", !testJournal.PostPeriodInfo.ReadOnly);

				testJournal.Factory.Save();
				testJournal.RefreshBinding();
				Assert("Post Period should be read only", testJournal.PostPeriodInfo.ReadOnly);
				Assert("Age Period should be read only", testJournal.AgePeriodInfo.ReadOnly);

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

				testJournal = Factory.NewWithValidTestData<GLJournal>();
				Assert("Post Period should not be read only", !testJournal.PostPeriodInfo.ReadOnly);

				testJournal.Factory.Save();
				testJournal.RefreshBinding();
				Assert("Post Period should not be read only", !testJournal.PostPeriodInfo.ReadOnly);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestReadOnlyForReversing()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.GenerateReverseTransaction(true);
			var reverseJournal = (GLJournal)journal.ReverseTransaction;

			Assert("AH_Desc should not be readonly", !reverseJournal.AH_DescInfo.ReadOnly);
			Assert("AH_NumberOfSupportingDocuments should not be readonly", !reverseJournal.AH_NumberOfSupportingDocumentsInfo.ReadOnly);
			Assert("PostPeriod should not be readonly", !reverseJournal.PostPeriodInfo.ReadOnly);
			Assert("AH_PostDateInfo should not be readonly", !reverseJournal.AH_PostDateInfo.ReadOnly);

			Assert(reverseJournal.AH_TransactionTypeInfo.ReadOnly);
			Assert(reverseJournal.AH_TransactionNumInfo.ReadOnly);
			Assert(reverseJournal.ApprovalRequestStatusInfo.ReadOnly);
			Assert(reverseJournal.AH_TransactionCategoryInfo.ReadOnly);
			Assert(reverseJournal.Lines.ReadOnly);
			Assert(reverseJournal.AH_OSExTaxAmountInfo.ReadOnly);

			AssertAgePeriodReadOnlyForReversing(TransactionTypes.GLStandardJournal, true);
			AssertAgePeriodReadOnlyForReversing(TransactionTypes.GLAutoJournal, false);
			AssertAgePeriodReadOnlyForReversing(TransactionTypes.GLReversingJournal, false);
			AssertAgePeriodReadOnlyForReversing(TransactionTypes.GLNoteJournal, true);
		}

		void AssertAgePeriodReadOnlyForReversing(ZString transactionType, bool isReadonly)
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionType = transactionType;
			journal.GenerateReverseTransaction(true);
			var reverseJournal = (GLJournal)journal.ReverseTransaction;

			AssertEquals(isReadonly, reverseJournal.AgePeriodInfo.ReadOnly);
			AssertEquals(isReadonly, reverseJournal.AH_DueDateInfo.ReadOnly);
		}

		public void TestReadOnlyForPostedReverseJournal()
		{
			AssertReadOnlyForPostedReverseJournal(TransactionTypes.GLStandardJournal);
			AssertReadOnlyForPostedReverseJournal(TransactionTypes.GLReversingJournal);
			AssertReadOnlyForPostedReverseJournal(TransactionTypes.GLAutoJournal);
			AssertReadOnlyForPostedReverseJournal(TransactionTypes.GLNoteJournal);
		}

		void AssertReadOnlyForPostedReverseJournal(ZString transactionType)
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionType = transactionType;
			journal.AH_DueDate = ZDateTime.Now;
			journal.GenerateReverseTransaction(true);
			var reverseJournal = (GLJournal)journal.ReverseTransaction;

			var request = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			request.Initialize(reverseJournal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			request.XP_ParentID = reverseJournal.PK;
			Factory.Save();

			Assert(reverseJournal.AH_TransactionTypeInfo.ReadOnly);
			Assert(reverseJournal.AH_TransactionNumInfo.ReadOnly);
			Assert(reverseJournal.ApprovalRequestStatusInfo.ReadOnly);
			Assert(reverseJournal.AH_TransactionCategoryInfo.ReadOnly);
			Assert(reverseJournal.Lines.ReadOnly);
			Assert(reverseJournal.AH_OSExTaxAmountInfo.ReadOnly);
			Assert(reverseJournal.AgePeriodInfo.ReadOnly);
			Assert(reverseJournal.PostPeriodInfo.ReadOnly);
			Assert(reverseJournal.AH_PostDateInfo.ReadOnly);
			Assert(reverseJournal.AH_DueDateInfo.ReadOnly);
		}

		public void TestLinkOriginalJournalToApprovalRequest()
		{
			Factory.Save();

			var newJournal = Factory.NewWithValidTestData<GLJournal>();
			newJournal.IsReverseTransaction = true;
			newJournal.OriginalTransaction = Journal;

			var request = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			newJournal.LatestLinkedApprovalRequestPK_ForTestOnly = request.PK;

			newJournal.LinkOriginalJournalToApprovalRequest(Factory);
			AssertEquals(request.PK, Journal.AH_TransactionBelongsToGroup);
		}

		public void TestGetPendingApprovalRequestByTransactionBelongsToGroup()
		{
			Journal.AH_TransactionBelongsToGroup = Guid.Empty;
			AssertNull(Journal.GetPendingApprovalRequestByTransactionBelongsToGroup());

			var request = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			request.Initialize(Journal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Journal.AH_TransactionBelongsToGroup = request.PK;
			Factory.Save();

			AssertEquals(request, Journal.GetPendingApprovalRequestByTransactionBelongsToGroup());

			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();
			AssertEquals(request, Journal.GetPendingApprovalRequestByTransactionBelongsToGroup());
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should be readonly", Journal.AH_PostDateInfo.ReadOnly);
		}

		public override void TestAH_GB_TaxBranchReadOnlyWithSecurity()
		{
			using (TestObjectCreator.SetUpTaxBranchRegistry(true))
			{
				TestObjectCreator.ResetSecurityCore();

				TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.TestOrganisation.CompanyData.SetAPTaxApplicable(true);

				var header = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as TransactionHeaderWithLines;
				header.AH_OH = TestObjectCreator.TestOrganisation.PK;

				Assert(!header.AH_GB_TaxBranchInfo.ReadOnly);

				GetOverrideTaxBranchSecurity(header.AH_Ledger).IsAllowed = false;
				Assert(!header.AH_GB_TaxBranchInfo.ReadOnly);

				GetOverrideTaxBranchSecurity(header.AH_Ledger).IsAllowed = true;
				Assert(!header.AH_GB_TaxBranchInfo.ReadOnly);
			}
		}

		#region Post/Reverse Period Readonly For Saved Journals

		public void TestGeneralLedgerJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, true, false);
		}

		public void TestNoteJournalJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, true, false);
		}

		public void TestGeneralLedgerPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			Journal.AH_TransactionCategory = "AAA";
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, true, true);
		}

		public void TestNoteJournalPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			Journal.AH_TransactionCategory = "AAA";
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, true, true);
		}

		public void TestReversingJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			Journal.AgePeriod = FuturePeriod.AM_Period;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, false, false);
			AssertPeriodReadOnly(GLJournal.Schema.AgePeriod, false, false);
		}

		public void TestReversingPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			Journal.AH_TransactionCategory = "AAA";
			Journal.AgePeriod = FuturePeriod.AM_Period;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, false, true);
			AssertPeriodReadOnly(GLJournal.Schema.AgePeriod, false, true);
		}

		public void TestAutoJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.AgePeriod = FuturePeriod.AM_Period;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, false, false);
			AssertPeriodReadOnly(GLJournal.Schema.AgePeriod, false, false);

			Journal.PostPeriod = PreviousOpenPeriod.AM_Period;
			Journal.AgePeriod = FuturePeriod.AM_Period;

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Journal.Factory))
			{
				Journal.Factory.Save();
			}

			CurrentPeriod.AM_IsGeneralLedgerClosed = true;
			CurrentPeriod.AM_IsSubledgerClosedForAdjustments = false;
			CurrentPeriod.Factory.Save();
			Header = (new BusinessObjectFactory()).Load<GLJournalForTest>(Journal.PK);
			AssertAllInfosReadOnly();
			Assert("Grid (i.e. lines) should be readonly.", Journal.GLJournalLines.ReadOnly);

			CurrentPeriod.AM_IsGeneralLedgerClosed = false;
			CurrentPeriod.AM_IsSubledgerClosedForAdjustments = true;
			CurrentPeriod.Factory.Save();
			Header = (new BusinessObjectFactory()).Load<GLJournalForTest>(Journal.PK);
			AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AH_PlaceOfSupplyInfo);
			Assert("Grid (i.e. lines) should not be readonly.", !Journal.GLJournalLines.ReadOnly);
		}

		public void TestAutoPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal.AH_TransactionCategory = "AAA";
			Journal.AgePeriod = FuturePeriod.AM_Period;
			AssertPeriodReadOnly(GLJournal.Schema.PostPeriod, false, true);
			AssertPeriodReadOnly(GLJournal.Schema.AgePeriod, false, true);

			Journal.PostPeriod = PreviousOpenPeriod.AM_Period;
			Journal.AgePeriod = FuturePeriod.AM_Period;

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Journal.Factory))
			{
				Journal.Factory.Save();
			}

			CurrentPeriod.AM_IsGeneralLedgerClosed = false;
			CurrentPeriod.AM_IsSubledgerClosedForAdjustments = true;
			CurrentPeriod.Factory.Save();
			Header = (new BusinessObjectFactory()).Load<GLJournalForTest>(Journal.PK);
			AssertAllInfosReadOnly();
			Assert("Grid (i.e. lines) should be readonly.", Journal.GLJournalLines.ReadOnly);

			CurrentPeriod.AM_IsGeneralLedgerClosed = true;
			CurrentPeriod.AM_IsSubledgerClosedForAdjustments = false;
			CurrentPeriod.Factory.Save();
			Header = (new BusinessObjectFactory()).Load<GLJournalForTest>(Journal.PK);
			AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AH_PlaceOfSupplyInfo);
			Assert("Grid (i.e. lines) should not be readonly.", !Journal.GLJournalLines.ReadOnly);
		}

		#endregion

		[TestDate(2016, 08, 25, 10, 10, 10)]
		public void TestHasAssignedExportBatchNumberLines()
		{
			var testDate = TestDateAttribute.Date;
			int cacheInMinutes = 10;
			Journal.SetCacheIntervalForHasAssignedExportBatchNumberLines_ForTestOnly(cacheInMinutes * 60);

			AssertEquals("Precondition:", 2, Journal.Lines.Count);
			AssertTransactionTypeCategoryReadOnlyness();
			Assert("Precondition:", !Journal.AH_DescInfo.ReadOnly);
			Assert("Precondition:", !Journal.PostPeriodInfo.ReadOnly);
			Assert("Precondition:", Journal.AgePeriodInfo.ReadOnly);

			Assert("There are no line assigned a batch number.", !Journal.HasAssignedExportBatchNumberLines_ForTestOnly);

			var exportBatchSequence = Factory.NewWithValidTestData<GenExportBatchSequence>();
			exportBatchSequence.XB_ParentID = Journal.Lines[0].PK;
			Factory.Save();
			Assert("At least one line should have batch number assigned.", Journal.HasAssignedExportBatchNumberLines_ForTestOnly);

			exportBatchSequence.XB_ParentID = Journal.Lines[1].PK;
			Factory.Save();
			Assert("At least one line should have batch number assigned.", Journal.HasAssignedExportBatchNumberLines_ForTestOnly);
			TestDateAttribute.Date = testDate.AddMinutes(cacheInMinutes - 1);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionCategoryInfo.ReadOnly);
			Assert("Cached value", !Journal.AH_DescInfo.ReadOnly);
			Assert("Cached value", !Journal.PostPeriodInfo.ReadOnly);
			Assert("Always readonly for standard journal", Journal.AgePeriodInfo.ReadOnly);

			testDate = testDate.AddMinutes(cacheInMinutes + 1);
			TestDateAttribute.Date = testDate;
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionCategoryInfo.ReadOnly);
			Assert(Journal.AH_DescInfo.ReadOnly);
			Assert(Journal.PostPeriodInfo.ReadOnly);
			Assert("Always readonly for standard journal", Journal.AgePeriodInfo.ReadOnly);

			exportBatchSequence.XB_ParentID = ZGuid.NewZGuid();
			Factory.Save();
			Assert("There are no line assigned a batch number.", !Journal.HasAssignedExportBatchNumberLines_ForTestOnly);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionCategoryInfo.ReadOnly);
			Assert("Cached value", Journal.AH_DescInfo.ReadOnly);
			Assert("Cached value", Journal.PostPeriodInfo.ReadOnly);
			Assert("Always readonly for standard journal", Journal.AgePeriodInfo.ReadOnly);

			TestDateAttribute.Date = testDate.AddMinutes(cacheInMinutes + 1);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("Still readonly because it's already saved.", Journal.AH_TransactionCategoryInfo.ReadOnly);
			Assert(!Journal.AH_DescInfo.ReadOnly);
			Assert(!Journal.PostPeriodInfo.ReadOnly);
			Assert("Always readonly for standard journal", Journal.AgePeriodInfo.ReadOnly);
		}

		protected virtual void AssertTransactionTypeCategoryReadOnlyness()
		{
			Assert("Precondition:", !Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("Precondition:", !Journal.AH_TransactionCategoryInfo.ReadOnly);
		}

		public void TestClearAgePeriod()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			AssertClearAgePeriod(TransactionTypes.GLStandardJournal);
			AssertClearAgePeriod(TransactionTypes.GLAutoJournal);
			AssertClearAgePeriod(TransactionTypes.GLReversingJournal);
			AssertClearAgePeriod(TransactionTypes.GLNoteJournal);

			void AssertClearAgePeriod(ZString transationType)
			{
				journal.AgePeriod = 202004;
				journal.AH_TransactionType = transationType;
				AssertEquals(transationType == TransactionTypes.GLStandardJournal || transationType == TransactionTypes.GLNoteJournal ? 0 : 202004, journal.AgePeriod);
			}
		}

		public void TestResetLineDefaultValuesForNoteJournal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
				var line = (GLJournalLine)journal.Lines.AddNew();

				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_OH);
				AssertEquals("Pre-condition", Constants.CurrencyCodes.UnitedStates, line.AL_RX_NKTransactionCurrency);

				line.AL_OH = TestObjectCreator.ABIGAS.PK;
				line.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.China;

				AssertEquals("Organization", TestObjectCreator.ABIGAS.PK, line.AL_OH);
				AssertEquals("Currency", Constants.CurrencyCodes.China, line.AL_RX_NKTransactionCurrency);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;

				AssertEquals("Organization should be empty after reset", ZGuid.Empty, line.AL_OH);
				AssertEquals("Currency should be local currency after reset", Constants.CurrencyCodes.UnitedStates, line.AL_RX_NKTransactionCurrency);
			}
		}

		public void TestIsNoteOrStdOrRevOrAutoJournal()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			AssertEquals(false, journal.IsNoteJournal);
			AssertEquals(true, journal.IsStdJournal);
			AssertEquals(false, journal.IsRevJournal);
			AssertEquals(false, journal.IsAutoJournal);

			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertEquals(true, journal.IsNoteJournal);
			AssertEquals(false, journal.IsStdJournal);
			AssertEquals(false, journal.IsRevJournal);
			AssertEquals(false, journal.IsAutoJournal);

			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			AssertEquals(false, journal.IsNoteJournal);
			AssertEquals(false, journal.IsStdJournal);
			AssertEquals(false, journal.IsRevJournal);
			AssertEquals(true, journal.IsAutoJournal);

			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			AssertEquals(false, journal.IsNoteJournal);
			AssertEquals(false, journal.IsStdJournal);
			AssertEquals(true, journal.IsRevJournal);
			AssertEquals(false, journal.IsAutoJournal);
		}

		public void TestIsPostDateAndDueDateEnabled()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			AssertEquals(false, journal.IsPostDateEnabled);
			AssertEquals(false, journal.IsDueDateEnabled);

			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertEquals(false, journal.IsPostDateEnabled);
			AssertEquals(false, journal.IsDueDateEnabled);

			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			AssertEquals(false, journal.IsPostDateEnabled);
			AssertEquals(false, journal.IsDueDateEnabled);

			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			AssertEquals(false, journal.IsPostDateEnabled);
			AssertEquals(false, journal.IsDueDateEnabled);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, journal.IsPostDateEnabled);
				AssertEquals(true, journal.IsDueDateEnabled);

				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				AssertEquals(false, journal.IsPostDateEnabled);
				AssertEquals(false, journal.IsDueDateEnabled);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(true, journal.IsPostDateEnabled);
				AssertEquals(false, journal.IsDueDateEnabled);

				journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(true, journal.IsPostDateEnabled);
				AssertEquals(false, journal.IsDueDateEnabled);
			}
		}

		public void TestJournalDescriptionDefaultValue()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var registryValue = AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.DefaultValue;
			var autoJRJEntry = registryValue.Cast<DefaultNumberOfSupportingDocuments>().First(x => x.Code == $"{journal.AH_Ledger}{journal.AH_TransactionType}");

			var list = new DefaultNumberOfSupportingDocumentsCollection();
			list.AddDefaultValues($"{LedgerTypes.General}{TransactionTypes.GLNoteJournal}", (NoResString)"Note Journal", (NoResString)"Note Journal Description", 1);

			using (AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				AssertEquals("Pre-condition", "GENERAL LEDGER JOURNAL", journal.AH_Desc);
				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals("Default Value should come from the registery", "NOTE JOURNAL DESCRIPTION", journal.AH_Desc);
			}
		}

		public void TestRoundAmountToLocalDecimals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				var glJournalForTest = Factory.New<GLJournalForTest>();

				glJournalForTest.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(1m, glJournalForTest.RoundAmountToLocalDecimals_ForTestOnly(1.11m));

				glJournalForTest.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(1.11m, glJournalForTest.RoundAmountToLocalDecimals_ForTestOnly(1.11m));
			}
		}

		public void TestRoundAmountToCurrencyDecimals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				var glJournalForTest = Factory.New<GLJournalForTest>();

				glJournalForTest.AH_TransactionType = TransactionTypes.GLStandardJournal;
				AssertEquals(1m, glJournalForTest.RoundAmountToCurrencyDecimals_ForTestOnly(1.11m));

				glJournalForTest.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertEquals(1.11m, glJournalForTest.RoundAmountToCurrencyDecimals_ForTestOnly(1.11m));
			}
		}

		public void TestGLJournalCurrencyDecimals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Austria))
			{
				AssertLineCurrencyDecimals();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				AssertLineCurrencyDecimals();
			}

			void AssertLineCurrencyDecimals()
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);

				journal.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Australia;
				AssertEquals("transaction currency decimal should be 2 when currency is 'AUD'.", 2, journal.OSCurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when currency is 'AUD'.", GlbCompany.CurrentCompany.LocalCurrency.Decimals, journal.LocalCurrencyDecimals);

				journal.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Japan;
				AssertEquals("transaction currency decimal should be 2 when currency is 'JPY'.", 0, journal.OSCurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when currency is 'JPY'.", GlbCompany.CurrentCompany.LocalCurrency.Decimals, journal.LocalCurrencyDecimals);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;

				journal.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Australia;
				AssertEquals("transaction currency decimal should be 2 when transaction is Note Journal.", 2, journal.OSCurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when transaction is Note Journal.", 2, journal.LocalCurrencyDecimals);

				journal.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Japan;
				AssertEquals("transaction currency decimal should be 2 when transaction is Note Journal.", 2, journal.OSCurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when transaction is Note Journal.", 2, journal.LocalCurrencyDecimals);
			}
		}

		#endregion

		#region Implementation

		protected override Type TypeOfReversalValidation => typeof(GLJournalValidation);

		protected override Type TypeOfValidation => typeof(GLJournalValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		#region GLJournalForTest

		class GLJournalForTest : GLJournal
		{
			public GLJournalForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override Type DependentTransactionLineType => typeof(GLJournalForTestLine);

			public void SetCanUserPostToPreviousPeriods(bool value)
			{
				fCanUserPostToPreviousPeriods = value;
			}

			bool fCanUserPostToPreviousPeriods;

			public override bool CanUserPostToPreviousPeriods => fCanUserPostToPreviousPeriods;

			protected override bool ShouldCreateEDocOnSaving => false;

			protected override DependentTransactionLineCollection GetDependentLinesCollection()
			{
				ZQuery orderByQuery = new ZQuery();
				orderByQuery.OrderBy = AccTransactionLinesSchema.Constants.AL_Sequence;
				GLJournalForTestLineCollection journalLines = GetDependentLinesCollectionCore1(orderByQuery);

				bool journalLinesReadOnlyState = GetPropertiesReadOnlyState(null);
				if (journalLinesReadOnlyState)
				{
					journalLines.SetReadOnlyIncludingChildren(true);
				}

				return journalLines;
			}

			public ZDecimal TestRoundAmountToLocalDecimals(ZDecimal amount)
			{
				return RoundAmountToLocalDecimals(amount);
			}

			GLJournalForTestLineCollection GetDependentLinesCollectionCore1(ZQuery orderByQuery)
			{
				return new GLJournalForTestLineCollection(this, orderByQuery);
			}
		}

		class GLJournalForTestLine : GLJournalLine
		{
			public GLJournalForTestLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override AccTransactionHeader TransactionHeader => GLJournalForTest ?? base.TransactionHeader;

			GLJournalForTest GLJournalForTest => MasterTransactionHeader as GLJournalForTest;

			protected override AccTransactionLinesLookups GetNewLookups()
			{
				return new GLJournalForTestLineLookups(this);
			}
		}

		class GLJournalForTestLineCollection : GLJournalLineCollection
		{
			public GLJournalForTestLineCollection(GLJournalForTest @object, ZQuery query) : base(@object, query)
			{
			}
		}

		class GLJournalForTestCollection : GLJournalCollection
		{
			public GLJournalForTestCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pk)
			{
				return typeof(GLJournalForTest);
			}
		}

		class GLJournalForTestLineLookups : TransactionLineLookups
		{
			public GLJournalForTestLineLookups(DependentTransactionLine parent) : base(parent)
			{
			}

			public override AccTransactionHeaderCollection TransactionHeaders => new GLJournalForTestCollection(Factory);
		}

		#endregion

		protected virtual GLJournal Journal => (GLJournalForTest)Header;

		GLJournal fTestJournal;
		GLJournal TestJournal
		{
			get
			{
				if (fTestJournal == null)
				{
					fTestJournal = (GLJournal)Factory.New(GetExpectedBusinessObjectType());
				}
				return fTestJournal;
			}
		}

		protected void AddLine(decimal absoluteAmount, DebitCredit debitOrCredit)
		{
			GLJournalLine newLine = Journal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = absoluteAmount;
			newLine.DebitCreditSign = debitOrCredit.ToString();
		}

		protected void AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(params ZPropertyInfo[] infosToExclude)
		{
			List<ZPropertyInfo> newInfosToExclude = new List<ZPropertyInfo>(infosToExclude);
			newInfosToExclude.Add(Journal.AH_TransactionTypeInfo);
			newInfosToExclude.Add(Journal.AH_TransactionCategoryInfo);

			AssertAllInfosNotReadOnlyWithPreExcludedUnusedProperties(newInfosToExclude.ToArray());
			Assert("AH_TransactionType must be always readonly for saved Journals.", Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert("AH_TransactionCategory must be always readonly for saved Journals.", Journal.AH_TransactionCategoryInfo.ReadOnly);
		}

		protected void AssertAllInfosNotReadOnlyWithPreExcludedUnusedProperties(params ZPropertyInfo[] infosToExclude)
		{
			List<ZPropertyInfo> newInfosToExclude = new List<ZPropertyInfo>(infosToExclude);
			newInfosToExclude.Add(Journal.AH_TransactionNumInfo);
			newInfosToExclude.Add(Journal.AH_OSExTaxAmountInfo);
			newInfosToExclude.Add(Journal.AH_ExchangeRateInfo);
			newInfosToExclude.Add(Journal.AH_LocalExTaxAmountInfo);
			newInfosToExclude.Add(Journal.AH_OSExtraTaxAmountInfo);
			newInfosToExclude.Add(Journal.AH_LocalExtraTaxAmountInfo);
			newInfosToExclude.Add(Journal.AH_PostDateInfo);
			newInfosToExclude.Add(Journal.AH_DueDateInfo);
			newInfosToExclude.Add(Journal.ExchangeRate.RateInfo);
			newInfosToExclude.Add(Journal.UnmatchDateInfo);
			newInfosToExclude.Add(Journal.BindableInvoiceAmountInfo);
			newInfosToExclude.Add(Journal.DisplayInvoiceAddressOverrideInfo);
			newInfosToExclude.Add(Journal.DisplayInvoiceContactOverrideInfo);
			newInfosToExclude.Add(Journal.OriginalTransactionNumberInfo);
			newInfosToExclude.Add(Journal.OriginalTransactionTypeInfo);
			newInfosToExclude.Add(Journal.SupportingDocumentNumberInfo);
			newInfosToExclude.Add(Journal.SourceReferenceInfo);

			AssertInfoReadOnlyStatus(false, newInfosToExclude.ToArray());
			Assert("AH_TransactionNum must be always readonly.", Journal.AH_TransactionNumInfo.ReadOnly);
			Assert("AH_OSExTaxAmount must be always readonly.", Journal.AH_OSExTaxAmountInfo.ReadOnly);
		}

		protected void AssertAllInfosReadOnly(params ZPropertyInfo[] infosToExclude)
		{
			AssertInfoReadOnlyStatus(true, infosToExclude);
		}

		protected void AssertInfoReadOnlyStatus(bool shouldBeReadOnly, params ZPropertyInfo[] infosToExclude)
		{
			List<ZPropertyInfo> excludedInfoList = new List<ZPropertyInfo>(infosToExclude);

			foreach (ZPropertyInfo info in Journal.ZPropertyInfoHash)
			{
				if (info.HasSetter)
				{
					if (infosToExclude.Length > 0)
					{
						if (!excludedInfoList.Contains(info))
						{
							string assertionFailedMessage = string.Format("Info: " + info.Name + " was {0}readonly", shouldBeReadOnly ? "not " : "");
							AssertEquals(assertionFailedMessage, shouldBeReadOnly, info.ReadOnly);
						}
					}
					else
					{
						string assertionFailedMessage = string.Format("Info: " + info.Name + " was {0}readonly", shouldBeReadOnly ? "not " : "");
						AssertEquals(assertionFailedMessage, shouldBeReadOnly, info.ReadOnly);
					}
				}
			}
		}

		void AssertPeriodReadOnly(string periodPropertyNameToTest, bool isAgePeriodAlwaysReadOnly, bool isPresentationJournalTested)
		{
			object prevValue = Journal[periodPropertyNameToTest];

			Journal[periodPropertyNameToTest] = PreviousOpenPeriod.AM_Period;
			Journal.Factory.Save();
			ReLoadHeaderForTest();
			if (isAgePeriodAlwaysReadOnly)
			{
				AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AgePeriodInfo, Journal.AH_PlaceOfSupplyInfo);
				Assert("AgePeriod must be always readonly for General Journals.", Journal.AgePeriodInfo.ReadOnly);
			}
			else
			{
				AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AH_PlaceOfSupplyInfo);
			}
			Assert("Grid (i.e. lines) should not be readonly.", !Journal.GLJournalLines.ReadOnly);

			PreviousGLClosedPeriod.AM_IsGeneralLedgerClosed = !isPresentationJournalTested;
			PreviousGLClosedPeriod.AM_IsSubledgerClosedForAdjustments = isPresentationJournalTested;
			PreviousGLClosedPeriod.Factory.Save();
			Journal[periodPropertyNameToTest] = PreviousGLClosedPeriod.AM_Period;

			// Two Journal instance with the same PK: GLJournal & JournalForTest. Need to suspend post date validation for both bizO.
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Journal.Factory))
			{
				Journal.Factory.Save();
			}

			ReLoadHeaderForTest();
			AssertAllInfosReadOnly();
			Assert("Grid (i.e. lines) should be readonly.", Journal.GLJournalLines.ReadOnly);

			PreviousGLClosedPeriod.AM_IsGeneralLedgerClosed = !PreviousGLClosedPeriod.AM_IsGeneralLedgerClosed;
			PreviousGLClosedPeriod.AM_IsSubledgerClosedForAdjustments = !PreviousGLClosedPeriod.AM_IsSubledgerClosedForAdjustments;
			PreviousGLClosedPeriod.Factory.Save();
			ReLoadHeaderForTest();
			if (isAgePeriodAlwaysReadOnly)
			{
				AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AgePeriodInfo, Journal.AH_PlaceOfSupplyInfo);
				Assert("AgePeriod must be always readonly for General Journals.", Journal.AgePeriodInfo.ReadOnly);
			}
			else
			{
				AssertAllInfosNotReadOnlyWithPreExcludedUnusedPropertiesForSavedJournal(Journal.AH_PlaceOfSupplyInfo);
			}
			Assert("Grid (i.e. lines) should not be readonly.", !Journal.GLJournalLines.ReadOnly);

			Journal[periodPropertyNameToTest] = prevValue;

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Journal.Factory))
			{
				Journal.Factory.Save();
			}

			PreviousGLClosedPeriod.AM_IsGeneralLedgerClosed = true;
			PreviousGLClosedPeriod.AM_IsSubledgerClosedForAdjustments = false;
			PreviousGLClosedPeriod.Factory.Save();
		}

		protected virtual void ReLoadHeaderForTest()
		{
			Header = (new BusinessObjectFactory()).Load<GLJournalForTest>(Journal.PK);
		}

		#endregion

		protected override Type GetExpectedBusinessObjectLineType() => typeof(GLJournalLine);
	}
}
