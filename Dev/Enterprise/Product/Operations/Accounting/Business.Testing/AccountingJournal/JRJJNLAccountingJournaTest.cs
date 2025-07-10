using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JRJJNLAccountingJournal))]
	public class JRJJNLAccountingJournalTest : AccountingJournalTest
	{
		public override void TestAccountingJournalLines()
		{
			Assert(true);
		}

		public override void TestAJOptionalFields()
		{
			var job = Creator.CreateJob("S00001222", Creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jrj = Creator.CreateJobRevenueJournal(Creator.CC1, job, 200m);
			Factory.Save();

			var aj = new JRJJNLAccountingJournal(jrj, ReadonlyFactory);
			AssertEquals("Optional Field Count", 1, aj.ApplicableOptionalFields.Count);
			AssertEquals("STATUS", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.StatusText));
			AssertEquals("STATUS value", "Completed", aj.ApplicableOptionalFields[AccountingJournal.StatusText]);
		}

		protected override AccountingJournal CreateTestAccountingJournal()
		{
			return GetNewBusinessObject() as AccountingJournal;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = Creator.CreateJob("S00001222", Creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jrj = Creator.CreateJobRevenueJournal(Creator.CC1, job, 200m);
			transaction = jrj;
			return new JRJJNLAccountingJournal(jrj, ReadonlyFactory);
		}

		public void TestNotSupportMultiSubAccountTypeCode()
		{
			GetNewBusinessObject();
			var journal = GetJournalForMultiSubAccountTypeCode(transaction, LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal, false);
			AssertNull(journal);
		}
	}
}
