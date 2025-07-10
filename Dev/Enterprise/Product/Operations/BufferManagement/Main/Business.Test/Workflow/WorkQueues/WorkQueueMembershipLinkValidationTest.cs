using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test.Workflow.WorkQueues
{
	class WorkQueueMembershipLinkValidationTest : BMSTestCaseWithFactory
	{
		public void TestSequence_ForWorkQueueLink_ShouldNotGetErrorsWhenJustOneLink()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var link = jobHeader.AddTag(queue).Link;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedLink = newFactory.Load<WorkQueueMembershipLink>(link.Identifier);

			loadedLink.ShouldValidateSequenceUniqueness = true;
			loadedLink.Validation.ValidateAll();

			AssertNoErrors(loadedLink);
		}

		public void TestSequence_ForWorkQueueLink_UniqueValidationShouldApplyToEmptyValues()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link1 = (TagLink)workflow1.AddTag(queue).Link;
			var link2 = (TagLink)workflow2.AddTag(queue).Link;

			var queueLink1 = Factory.Load<WorkQueueMembershipLink>(link1.PK);
			var queueLink2 = Factory.Load<WorkQueueMembershipLink>(link2.PK);

			queueLink1.ShouldValidateSequenceUniqueness = true;
			queueLink2.ShouldValidateSequenceUniqueness = true;

			queueLink1.TGL_Sequence = 0;
			queueLink2.TGL_Sequence = 0;

			AssertHasError(queueLink2.TGL_SequenceInfo, "The Sequence has been duplicated and must be unique for each item in a Work Queue.");

			queueLink2.TGL_Sequence = 1;
			AssertNoErrors(queueLink2);
		}

		public void TestCheckSequence_ValidationShouldBeDisabledByDefault()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link1 = (TagLink)workflow1.AddTag(queue).Link;
			var link2 = (TagLink)workflow2.AddTag(queue).Link;

			var queueLink1 = Factory.Load<WorkQueueMembershipLink>(link1.PK);
			var queueLink2 = Factory.Load<WorkQueueMembershipLink>(link2.PK);

			queueLink1.TGL_Sequence = 0;
			queueLink2.TGL_Sequence = 0;

			AssertNoErrors("Sequence validation hasn't been enabled, so there should be no errors even though the sequence is not unique. SAD!", queueLink2);

			queueLink2.TGL_Sequence = 1;
			AssertNoErrors(queueLink2);
		}

		public void TestValidateSequence_WhenLessThanZero_ShouldHaveError()
		{
			var link = Factory.New<WorkQueueMembershipLink>();
			link.TGL_Sequence = -1;

			AssertHasError(link.TGL_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 0.");

			link.TGL_Sequence = 0;
			AssertNoErrors(link.TGL_SequenceInfo);
		}
	}
}
