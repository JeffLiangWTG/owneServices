using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(AcceptBufferAction))]
	class AcceptBufferActionTest : JobNetworkActionTestCase<AcceptBufferAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

			network.Refresh(RefreshType.Saving);

			new SuggestBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			networkViewModel.CreateNodeViewModelsForEntitiesToLetNetworkActionsWork();

			AssertEquals(3, network.Entities.Count);
			var buffer = network.Shapes.Single(s => s.IsBufferShape);

			AssertEquals(false, buffer.Active);

			AssertAcceptBufferActionEnabledness(true, networkViewModel, network, buffer);

			var refreshed = false;
			network.Refreshed += (_, x_) => refreshed = true;

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(buffer);

			AssertEquals(true, refreshed);
			AssertEquals("Should activate the buffer shape", true, buffer.Active);

			AssertAcceptBufferActionEnabledness(false, networkViewModel, network, buffer);
		}

		void AssertAcceptBufferActionEnabledness(bool shouldBeEnabled, NetworkViewModel networkViewModel, IJobNetwork network, BMNCNShape shape)
		{
			var action = networkViewModel.GetCoreCustomNetworkAction_ForTesting<AcceptBufferAction>();
			AssertNotNull(action);
			AssertEquals(shouldBeEnabled, action.IsEnabledForEntity(shape.AsEntity(network)).IsAllowed);
		}

		public void TestExecute_ShouldSetBufferedFlagOnAllNetworkShapes()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader, name: "diagram");
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.ScaleAndRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			networkViewModel.CreateNodeViewModelsForEntitiesToLetNetworkActionsWork();
			var buffer = network.Entities.AsShapes().Single(s => s.IsBufferShape);

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);

			new AcceptBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(buffer);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			buffer.Active = false;

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

			var action = GetAction(CreateNetworkViewModel(diagram));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be a buffer.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be a buffer.", action.IsApplicableAfterActivatingEntity_ForTest(childShape));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(buffer));
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var action = GetAction(CreateNetworkViewModel(diagram));

			buffer.Active = false;
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(buffer));

			buffer.Active = true;
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The buffer should not be accepted yet.", action.IsEnabledAfterActivatingEntity_ForTest(buffer));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Accept buffer", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Makes this buffer active in the diagram", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Approve");
		}

		protected override AcceptBufferAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new AcceptBufferAction(networkViewModel);
		}
	}
}
