using System;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ApplyAffinitiesActionTest : NodeNetworkActionsTestCase
	{
		#region Accessibility

		public void TestApplicability()
		{
			AssertActionIsAlwaysAllowed((networkViewModel) => new ApplyAffinitiesAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new ApplyAffinitiesAction(networkViewModel);
			network.Refresher.Refreshed += (sender, args) => action.Refresh(args);

			var simpleEntity = CreateEntityToSupportActions(networkViewModel, NetworkActions.None);

			networkViewModel.SelectSingleEntity(simpleEntity);

			AssertEquals("Precondition", 0, simpleEntity.AvailableAffinities.Count);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There should be at least one affinity defined for the diagram which is not applied to the entity.", action.IsEnabled());

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);
			simpleEntity.AvailableAffinities.Add(affinity);
			AssertEquals("Precondition", 1, simpleEntity.AvailableAffinities.Count);
			network.Refresh(RefreshType.Affinities);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
		}

		public void TestShouldBeAbleToApplyAffinityToMultipleSelectedEntities_WhenAffinityIsNotAppliedToAtLeastOneEntity()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;
			var entity3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity3").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity2.AvailableAffinities.Add(affinity);

			entity1.AppliedAffinities.Add(affinity);
			entity3.AppliedAffinities.Add(affinity);

			var action = new ApplyAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());
		}

		public void TestShouldNotBeAbleToApplyAffinityToMultipleSelectedEntities_WhenAffinityIsAppliedToAllSelectedEntities()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1").Entity;
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2").Entity;

			var affinity = new DummyAffinity(Guid.NewGuid(), "white", Color.MintCream);
			network.DiagramEntity.AvailableAffinities.Add(affinity);

			entity1.AppliedAffinities.Add(affinity);
			entity2.AppliedAffinities.Add(affinity);

			var action = new ApplyAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2 });

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] {
				new NetworkActionDenialReason(entity1, "There should be at least one affinity defined for the diagram which is not applied to the entity."),
				new NetworkActionDenialReason(entity2, "There should be at least one affinity defined for the diagram which is not applied to the entity.") }, action.IsEnabled());
		}

		#endregion

		#region Execution

		public void TestShouldApplyAffinityToMutlipleSelectedEntities_WhenAffinityIsNotApplied()
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

			entity1.AvailableAffinities.Add(affinity);
			entity2.AvailableAffinities.Add(affinity);
			entity3.AvailableAffinities.Add(affinity);

			Assert("Precondition", !entity1.AppliedAffinities.Any());
			Assert("Precondition", !entity2.AppliedAffinities.Any());
			Assert("Precondition", !entity3.AppliedAffinities.Any());

			var action = new ApplyAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());

			childAction.Execute();
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity1.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity2.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity3.AppliedAffinities);

			interactionImplementor.Verify(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			interactionImplementor.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestShouldApplyAffinityToEntitiesAffinityIsNotAppliedTo_WithoutBotheringUserWithNotification()
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

			var action = new ApplyAffinitiesAction(networkViewModel);

			networkViewModel.SelectEntities(new[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals(1, action.GetChildActions().Count());
			var childAction = action.GetChildActions().Single();
			NetworkActionAccessibilityTest.AssertAllowed(childAction.CheckCanStartExecution());

			childAction.Execute();
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity1.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity2.AppliedAffinities);
			AssertContainsExactElementsInAnyOrder(new IAffinity[] { affinity }, entity3.AppliedAffinities);

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		#endregion

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new ApplyAffinitiesAction(networkViewModel), "Plus");
		}
	}
}
