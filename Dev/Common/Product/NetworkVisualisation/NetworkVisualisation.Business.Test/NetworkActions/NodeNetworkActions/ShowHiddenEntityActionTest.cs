using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ShowHiddenEntityActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsAlwaysAllowed((networkViewModel) => new ShowHiddenEntityAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowHiddenEntityAction(networkViewModel);

			var entityWithoutHiddenEntities = mocks.Create<INetworkEntityWithChildren>();
			var entityWithHiddenEntities = mocks.Create<INetworkEntityWithChildren>();

			entityWithoutHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>());
			entityWithHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>(() => new IProposedNetworkEntity[] { new Entity() }));

			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithoutHiddenEntities.Object);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithHiddenEntities.Object);

			AssertEquals("Precondition", 0, entityWithoutHiddenEntities.Object.HiddenEntities.Count);
			AssertEquals("Precondition", 1, entityWithHiddenEntities.Object.HiddenEntities.Count);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There should be at least one hidden entity.", action.IsEnabledForEntity(entityWithoutHiddenEntities.Object));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithHiddenEntities.Object));
		}

		public void TestMenu()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowHiddenEntityAction(networkViewModel);

			var entityWithoutHiddenEntities = mocks.Create<INetworkEntityWithChildren>();
			var entityWithHiddenEntities = mocks.Create<INetworkEntityWithChildren>();

			entityWithoutHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>());
			entityWithHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>(() => new IProposedNetworkEntity[] { new Entity(), new Entity(), new Entity() }));

			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithoutHiddenEntities.Object);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithHiddenEntities.Object);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entityWithoutHiddenEntities.Object))
			{
				action.Refresh(new RefreshArgs(RefreshType.RefreshButton, entityWithoutHiddenEntities.Object));
				var noHiddenEntityActions = action.GetChildActions().ToList();
				AssertEquals("There are no hidden entities, therefore there should be no show actions", 0, noHiddenEntityActions.Count);
			}

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entityWithHiddenEntities.Object))
			{
				action.Refresh(new RefreshArgs(RefreshType.RefreshButton, entityWithHiddenEntities.Object));
				var hiddenEntityActions = action.GetChildActions().ToList();
				AssertEquals("There are three hidden entities plus the add-all entities", 4, hiddenEntityActions.Count);
			}
		}

		public void TestShowingSingleEntity()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowHiddenEntityAction(networkViewModel);

			var entityWithHiddenEntities = mocks.Create<INetworkEntityWithChildren>();
			var desiredHiddenEntity = (INetworkEntity)new Entity();

			entityWithHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>(() => new IProposedNetworkEntity[] { new Entity(), new Entity(), desiredHiddenEntity }));

			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithHiddenEntities.Object);

			var entities = from node in networkViewModel.Nodes select node.Entity;
			AssertEquals("Entity should not be in network", false, entities.Contains(desiredHiddenEntity));

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entityWithHiddenEntities.Object))
			{
				action.Refresh(new RefreshArgs(RefreshType.RefreshButton, entityWithHiddenEntities.Object));
				((PositionalNetworkAction)action.GetChildActions().LastOrDefault()).Execute(new Location(0, 0));
			}

			entities = from node in networkViewModel.Nodes select node.Entity;
			AssertEquals("Entity should now be in network", true, entities.Contains(desiredHiddenEntity));
		}

		public void TestShowingAllEntities()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowHiddenEntityAction(networkViewModel);

			var entityWithHiddenEntities = mocks.Create<INetworkEntityWithChildren>();

			entityWithHiddenEntities.Setup(m => m.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>(() => new IProposedNetworkEntity[] { new Entity(), new Entity(), new Entity() }));

			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithHiddenEntities.Object);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entityWithHiddenEntities.Object))
			{
				action.Refresh(new RefreshArgs(RefreshType.RefreshButton, entityWithHiddenEntities.Object));
				((StaticNetworkAction)action.GetChildActions().FirstOrDefault()).Execute();
			}

			var entities = from node in networkViewModel.Nodes select node.Entity;
			AssertEquals("Network should now contain 4 entities", 4, entities.Count());
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new ShowHiddenEntityAction(networkViewModel), "Entity");
		}
	}
}
