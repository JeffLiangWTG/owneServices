using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class CCPMTagRulesTest : TagReleaseRulesTest
	{
		#region Rule Matching

		public void TestRuleMatching_ReadyToRelease_ReleaseBlocked()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2");

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1");
			var workflow2_2 = CreateWorkflow(jobHeader2, "workflow2_2");

			var link1_2 = jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>(), name: "diagram");
			var subDiagram1 = CreateShape(jobHeader1, diagram, "subDiagram1");
			var subDiagram2 = CreateShape(jobHeader2, diagram, "subDiagram2");
			var subDiagram3 = CreateShape(diagram, "non-linked subDiagram3");
			var nonLinkedProcessHeader = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Not linked");
			subDiagram3.BNS_RelatedEntityID = nonLinkedProcessHeader.PK;

			var subDiagram1_child1 = CreateShape(workflow1_1, subDiagram1, "subDiagram1_child1");
			var subDiagram1_child2 = CreateShape(workflow1_2, subDiagram1, "subDiagram1_child2");
			var subDiagram2_child1 = CreateShape(workflow2_1, subDiagram2, "subDiagram2_child1");

			var arrow1_2 = subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2, diagram);
			var arrow11_12 = subDiagram1_child1.MakeVisiblePrerequisiteOf(subDiagram1_child2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			subDiagram1.AsEntity(network).Width = 600;
			ShapeFloatCalculator.CalculateFloatForChildren(network);
			UnitTestUserNotification.Instance.AddOKAnswer();
			UnitTestUserNotification.Instance.AddOKAnswer();

			networkViewModel.PushAsLateAsPossible();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), subDiagram1.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram1.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), subDiagram1_child1.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(2), subDiagram1_child1.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(2), subDiagram1_child2.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram1_child2.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram3.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(12), subDiagram3.ScheduledFinishTimeUtc);

			var rtrRule = GetReadyToReleaseRule();
			var rblRule = GetReleaseBlockedRule();

			AssertRuleMatches("Should match job and workflow whose scheduled start times are in the past, and are linked to real jobs", rtrRule, jobHeader1, workflow1_1);
			AssertRuleMatches("Should only match jobs and workflows with a scheduled date in the future", rblRule, jobHeader2, workflow1_2, workflow2_1);

			//Changing start date to future should replace RTR to RBL
			diagram.ProcessHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();
			AssertRuleMatches("Should only match jobs and workflows with a scheduled date in the future", rblRule, jobHeader1, jobHeader2, workflow1_1, workflow1_2, workflow2_1);

			//Changing start date to the past should replace RBL with RTR
			diagram.ProcessHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-10);
			Factory.Save();
			AssertRuleMatches("Should match job and workflow whose scheduled start times are in the past, and are linked to real jobs", rtrRule, jobHeader1, jobHeader2, workflow1_1, workflow1_2, workflow2_1);
		}

		public void TestRuleMatching_OnJobs()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var shape1 = CreateShape(jobHeader1, diagram);
			var shape2 = CreateShape(jobHeader2, diagram);
			var shape3 = CreateShape(jobHeader3, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), shape1.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(2), shape1.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(2), shape2.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(7), shape2.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(7), shape3.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(12), shape3.ScheduledFinishTimeUtc);

			var rtrRule = GetReadyToReleaseRule();
			var rblRule = GetReleaseBlockedRule();

			AssertRuleMatches("Should match job whose scheduled start times are in the past, and are linked to real jobs", rtrRule, jobHeader1);
			AssertRuleMatches("Should only match jobs with a scheduled date in the future", rblRule, jobHeader2, jobHeader3);
		}

		#endregion

		protected override string TagDefinitionCode
		{
			get { return BMConstants.CCPMReleaseRulesTagGroupCode; }
		}

		protected override string TagRulePrefix
		{
			get { return "CCPM"; }
		}
	}
}
