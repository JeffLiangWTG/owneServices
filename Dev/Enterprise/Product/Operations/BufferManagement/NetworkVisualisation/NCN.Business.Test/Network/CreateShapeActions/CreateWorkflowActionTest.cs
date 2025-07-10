using System.Linq;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateWorkflowAction))]
	class CreateWorkflowActionTest : JobNetworkActionTestCase<CreateWorkflowAction>
	{
		public void TestExecute_WhenParentIsWorkflowShape_ShouldCreateSubWorkflow()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pickle Rick");

			var diagram = CreateDiagram(workflow.JobHeader);
			var shape = CreateShape(workflow, diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			AssertEquals(true, action.IsEnabledAfterActivatingEntity_ForTest(shape).IsAllowed);
			AssertEquals(1, network.Entities.Count);
			AssertEquals(1, workflow.JobHeader.ProcessHeaders.Count);

			action.ExecuteForEntityWithoutAccessCheck(shape);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(2, workflow.JobHeader.ProcessHeaders.Count);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var newShape = network.Shapes.Single(s => !s.IsInDatabase);

			AssertEquals("New Workflow", newShape.Name);
			AssertNotNull(newShape.ProcessHeader);
			AssertEquals(workflow.JobHeader, newShape.ProcessHeader.JobHeader);
			AssertIsParent(newShape.ProcessHeader, shape.ProcessHeader);
		}

		protected override void TestExecuteCore()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(1, network.Entities.Count);

			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var newShape = network.Shapes[1];
			AssertEquals(ShapeTypeList.Codes.Shape, newShape.BNS_ShapeType);
			AssertEquals("Bisque", newShape.BackColor);
			AssertNotNull(newShape.ProcessHeader);
			AssertEquals(true, newShape.ProcessHeader.IsWorkflow);

			AssertEquals(bucket, workflow.CurrentComponent);
			AssertEquals(bucket, newShape.ProcessHeader.CurrentComponent);
		}

		protected override void TestIsApplicableCore()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			CombineAssertions("Linked diagram and shapes", () =>
			{
				var linkedDiagram = CreateDiagram(jobHeader);
				AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, linkedDiagram.BNS_ShapeType);
				AssertNotNull("Precondition", linkedDiagram.ProcessHeader);
				var networkViewModel = CreateNetworkViewModel(linkedDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(linkedDiagram));

				var linkedShape = networkViewModel.CreateNewWorkflow(linkedDiagram);
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(linkedShape));
			});

			CombineAssertions("Non linked diagram and shapes", () =>
			{
				var nonLinkedDiagram = CreateDiagram(Factory);
				var nonLinkedShape = CreateShape(nonLinkedDiagram);
				var buffer = CreateShape(nonLinkedDiagram, shapeType: ShapeTypeList.Codes.Buffer);
				var annotation = CreateShape(nonLinkedDiagram, shapeType: ShapeTypeList.Codes.Annotation);

				var networkViewModel = CreateNetworkViewModel(nonLinkedDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to a job or workflow.", action.IsApplicableAfterActivatingEntity_ForTest(nonLinkedDiagram));

				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to a job or workflow.", action.IsApplicableAfterActivatingEntity_ForTest(nonLinkedShape));

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"The shape should not be a buffer or annotation.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"The shape should not be a buffer or annotation.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));
			});

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();

			var linkedDiagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(linkedDiagram);

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(linkedDiagram));
		}

		public void TestShouldBeOnlyEnabledForJobsWhichAreAssociatedWithBMS()
		{
			var system = CreateSystem("ORG");
			var disabledWorkflowType = system.RelatedWorkflowTypes.AddNew();
			disabledWorkflowType.FSW_WorkflowType = "INQ"; //sales enquiry
			disabledWorkflowType.FSW_IsActive = false;

			var jobHeaderEnabledForBMS = CreateJobHeader<OrgHeader>();
			var jobHeaderDisabledForBMS = CreateJobHeader<SalesEnquiry>();
			var jobHeaderNoBMS = CreateJobHeader<OrgOpportunity>();

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			var shapeEnabledForBMS = CreateShape(jobHeaderEnabledForBMS, diagram);
			var shapeDisabledForBM = CreateShape(jobHeaderDisabledForBMS, diagram);
			var shapeNoBMS = CreateShape(jobHeaderNoBMS, diagram);
			networkViewModel.Refresh();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(true, shapeEnabledForBMS.IsLinkedToRealEntity);
				AssertEquals(true, shapeDisabledForBM.IsLinkedToRealEntity);
				AssertEquals(true, shapeNoBMS.IsLinkedToRealEntity);
				AssertEquals(jobHeaderEnabledForBMS, shapeEnabledForBMS.LinkedEntity);
				AssertEquals(jobHeaderDisabledForBMS, shapeDisabledForBM.LinkedEntity);
				AssertEquals(jobHeaderNoBMS, shapeNoBMS.LinkedEntity);
				AssertEquals(true, shapeEnabledForBMS.IsLinkedToRealEntityEnabledForBMS);
				AssertEquals(false, shapeDisabledForBM.IsLinkedToRealEntityEnabledForBMS);
				AssertEquals(false, shapeNoBMS.IsLinkedToRealEntityEnabledForBMS);
			});

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeEnabledForBMS))
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
			}

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeDisabledForBM))
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to a job or workflow of a job which type is associated with and enabled for a Buffer Management System.", action.IsEnabled());
			}

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeNoBMS))
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to a job or workflow of a job which type is associated with and enabled for a Buffer Management System.", action.IsEnabled());
			}
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Workflow", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates a new workflow for this job", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Workflow");
		}

		protected override CreateWorkflowAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateWorkflowAction(networkViewModel);
		}
	}
}
