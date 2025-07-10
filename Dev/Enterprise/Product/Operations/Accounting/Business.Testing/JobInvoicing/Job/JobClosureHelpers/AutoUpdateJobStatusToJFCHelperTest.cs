namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class AutoUpdateJobStatusToJFCHelperTest : JobClosureProcessorTestHelper
	{
		[TestDate(2020, 09, 21)]
		public void TestJobStatusIsAlreadyJFC()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Closing few Jobs
			reloadedTestJob.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			reloadedTestJob = Factory.Load<Job>(reloadedTestJob.PK);
			helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: JFC.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - From Status: ) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 11 Sep 2020.
Calculated earliest job ready for financial closure date: 21 Sep 2020.
Did not satisfy registry settings.";

			AssertContains("Job status is already JFC", $"Following job status is already JFC: {reloadedTestJob.JH_JobNum}", msg);
			Assert("Job that can not be updated job status to JFC", !helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}

		[TestDate(2020, 09, 21)]
		public void TestJobStatusIsAlreadyCLS()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			reloadedTestJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			reloadedTestJob = Factory.Load<Job>(reloadedTestJob.PK);
			helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: CLS.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - From Status: ) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 11 Sep 2020.
Calculated earliest job ready for financial closure date: 21 Sep 2020.
Did not satisfy registry settings.";

			AssertContains("Job status is already CLS", $"Following job status is already CLS: {reloadedTestJob.JH_JobNum}", msg);
			Assert("Job that can not be updated job status to JFC", !helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}

		[TestDate(2020, 09, 21)]
		public void TestJobWithFutureClosureDateAreNotSelectedForAutoClosure()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Changing JOP date
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			Factory.Save();

			reloadedTestJob = Factory.Load<Job>(reloadedTestJob.PK);
			helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: WRK.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - From Status: ) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 16 Sep 2020.
Calculated earliest job ready for financial closure date: 26 Sep 2020.
Did not satisfy registry settings.";

			AssertContains("Too young to be updated", $"Following job are not old enough to be automatically updated job status to JFC: {reloadedTestJob.JH_JobNum}", msg);
			Assert("Job that can not be updated job status to JFC", !helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}

		[TestDate(2020, 09, 21)]
		public void TestJobWithFromJobStatusAreNotSelectedForAutoClosure()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD", fromJobStatus: "INV"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			testJob.JH_Status = "INV";

			Factory.Save();

			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Changing job status
			testJob.JH_Status = "WRK";
			Factory.Save();

			reloadedTestJob = Factory.Load<Job>(reloadedTestJob.PK);
			helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: WRK.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - From Status: INV) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 11 Sep 2020.
Calculated earliest job ready for financial closure date: 21 Sep 2020.
Did not satisfy registry settings.";

			AssertContains("Is not a match job status can not be updated.", $"Following job could not be automatically updated job status to JFC due to their current status: {testJob.JH_JobNum}", msg);
			Assert("Job that can not be updated job status to JFC", !helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}

		[TestDate(2020, 09, 21)]
		public void TestJobWithoutAnyError()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: WRK.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - From Status: ) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 11 Sep 2020.
Calculated earliest job ready for financial closure date: 21 Sep 2020.
Satisfied registry settings.";

			AssertEquals("No Error", string.Empty, msg);
			Assert("JobsThatCanBeUpdatedJobStatusToJFC", helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}

		public void TestJobNoMatchingConfigurationFound()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			AssertJobNoMatchingConfigurationFound(testJob);
		}

		public void TestJobNoMatchingConfigurationFound_WithoutUpdateTypeItem()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			var testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			AssertJobNoMatchingConfigurationFound(testJob);
		}

		void AssertJobNoMatchingConfigurationFound(Job testJob)
		{
			var reloadedTestJob = Factory.Load<Job>(testJob.PK);
			var helper = new AutoUpdateJobStatusToJFCHelper(reloadedTestJob);
			var msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{reloadedTestJob.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Status: WRK.
No matching configuration found.";

			AssertContains("No matching configuration found", $"Following job are not old enough to be automatically updated job status to JFC: {reloadedTestJob.JH_JobNum}", msg);
			Assert("Job that can not be updated job status to JFC", !helper.JobThatCanBeUpdatedJobStatusToJFC);
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails());
		}
	}
}
