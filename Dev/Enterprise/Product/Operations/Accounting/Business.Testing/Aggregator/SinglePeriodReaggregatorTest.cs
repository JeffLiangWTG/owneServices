using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	[TestedType(typeof(SinglePeriodReaggregator))]
	class SinglePeriodReaggregatorTest : NonPersistentBusinessObjectTestCase
	{
		GLJournal CreateGLJournal(string gLJournalType, ZInt postPeriod, ZInt reverseOrEndPeriod, GlbBranch branch, string presentationCategory = "")
		{
			GLJournal result = Factory.New<GLJournal>();
			result.AH_TransactionType = gLJournalType;
			result.PostPeriod = postPeriod;
			if (gLJournalType != ZArchitecture.Core.TransactionTypes.GLStandardJournal)
			{
				result.AgePeriod = reverseOrEndPeriod;
			}
			result.AH_GB = branch.PK;
			result.AH_TransactionCategory = presentationCategory;
			return result;
		}

		GLJournalLine CreateJournalLine(GLJournal journal, ZGuid gLAccount, GlbBranch branch, ZDecimal amount, ZString debitCredit)
		{
			GLJournalLine result = (GLJournalLine)journal.Lines.AddNew();
			result.AL_AG = gLAccount;
			result.AL_GB = branch.PK;
			result.UnsignedOSLineAmount = amount;
			result.DebitCreditSign = debitCredit;
			return result;
		}

		GLJournal CreateNoteJournal(ZDateTime postDate, ZGuid glHeaderPK, ZDecimal amount, DebitCredit debitCredit)
		{
			var journal = Creator.CreateGLJournal(TransactionTypes.GLNoteJournal, postDate, postDate);
			Creator.CreateGLJournalLine(journal, amount, debitCredit, glHeaderPK);
			return journal;
		}

		ZDecimal GetTotalAmountForAccountAndPeriod(ZGuid gLAccount, ZInt period)
		{
			string sQL = "SELECT ISNULL(SUM(AA_Amount), 0) FROM dbo.AccGLAggregate WHERE AA_AG = @AG AND AA_Period = @Period";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.AddParameter("@AG", SqlDbType.UniqueIdentifier, gLAccount.ToGuid());
			cmd.AddParameter("@Period", SqlDbType.Int, (int)period);
			object result = cmd.ExecuteScalar();
			return (decimal)result;
		}

		DataTable GetAccGLAggregateDataTable()
		{
			string sql = "SELECT AA_PK, AA_Amount, AA_AG, AA_TransactionCategory FROM dbo.AccGLAggregate";
			DbCommand cmd = Db.Connection.Command(sql);

			DataTable table = new DataTable();
			cmd.NewDataAdapter().Fill(table);

			return table;
		}

		public void TestGJLTakeUpWithPresentationCategory()
		{
			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, PeriodHelper.CurrentPeriodInt, 0
				, GlbBranch.CurrentBranch, "ABC");
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			GLJournal currentPeriodJournal2 = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, PeriodHelper.CurrentPeriodInt, 0
				, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal2, Creator.GLHeader2.PK, GlbBranch.CurrentBranch, 200m, nameof(DebitCredit.DR));
			currentPeriodJournal2.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			var accGLAggregates = GetAccGLAggregateDataTable();

			AssertEquals(4, accGLAggregates.Rows.Count);
			var rows = accGLAggregates.Select("AA_TransactionCategory = 'ABC'");
			AssertEquals(2, rows.Length);
		}

		public void TestTakeUpGJLs()
		{
			GLJournal priorPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, PeriodHelper.PreviousOpenPeriodInt, 0, GlbBranch.CurrentBranch);
			CreateJournalLine(priorPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			priorPeriodJournal.Balance();

			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, PeriodHelper.CurrentPeriodInt, 0, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			GLJournal futurePeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, PeriodHelper.FuturePeriodInt, 0, GlbBranch.CurrentBranch);
			CreateJournalLine(futurePeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			futurePeriodJournal.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			AssertEquals(100m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(-100m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.CurrentPeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.PreviousOpenPeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.FuturePeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.FuturePeriodInt));
		}

		public void TestAJLTakeUpWithPresentationCategory()
		{
			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLAutoJournal, PeriodHelper.PreviousOpenPeriodInt, PeriodHelper.FuturePeriodInt
				, GlbBranch.CurrentBranch, "ABC");
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			GLJournal currentPeriodJournal2 = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLAutoJournal, PeriodHelper.PreviousOpenPeriodInt, PeriodHelper.FuturePeriodInt, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal2, Creator.GLHeader2.PK, GlbBranch.CurrentBranch, 200m, nameof(DebitCredit.DR));
			currentPeriodJournal2.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			var accGLAggregates = GetAccGLAggregateDataTable();

			AssertEquals(4, accGLAggregates.Rows.Count);
			var rows = accGLAggregates.Select("AA_TransactionCategory = 'ABC'");
			AssertEquals(2, rows.Length);
		}

		public void TestTakeUpAJLs()
		{
			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLAutoJournal, PeriodHelper.PreviousOpenPeriodInt, PeriodHelper.FuturePeriodInt, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			AssertEquals(100m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(-100m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.CurrentPeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.PreviousOpenPeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.FuturePeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.FuturePeriodInt));
		}

		public void TestRJLTakeUpWithPresentationCategory()
		{
			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLReversingJournal, PeriodHelper.PreviousOpenPeriodInt
				, PeriodHelper.CurrentPeriodInt, GlbBranch.CurrentBranch, "ABC");
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			GLJournal currentPeriodJournal2 = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLReversingJournal, PeriodHelper.PreviousOpenPeriodInt, PeriodHelper.CurrentPeriodInt, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal2, Creator.GLHeader2.PK, GlbBranch.CurrentBranch, 200m, nameof(DebitCredit.DR));
			currentPeriodJournal2.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			var accGLAggregates = GetAccGLAggregateDataTable();

			AssertEquals(4, accGLAggregates.Rows.Count);
			var rows = accGLAggregates.Select("AA_TransactionCategory = 'ABC'");
			AssertEquals(2, rows.Length);
		}

		public void TestTakeUpRJLs()
		{
			GLJournal currentPeriodJournal = CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLReversingJournal, PeriodHelper.PreviousOpenPeriodInt, PeriodHelper.CurrentPeriodInt, GlbBranch.CurrentBranch);
			CreateJournalLine(currentPeriodJournal, Creator.GLHeader1.PK, GlbBranch.CurrentBranch, 100m, nameof(DebitCredit.DR));
			currentPeriodJournal.Balance();

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			AssertEquals(-100m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(100m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.CurrentPeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), PeriodHelper.PreviousOpenPeriodInt));
		}

		public void TestNJLTakeUpWithPresentationCategory()
		{
			var glHeaderNTE1 = Creator.CreateAccGLHeader(TestObjectCreator.GetRandomString(10), string.Empty, string.Empty, Core.Constants.AccountType.Note, DebitCreditDataEntry.DR);

			var nteJournal1 = CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate, glHeaderNTE1.PK, 90m, DebitCredit.DR);
			nteJournal1.AH_TransactionCategory = "TST";
			var nteJournal2 = CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate.AddDays(1), glHeaderNTE1.PK, 20m, DebitCredit.DR);
			nteJournal2.AH_TransactionCategory = "TST";
			var nteJournal3 = CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate, glHeaderNTE1.PK, 50m, DebitCredit.DR);
			nteJournal3.AH_TransactionCategory = "XXX";
			CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate.AddDays(1), glHeaderNTE1.PK, 150m, DebitCredit.DR);

			Factory.Save();

			var reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			var accGLAggregates = GetAccGLAggregateDataTable();

			AssertEquals(3, accGLAggregates.Rows.Count);

			var rows = accGLAggregates.Select("AA_TransactionCategory = 'TST'");
			AssertEquals("Should be grouped by Transaction Category", 1, rows.Length);
			AssertEquals("Amount should be aggregated", 110m, rows[0]["AA_Amount"]);

			rows = accGLAggregates.Select("AA_TransactionCategory = 'XXX'");
			AssertEquals(1, rows.Length);
			AssertEquals(50m, rows[0]["AA_Amount"]);

			rows = accGLAggregates.Select("AA_TransactionCategory = ''");
			AssertEquals(1, rows.Length);
			AssertEquals(150m, rows[0]["AA_Amount"]);
		}

		public void TestTakeUpNJLs()
		{
			var glHeaderNTE1 = Creator.CreateAccGLHeader(TestObjectCreator.GetRandomString(10), string.Empty, string.Empty, Core.Constants.AccountType.Note, DebitCreditDataEntry.DR);
			var glHeaderNTE2 = Creator.CreateAccGLHeader(TestObjectCreator.GetRandomString(10), string.Empty, string.Empty, Core.Constants.AccountType.Note, DebitCreditDataEntry.CR);

			CreateNoteJournal(PeriodHelper.PreviousOpenPeriod.AM_StartDate, glHeaderNTE1.PK, 190m, DebitCredit.DR);
			CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate, glHeaderNTE1.PK, 90m, DebitCredit.DR);
			CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate.AddDays(1), glHeaderNTE1.PK, 20m, DebitCredit.DR);
			CreateNoteJournal(PeriodHelper.FuturePeriod.AM_StartDate, glHeaderNTE1.PK, 290m, DebitCredit.DR);
			CreateNoteJournal(PeriodHelper.PreviousOpenPeriod.AM_StartDate, glHeaderNTE2.PK, 150m, DebitCredit.CR);
			CreateNoteJournal(PeriodHelper.CurrentPeriod.AM_StartDate, glHeaderNTE2.PK, 50m, DebitCredit.CR);
			CreateNoteJournal(PeriodHelper.FuturePeriod.AM_StartDate, glHeaderNTE2.PK, 250m, DebitCredit.CR);
			CreateNoteJournal(PeriodHelper.FuturePeriod.AM_StartDate.AddDays(1), glHeaderNTE2.PK, 30m, DebitCredit.CR);

			Factory.Save();

			var reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(glHeaderNTE1.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals("Only current period is re-aggregated", 110m, GetTotalAmountForAccountAndPeriod(glHeaderNTE1.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(glHeaderNTE1.PK, PeriodHelper.FuturePeriodInt));

			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(glHeaderNTE2.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals(-50m, GetTotalAmountForAccountAndPeriod(glHeaderNTE2.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(glHeaderNTE2.PK, PeriodHelper.FuturePeriodInt));
		}

		public void TestUnMarkHeadersAsPosted()
		{
			var inv = Factory.New<ARInvoice>();
			var line = (ARInvoiceLine)inv.Lines.AddNew();
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100m;
			inv.AH_PostToGL = Core.Constants.BooleanTrueString;

			Factory.Save();

			AssertEquals("Precondition: AH_PostToGL", Core.Constants.BooleanTrueString, inv.AH_PostToGL);

			ReleaseFactory();

			var reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			inv = Factory.Load<ARInvoice>(inv.PK);
			AssertEquals("AH_PostToGL", Core.Constants.BooleanFalseString, inv.AH_PostToGL);
		}

		[SuspendCriticalValidation]
		public void TestUnMarkLinesAsPosted()
		{
			var wip = Factory.New<WIP>();
			wip.AL_AC = Creator.CC1.PK;
			wip.AL_OSExTaxAmount = 100m;
			wip.AL_PostToGL = Core.Constants.BooleanTrueString;
			wip.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			wip.AL_ReverseDate = ZDateTime.Now;

			var oldWIPInDBBeforeTransformation = Factory.New<WIP>();
			oldWIPInDBBeforeTransformation.AL_AC = Creator.CommentChargeCode.PK;
			oldWIPInDBBeforeTransformation.AL_OSExTaxAmount = 0m;
			oldWIPInDBBeforeTransformation.AL_PostToGL = Core.Constants.BooleanTrueString;
			oldWIPInDBBeforeTransformation.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			oldWIPInDBBeforeTransformation.AL_ReverseDate = ZDateTime.Now;

			var inv = Factory.New<ARInvoice>();
			var invoiceLine = (TransactionLine)inv.Lines.AddNew();
			invoiceLine.AL_AC = Creator.CC1.PK;
			invoiceLine.AL_PostToGL = Core.Constants.BooleanTrueString;
			invoiceLine.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			invoiceLine.AL_ReverseDate = ZDateTime.Now;

			var commentInvoiceLine = (TransactionLine)inv.Lines.AddNew();
			commentInvoiceLine.AL_AC = Creator.CommentChargeCode.PK;
			commentInvoiceLine.AL_PostToGL = Core.Constants.BooleanTrueString;
			commentInvoiceLine.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			commentInvoiceLine.AL_ReverseDate = ZDateTime.Now;

			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionLines_WIPACRMustHaveGLAccount ON AccTransactionLines");

			Factory.Save();

			TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_AccTransactionLines_WIPACRMustHaveGLAccount ON AccTransactionLines;");

			AssertNotEquals("Precondition: wip.AL_AG", ZGuid.Empty, wip.AL_AG);
			AssertEquals("Precondition: wip.AL_PostToGL", Core.Constants.BooleanTrueString, wip.AL_PostToGL);
			AssertEquals("Precondition: wip.AL_ReverseToGL", Core.Constants.BooleanTrueString, wip.AL_ReverseToGL);

			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_AG", ZGuid.Empty, oldWIPInDBBeforeTransformation.AL_AG);
			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_PostToGL", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_PostToGL);
			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_ReverseToGL", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_ReverseToGL);

			AssertNotEquals("Precondition: invoiceLine.AL_AG", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("Precondition: invoiceLine.AL_PostToGL", Core.Constants.BooleanTrueString, invoiceLine.AL_PostToGL);
			AssertEquals("Precondition: invoiceLine.AL_ReverseToGL", Core.Constants.BooleanTrueString, invoiceLine.AL_ReverseToGL);

			AssertEquals("Precondition: commentInvoiceLine.AL_AG", ZGuid.Empty, commentInvoiceLine.AL_AG);
			AssertEquals("Precondition: commentInvoiceLine.AL_PostToGL", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_PostToGL);
			AssertEquals("Precondition: commentInvoiceLine.AL_ReverseToGL", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_ReverseToGL);

			ReleaseFactory();

			var reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			wip = Factory.Load<WIP>(wip.PK);
			AssertEquals("wip.AL_PostToGL", Core.Constants.BooleanFalseString, wip.AL_PostToGL);
			AssertEquals("wip.AL_ReverseToGL", Core.Constants.BooleanFalseString, wip.AL_ReverseToGL);

			oldWIPInDBBeforeTransformation = Factory.Load<WIP>(oldWIPInDBBeforeTransformation.PK);
			AssertEquals("oldWIPInDBBeforeTransformation.AL_PostToGL remains untouched", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_PostToGL);
			AssertEquals("oldWIPInDBBeforeTransformation.AL_ReverseToGL remains untouched", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_ReverseToGL);

			invoiceLine = Factory.Load<TransactionLine>(invoiceLine.PK);
			AssertEquals("invoiceLine.AL_PostToGL", Core.Constants.BooleanFalseString, invoiceLine.AL_PostToGL);
			AssertEquals("invoiceLine.AL_ReverseToGL", Core.Constants.BooleanFalseString, invoiceLine.AL_ReverseToGL);

			commentInvoiceLine = Factory.Load<TransactionLine>(commentInvoiceLine.PK);
			AssertEquals("commentInvoiceLine.AL_PostToGL remains untouched", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_PostToGL);
			AssertEquals("commentInvoiceLine.AL_ReverseToGL remains untouched", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_ReverseToGL);
		}

		public void TestDeleteAggregateInformation()
		{
			AccGLAggregate aggregate = Factory.New<AccGLAggregate>();
			aggregate.AA_AG = Creator.GLHeader1.PK;
			aggregate.AA_Period = PeriodHelper.CurrentPeriodInt;
			aggregate.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregate.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregate.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregate.AA_Amount = 100m;

			AccGLAggregate priorAggregate = Factory.New<AccGLAggregate>();
			priorAggregate.AA_AG = Creator.GLHeader1.PK;
			priorAggregate.AA_Period = PeriodHelper.PreviousOpenPeriodInt;
			priorAggregate.AA_GB = GlbBranch.CurrentBranch.PK;
			priorAggregate.AA_GC = GlbCompany.CurrentCompany.PK;
			priorAggregate.AA_GE = GlbDepartment.CurrentDepartment.PK;
			priorAggregate.AA_Amount = 100m;

			AccGLAggregate futureAggregate = Factory.New<AccGLAggregate>();
			futureAggregate.AA_AG = Creator.GLHeader1.PK;
			futureAggregate.AA_Period = PeriodHelper.FuturePeriodInt;
			futureAggregate.AA_GB = GlbBranch.CurrentBranch.PK;
			futureAggregate.AA_GC = GlbCompany.CurrentCompany.PK;
			futureAggregate.AA_GE = GlbDepartment.CurrentDepartment.PK;
			futureAggregate.AA_Amount = 100m;

			Factory.Save();

			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			reAggregator.ReAggregate();

			AssertEquals(100m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.PreviousOpenPeriodInt));
			AssertEquals(0m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.CurrentPeriodInt));
			AssertEquals(100m, GetTotalAmountForAccountAndPeriod(Creator.GLHeader1.PK, PeriodHelper.FuturePeriodInt));
		}

		public void TestValidatePeriod()
		{
			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = 0;
			AssertHasErrors(reAggregator.PeriodToReaggregateInfo);

			reAggregator.PeriodToReaggregate = 10000;
			AssertHasErrors(reAggregator.PeriodToReaggregateInfo);

			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			Assert(!reAggregator.PeriodToReaggregateInfo.HasErrors());
		}

		[TestDate(2015, 01, 01)]
		[SuspendCriticalValidation]
		public void TestReaggregationReQueueCashVATSinglePeriod()
		{
			var testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 201501;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.False;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			testPeriod.AM_StartDate = new ZDateTime(2015, 1, 1);
			testPeriod.AM_EndDate = new ZDateTime(2015, 1, 31);

			var testObjectCreator = new TestObjectCreator(Factory);

			var invoice = (APInvoice)testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", testObjectCreator.AUD, 1, 100, 10, 100, 10);
			invoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = invoice.Lines[0];
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashVATRecord = testObjectCreator.CreateCashBasisVAT(invoice.Lines[0], -100, -10);
			Factory.Save();

			var sql = string.Format("SELECT TOP 1 YCC_YC FROM dbo.AccCashBasisVATQueue WHERE YCC_YC = '{0}';", cashVATRecord.PK);

			var queuePK = TestConnection.ExecuteScalar(sql);
			AssertEquals(queuePK, cashVATRecord.PK);

			TestConnection.ExecuteNonQuery("TRUNCATE TABLE AccCashBasisVATQueue");
			queuePK = TestConnection.ExecuteScalar(sql);
			AssertNull(queuePK);

			var reAggregator = new SinglePeriodReaggregator(Factory);
			reAggregator.PeriodToReaggregate = 201501;
			reAggregator.ReAggregate();

			queuePK = TestConnection.ExecuteScalar(sql);
			AssertEquals(queuePK, cashVATRecord.PK);
		}

		[SuspendCriticalValidation]
		[TestDate(2020, 01, 15)]
		public void TestReAggregationReQueuesTaxGLMovementRecords()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var currentBranch = GlbBranch.CurrentBranch;

			var companyABC = Creator.CreateNewCompany("ABC");
			var branchABC = Creator.CreateNewBranch(companyABC, "ABC");

			var companyXYZ = Creator.CreateNewCompany("XYZ");
			var branchXYZ = Creator.CreateNewBranch(companyXYZ, "XYZ");

			var currentPeriod = PeriodHelper.CurrentPeriod.AM_Period;
			var futurePeriod = PeriodHelper.FuturePeriod.AM_Period;

			var taxTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			var taxTransaction1 = taxTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Branch = branchABC });
			var taxTransaction2 = taxTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyXYZ, Branch = branchXYZ });
			var taxTransaction3 = taxTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = currentCompany, Branch = currentBranch });

			var glMovement_abcCompanyCurrentPeriod = taxTestObjectCreator.CreateAccTaxGLMovement(taxTransaction1.PK, Creator.GLHeader1.PK, Creator.GLHeader2.PK, 10, currentPeriod);
			var glMovement_xyzCompanyFuturePeriod = taxTestObjectCreator.CreateAccTaxGLMovement(taxTransaction2.PK, Creator.GLHeader1.PK, Creator.GLHeader2.PK, 10, futurePeriod);
			var glMovement_currentCompanyAndPeriod = taxTestObjectCreator.CreateAccTaxGLMovement(taxTransaction3.PK, Creator.GLHeader1.PK, Creator.GLHeader2.PK, 10, currentPeriod);

			Factory.Save();

			var actualDBCount = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.AccTaxGLMovementQueue");
			AssertEquals("DB count", 3, actualDBCount);

			TestConnection.ExecuteNonQuery("TRUNCATE TABLE AccTaxGLMovementQueue");

			actualDBCount = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.AccTaxGLMovementQueue");
			AssertEquals("DB count after truncate", 0, actualDBCount);

			ReaggregateAndAssert(currentCompany, currentPeriod, glMovement_currentCompanyAndPeriod);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchABC.PK.ToGuid(), Env.CurrentDepartmentPK)) //switch to company ABC
			{
				ReaggregateAndAssert(companyABC, currentPeriod, glMovement_abcCompanyCurrentPeriod);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchXYZ.PK.ToGuid(), Env.CurrentDepartmentPK)) //switch to company ABC
			{
				ReaggregateAndAssert(companyXYZ, futurePeriod, glMovement_xyzCompanyFuturePeriod);
			}

			void ReaggregateAndAssert(GlbCompany company, ZInt period, AccTaxGLMovement glMovement)
			{
				var sql = string.Format("SELECT TOP 1 ATQ_ATM FROM dbo.AccTaxGLMovementQueue WHERE ATQ_ATM = '{0}';", glMovement.PK);
				var reAggregator = new SinglePeriodReaggregator(Factory);
				reAggregator.PeriodToReaggregate = period;
				reAggregator.ReAggregate();

				var queuePK = TestConnection.ExecuteScalar(sql);
				AssertEquals($"Result of reaggregating Company: {company.GC_Code} for Period: {period}", glMovement.PK, queuePK);
			}
		}

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SinglePeriodReaggregator(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();
		}

		AccountingPeriodTestHelper PeriodHelper;
		TestObjectCreator Creator;

		#endregion
	}
}
