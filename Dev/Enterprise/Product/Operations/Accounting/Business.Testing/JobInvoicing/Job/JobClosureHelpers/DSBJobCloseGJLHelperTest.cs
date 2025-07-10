using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DSBJobCloseGJLHelperTest : TestCaseWithFactory
	{
		[TestDate(2020, 6, 20)]
		public void TestCreateGJLForDsbClosureJob()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-3));
			Factory.Save();

			var collection = AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.Value;
			collection.AddDefaultValues(AccountingConstants.DisbursementShortfallSurplusCode.DisbursementShortfallSurplus,
				AccountingConfigurationRegistry.Instance.GetDefaultDisbursementShortfallSurplusDesc(),
				(NoResString)"Test Dsb Desc", 1);
			AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var creator = new DSBJobCloseGJLHelper(Factory);

			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out var job1, out var job2, out var batch1,
				out var line11, out var line12, out var line21, out var line22,
				out var chargeCode1, out var chargeCode2,
				out var charge1Cst, out var charge1Rev, out var charge2Cst, out var charge2Rev);

			var surplusGLAccount1 = chargeCode1.AC_AG_DisbursementSurplusAccount;
			var shortfallGLAccount1 = chargeCode1.AC_AG_DisbursementShortfallAccount;
			var surplusGLAccount2 = chargeCode2.AC_AG_DisbursementSurplusAccount;
			var shortfallGLAccount2 = chargeCode2.AC_AG_DisbursementShortfallAccount;

			var costGLAccount1 = chargeCode1.AC_AG_CostAccount;
			var revenueGLAccount1 = chargeCode1.AC_AG_RevenueAccount;
			var costGLAccount2 = chargeCode2.AC_AG_CostAccount;
			var revenueGLAccount2 = chargeCode2.AC_AG_RevenueAccount;

			var journal = creator.CreateGLJournal(batch1.PK);

			AssertEquals("Pre-condition", 0, Factory.Load<AccGLAggregate>(new ZQuery()).Length);

			Factory.Save();

			var lines = journal.Lines.Cast<GLJournalLine>();
			AssertEquals(8, lines.Count());

			var gjlLine1 = lines.Single(x => x.AL_AG == revenueGLAccount1);
			var gjlLine1Balance = lines.Single(x => x.AL_AG == shortfallGLAccount1 && x.AL_LineAmount == -200m);
			var gjlLine2 = lines.Single(x => x.AL_AG == costGLAccount1);
			var gjlLine2Balance = lines.Single(x => x.AL_AG == shortfallGLAccount1 && x.AL_LineAmount == 100m);

			var gjlLine3 = lines.Single(x => x.AL_AG == revenueGLAccount2);
			var gjlLine3Balance = lines.Single(x => x.AL_AG == shortfallGLAccount2 && x.AL_LineAmount == -50m);
			var gjlLine4 = lines.Single(x => x.AL_AG == costGLAccount2);
			var gjlLine4Balance = lines.Single(x => x.AL_AG == shortfallGLAccount2 && x.AL_LineAmount == 200m);

			AssertEquals("TEST DSB DESC - Bulk Closure [B001]", journal.AH_Desc);
			AssertEquals(new AccountingPeriodCalculator(Factory).GetLastDayForPeriod(ZDateTime.Now), journal.AH_PostDate);

			AssertGLJournalLine(gjlLine1, 200m, nameof(DebitCredit.DR));
			AssertGLJournalLine(gjlLine1Balance, -200m, nameof(DebitCredit.CR));
			AssertGLJournalLine(gjlLine2, -100m, nameof(DebitCredit.CR));
			AssertGLJournalLine(gjlLine2Balance, 100m, nameof(DebitCredit.DR));
			AssertGLJournalLine(gjlLine3, 50m, nameof(DebitCredit.DR));
			AssertGLJournalLine(gjlLine3Balance, -50m, nameof(DebitCredit.CR));
			AssertGLJournalLine(gjlLine4, -200m, nameof(DebitCredit.CR));
			AssertGLJournalLine(gjlLine4Balance, 200m, nameof(DebitCredit.DR));

			var requests = Factory.Load<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, journal.PK));
			AssertEquals(1, requests.Length);
			var request = requests.First();
			AssertEquals(GenApprovalRequestApprovalStatus.Posted, request.XP_ApprovalStatus);
			AssertEquals("E", request.XP_GS_NKApprovingUser1);
			AssertEquals(ZDateTime.Now, request.XP_ApprovalDate);

			var aggregateLines = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals(8, aggregateLines.Length);

			var aggregateLine1 = aggregateLines.Single(x => x.AA_AG == revenueGLAccount1);
			var aggregateLine1Balance = aggregateLines.Single(x => x.AA_AG == shortfallGLAccount1 && x.AA_Amount == -200);
			var aggregateLine2 = aggregateLines.Single(x => x.AA_AG == costGLAccount1);
			var aggregateLine2Balance = aggregateLines.Single(x => x.AA_AG == shortfallGLAccount1 && x.AA_Amount == 100);
			var aggregateLine3 = aggregateLines.Single(x => x.AA_AG == revenueGLAccount2);
			var aggregateLine3Balance = aggregateLines.Single(x => x.AA_AG == shortfallGLAccount2 && x.AA_Amount == -50);
			var aggregateLine4 = aggregateLines.Single(x => x.AA_AG == costGLAccount2);
			var aggregateLine4Balance = aggregateLines.Single(x => x.AA_AG == shortfallGLAccount2 && x.AA_Amount == 200);

			AssertAccGLAggregateLine(aggregateLine1, 200);
			AssertAccGLAggregateLine(aggregateLine1Balance, -200);
			AssertAccGLAggregateLine(aggregateLine2, -100);
			AssertAccGLAggregateLine(aggregateLine2Balance, 100);
			AssertAccGLAggregateLine(aggregateLine3, 50);
			AssertAccGLAggregateLine(aggregateLine3Balance, -50);
			AssertAccGLAggregateLine(aggregateLine4, -200);
			AssertAccGLAggregateLine(aggregateLine4Balance, 200);
		}

		void AssertGLJournalLine(GLJournalLine line, decimal amount, string debitCreditSign)
		{
			AssertEquals(amount, line.AL_LineAmount);
			AssertEquals(debitCreditSign, line.DebitCreditSign);
			AssertEquals("TEST DSB DESC - Bulk Closure [B001]", line.AL_Desc);
			AssertEquals(GlbCompany.CurrentCompany.PK, line.AL_GC);
			AssertEquals(GlbBranch.CurrentBranch.PK, line.AL_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, line.AL_GE);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
		}

		void AssertAccGLAggregateLine(AccGLAggregate line, decimal amount)
		{
			AssertEquals(amount, line.AA_Amount);
			AssertEquals(202101, line.AA_Period);
			AssertEquals(GlbCompany.CurrentCompany.PK, line.AA_GC);
			AssertEquals(GlbBranch.CurrentBranch.PK, line.AA_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, line.AA_GE);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
