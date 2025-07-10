using System;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public abstract class NodeNetworkActionsTestCase : TestCase
	{
		public void TestGetIconName()
		{
			TestGetIconNameCore();
		}

		#region Abstract Methods

		protected abstract void TestGetIconNameCore();

		#endregion

		#region Helper Methods

		protected Entity CreateEntityToSupportActions(NetworkViewModel networkViewModel, NetworkActions supportedActions)
		{
			var entity = new Entity
			{
				SupportedActions = supportedActions
			};
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);

			return entity;
		}

		protected Entity CreateChildEntity(NetworkViewModel networkViewModel)
		{
			var entity = new Entity
			{
				Parent = networkViewModel.Network.DiagramEntity
			};
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);

			return entity;
		}

		#endregion

		#region Assertions

		protected enum AccessibilityLevel { Applicability, Enabledness }

		protected void AssertActionIsAlwaysAllowed(Func<NetworkViewModel, INetworkAction> actionProvider, AccessibilityLevel accessibilityLevel)
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var entity = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			networkViewModel.SelectSingleEntity(entity);

			var action = actionProvider.Invoke(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(GetNetworkActionAccessibility(action, accessibilityLevel));
		}

		protected void AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes(Func<NetworkViewModel, INetworkAction> actionProvider, AccessibilityLevel accessibilityLevel)
		{
			AssertActionAccessibilityForRootDiagram(actionProvider, accessibilityLevel, shouldBeAllowed: false);
			AssertActionAccessibilityForChildShapes(actionProvider, accessibilityLevel, shouldBeAllowed: true);
		}

		protected void AssertActionIsAllowedForRootDiagramButNotAllowedForChildShapes(Func<NetworkViewModel, INetworkAction> actionProvider, AccessibilityLevel accessibilityLevel)
		{
			AssertActionAccessibilityForRootDiagram(actionProvider, accessibilityLevel, shouldBeAllowed: true);
			AssertActionAccessibilityForChildShapes(actionProvider, accessibilityLevel, shouldBeAllowed: false);
		}

		protected void AssertActionAccessibilityForRootDiagram(Func<NetworkViewModel, INetworkAction> actionProvider, AccessibilityLevel accessibilityLevel, bool shouldBeAllowed = true)
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var action = actionProvider.Invoke(networkViewModel);

			AssertEquals("Precondition: root diagram is active", networkViewModel.DiagramNodeViewModel, networkViewModel.ActiveNode);
			AssertEquals("Allowed for root diagram", shouldBeAllowed, GetNetworkActionAccessibility(action, accessibilityLevel).IsAllowed);
		}

		protected void AssertActionAccessibilityForChildShapes(Func<NetworkViewModel, INetworkAction> actionProvider, AccessibilityLevel accessibilityLevel, bool shouldBeAllowed = true)
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var entity = new Entity
			{
				Parent = network.DiagramEntity
			};
			var node = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			networkViewModel.SelectSingleEntity(node.Entity);

			var action = actionProvider.Invoke(networkViewModel);

			AssertEquals("Precondition: shape is active", node, networkViewModel.ActiveNode);
			AssertEquals("Precondition: not root", false, node.Entity.IsRoot());
			AssertEquals("Allowed for shapes", shouldBeAllowed, GetNetworkActionAccessibility(action, accessibilityLevel).IsAllowed);
		}

		protected void AssertActionHasCorrectIconName(Func<NetworkViewModel, INetworkAction> actionProvider, string iconName)
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = actionProvider.Invoke(networkViewModel);
			AssertEquals(iconName, action.GetIconName());
		}

		INetworkActionAccessibility GetNetworkActionAccessibility(INetworkAction action, AccessibilityLevel accessibilityLevel)
		{
			return accessibilityLevel == AccessibilityLevel.Applicability
				? action.IsApplicable()
				: action.IsEnabled();
		}

		#endregion
	}
}
