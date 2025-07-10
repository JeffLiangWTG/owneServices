using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobRevenueJournalReOpenClosedJobDataProviderTest : TestCaseWithFactory
	{
		public void TestJRJReOpenClosedJobDataProvider_Exception_WhenJobRevenueJournalIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: jobRevenueJournal", () => new JobRevenueJournalReOpenClosedJobDataProvider(null));
			AssertNoExceptionThrown(() => new JobRevenueJournalReOpenClosedJobDataProvider(Factory.New<JobRevenueJournal>()));
		}

		public void TestGetAllJobs_EmptyList_WhenNoJournalLines()
		{
			var jobRevenueJournal = Factory.New<JobRevenueJournal>();

			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new JobRevenueJournalReOpenClosedJobDataProvider(jobRevenueJournal);
			var allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();

			AssertEquals("allRelatedJobList count", 0, allRelatedJobList.Count);
		}

		public void TestGetAllJobs_ValidJobsList_WhenHasJournalLines()
		{
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1001"));
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1002"));
			Factory.Save();

			var jobRevenueJournal = Factory.New<JobRevenueJournal>();
			TestObjectCreator.CreateJobRevenueJournalLine(jobRevenueJournal, TestObjectCreator.CC1, job1.PK, 100M, "CR");
			TestObjectCreator.CreateJobRevenueJournalLine(jobRevenueJournal, TestObjectCreator.CC1, job2.PK, 100M, "DR");
			TestObjectCreator.CreateJobRevenueJournalLine(jobRevenueJournal, TestObjectCreator.CC1, job1.PK, 150M, "CR");

			var journalLineWithNullJob = TestObjectCreator.CreateJobRevenueJournalLine(jobRevenueJournal, TestObjectCreator.CC1, ZGuid.Empty, 150M, "DR");

			AssertNull("Precondition: line job", journalLineWithNullJob.Job);
			AssertEquals("Precondition: lines count", 4, jobRevenueJournal.JournalLines.Count);

			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new JobRevenueJournalReOpenClosedJobDataProvider(jobRevenueJournal);

			var allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();

			AssertContainsExactElementsInAnyOrder("allRelatedJobList", new[] { job1, job1, job2 }, allRelatedJobList);
		}

		public void TestJobReopenLogText()
		{
			var jobRevenueJournal = Factory.New<JobRevenueJournal>();
			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new JobRevenueJournalReOpenClosedJobDataProvider(jobRevenueJournal);

			var jobReopenLogText = reOpenClosedJobDataProvider.JobReopenLogText();
			AssertEquals(" - JC JRJ", jobReopenLogText);
		}

		public void TestFactoryReturnsJobRevenueJournalFactory()
		{
			var jobRevenueJournal = Factory.New<JobRevenueJournal>();
			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new JobRevenueJournalReOpenClosedJobDataProvider(jobRevenueJournal);

			AssertNotNull(reOpenClosedJobDataProvider.Factory);
			AssertEquals(Factory, reOpenClosedJobDataProvider.Factory);
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
