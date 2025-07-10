using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(PushAllEntitiesAction))]
	class PushAllEntitiesActionTest : JobNetworkActionTestCase<PushAllEntitiesAction>
	{
		protected override void TestExecuteCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var childShape1 = CreateShape(workflow1, diagram, "childShape1");
			var childShape2 = CreateShape(workflow2, diagram, "childShape2");
			SetShapeOffset(childShape1, diagram, 100, 50);
			SetShapeOffset(childShape2, diagram, 100, 50);
			SetShapeSize(childShape1, diagram, 300, 20);
			SetShapeSize(childShape2, diagram, 300, 20);

			var dependency = CreateDependencyAttachment(diagram.Shape, link, childShape1.Shape, childShape2.Shape);

			network.FullRefresh(); // Refresh before push to ensure float is calculated.

			var refreshed = false;
			network.Refreshed += (_, x_) => refreshed = true;

			var action = GetAction(networkViewModel);

			var pushEarlyAction = action.GetChildActionsAfterActivatingEntity_ForTest(diagram).Cast<JobNetworkAction>().First();
			AssertEquals("As Early As Possible", pushEarlyAction.GetName());

			pushEarlyAction.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(true, refreshed);

			AssertShapeOffset(childShape1, diagram, 0, 50);
			AssertShapeOffset(childShape2, diagram, 300, 50);
		}

		public void TestExecute_PushLate()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var child1 = CreateShape(workflow1, diagram, "child1");
			var child2 = CreateShape(workflow2, diagram, "child2");
			var child3 = CreateShape(workflow3, diagram, "child3");

			var arrow1_3 = CreateDependencyAttachment(diagram.Shape, link1_3, child1.Shape, child3.Shape);
			var arrow2_3 = CreateDependencyAttachment(diagram.Shape, link2_3, child2.Shape, child3.Shape);

			SetShapeOffset(child1, diagram, 0, 10);
			SetShapeOffset(child2, diagram, 0, 110);
			SetShapeOffset(child3, diagram, 0, 10);

			SetShapeSize(child1, diagram, 100, 50);
			SetShapeSize(child2, diagram, 300, 50);
			SetShapeSize(child3, diagram, 100, 50);

			network.FullRefresh(); // Refresh before push to ensure float is calculated.

			var refreshed = false;
			network.Refreshed += (_, x_) => refreshed = true;

			var action = GetAction(networkViewModel);

			var pushLateAction = action.GetChildActionsAfterActivatingEntity_ForTest(diagram).Cast<JobNetworkAction>().Skip(1).First();
			AssertEquals("As Late As Possible", pushLateAction.GetName());

			pushLateAction.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(true, refreshed);

			AssertShapeOffset("Should move child1 right up to its postrequisite", child1, diagram, 200, 10);
			AssertShapeOffset("child2 should still be at the start", child2, diagram, 0, 110);
			AssertShapeOffset("child3 should be moved after its prerequisites", child3, diagram, 300, 10);
		}

		public void TestExecute_ShouldNotShuntAnnotations()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.ScaleAndRefresh();

			var annotation = networkViewModel.CreateNewAnnotation(network.DiagramEntity);
			SetShapeOffset(annotation, network.DiagramEntity, 100, 10);

			GetAction(networkViewModel).GetChildActionsAfterActivatingEntity_ForTest(diagram).Cast<JobNetworkAction>().Skip(1).First().ExecuteForEntityWithoutAccessCheck(diagram);

			AssertShapeOffset(annotation, diagram.AsEntity(network), 100, 10);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child of a non scaled diagram", new string[] {
					"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled diagram", new string[] {
				"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(childShape));
			NetworkActionAccessibilityTest.AssertAllowed("scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsEnabledAfterActivatingEntity_ForTest(childShape));
			NetworkActionAccessibilityTest.AssertAllowed("scaled diagram", action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			var action = GetAction(CreateNetworkViewModel(diagram));
			diagram.SwitchToScaled();
			AssertEquals("Push All Entities", action.GetName());
			AssertEquals("As Early As Possible", action.GetChildActions().ElementAt(0).GetName());
			AssertEquals("As Late As Possible", action.GetChildActions().ElementAt(1).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Pushes all entities either as early or late as possible, without violating dependencies.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestChildActionsCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			Assert("Precondition", !action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals(0, action.GetChildActions().Count());

			diagram.SwitchToScaled();
			Assert("Precondition", action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals(2, action.GetChildActions().Count());
		}

		protected override PushAllEntitiesAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new PushAllEntitiesAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(CommonNetworkActionExecutionStrategy);

		[TestedType(typeof(PushAllEntitiesAction.PushEntityChildAction))]
		class ChildActionTest : TokenJobNetworkActionTestCaseForChildActionsTestedWithParent<PushAllEntitiesAction.PushEntityChildAction>
		{
			protected override PushAllEntitiesAction.PushEntityChildAction GetActionCore(INetworkViewModel networkViewModel)
			{
				return new PushAllEntitiesAction.PushEntityChildAction(networkViewModel, PushDirection.Early);
			}

			protected override Type GetExpectedExecutionStrategyType() => typeof(CommonNetworkActionExecutionStrategy);

			protected override void TestGetIconCore()
			{
				var diagram = CreateDiagram(Factory);
				var networkViewModel = CreateNetworkViewModel(diagram);
				AssertEquals("ArrowLeft", new PushAllEntitiesAction.PushEntityChildAction(networkViewModel, PushDirection.Early).GetIconName());
				AssertEquals("ArrowRight", new PushAllEntitiesAction.PushEntityChildAction(networkViewModel, PushDirection.Late).GetIconName());
			}
		}
	}
}
