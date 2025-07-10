using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class BMReleaseSequenceItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequenceValidation()
		{
			releaseSequenceItem1.BMI_BMR_Sequence = ZGuid.Empty;
			releaseSequenceItem2.BMI_BMR_Sequence = ZGuid.NewZGuid();

			AssertHasError(releaseSequenceItem1.BMI_BMR_SequenceInfo, "Please enter a value.");
			AssertHasError(releaseSequenceItem2.BMI_BMR_SequenceInfo, "Sequence item must belong to a valid sequence.");

			releaseSequenceItem1.BMI_BMR_Sequence = releaseSequence.PK;
			releaseSequenceItem2.BMI_BMR_Sequence = releaseSequence.PK;

			AssertNoErrors(releaseSequenceItem1.BMI_BMR_SequenceInfo);
			AssertNoErrors(releaseSequenceItem2.BMI_BMR_SequenceInfo);
		}

		public void TestProcessHeaderValidation()
		{
			releaseSequenceItem1.BMI_FH_ProcessHeader = ZGuid.Empty;
			releaseSequenceItem2.BMI_FH_ProcessHeader = ZGuid.NewZGuid();

			AssertHasError(releaseSequenceItem1.BMI_FH_ProcessHeaderInfo, "Please enter a value.");
			AssertHasError(releaseSequenceItem2.BMI_FH_ProcessHeaderInfo, "Sequence item must reference a valid process header.");

			releaseSequenceItem1.BMI_FH_ProcessHeader = workflow1.PK;
			releaseSequenceItem2.BMI_FH_ProcessHeader = workflow2.PK;

			AssertNoErrors(releaseSequenceItem1.BMI_FH_ProcessHeaderInfo);
			AssertNoErrors(releaseSequenceItem2.BMI_FH_ProcessHeaderInfo);
		}

		public void TestValueValidation()
		{
			releaseSequenceItem1.BMI_Value = 11;
			releaseSequenceItem2.BMI_Value = 0;

			AssertHasError(releaseSequenceItem1.BMI_ValueInfo, "Please enter a 'Value' within the set [1, 2, 3, 5, 8, 15, 25, 40, 75, 100, 250, 600, 1000].");
			AssertHasError(releaseSequenceItem2.BMI_ValueInfo, "Please enter a 'Value' within the set [1, 2, 3, 5, 8, 15, 25, 40, 75, 100, 250, 600, 1000].");

			releaseSequenceItem1.BMI_Value = 1;
			releaseSequenceItem2.BMI_Value = 5;

			AssertNoErrors(releaseSequenceItem1.BMI_ValueInfo);
			AssertNoErrors(releaseSequenceItem2.BMI_ValueInfo);
		}

		public void TestInvestmentValidation()
		{
			releaseSequenceItem1.BMI_Investment = 22;
			releaseSequenceItem2.BMI_Investment = -10;

			AssertHasError(releaseSequenceItem1.BMI_InvestmentInfo, "Please enter an 'Investment' within the set [1, 2, 3, 5, 8, 15, 25, 40, 75, 100, 250, 600, 1000].");
			AssertHasError(releaseSequenceItem2.BMI_InvestmentInfo, "Please enter an 'Investment' within the set [1, 2, 3, 5, 8, 15, 25, 40, 75, 100, 250, 600, 1000].");

			releaseSequenceItem1.BMI_Investment = 40;
			releaseSequenceItem2.BMI_Investment = 25;

			AssertNoErrors(releaseSequenceItem1.BMI_InvestmentInfo);
			AssertNoErrors(releaseSequenceItem2.BMI_InvestmentInfo);
		}

		public void TestPositionValidation()
		{
			releaseSequenceItem1.BMI_BMR_Sequence = releaseSequence.PK;
			releaseSequenceItem1.BMI_Position = 2;
			releaseSequenceItem2.BMI_BMR_Sequence = releaseSequence.PK;
			releaseSequenceItem2.BMI_Position = 2;

			AssertNoErrors(releaseSequenceItem1.BMI_PositionInfo);
			AssertHasError(releaseSequenceItem2.BMI_PositionInfo, "The Position has been duplicated and must be unique.");

			releaseSequenceItem1.BMI_Position = 1;
			releaseSequenceItem2.BMI_Position = 2;

			AssertNoErrors(releaseSequenceItem1.BMI_PositionInfo);
			AssertNoErrors(releaseSequenceItem2.BMI_PositionInfo);
		}

		public void TestEstimatedLeadTimeDaysValidation()
		{
			releaseSequenceItem1.BMI_EstimatedLeadTimeDays = -1;
			AssertHasError(releaseSequenceItem1.BMI_EstimatedLeadTimeDaysInfo, "Please enter an 'Estimated Lead Time Days' within the range 0 to 365.");
			releaseSequenceItem1.BMI_EstimatedLeadTimeDays = 0;
			AssertNoErrors(releaseSequenceItem1.BMI_EstimatedLeadTimeDaysInfo);
			releaseSequenceItem1.BMI_EstimatedLeadTimeDays = 366;
			AssertHasError(releaseSequenceItem1.BMI_EstimatedLeadTimeDaysInfo, "Please enter an 'Estimated Lead Time Days' within the range 0 to 365.");
			releaseSequenceItem1.BMI_EstimatedLeadTimeDays = 365;
			AssertNoErrors(releaseSequenceItem1.BMI_EstimatedLeadTimeDaysInfo);
		}

		ProcessHeader workflow1, workflow2;
		BMReleaseSequence releaseSequence;
		BMReleaseSequenceItem releaseSequenceItem1, releaseSequenceItem2;

		protected override void SetUp()
		{
			base.SetUp();

			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, code: "GRP");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);
			workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "blah", buffer);

			releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			releaseSequence.BMR_Name = "Seq1";
			releaseSequence.BMR_GG_ReleaseGroup = group.PK;

			releaseSequenceItem1 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem2 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
		}
	}
}
