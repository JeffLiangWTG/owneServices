
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(OpenShapeListAction))]
	class OpenShapeListActionTest : JobNetworkActionTestCase<OpenShapeListAction>
	{
		// OpenShapeListAction logic tested in NetworkDiagramFormTest

		protected override void TestExecuteCore()
		{
			AssertExecute((network, shape) => network.Setup(m => m.ShowNetworkDiagramsModule(shape)), executeOnDiagram: true);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var action = GetAction(CreateNetworkViewModel(diagram));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is accessible to the root diagram only.", action.IsApplicableAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			NetworkActionAccessibilityTest.AssertAllowed(GetAction(CreateNetworkViewModel(diagram)).IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Open Shape List", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Opens a Network Diagrams module popup containing a list of shapes within this diagram.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("ShapeStack");
		}

		protected override OpenShapeListAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new OpenShapeListAction(networkViewModel);
		}

		public void TestExecute_ForNonSavedDiagram()
		{
			IJobNetwork network = null;

			var controller = CreateMockableController(Mocks);
			controller
				.Setup(m => m.TriggerSaveAction())
				.Returns(() =>
				{
					network.Refresh(RefreshType.Saving);
					Factory.Save();

					return ContinueWithSave.Yes;
				});
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			Factory.Save();

			var shape = networkViewModel.CreateNewShape(diagram);
			shape.Name = "Jimminy Jillikers!";

			AssertEquals(true, diagram.HasChanges);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			var newFactory = Factory.CreateNewFactory();
			var loadedShape = newFactory.Load<BMNCNShape>(shape.PK);

			AssertNotNull(loadedShape);
			AssertEquals("The form will attempt to save before performing this operation.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
