using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSQueuePopulatorTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestPopulate_OnlyConfiguredCompaniesJob()
		{
			var companies = Helper.CreateCompany(3);
			Helper.CreateBranch(companies);
			var companiesWithConfig = new[] { companies[0].PK, companies[1].PK };
			var jobs = Helper.CreateJobs(25, companies);

			var expectedJobs = jobs.Where(j => companiesWithConfig.Contains(j.JH_GC));
			var notExpectedJobs = jobs.Where(j => companies[2].PK == j.JH_GC);

			var populator = new JCSQueuePopulator();
			populator.Populate(new DateTime(2020, 1, 1), companiesWithConfig);

			AssertQueuedJobs(expectedJobs, notExpectedJobs);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestPopulate_OnlyNonClosedJobs()
		{
			var companies = Helper.CreateCompany(2);
			Helper.CreateBranch(companies);

			var companiesWithConfig = new[] { companies[0].PK, companies[1].PK };
			var jobs = Helper.CreateJobs(25, companies);
			jobs[0].JH_Status = "CLS";
			jobs[1].JH_Status = "CLS";
			jobs[2].JH_Status = "CLS";
			Factory.Save();

			var expectedJobs = jobs.Where(j => j.JH_Status != "CLS");
			var notExpectedJobs = new[] { jobs[0], jobs[1], jobs[2] };

			var populator = new JCSQueuePopulator();
			populator.Populate(new DateTime(2020, 1, 1), companiesWithConfig);

			AssertQueuedJobs(expectedJobs, notExpectedJobs);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestQuotationaAreNotQueued()
		{
			var companies = Helper.CreateCompany(2);
			var branches = Helper.CreateBranch(companies);

			var companiesWithConfig = new[] { companies[0].PK, companies[1].PK };
			var jobs = Helper.CreateJobs(25, companies);
			var thJob1 = Helper.CreateTHJob(branches[0].PK, branches[0].GB_GC, "thjob1");
			var thJob2 = Helper.CreateTHJob(branches[1].PK, branches[1].GB_GC, "thjob2");
			Factory.Save();

			var notExpectedJobs = new[] { thJob1, thJob2 };

			var populator = new JCSQueuePopulator();
			populator.Populate(new DateTime(2020, 1, 1), companiesWithConfig);

			AssertQueuedJobs(jobs, notExpectedJobs);
		}

		[TestDate(2020, 08, 12)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestPopulate_QueueSize()
		{
			var companies = Helper.CreateCompany(2);
			Helper.CreateBranch(companies);

			var companiesWithConfig = new[] { companies[0].PK, companies[1].PK };
			var jobs = Helper.CreateJobs(25, companies);
			jobs[0].JH_SystemCreateTimeUtc = new ZDateTime(2019, 01, 12);
			jobs[1].JH_Status = "CLS";
			jobs[2].JH_SystemCreateTimeUtc = new ZDateTime(2020, 08, 25);
			Factory.Save();

			var notExpectedJobs = new[] { jobs[0], jobs[1], jobs[2] };
			var expectedJobs = jobs.Except(notExpectedJobs);

			using (AccountingConfigurationRegistry.Instance.AutoJobClosureQueueMaximumLength.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var populator = new JCSQueuePopulator();
				populator.Populate(new DateTime(2020, 1, 1), companiesWithConfig);
			}

			AssertQueuedJobs(expectedJobs, notExpectedJobs);
		}

		void AssertQueuedJobs(IEnumerable<Job> expectJobs, IEnumerable<Job> notExpectedJobs = null)
		{
			var queuedJobs = Helper.GetQueuedJobs();
			AssertEquals("Job Count", expectJobs.Count(), queuedJobs.Count());

			foreach (var queuedJob in queuedJobs)
			{
				var job = expectJobs.First(j => j.PK == queuedJob.JobPK);
				AssertNotNull(job);
				AssertEquals("Company", job.JH_GC, queuedJob.CompanyPK);
				AssertEquals("CreationTime", job.JH_SystemCreateTimeUtc.ToSmallDateTimeFloor(), queuedJob.CreationTime);
			}

			foreach (var queuedJob in queuedJobs)
			{
				var job = notExpectedJobs?.FirstOrDefault(j => j.PK == queuedJob.JobPK);
				AssertNull(job);
			}
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
