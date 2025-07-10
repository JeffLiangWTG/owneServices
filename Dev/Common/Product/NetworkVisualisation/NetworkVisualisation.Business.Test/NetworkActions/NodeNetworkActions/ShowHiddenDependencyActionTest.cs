using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ShowHiddenDependencyActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsAlwaysAllowed((networkViewModel) => new ShowHiddenDependencyAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new ShowHiddenDependencyAction(networkViewModel);

			var entityWithoutHiddenDependencies = mocks.Create<INetworkEntityWithChildren>();
			var entityWithHiddenDependencies = mocks.Create<INetworkEntityWithChildren>();

			entityWithoutHiddenDependencies.Setup(m => m.HiddenRelationships).Returns(new ImpObservableCollection<IEntityRelationship>());
			entityWithHiddenDependencies.Setup(m => m.HiddenRelationships).Returns(new ImpObservableCollection<IEntityRelationship>(() => new IEntityRelationship[] { new Relationship() }));
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithoutHiddenDependencies.Object);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityWithHiddenDependencies.Object);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There should be at least one hidden dependency.", action.IsEnabledForEntity(entityWithoutHiddenDependencies.Object));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(entityWithHiddenDependencies.Object));
		}

		public void TestShouldNotifyRefresherAfterExecution()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var refresherNotified = false;
			refresher.Refreshed += (s, e) =>
			{
				if (e.RefreshType == RefreshType.ConnectionAdded)
				{
					refresherNotified = true;
				}
			};

			var parentNode = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Parent");
			var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child 1");
			var node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child 2");
			var entity1 = (Entity)node1.Entity;
			var entity2 = (Entity)node2.Entity;

			var relationship = new Relationship { From = entity1, To = entity2 };
			entity1.Links.Add(relationship);
			entity2.Links.Add(relationship);

			parentNode.HiddenDependencies.Add(relationship);
			AssertEquals("Precondition", 1, parentNode.HiddenDependencies.Count);

			var action = new ShowHiddenDependencyAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, parentNode.Entity))
			{
				var childActions = action.GetChildActions();
				AssertEquals("Precondition", 2, childActions.Count());
				var childAction = childActions.FirstOrDefault();

				NetworkActionAccessibilityTest.AssertAllowed("Precondition", childAction.IsEnabled());
				childAction.Execute();
			}

			Assert("Should notify the refresher", refresherNotified);
		}

		public void TestMenu()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var parentNode = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Parent");
			var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child 1");
			var node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child 2");
			var node3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child 3");
			var entity1 = (Entity)node1.Entity;
			var entity2 = (Entity)node2.Entity;
			var entity3 = (Entity)node3.Entity;

			var relationship = new Relationship { From = entity1, To = entity2 };
			var relationship2 = new Relationship { From = entity2, To = entity3 };
			entity1.Links.Add(relationship);
			entity2.Links.Add(relationship);
			entity2.Links.Add(relationship2);
			entity3.Links.Add(relationship2);

			parentNode.HiddenDependencies.Add(relationship);
			parentNode.HiddenDependencies.Add(relationship2);
			AssertEquals("Precondition", 2, parentNode.HiddenDependencies.Count);

			var action = new ShowHiddenDependencyAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, parentNode.Entity))
			{
				var childActions = action.GetChildActions().ToList();

				AssertEquals("Should be 3 child actions - 2 single dependency adds and one add all", 3, childActions.Count);
			}
		}

		public void TestShowSingleHiddenDependency()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var parent = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Parent");
			var from = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "From");
			var to = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "To");

			var relationship = network.CreateRelationship(from.Entity, to.Entity);
			network.HideRelationship(relationship);

			parent.HiddenDependencies.Add(relationship);

			var action = new ShowHiddenDependencyAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, parent.Entity))
			{
				((StaticNetworkAction)action.GetChildActions().FirstOrDefault()).Execute();

				AssertEquals("This entity should not able to be shown twice", default(IEntityRelationship), network.ShowRelationship(from.Entity, to.Entity));
			}
		}

		public void TestShowAllHiddenDependency()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var parent = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Parent");
			var entity1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child1");
			var entity2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child2");
			var entity3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Child3");

			var relationship1 = network.CreateRelationship(entity1.Entity, entity2.Entity);
			var relationship2 = network.CreateRelationship(entity2.Entity, entity3.Entity);

			network.HideRelationship(relationship1);
			network.HideRelationship(relationship2);

			parent.HiddenDependencies.Add(relationship1);
			parent.HiddenDependencies.Add(relationship2);

			var action = new ShowHiddenDependencyAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, parent.Entity))
			{
				((StaticNetworkAction)action.GetChildActions().First()).Execute();

				AssertEquals("This entity should not able to be shown twice", default(IEntityRelationship), network.ShowRelationship(entity1.Entity, entity2.Entity));
				AssertEquals("This entity should not able to be shown twice", default(IEntityRelationship), network.ShowRelationship(entity2.Entity, entity3.Entity));
			}
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new ShowHiddenDependencyAction(networkViewModel), "Link");
		}
	}
}
