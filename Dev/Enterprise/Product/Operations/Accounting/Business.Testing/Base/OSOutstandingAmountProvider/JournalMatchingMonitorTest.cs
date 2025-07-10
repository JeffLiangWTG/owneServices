using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class JournalMatchingMonitorTest : TestCaseWithFactory
	{
		public void TestShouldMakeOSOutstandingAmountApplicable()
		{
			var transactionHeader = Creator.CreateJournal<APJournal>(199m, new CargoWise.Types.ZDateTime(2025, 01, 22), Creator.ABIGAS.PK);
			var journalMonitor = new JournalMatchingMonitor_ForTest(transactionHeader);

			Assert("AH_IsOSOutstandingAmountApplicable should be false and transaction InvoiceUnpaid is false and New OS Outstanding Amount feature is not enabled", !journalMonitor.ShouldMakeOSOutstandingAmountApplicable_ForTest);
			transactionHeader.AH_InvoiceAmount = -5;
			transactionHeader.AH_LocalOutstandingAmount = 5;
			Assert("Only InvoiceUnpaid is true", !journalMonitor.ShouldMakeOSOutstandingAmountApplicable_ForTest);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(journalMonitor.ShouldMakeOSOutstandingAmountApplicable_ForTest);
			var matchLink = Creator.CreateMatchLink(transactionHeader);
			Assert("transactionHeader's matchLink is not null", !journalMonitor.ShouldMakeOSOutstandingAmountApplicable_ForTest);
			matchLink.Delete();
			transactionHeader.AH_IsOSOutstandingAmountApplicable = true;
			Assert("Journal's AH_IsOSOutstandingAmountApplicable is true", !journalMonitor.ShouldMakeOSOutstandingAmountApplicable_ForTest);
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
	}

	public class JournalMatchingMonitor_ForTest : JournalMatchingMonitor
	{
		public JournalMatchingMonitor_ForTest(TransactionHeader sourceHeader) : base(sourceHeader) { }

		public bool ShouldMakeOSOutstandingAmountApplicable_ForTest => base.ShouldMakeOSOutstandingAmountApplicable;
	}
}
