using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobStatusUpdateRestrictionRuleHelperTest : TestCaseWithFactory
	{
		public void TestValidateChangeJobStatus()
		{
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL");
			var job = new Job.Loader(Factory, shipment).TryCreateWithMutex(false);

			var fromJobStatus = JobHeaderStatus.Working.Code;

			var jobStatusUpdateRestrictionRuleCollection = AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.DefaultValue;
			var jobStatusUpdateRestrictionRule = jobStatusUpdateRestrictionRuleCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == fromJobStatus);
			jobStatusUpdateRestrictionRule.WorkOnHold = "YES";
			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "NO";

			using (AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobStatusUpdateRestrictionRuleCollection))
			{
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				var result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("Job should not be in database", false, job.IsInDatabase);
				AssertEquals("Should be no errors when job is not in database", "", result);

				Factory.Save();
				AssertEquals("Job should be in database", true, job.IsInDatabase);

				job.JH_Status = "";
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("Should be no errors when to job status is empty", "", result);

				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();
				job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("Should be no errors when the configuration of 'CLS' cannot be found in the registry 'Job Status Update Restriction Rule'", "", result);

				job.JH_Status = fromJobStatus;
				Factory.Save();
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = false;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Change Status of Working Jobs

You must revert the value of this field to its original value of 'WRK - Working'", result);

				job.JH_Status = JobHeaderStatus.Closed.Code;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Change Status of Working Jobs

You must revert the value of this field to its original value of 'WRK - Working'", result);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = true;
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("User has security from WRK to WHL", "", result);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = false;
				job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("No security is required to change from status WKR to JRB", "", result);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = true;
				result = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(job);
				AssertEquals("No security is required to change from status WKR to JRB", "", result);
			}
		}

		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;
	}
}
