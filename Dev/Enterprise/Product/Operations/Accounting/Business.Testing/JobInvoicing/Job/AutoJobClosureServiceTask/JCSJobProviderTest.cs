using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSJobProviderTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoadJobPKFromQueue()
		{
			var companies = Helper.CreateCompany(3);
			Helper.CreateBranch(companies);

			Helper.CreateSHPJob(companies[0].PK, companies[0].Branches[0].PK, ZDateTime.Today);
			Helper.CreateSHPJob(companies[1].PK, companies[1].Branches[0].PK, ZDateTime.Today);
			Helper.CreateSHPJob(companies[2].PK, companies[2].Branches[0].PK, ZDateTime.Today);
			Factory.Save();

			new JobQueueResetter().Reset();
			new JCSQueuePopulator().Populate(new DateTime(1900, 1, 1), new ZGuid[] { companies[0].PK, companies[1].PK, companies[2].PK });

			var jobProvider = new JCSJobProvider();
			var job = jobProvider.LoadJobPKFromQueue();
			AssertEquals("Job with Row Number 1 will be picked", 1, job.Value.RowNumber);

			job = jobProvider.LoadJobPKFromQueue();
			AssertEquals("Job with Row Number 2 will be picked", 2, job.Value.RowNumber);

			job = jobProvider.LoadJobPKFromQueue();
			AssertEquals("Job with Row Number 3 will be picked", 3, job.Value.RowNumber);

			job = jobProvider.LoadJobPKFromQueue();
			Assert("Job should be null as no jobs remain in queue", !job.HasValue);
		}

		JCSTestHelper Helper
		{
			get { return helper ?? (helper = new JCSTestHelper(TestObjectCreator)); }
		}
		JCSTestHelper helper;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
