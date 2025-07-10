using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobStatusActionMethodApplicator))]
	public class UpdateJobStatusActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestStatusCode()
		{
			SetupJobPropertySameWithApplicatorValue();

			AssertEquals(JobHeaderStatus.Complete.Code, Applicator.StatusCode);
			AssertEquals(Factory.GetCachedValue<JobHeaderStatusList>(), Applicator.StatusCodeList);
			AssertNotNull(Applicator.StatusCodeInfo);
		}

		public void TestValidateStatus()
		{
			Applicator.StatusCode = string.Empty;
			AssertHasError(Applicator.StatusCodeInfo, "Please enter a value.");

			SetupApplicatorPropertyForValidation();
			AssertHasError(Applicator.StatusCodeInfo, "Enter a valid selection.");
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			Applicator.StatusCode = JobHeaderStatus.Complete.Code;
			Shipment1.Job.JH_Status = JobHeaderStatus.Complete.Code;
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			Applicator.StatusCode = "WRK";
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.StatusCode = "XXX";
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			AssertNotEquals(JobHeaderStatus.Complete.Code, Shipment1.Job.JH_Status);
			AssertNotEquals(JobHeaderStatus.Complete.Code, Shipment2.Job.JH_Status);
			Applicator.StatusCode = JobHeaderStatus.Complete.Code;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(JobHeaderStatus.Complete.Code, Shipment1.Job.JH_Status);
			AssertEquals(JobHeaderStatus.Complete.Code, Shipment2.Job.JH_Status);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Job Status.
";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobStatusActionMethodApplicator(Factory);
		}

		new UpdateJobStatusActionMethodApplicator Applicator => (UpdateJobStatusActionMethodApplicator)base.Applicator;
	}
}
