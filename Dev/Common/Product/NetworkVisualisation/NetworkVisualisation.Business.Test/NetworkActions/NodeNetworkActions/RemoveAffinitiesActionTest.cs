using System;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class RemoveAffinitiesActionTest : NodeNetworkActionsTestCase
	{
		#region Accessibility

		public void TestApplicability()
		{
			AssertActionIsAlwaysAllowed((networkViewModel) => new RemoveAffinitiesAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new RemoveAffinitiesAction(networkViewModel);

			var simpleEntity = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);

			networkViewModel.SelectSingleEntity(simpleEntity);

			AssertEquals("Precondition", 0, simpleEntity.AppliedAffinities.Count);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There should be at least one affinity applied.", action.IsEnabled());

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);
			simpleEntity.AppliedAffinities.Add(affinity);
			AssertEquals("Precondition", 1, simpleEntity.AppliedAffinities.Count);
			action.Refresh(new RefreshArgs(RefreshType.Affinities));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
		}

		public void TestShouldBeAbleToRemoveAffinityFromMultipleSelectedEntities_WhenAffinityIsAppliedToAtLeastOneEntity()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;
			var entity3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity3").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity1.AvailableAffinities.Add(affinity);
			entity2.AvailableAffinities.Add(affinity);

			entity3.AppliedAffinities.Add(affinity);

			var action = new RemoveAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());
		}

		public void TestShouldNotBeAbleToRemoveAffinityFromMultipleSelectedEntities_WhenAffinityIsNotAppliedToAnyOfSelectedEntities()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity1.AvailableAffinities.Add(affinity);
			entity2.AvailableAffinities.Add(affinity);

			Assert("Precondition", !entity1.AppliedAffinities.Any());
			Assert("Precondition", !entity2.AppliedAffinities.Any());

			var action = new RemoveAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2 });

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] {
				new NetworkActionDenialReason(entity1, "There should be at least one affinity applied."),
				new NetworkActionDenialReason(entity2, "There should be at least one affinity applied.") }, action.IsEnabled());
		}

		#endregion

		#region Execution

		public void TestShouldRemoveAffinityFromMutlipleSelectedEntities_WhenAffinityIsAppliedToAllEntities()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var interactionImplementor = mocks.Create<INetworkUserInteractionImplementor>();
			var controller = mocks.Create<INetworkEntityController>();

			controller.Setup(c => c.UserInteractionImplementor).Returns(interactionImplementor.Object);

			interactionImplementor.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()));
			interactionImplementor.Setup(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()));

			network.Controller = controller.Object;

			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;
			var entity3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity3").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity1.AppliedAffinities.Add(affinity);
			entity2.AppliedAffinities.Add(affinity);
			entity3.AppliedAffinities.Add(affinity);

			var action = new RemoveAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());

			childAction.Execute();
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity1.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity2.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity3.AppliedAffinities);

			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestShouldRemoveAffinityFromEntitiesAffinityIsAppliedTo_WithoutBotheringUserWithNotification()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var controller = mocks.Create<INetworkEntityController>();

			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));
			network.Controller = controller.Object;

			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;
			var entity3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity3").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity2.AvailableAffinities.Add(affinity);

			entity1.AppliedAffinities.Add(affinity);
			entity3.AppliedAffinities.Add(affinity);

			Assert("Precondition", !entity2.AppliedAffinities.Any());

			var action = new RemoveAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());

			childAction.Execute();
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity1.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity2.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(Array.Empty<IAffinity>(), entity3.AppliedAffinities);

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		#endregion

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new RemoveAffinitiesAction(networkViewModel), "Minus");
		}
	}
}
