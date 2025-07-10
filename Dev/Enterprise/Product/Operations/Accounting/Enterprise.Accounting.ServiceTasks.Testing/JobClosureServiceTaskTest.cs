using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobClosureServiceTask))]
	public class JobClosureServiceTaskTest : ServiceTaskTestCase<JobClosureServiceTask>
	{
		[TestDate(2018, 08, 05)]
		public void TestJobClosureServiceTask_RunTask()
		{
			GlbCompany[] companies = AccountingUtils.GetAllActiveCompanies(Factory);
			GlbBranch[] branches = CreateBranch(companies);

			SetRegistryForTest(companies[0]);
			var job = SaveJobForTest(companies[0], branches[0]);

			var task = new JobClosureServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertEquals(27, serviceLog.Count);
			var expectedMessage = $@"Debug|Starting job status updating process.
Debug|Checking queue for jobs.
Debug|All jobs in the queue have been processed. Attempting to repopulate the queue.
Debug|Truncating the queue table before populating it again.
Debug|Populating queue...
Debug|Moving watermark forward.
Debug|Checking queue for jobs.
Debug|[Row 1][DSB]: Loaded job EBM22Q33TU475BXH3P60 into memory along with all related data
Debug|[EDI][EBM22Q33TU475BXH3P60][DSB]: This job is not eligible for automatic status update
Debug|[Row 1][JFC]: Loaded job EBM22Q33TU475BXH3P60 into memory along with all related data
Debug|[EDI][EBM22Q33TU475BXH3P60][JFC]: This job is not eligible for automatic status update
Debug|[Row 1][JCS]: Loaded job EBM22Q33TU475BXH3P60 into memory along with all related data
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Verifying whether this job can be closed.
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Auto Job Closure eligibility verification details - 
 [EBM22Q33TU475BXH3P60]:
Job Type: SHP | Direction: UKN | Mode:  | Department: BRN | Open WIP: No | Open Accrual: No | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: No.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: No- Open Accrual: No- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 24 Jul 2018.
Calculated earliest job closure date: 03 Aug 2018.
Satisfied registry settings.
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: There is no unposted consol cost to delete.
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Attempting to close EBM22Q33TU475BXH3P60.
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Status of EBM22Q33TU475BXH3P60 is changed from 'WRK' to 'CLS' and Job Close Date is updated to 05 Aug 2018
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Attempting to save changes.
Debug|[EDI][EBM22Q33TU475BXH3P60][JCS]: Successfully saved changes.
Debug|Checking queue for jobs.
Debug|All jobs in the queue have been processed. Attempting to repopulate the queue.
Debug|Truncating the queue table before populating it again.
Debug|Populating queue...
Debug|Moving watermark forward.
Debug|No more job for auto update status.
Information|Job Status Update cycle completed.
Assessed 1 jobs for status update.
Debug|Job Status Update cycle completed.";
			AssertMultilineASCIIEquals(expectedMessage, serviceLog.ToString());

			using (Env.Instance.TemporaryServiceTaskContext(JobClosureServiceTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
		}

		public void TestJobClosureTaskShouldUseMultipleInstances()
		{
			Assert("There is no sql lock to synchronize job closing among multiple instances. So, running multiple instances can cause inconsistency", !GetHostedServiceAttributes()[0].AllowsMultipleInstances);
		}

		public void TestJobClosureTaskRunsOnceDaily()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		// No nudging: queue table is present, but processing is done in batches and queue is reported via IHostedServiceQueueProvider.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestQueueProvider()
		{
			SetRegistryForTest(GlbCompany.CurrentCompany);

			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(JobClosureServiceTask.Code);
			AssertNotNull("Service task attribute exists and refers to a valid queue provider.", queueProvider);

			var queueCount = Db.Connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.JobToCloseQueue");
			AssertEquals("Precondition: no records in JobToCloseQueue", 0, queueCount);

			AssertEquals("Queue for service task is empty when JobToCloseQueue is empty", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			var job1 = SaveJobForTest(GlbCompany.CurrentCompany, GlbCompany.CurrentCompany.FirstActiveBranch);
			var createDate1 = new DateTime(2023, 11, 1);
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.JobToCloseQueue
([JHC_SystemCreateTimeUtc], [JHC_GC], [JHC_JH])
VALUES ('{createDate1.ToString("s")}', '{GlbCompany.CurrentCompany.PK}', '{job1.PK}')");
			AssertEquals("Queue for service task matches JobToCloseQueue", 1, queueProvider.QueueResult.QueueSize);
			var queueAgeInSeconds = (int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds;
			var createdTimeInSeconds = (int)(DateTime.UtcNow - createDate1).TotalSeconds;
			NUnit.Framework.Assert.That(queueAgeInSeconds, NUnit.Framework.Is.EqualTo(createdTimeInSeconds).Within(60));

			var job2 = SaveJobForTest(GlbCompany.CurrentCompany, GlbCompany.CurrentCompany.FirstActiveBranch);
			Factory.Save();
			var createDate2 = new DateTime(2023, 10, 1);
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.JobToCloseQueue
([JHC_SystemCreateTimeUtc], [JHC_GC], [JHC_JH])
VALUES ('{createDate2.ToString("s")}', '{GlbCompany.CurrentCompany.PK}', '{job2.PK}')");
			AssertEquals("Queue for service task matches JobToCloseQueue", 2, queueProvider.QueueResult.QueueSize);
			NUnit.Framework.Assert.That((int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)(DateTime.UtcNow - createDate2).TotalSeconds).Within(60));

			AccountingConfigurationRegistry.Instance.NumberOfJobsProcessedForAutoClosingInTheCurrentBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var task = new JobClosureServiceTask();
			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Queue for service task is reset to zero when service task runs", 0, queueProvider.QueueResult.QueueSize);
		}

		protected GlbBranch[] CreateBranch(GlbCompany[] companies)
		{
			GlbBranch[] branches = new GlbBranch[companies.Length];
			for (int i = 0; i < companies.Length; i++)
			{
				branches[i] = TestObjectCreator.CreateNewBranch(companies[i], "BR" + i.ToString());
			}
			return branches;
		}

		void SetRegistryForTest(GlbCompany company)
		{
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			var closureConfigLine = new JobClosureConfiguration();
			closureConfigLine.JobType = "ALL";
			closureConfigLine.DirectionCode = "";
			closureConfigLine.Mode = "";
			closureConfigLine.JobClosureDateOptionCode = "JOP";
			closureConfigLine.Offset = 10;
			regValue.ConfigurationCollection.AddRange(closureConfigLine);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, regValue);
		}

		Job SaveJobForTest(GlbCompany company, GlbBranch branch)
		{
			var plugin = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = TestObjectCreator.CreateJob(plugin, false);
			job.JH_GC = company.PK;
			job.JH_GB = branch.PK;
			job.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-12);
			Factory.Save();
			return job;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
