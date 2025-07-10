using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderPrerequisiteTest : BMSTestCaseWithFactory
	{
		public void TestHasOpenPrerequisite_None()
		{
			AssertEquals("No prerequisites for JobHeader exist", false, JobHeader.HasOpenPrerequisites);
			AssertEquals("No prerequisites for Workflow exist", false, Workflow.HasOpenPrerequisites);
			AssertEquals("No prerequisites for Child exist", false, QualityIterationWorkflow.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_Workflow()
		{
			CreateOpenPrereq(Workflow, "Prereq");
			AssertEquals("Prerequisites of children do not affect parents.", false, JobHeader.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should always affect postreq.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, QualityIterationWorkflow.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_JobHeader()
		{
			CreateOpenPrereq(JobHeader, "Prereq");
			AssertEquals("Open prerequisites should always affect postreq.", true, JobHeader.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, QualityIterationWorkflow.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_ParentJobHeader_HasPrerequisite()
		{
			var workflow = CreateSimpleWorkflow("ParentWorkflow");
			BMSTestHelper.MakeChildOf(JobHeader, workflow.JobHeader);
			CreateOpenPrereq(JobHeader, "Prereq");

			AssertEquals("Open prerequisites should cascade down to children.", true, JobHeader.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, QualityIterationWorkflow.HasOpenPrerequisites);
		}
		public void TestHasOpenPrerequisites_SiblingPrereqDoesNotOverrideAncestor()
		{
			var simpleOpenWorkflow = CreateSimpleWorkflow("s1");
			var simpleClosedWorkflow = CreateWorkflow(simpleOpenWorkflow.JobHeader, "s2");
			simpleOpenWorkflow.MakePrerequisiteOf(JobHeader);
			simpleClosedWorkflow.MakePrerequisiteOf(Workflow);

			AssertEquals("Direct prerequisite should be considered open.", true, JobHeader.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children, even when closed prereqs exist.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children, even when closed prereqs exist.", true, QualityIterationWorkflow.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_DescendantPrereqOverridesAncestor()
		{
			var jobLevelPrereq = CreateOpenPrereq(JobHeader, "Open Job Prereq");
			var childLevelPrereq = CreateWorkflow(jobLevelPrereq.JobHeader, "Closed child prereq");
			BMSTestHelper.MakeChildOf(childLevelPrereq, jobLevelPrereq);
			childLevelPrereq.MakePrerequisiteOf(Workflow);

			AssertEquals("Cross Heirarchic prerequisite should override parent prerequisite.", false, Workflow.HasOpenPrerequisites);
			AssertEquals("Cross Heirarchic prerequisite should override descendant.", false, QualityIterationWorkflow.HasOpenPrerequisites);
			AssertEquals("The link between the the two parent entities is considered, because links between children can't override their parents.", true, JobHeader.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_DescendantPrereqOverridesAncestor_Sometimes()
		{
			var jobLevelPrereq = CreateOpenPrereq(JobHeader, "Open Job Prereq");
			var childLevelPrereq = CreateWorkflow(jobLevelPrereq.JobHeader, "Closed child prereq");
			BMSTestHelper.MakeChildOf(childLevelPrereq, jobLevelPrereq);
			childLevelPrereq.MakePrerequisiteOf(JobHeader);

			AssertEquals("Cross Heirarchic prerequisite should override parent prerequisite.", false, Workflow.HasOpenPrerequisites);
			AssertEquals("Cross Heirarchic prerequisite should override descendant.", false, QualityIterationWorkflow.HasOpenPrerequisites);
			AssertEquals("Direct prerequisite should be considered open because dependencies between children can't override their parents.", true, JobHeader.HasOpenPrerequisites);
		}

		public void TestWorkflowAsParentOfJobIsValid()
		{
			var workflow = CreateSimpleWorkflow("Berkflow");
			var link = BMSTestHelper.MakeChildOfAndGetLink(JobHeader, workflow);

			AssertNoErrors(workflow);
			AssertNoErrors(link);
		}

		public void TestHasOpenPrerequisites_JobHeaderParentsCascadeToChilden_WorkflowParent()
		{
			var workflow = CreateSimpleWorkflow("Berkflow");
			var link = BMSTestHelper.MakeChildOfAndGetLink(JobHeader, workflow);
			CreateOpenPrereq(workflow, "Open wokrflow prereq");

			AssertEquals("Open prerequisites should cascade down to children.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, QualityIterationWorkflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, JobHeader.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_JobHeaderParentsCascadeToChilden_JobParent()
		{
			var workflow = CreateSimpleWorkflow("Berkflow");
			var link = BMSTestHelper.MakeChildOfAndGetLink(JobHeader, workflow.JobHeader);
			CreateOpenPrereq(workflow.JobHeader, "Open wokrflow prereq");

			AssertEquals("Open prerequisites should cascade down to children.", true, Workflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, QualityIterationWorkflow.HasOpenPrerequisites);
			AssertEquals("Open prerequisites should cascade down to children.", true, JobHeader.HasOpenPrerequisites);
		}

		#region Implementation

		ProcessHeader CreateOpenPrereq(ProcessHeader postreq, string name)
		{
			var workflow = CreateSimpleWorkflow(name);
			workflow.MakePrerequisiteOf(postreq);
			return workflow;
		}

		ProcessHeader CreateSimpleWorkflow(string name)
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>(false);
			var workflow = CreateWorkflow(jobHeader, name);
			var task = CreateTask(workflow, Staff.GS_Code, 30);
			return workflow;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Staff = CreateStaffInCurrentBranchDept("MON", "Mr Monday");
			JobHeader = CreateJobHeader<DummyWithWorkflow>(false);
			Workflow = CreateWorkflow(JobHeader, "Workflow");
			CreateTask(Workflow, Staff.GS_Code, 30);

			QualityIterationWorkflow = CreateWorkflow(JobHeader, "WorkflowChild");
			CreateTask(QualityIterationWorkflow, Staff.GS_Code, 30);
			Assert(BMSTestHelper.MakeChildOf(QualityIterationWorkflow, Workflow));
		}

		GlbStaff Staff { get; set; }
		ProcessJobHeader JobHeader { get; set; }
		ProcessHeader Workflow { get; set; }
		ProcessHeader QualityIterationWorkflow { get; set; }

		#endregion
	}
}
