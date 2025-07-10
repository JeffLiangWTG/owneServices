using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NCNTagRulesTest : TagReleaseRulesTest
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

			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(jobHeader1, diagram, "subDiagram1");
			var subDiagram2 = CreateShape(jobHeader2, diagram, "subDiagram2");
			var subDiagram3 = CreateShape(diagram, "non-linked subDiagram3");
			var nonLinkedProcessHeader = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Not linked");
			subDiagram3.Shape.BNS_RelatedEntityID = nonLinkedProcessHeader.PK;

			var subDiagram1_child1 = CreateShape(workflow1_1, subDiagram1, "subDiagram1_child1");
			var subDiagram1_child2 = CreateShape(workflow1_2, subDiagram1, "subDiagram1_child2");
			var subDiagram2_child1 = CreateShape(workflow2_1, subDiagram2, "subDiagram2_child1");

			var arrow1_2 = subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2);
			var arrow11_12 = subDiagram1_child1.MakeVisiblePrerequisiteOf(subDiagram1_child2);

			network.SwitchToScaled();
			diagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			subDiagram1.Width = 600;
			ShapeFloatCalculator.CalculateFloatForChildren(network);
			networkViewModel.PushAsLateAsPossible();
			network.FullRefresh();

			networkViewModel.ToggleApproval();

			networkViewModel.GetJobController().TriggerSaveAction();

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), subDiagram1.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram1.Shape.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), subDiagram1_child1.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(2), subDiagram1_child1.Shape.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(2), subDiagram1_child2.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram1_child2.Shape.ScheduledFinishTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(7), subDiagram3.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(12), subDiagram3.Shape.ScheduledFinishTimeUtc);

			var rtrRule = Factory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_Name, "NCN Ready to Release Rule"));
			var rblRule = Factory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_Name, "NCN Release Blocked"));

			AssertRuleMatches("Should match job and workflow whose scheduled start times are in the past, and are linked to real jobs", rtrRule, jobHeader1, workflow1_1);
			AssertRuleMatches("Should only match jobs and workflows with a scheduled date in the future", rblRule, jobHeader2, workflow1_2, workflow2_1);

			//Changing start date to future should replace RTR to RBL
			diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();
			AssertRuleMatches("Should only match jobs and workflows with a scheduled date in the future", rblRule, jobHeader1, jobHeader2, workflow1_1, workflow1_2, workflow2_1);

			//Changing start date to the past should replace RBL with RTR
			diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			Factory.Save();
			AssertRuleMatches("Should match job and workflow whose scheduled start times are in the past, and are linked to real jobs", rtrRule, jobHeader1, jobHeader2, workflow1_1, workflow1_2, workflow2_1);
		}

		#endregion

		protected override string TagDefinitionCode
		{
			get { return BMConstants.NCNReleaseRulesTagGroupCode; }
		}

		protected override string TagRulePrefix
		{
			get { return "NCN"; }
		}
	}
}
