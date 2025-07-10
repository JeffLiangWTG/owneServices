using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class CommonNetworkActionExecutionStrategyTest : NetworkActionExecutionStrategyTestCase
	{
		#region IsApplicable

		public void TestShouldCheckApplicabilityForActiveEntityOnly()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(), entitiesActionIsApplicableTo: new[] { node2.Entity });

			AssertChecksAccessibilityForActiveEntityOnly(networkViewModel,
				nonAllowedEntity: node1.Entity,
				allowedEntity: node2.Entity,
				delegateForExactEntity: (e) => action.IsApplicableToEntity(e),
				delegateForWholeNetwork: () => action.IsApplicable());
		}

		#endregion

		#region IsEnabled

		public void TestShouldCheckEnablednessForActiveEntityOnly()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity },
				entitiesActionIsEnabledFor: new[] { node2.Entity });

			AssertChecksAccessibilityForActiveEntityOnly(networkViewModel,
				nonAllowedEntity: node1.Entity,
				allowedEntity: node2.Entity,
				delegateForExactEntity: (e) => action.IsEnabledForEntity(e),
				delegateForWholeNetwork: () => action.IsEnabled());
		}

		#endregion

		#region CheckCanStartExecution

		public void TestShouldPerformPreExecutionChecksForActiveEntityOnly()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity },
				entitiesActionCanExecuteFor: new[] { node2.Entity });

			AssertChecksAccessibilityForActiveEntityOnly(networkViewModel,
				nonAllowedEntity: node1.Entity,
				allowedEntity: node2.Entity,
				delegateForExactEntity: (e) => action.PerformPreExecutionChecksForEntity(e),
				delegateForWholeNetwork: () => action.CheckCanStartExecution());
		}

		public void TestCanStartExecutionChecks_ShouldCheckIfCanStartExecutionForNetwork_AfterPerformingPreExecutionChecksForActiveEntity()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity },
				entitiesActionCanExecuteFor: new[] { node2.Entity });

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", node1.Entity, action.PerformPreExecutionChecksForEntity(node1.Entity));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.PerformPreExecutionChecksForEntity(node2.Entity));
			AssertEquals("Should not call to CanStartExecutionForNetwork when performing checks for a particular entity", false, action.CanStartExecutionChecksForNetworkInvoked);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node1.Entity, action.CheckCanStartExecution());
				AssertEquals("Should not call to CanStartExecutionForNetwork when performing checks for the whole network if cannot execute for the active entity", false, action.CanStartExecutionChecksForNetworkInvoked);
			}

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node2.Entity))
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
				AssertEquals("Should call to CanStartExecutionForNetwork when performing checks for the whole network if can execute for the active entity", true, action.CanStartExecutionChecksForNetworkInvoked);
			}
		}

		#endregion

		#region Execute

		public void TestShouldExecuteForActiveEntityOnly()
		{
			networkViewModel.SelectEntities(new[] { node1.Entity, node2.Entity });
			AssertContainsExactElementsInAnyOrder("Precondition: selected entities", new[] { node1.Entity, node2.Entity }, networkViewModel.SelectedEntities);

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity });

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				action.Execute();

				AssertEquals(1, action.ProcessedEntities.Count);
				AssertEquals(action.ProcessedEntities[0], node1.Entity);
			}
		}

		public void TestShouldExecuteWithoutNotification_WhenCanExecuteForEntity_AndNetworkAllowsExecution()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity });

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				action.Execute();

				AssertEquals("Should execute", 1, action.ProcessedEntities.Count);
				AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

				// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
				NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node1.Entity));
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());
			}

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldNotExecuteAndShouldNotify_WhenCannotExecuteForEntity()
		{
			controller
				.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node1.Entity, accessibility);
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(
						"Action cannot execute for this entity.", accessibility);
				}));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity },
				entitiesActionCanExecuteFor: Array.Empty<INetworkEntity>());

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				action.Execute();

				AssertEquals("Should not execute as cannot execute for the entity", 0, action.ProcessedEntities.Count);
				AssertEquals("Should not check if can execute for network when cannot execute for the entity", false, action.CanStartExecutionChecksForNetworkInvoked);

				// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node1.Entity, action.PerformPreExecutionChecksForEntity(node1.Entity));
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node1.Entity, action.CheckCanStartExecution());
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());
			}
		}

		public void TestShouldNotExecuteAndShouldNotify_WhenCannotExecuteForNetwork()
		{
			controller
				.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Not allowed for network.",
						network.Object.DiagramEntity, accessibility);
				}));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity },
				allowedForNetwork: false);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				action.Execute();

				AssertEquals("Should not execute as not allowed for network", 0, action.ProcessedEntities.Count);
				AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

				// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
				NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node1.Entity));
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Not allowed for network.", action.CheckCanStartExecutionForNetwork());
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Not allowed for network.", action.CheckCanStartExecution());
			}
		}

		public void TestShouldNotNotifyWhenCancelledByUser()
		{
			var action = new ActionToTestExecutionStrategiesThatIsCancelledByUser(networkViewModel, GetStrategy());
			var preExecutionChecksResult = action.PerformPreExecutionChecksForEntity(node1.Entity);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", "Canceled by the user.", preExecutionChecksResult);
			var reason = preExecutionChecksResult.DenialReasons.Single();
			AssertEquals("Precondition", false, reason.NeedsNotification);

			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, node1.Entity))
			{
				action.Execute();
			}

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		#endregion

		#region Implementation

		protected override INetworkActionExecutionStrategy GetStrategy() => new CommonNetworkActionExecutionStrategy();

		void AssertChecksAccessibilityForActiveEntityOnly(NetworkViewModel networkViewModel,
			INetworkEntity nonAllowedEntity,
			INetworkEntity allowedEntity,
			Func<INetworkEntity, INetworkActionAccessibility> delegateForExactEntity,
			Func<INetworkActionAccessibility> delegateForWholeNetwork)
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", nonAllowedEntity, delegateForExactEntity(nonAllowedEntity));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", delegateForExactEntity(allowedEntity));

			CombineAssertions("Checking for an entity which does not allow execution", () =>
			{
				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, nonAllowedEntity))
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(nonAllowedEntity, delegateForWholeNetwork());
				}
			});

			CombineAssertions("Checking for an entity which allows execution", () =>
			{
				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, allowedEntity))
				{
					NetworkActionAccessibilityTest.AssertAllowed(delegateForWholeNetwork());
				}
			});
		}

		#endregion
	}
}
