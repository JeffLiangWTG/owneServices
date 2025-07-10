using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class BMReleaseSequenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestReleaseGroupValidation()
		{
			releaseSequence.BMR_GG_ReleaseGroup = ZGuid.Empty;
			AssertHasError(releaseSequence.BMR_GG_ReleaseGroupInfo, "Please enter a Release Group.");
			releaseSequence.BMR_GG_ReleaseGroup = ZGuid.NewZGuid();
			AssertHasError(releaseSequence.BMR_GG_ReleaseGroupInfo, "Enter a valid Release Group.");
			releaseSequence.BMR_GG_ReleaseGroup = group.PK;
			AssertNoErrors(releaseSequence.BMR_GG_ReleaseGroupInfo);
		}

		public void TestCapabilityValidation()
		{
			releaseSequence.BMR_G4_Capability = ZGuid.Empty;
			AssertNoErrors(releaseSequence.BMR_G4_CapabilityInfo);
			releaseSequence.BMR_G4_Capability = ZGuid.NewZGuid();
			AssertHasError(releaseSequence.BMR_G4_CapabilityInfo, "Enter a valid Capability.");
			releaseSequence.BMR_G4_Capability = capability.PK;
			AssertNoErrors(releaseSequence.BMR_G4_CapabilityInfo);
		}

		public void TestNameValidation()
		{
			releaseSequence.BMR_Name = ZString.Empty;
			AssertHasError(releaseSequence.BMR_NameInfo, "Please enter a Name.");
			releaseSequence.BMR_Name = "Seq1";
			AssertNoErrors(releaseSequence.BMR_NameInfo);
		}

		public void TestSequenceNudgeValidation()
		{
			releaseSequence.BMR_SequenceNudge = -36001;
			AssertHasError(releaseSequence.BMR_SequenceNudgeInfo, "Please enter a 'Sequence Nudge' within the range -36000 to 36000.");
			releaseSequence.BMR_SequenceNudge = -36000;
			AssertNoErrors(releaseSequence.BMR_SequenceNudgeInfo);
			releaseSequence.BMR_SequenceNudge = 36001;
			AssertHasError(releaseSequence.BMR_SequenceNudgeInfo, "Please enter a 'Sequence Nudge' within the range -36000 to 36000.");
			releaseSequence.BMR_SequenceNudge = 36000;
			AssertNoErrors(releaseSequence.BMR_SequenceNudgeInfo);
		}

		BMReleaseSequence releaseSequence;
		GlbGroup group;
		GlbCapability capability;

		protected override void SetUp()
		{
			base.SetUp();

			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			group = BMSTestHelper.CreateGroup(Factory, code: "GRP");
			capability = BMSTestHelper.CreateCapability(Factory, code: "CAP", isGroupScope: false);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "blah", buffer);

			releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
		}
	}
}
