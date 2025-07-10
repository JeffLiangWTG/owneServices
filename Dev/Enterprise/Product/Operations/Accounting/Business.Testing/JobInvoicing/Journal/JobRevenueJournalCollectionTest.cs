using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalCollection))]
	public class JobRevenueJournalCollectionTest : TransactionHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobRevenueJournalCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobRevenueJournal>();
		}

		public void TestRelationshipFilter()
		{
			GlbBranch anotherBranch = Factory.NewWithValidTestData<GlbBranch>();

			JobRevenueJournal journal = Factory.NewWithValidTestData<JobRevenueJournal>();
			JCJournalHeader cfxJournal = Factory.NewWithValidTestData<JCJournalHeader>();
			JobRevenueJournal journal_AnotherBranch = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal_AnotherBranch.AH_GB = anotherBranch.PK;

			Collection.Load();
			AssertEquals("Collection should contain 1 element", 1, Collection.Count);
			Assert("The element should be APInv", Collection.Contains(journal));
		}
	}
}
