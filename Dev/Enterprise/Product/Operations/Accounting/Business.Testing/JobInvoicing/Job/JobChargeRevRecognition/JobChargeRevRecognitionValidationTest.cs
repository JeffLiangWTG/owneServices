using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	internal class JobChargeRevRecognitionValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			revRecog = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRecog.D3_JH = job1.PK;
			testObjectCreator = new TestObjectCreator(Factory);
		}

		Job job1;
		Job job2;
		JobChargeRevRecognition revRecog;
		TestObjectCreator testObjectCreator;

		[TestDate(2009, 05, 30)]
		public void TestCheckD3_RecognitionDate()
		{
			revRecog.D3_RecognitionDate = new ZDateTime(2009, 02, 01);
			revRecog.Validation.ValidateD3_RecognitionDate();
			AssertHasErrors(revRecog.D3_RecognitionDateInfo);

			testObjectCreator.CreateTestPeriods(new ZDateTime(2009, 01, 01));
			revRecog.Validation.ValidateD3_RecognitionDate();
			AssertNoErrors(revRecog.D3_RecognitionDateInfo);
		}

		public void TestCheckD3_RecognitionType()
		{
			revRecog.D3_RecognitionType = "JCL";
			revRecog.Validation.ValidateD3_RecognitionType();

			var revRecogForAnotherJob = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRecogForAnotherJob.D3_JH = job2.PK;
			revRecogForAnotherJob.D3_RecognitionType = "JCL";

			AssertNoErrors(revRecog.D3_RecognitionTypeInfo);

			var revRecog2 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRecog2.D3_JH = job1.PK;
			revRecog2.D3_RecognitionType = "JCL";
			revRecog.Validation.ValidateD3_RecognitionType();
			AssertHasErrors(revRecog.D3_RecognitionTypeInfo);
		}
	}
}