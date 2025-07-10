using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ProcessHeaderLoopsCheckerTest : BMSTestCaseWithFactory
	{
		[GuiTest]
		public void TestNoLoopsDetected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "w1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "w2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "w3", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			(new ProcessHeaderLoopsChecker()).CheckLoops(new DefaultProgressReporterProvider(new Form()), new ProcessHeader[] { workflow1, workflow2, workflow3 });
			AssertEquals("Manually invoked workflow loop validation completed with no errors.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[GuiTest]
		public void TestLoopsDetected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "w1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "w2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "w3", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "w4", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow1);

			BMSTestHelper.MakeChildOf(workflow3, workflow2);
			BMSTestHelper.MakeChildOf(workflow4, workflow3);
			BMSTestHelper.MakeChildOf(workflow2, workflow4);

			(new ProcessHeaderLoopsChecker()).CheckLoops(new DefaultProgressReporterProvider(new Form()), new ProcessHeader[] { workflow1, workflow2, workflow3 });
			AssertEquals(@"Manually invoked workflow loop validation completed with the following errors:

hierarchic cycles:
- w2, w3, w4.

dependency cycles:
- w1, w3, w2, w4.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
