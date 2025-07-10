using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(AddBufferAction))]
	class AddBufferActionTest : JobNetworkActionTestCase<AddBufferAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var hasRefreshed = false;
			network.Refreshed += (s, e) => hasRefreshed = true;

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertEquals(2, network.Entities.Count);
			AssertNull(dependency.GetBuffer());

			var action = GetAction(networkViewModel);

			action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).Single().AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(childShape1);

			AssertEquals(true, hasRefreshed);
			AssertEquals(3, network.Entities.Count);

			Factory.Save();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			AssertEquals(buffer, dependency.GetBuffer().Shape);
			AssertEquals(BufferType.Feeding, buffer.Type);
		}

		public void TestExecute_ReloadedNetwork()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var hasRefreshed = false;
			network.Refreshed += (s, e) => hasRefreshed = true;

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertEquals(2, network.Entities.Count);
			AssertNull(dependency.GetBuffer());

			var action = GetAction(networkViewModel);
			var childAction = action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).Single();
			childAction.AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(childShape1);

			AssertEquals(true, hasRefreshed);
			AssertEquals(3, network.Entities.Count);

			Factory.Save();

			var buffer = dependency.GetBuffer();
			AssertNotNull(buffer);

			var newNetwork = CreateNetwork(new BusinessObjectFactory().Load<BMNCNShape>(diagram.PK));
			AssertEquals(3, newNetwork.Entities.Count);

			var loadedBuffer = (ShapeNetworkEntity)newNetwork.Entities.Cast<INetworkEntity>().Single(s => s.ShapeType == ShapeTypeList.Codes.Buffer);
			AssertEquals(buffer.PK, loadedBuffer.PK);
		}

		public void TestExecute_ShouldSetBufferedFlag()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			var attachment = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(2, network.Entities.Count);
			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);

			GetAction(networkViewModel).GetChildActionsAfterActivatingEntity_ForTest(shape1).Single().AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(shape1);

			AssertEquals(3, network.Entities.Count);
			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			AssertEquals(BufferType.Feeding, buffer.Type);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child of a non scaled diagram", new string[] {
					"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled diagram", new string[] {
				"Cannot execute when the diagram is not in scaled mode.",
				"This action is not accessible to the root diagram." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);
			var childShape4 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(childShape1, childShape2);
			network.CreateRelationship(childShape2, childShape3);

			networkViewModel.SelectEntities(new[] { childShape3, childShape4 });
			var resourceDependencyAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			resourceDependencyAction.ExecuteForEntityWithoutAccessCheck(childShape4);
			networkViewModel.SelectEntities(System.Array.Empty<INetworkEntity>());

			var action = GetAction(networkViewModel);
			AssertEquals(true, action.IsEnabledAfterActivatingEntity_ForTest(childShape1).IsAllowed);
			AssertEquals(true, action.IsEnabledAfterActivatingEntity_ForTest(childShape2).IsAllowed);
			AssertEquals("Only shapes with outgoing dependency arrows should be able to add a buffer", false, action.IsEnabledAfterActivatingEntity_ForTest(childShape3).IsAllowed);
			AssertEquals("Only shapes with outgoing dependency arrows should be able to add a buffer (not resource dependencies)", false, action.IsEnabledAfterActivatingEntity_ForTest(childShape4).IsAllowed);
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Add Buffer", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Adds a buffer for the specified dependency link.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestChildActionsCore()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);
			var childShape4 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";
			childShape3.Name = "childShape3";
			childShape4.Name = "childShape4";

			network.CreateRelationship(childShape1, childShape2);
			network.CreateRelationship(childShape2, childShape3);
			network.CreateRelationship(childShape2, childShape4);

			var action = GetAction(networkViewModel);
			AssertEquals(1, action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).Count());
			AssertEquals("childShape1 -> childShape2", action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).ElementAt(0).GetName());

			AssertEquals(2, action.GetChildActionsAfterActivatingEntity_ForTest(childShape2).Count());
			AssertEquals("childShape2 -> childShape3", action.GetChildActionsAfterActivatingEntity_ForTest(childShape2).ElementAt(0).GetName());
			AssertEquals("childShape2 -> childShape4", action.GetChildActionsAfterActivatingEntity_ForTest(childShape2).ElementAt(1).GetName());

			AssertEquals(0, action.GetChildActionsAfterActivatingEntity_ForTest(childShape3).Count());
			AssertEquals(0, action.GetChildActionsAfterActivatingEntity_ForTest(childShape4).Count());
		}

		public void TestChildActions_ShouldNotIncludeLinksForBuffers()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";
			childShape3.Name = "childShape3";

			var dependency1_2 = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			var dependency1_3 = (NetworkAttachment)network.CreateRelationship(childShape1, childShape3);
			AssertEquals(3, network.Entities.Count);
			AssertNull(dependency1_2.GetBuffer());
			AssertNull(dependency1_3.GetBuffer());

			var action = GetAction(networkViewModel);
			var childActions = action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).Cast<JobNetworkAction>().ToArray();
			AssertEquals(2, childActions.Length);
			AssertEquals("childShape1 -> childShape2", childActions[0].GetNameAfterActivatingEntity_ForTest(childShape1));
			childActions[0].ExecuteForEntityWithoutAccessCheck(childShape1);

			childActions = action.GetChildActionsAfterActivatingEntity_ForTest(childShape1).Cast<JobNetworkAction>().ToArray();
			AssertEquals(1, childActions.Length);
			AssertEquals("childShape1 -> childShape3", childActions[0].GetNameAfterActivatingEntity_ForTest(childShape1));
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Plus");
		}

		protected override AddBufferAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new AddBufferAction(networkViewModel);
		}

		[TestedType(typeof(AddBufferAction.AddBufferForLinkAction))]
		class ChildActionTest : TokenJobNetworkActionTestCaseForChildActionsTestedWithParent<AddBufferAction.AddBufferForLinkAction>
		{
			protected override AddBufferAction.AddBufferForLinkAction GetActionCore(INetworkViewModel networkViewModel)
			{
				var jobHeader = CreateJobHeader<OrgHeader>();
				var workflow1 = jobHeader.ProcessHeaders[0];
				var workflow2 = jobHeader.ProcessHeaders.AddNew();

				var network = networkViewModel.GetJobNetwork();
				var diagram = network.DiagramShape;
				var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
				var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");

				var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

				return new AddBufferAction.AddBufferForLinkAction(networkViewModel, arrow);
			}
		}
	}
}
