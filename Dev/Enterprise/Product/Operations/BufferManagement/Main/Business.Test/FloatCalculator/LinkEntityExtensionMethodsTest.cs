using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class LinkEntityExtensionMethodsTest : BMSTestCaseWithFactory
	{
		public void TestIsPrereqOrPostreq_ParallelConnectedBranch()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = CreateWorkflow(jobHeader, "workflow5");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow4.GetOrCreateDependencyLink(workflow3);
			workflow4.GetOrCreateDependencyLink(workflow5);

			// 1 -> 2 -> 3
			//      4 -> 3
			//      4 -> 5

			// workflow1
			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow1));
			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow3));
			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow4));
			AssertEquals(false, workflow1.IsPrerequisiteOf(workflow5));

			AssertEquals(false, workflow1.IsPostrequisiteOf(workflow1));
			AssertEquals(false, workflow1.IsPostrequisiteOf(workflow2));
			AssertEquals(false, workflow1.IsPostrequisiteOf(workflow3));
			AssertEquals(false, workflow1.IsPostrequisiteOf(workflow4));
			AssertEquals(false, workflow1.IsPostrequisiteOf(workflow5));

			// workflow2
			AssertEquals(false, workflow2.IsPrerequisiteOf(workflow1));
			AssertEquals(true, workflow2.IsPrerequisiteOf(workflow3));
			AssertEquals(false, workflow2.IsPrerequisiteOf(workflow4));
			AssertEquals(false, workflow2.IsPrerequisiteOf(workflow5));

			AssertEquals(true, workflow2.IsPostrequisiteOf(workflow1));
			AssertEquals(false, workflow2.IsPostrequisiteOf(workflow3));
			AssertEquals(false, workflow2.IsPostrequisiteOf(workflow4));
			AssertEquals(false, workflow2.IsPostrequisiteOf(workflow5));

			// workflow3
			AssertEquals(false, workflow3.IsPrerequisiteOf(workflow1));
			AssertEquals(false, workflow3.IsPrerequisiteOf(workflow2));
			AssertEquals(false, workflow3.IsPrerequisiteOf(workflow4));
			AssertEquals(false, workflow3.IsPrerequisiteOf(workflow5));

			AssertEquals(true, workflow3.IsPostrequisiteOf(workflow1));
			AssertEquals(true, workflow3.IsPostrequisiteOf(workflow2));
			AssertEquals(true, workflow3.IsPostrequisiteOf(workflow4));
			AssertEquals(false, workflow3.IsPostrequisiteOf(workflow5));

			// workflow4
			AssertEquals(false, workflow4.IsPrerequisiteOf(workflow1));
			AssertEquals(false, workflow4.IsPrerequisiteOf(workflow2));
			AssertEquals(true, workflow4.IsPrerequisiteOf(workflow3));
			AssertEquals(true, workflow4.IsPrerequisiteOf(workflow5));

			AssertEquals(false, workflow4.IsPostrequisiteOf(workflow1));
			AssertEquals(false, workflow4.IsPostrequisiteOf(workflow2));
			AssertEquals(false, workflow4.IsPostrequisiteOf(workflow3));
			AssertEquals(false, workflow4.IsPostrequisiteOf(workflow5));

			// workflow5
			AssertEquals(false, workflow5.IsPrerequisiteOf(workflow1));
			AssertEquals(false, workflow5.IsPrerequisiteOf(workflow2));
			AssertEquals(false, workflow5.IsPrerequisiteOf(workflow3));
			AssertEquals(false, workflow5.IsPrerequisiteOf(workflow4));

			AssertEquals(false, workflow5.IsPostrequisiteOf(workflow1));
			AssertEquals(false, workflow5.IsPostrequisiteOf(workflow2));
			AssertEquals(false, workflow5.IsPostrequisiteOf(workflow3));
			AssertEquals(true, workflow5.IsPostrequisiteOf(workflow4));
		}

		public void TestIsPrereqOrPostreq_ForkedBranch()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow2.GetOrCreateDependencyLink(workflow4);

			// 1 -> 2 -> 3
			//      2 -> 4

			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow3));
			AssertEquals(true, workflow2.IsPrerequisiteOf(workflow3));
			AssertEquals(true, workflow2.IsPrerequisiteOf(workflow4));
			AssertEquals(false, workflow3.IsPrerequisiteOf(workflow4));

			AssertEquals(true, workflow2.IsPostrequisiteOf(workflow1));
			AssertEquals(true, workflow3.IsPostrequisiteOf(workflow1));
			AssertEquals(true, workflow3.IsPostrequisiteOf(workflow2));
			AssertEquals(true, workflow4.IsPostrequisiteOf(workflow2));
			AssertEquals(false, workflow3.IsPostrequisiteOf(workflow4));
		}

		public void TestIsPrereqOrPostreq_ShouldUseParentLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2");

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			AssertEquals(true, workflow1.IsPrerequisiteOf(workflow2));
			AssertEquals(true, workflow2.IsPostrequisiteOf(workflow1));

			AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));
			AssertEquals(true, jobHeader2.IsPostrequisiteOf(jobHeader1));

			AssertEquals(false, jobHeader2.IsPrerequisiteOf(jobHeader1));
			AssertEquals(false, jobHeader1.IsPostrequisiteOf(jobHeader2));
		}
	}
}
