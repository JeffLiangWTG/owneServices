using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShowHideShapeInspectorAction))]
	class ShowHideShapeInspectorActionTest : JobNetworkActionTestCase<ShowHideShapeInspectorAction>
	{
		public void TestShapeInspectorVisible()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			AssertEquals(false, diagram.ShapeInspectorVisible);

			action.ExecuteForEntityWithoutAccessCheck(diagram);
			AssertEquals(true, diagram.ShapeInspectorVisible);

			action.ExecuteForEntityWithoutAccessCheck(diagram);
			AssertEquals(false, diagram.ShapeInspectorVisible);
		}

		public void TestIsActivated()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			AssertEquals(false, action.IsActivated());

			action.ExecuteForEntityWithoutAccessCheck(diagram);
			AssertEquals(true, action.IsActivated());

			action.ExecuteForEntityWithoutAccessCheck(diagram);
			AssertEquals(false, action.IsActivated());
		}

		protected override void TestExecuteCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			AssertNull(action.ExecuteForEntityWithoutAccessCheck(diagram));
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicable());
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Shape Inspector", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Show / Hide Shape Inspector", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("PictureInPicture");
		}

		protected override ShowHideShapeInspectorAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ShowHideShapeInspectorAction(networkViewModel);
		}
	}
}
