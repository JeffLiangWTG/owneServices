using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public abstract class NetworkActionExecutionStrategyTestCase : TestCase
	{
		public void TestEnabledness_ShouldDependOnApplicability()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: System.Array.Empty<INetworkEntity>(),
				entitiesActionIsEnabledFor: new[] { node1.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity });
			Assert("Precondition", !action.IsApplicableToEntity(node1.Entity).IsAllowed);

			Assert("Should not be enabled as not applicable", !action.IsEnabled().IsAllowed);
			Assert("Should not be enabled for entity as not applicable", !action.IsEnabledForEntity(node1.Entity).IsAllowed);
			Assert("Should not perform enabledness checks as applicability checks failed", !action.IsEnabledForEntityInvoked);
		}

		public void TestChecksIfCanStartExecution_ShouldDependOnEnabledness()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: System.Array.Empty<INetworkEntity>(),
				entitiesActionCanExecuteFor: new[] { node1.Entity });
			Assert("Precondition", !action.IsEnabledForEntity(node1.Entity).IsAllowed);

			Assert("Should not be able to start execution as not enabled", !action.CheckCanStartExecution().IsAllowed);
			Assert("Should not be able to execute for entity as not enabled", !action.PerformPreExecutionChecksForEntity(node1.Entity).IsAllowed);
			Assert("Should not perform pre-execution checks as enabledness checks failed", !action.PerformPreExecutionChecksForEntityInvoked);
			Assert("Just checking that network allowed execution and execution failed because of enabledness, not network restrictions", action.CheckCanStartExecutionForNetwork().IsAllowed);
		}

		public void TestIsApplicable_ShouldNotShowNotifications()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller.Setup(m => m.UserInteractionImplementor);

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: System.Array.Empty<INetworkEntity>(),
				entitiesActionIsEnabledFor: System.Array.Empty<INetworkEntity>(),
				entitiesActionCanExecuteFor: System.Array.Empty<INetworkEntity>());

			networkViewModel.SelectEntities(new[] { node1.Entity });
			AssertContainsExactElementsInAnyOrder("Precondition: selected entities", new[] { node1.Entity }, networkViewModel.SelectedEntities);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition",
				"Action is not applicable to this entity.", action.IsApplicableToEntity(node1.Entity));

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
			controller.Verify(m => m.UserInteractionImplementor, Times.Never);
		}

		public void TestIsEnabled_ShouldNotShowNotifications()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller.Setup(m => m.UserInteractionImplementor);

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: System.Array.Empty<INetworkEntity>(),
				entitiesActionCanExecuteFor: System.Array.Empty<INetworkEntity>());

			networkViewModel.SelectEntities(new[] { node1.Entity });
			AssertContainsExactElementsInAnyOrder("Precondition: selected entities", new[] { node1.Entity }, networkViewModel.SelectedEntities);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition",
				"Action is not enabled for this entity.", action.IsEnabledForEntity(node1.Entity));

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never());
			controller.Verify(m => m.UserInteractionImplementor, Times.Never);
		}

		public void TestExecuteForEntity_ShouldRunWithoutAccessibilityCheck()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: System.Array.Empty<INetworkEntity>(),
				entitiesActionIsEnabledFor: System.Array.Empty<INetworkEntity>(),
				entitiesActionCanExecuteFor: System.Array.Empty<INetworkEntity>());
			Assert("Precondition", !action.PerformPreExecutionChecksForEntity(node1.Entity).IsAllowed);

			action.ExecuteForEntityWithoutAccessCheck(node1.Entity);
			AssertEquals("Should run without validation as all the validation checks should be done by execution strategies", 1, action.ProcessedEntities.Count);
			AssertEquals(action.ProcessedEntities[0], node1.Entity);
		}

		public void TestShouldNotExecute_WhenPreExecutionChecksDenyThat()
		{
			var action = new ActionToTestExecutionStrategiesThatDoesNotAllowExecutionForNetwork(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity });
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", "Not allowed.", action.CheckCanStartExecutionForNetwork());
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.PerformPreExecutionChecksForEntity(node1.Entity));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.PerformPreExecutionChecksForEntity(node2.Entity));

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				Assert(!action.CheckCanStartExecution().IsAllowed);
			}
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node2.Entity))
			{
				Assert(!action.CheckCanStartExecution().IsAllowed);
			}
		}

		#region Implementation

		protected abstract INetworkActionExecutionStrategy GetStrategy();

		protected MockRepository mocks;
		protected Mock<INetworkUserInteractionImplementor> interactionImplementor;
		protected Mock<INetworkEntityController> controller;
		protected Mock<INetwork> network;
		protected NetworkViewModel networkViewModel;
		protected NodeViewModel node1;
		protected NodeViewModel node2;
		protected NodeViewModel node3;
		protected NodeViewModel node4;

		protected override void SetUp()
		{
			base.SetUp();

			mocks = new MockRepository(MockBehavior.Loose);
			interactionImplementor = mocks.Create<INetworkUserInteractionImplementor>();
			controller = mocks.Create<INetworkEntityController>();
			network = mocks.Create<INetwork>();

			controller.Setup(m => m.UserInteractionImplementor).Returns(interactionImplementor.Object);
			network.Setup(m => m.Controller).Returns(controller.Object);
			network.Setup(m => m.DiagramEntity).Returns(new Entity());
			network.Setup(m => m.EntityPositionStrategy).Returns(new EntityPositionStrategy());

			networkViewModel = new NetworkViewModel(network.Object);
			node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
			node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2");
			node3 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity3");
			node4 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity4");
		}

		protected class ActionToTestExecutionStrategies : DynamicNetworkAction
		{
			public ActionToTestExecutionStrategies(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy strategy,
					IEnumerable<INetworkEntity> entitiesActionIsApplicableTo = null,
					IEnumerable<INetworkEntity> entitiesActionIsEnabledFor = null,
					IEnumerable<INetworkEntity> entitiesActionCanExecuteFor = null,
					bool allowedForNetwork = true)
				: base(networkViewModel, strategy)
			{
				this.entitiesActionIsApplicableTo = new List<INetworkEntity>(entitiesActionIsApplicableTo ?? System.Array.Empty<INetworkEntity>());
				this.entitiesActionIsEnabledFor = new List<INetworkEntity>(entitiesActionIsEnabledFor ?? System.Array.Empty<INetworkEntity>());
				this.entitiesActionCanExecuteFor = new List<INetworkEntity>(entitiesActionCanExecuteFor ?? System.Array.Empty<INetworkEntity>());
				this.allowedForNetwork = allowedForNetwork;
				NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, this);
			}

			readonly List<INetworkEntity> entitiesActionIsApplicableTo;
			readonly List<INetworkEntity> entitiesActionIsEnabledFor;
			readonly List<INetworkEntity> entitiesActionCanExecuteFor;
			readonly bool allowedForNetwork;

			public List<INetworkEntity> ProcessedEntities = new List<INetworkEntity>();

			public bool IsEnabledForEntityInvoked;
			public bool PerformPreExecutionChecksForEntityInvoked;
			public bool CanStartExecutionChecksForNetworkInvoked;

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				ProcessedEntities.Add(entity);
				return null;
			}

			protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
			{
				return new NetworkActionAccessibility(entitiesActionIsApplicableTo.Contains(entity), entity, () => "Action is not applicable to this entity.");
			}

			protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
			{
				IsEnabledForEntityInvoked = true;
				return new NetworkActionAccessibility(entitiesActionIsEnabledFor.Contains(entity), entity, () => "Action is not enabled for this entity.");
			}

			protected override INetworkActionAccessibility PerformPreExecutionChecksForEntityCore(INetworkEntity entity)
			{
				PerformPreExecutionChecksForEntityInvoked = true;
				return new NetworkActionAccessibility(entitiesActionCanExecuteFor.Contains(entity), entity, () => "Action cannot execute for this entity.");
			}

			protected override INetworkActionAccessibility CheckCanStartExecutionForNetworkCore()
			{
				CanStartExecutionChecksForNetworkInvoked = true;
				return new NetworkActionAccessibility(allowedForNetwork, NetworkViewModel.Network.DiagramEntity, () => "Not allowed for network.");
			}

			protected override ResourceString GetDefaultNameCore() => null;

			protected override ResourceString GetDefaultDescriptionCore() => null;

			protected override bool IsActivatedCore(INetworkEntity activeEntity) => false;

			protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity) => System.Array.Empty<INetworkAction>();
		}

		protected class ActionToTestExecutionStrategiesThatDoesNotAllowExecutionForNetwork : ActionToTestExecutionStrategies
		{
			public ActionToTestExecutionStrategiesThatDoesNotAllowExecutionForNetwork(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy strategy,
					IEnumerable<INetworkEntity> entitiesActionIsApplicableTo = null,
					IEnumerable<INetworkEntity> entitiesActionIsEnabledFor = null,
					IEnumerable<INetworkEntity> entitiesActionCanExecuteFor = null)
				: base(networkViewModel, strategy,
					entitiesActionIsApplicableTo,
					entitiesActionIsEnabledFor,
					entitiesActionCanExecuteFor)
			{
			}

			protected override INetworkActionAccessibility CheckCanStartExecutionForNetworkCore() => NetworkActionAccessibility.Denied_ForTesting;
		}

		protected class ActionToTestExecutionStrategiesThatIsCancelledByUser : DynamicNetworkAction
		{
			public ActionToTestExecutionStrategiesThatIsCancelledByUser(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy strategy)
				: base(networkViewModel, strategy)
			{
			}

			protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility PerformPreExecutionChecksForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.GetCancelledByUserWithoutNotification(entity);

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				throw new System.NotImplementedException();
			}

			protected override ResourceString GetDefaultNameCore() => null;

			protected override ResourceString GetDefaultDescriptionCore() => null;
		}

		#endregion
	}
}
