using System.Linq;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ToggleResourceDependencyVisibilityAction))]
	class ToggleResourceDependencyVisibilityActionTest : JobNetworkActionTestCase<ToggleResourceDependencyVisibilityAction>
	{
		protected override void TestGetNameCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var createDependencyAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			var action = GetAction(networkViewModel);

			AssertEquals("Show Resource Dependencies", action.GetNameAfterActivatingEntity_ForTest(diagram));

			Assert("Precondition", createDependencyAction.CheckCanStartExecution().IsAllowed);
			createDependencyAction.ExecuteAfterActivatingEntity_ForTest(childShape1);

			AssertEquals("Show Resource Dependencies", action.GetNameAfterActivatingEntity_ForTest(diagram));

			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			Assert("Precondition", network.Shapes.Any(s => s.DependencyAttachments.Any(a => a.IsResourceDependency)));
			AssertEquals("Show Resource Dependencies", action.GetNameAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"This action is accessible to the root diagram only." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var action = GetAction(networkViewModel);
			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"The network should have resource dependencies." }, action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteAfterActivatingEntity_ForTest(childShape1);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestExecuteCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			var createDependencyAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			createDependencyAction.ExecuteAfterActivatingEntity_ForTest(childShape1);

			createDependencyAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			networkViewModel.SelectEntities(new[] { childShape2, childShape3 });
			createDependencyAction.ExecuteAfterActivatingEntity_ForTest(childShape2);

			var resourceDependencies = network.Shapes.SelectMany(s => s.DependencyAttachments.Where(a => a.IsResourceDependency)).Distinct();

			AssertEquals(2, resourceDependencies.Count());
			foreach (var dep in resourceDependencies)
			{
				AssertEquals(false, dep.BNA_IsHidden);
			}

			var wasRefreshed = false;
			network.Refreshed += (s, e) => wasRefreshed = true;
			AssertEquals(false, wasRefreshed);

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, wasRefreshed);

			AssertEquals(2, resourceDependencies.Count());
			foreach (var dep in resourceDependencies)
			{
				AssertEquals(true, dep.BNA_IsHidden);
			}
		}

		public void TestShouldChangeActivationAfterExecution()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var action = GetAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteAfterActivatingEntity_ForTest(childShape1);
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			networkViewModel.SelectEntities(System.Array.Empty<INetworkEntity>());

			AssertEquals(true, action.IsActivated());

			action.Execute();
			AssertEquals(false, action.IsActivated());

			action.Execute();
			AssertEquals(true, action.IsActivated());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Toggle the visibility of all resource dependencies in this diagram.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Link");
		}

		protected override ToggleResourceDependencyVisibilityAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ToggleResourceDependencyVisibilityAction(networkViewModel);
		}
	}
}
