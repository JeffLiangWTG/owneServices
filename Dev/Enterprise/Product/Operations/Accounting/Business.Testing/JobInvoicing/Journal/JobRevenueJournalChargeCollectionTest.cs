using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalChargeCollection))]
	public class JobRevenueJournalChargeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobRevenueJournalChargeCollection>
	{
		public void TestJob()
		{
			Job job = Factory.NewJobForTesting<Job>();
			AssertEquals("Precondition: TestCollection.Count", 0, TestCollection.Count);
			TestCollection.Job = job;
			AssertEquals("TestCollection.Job", job, TestCollection.Job);

			TestCollection.Job = null;
			JobRevenueJournalCharge journalCharge = TestCollection.AddNew();
			AssertNull("Precondition: TestCollection.Job", journalCharge.Job);
			TestCollection.Job = job;
			AssertEquals("TestCollection.Job", job, journalCharge.Job);

			journalCharge = TestCollection.AddNew();
			AssertEquals("TestCollection.Job", job, journalCharge.Job);
		}

		public void TestSetDefaultsForNewChild()
		{
			JobRevenueJournalCharge journalCharge = TestCollection.AddNew();
			AssertEquals("Journal.Lines.Count", 2, Journal.Lines.Count);
			journalCharge.ChargeCode = TestObjectCreator.CC1.PK;
			journalCharge.Currency = TestObjectCreator.USD.RX_Code;
			journalCharge.ExchangeRate = 10M;

			journalCharge = TestCollection.AddNew();
			AssertEquals("Journal.Lines.Count", 4, Journal.Lines.Count);
			AssertEquals("journalCharge.ChargeCode", TestObjectCreator.CC1.PK, journalCharge.ChargeCode);
			AssertEquals("journalCharge.Currency", TestObjectCreator.USD.RX_Code, journalCharge.Currency);
			AssertEquals("journalCharge.ExchangeRate", 10M, journalCharge.ExchangeRate);
		}

		public void TestDetachCharges()
		{
			JobRevenueJournalCharge journalCharge1 = TestCollection.AddNew();
			JobRevenueJournalCharge journalCharge2 = TestCollection.AddNew();
			AssertEquals("Precondition: TestCollection.Count", 2, TestCollection.Count);
			AssertEquals("Precondition: Journal.Lines.Count", 4, Journal.Lines.Count);

			TestCollection.DetachCharges();
			AssertEquals("TestCollection.Count", 0, TestCollection.Count);
			AssertEquals("Journal.Lines.Count", 4, Journal.Lines.Count);
			AssertEquals("journalCharge1.IsDeleted", true, journalCharge1.IsDeleted);
			AssertEquals("journalCharge2.IsDeleted", true, journalCharge2.IsDeleted);
		}

		protected override JobRevenueJournalChargeCollection GetCollectionToTest()
		{
			return new JobRevenueJournalChargeCollection(Journal);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobRevenueJournalCharge(Journal);
		}

		JobRevenueJournal Journal
		{
			get { return Journal_cached ?? (Journal_cached = Factory.New<JobRevenueJournal>()); }
		}
		JobRevenueJournal Journal_cached;

		JobRevenueJournalChargeCollection TestCollection
		{
			get { return Collection; }
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
