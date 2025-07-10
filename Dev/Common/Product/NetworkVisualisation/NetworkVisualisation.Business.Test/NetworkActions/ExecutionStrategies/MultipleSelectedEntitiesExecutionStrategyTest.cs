using System;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class MultipleSelectedEntitiesExecutionStrategyTest : NetworkActionExecutionStrategyTestCase
	{
		#region IsApplicable

		public void TestShouldCheckApplicabilityForAllSelectedEntities_AndAllowsIfAllOfThemAllow()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(), entitiesActionIsApplicableTo: new[] { node3.Entity, node4.Entity });

			AssertChecksAccessibilityForAllSelectedEntities_AndAllowsIfAllOfThemAllow(networkViewModel,
				nonAllowedEntity1: node1.Entity,
				nonAllowedEntity2: node2.Entity,
				allowedEntity1: node3.Entity,
				allowedEntity2: node4.Entity,
				delegateForExactEntity: (e) => action.IsApplicableToEntity(e),
				delegateForWholeNetwork: () => action.IsApplicable());
		}

		public void TestShoudlCheckApplicabilityForDiagram_IfNothingIsSelected_AndAllowIfDiagramAllows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel,
				GetStrategy(),
				entitiesActionIsApplicableTo: new[] { network.Object.DiagramEntity });
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicable());
		}

		public void TestShoudlCheckApplicabilityForDiagram_IfNothingIsSelected_AndDisallowIfDiagramDisallows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy());
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(networkViewModel.Network.DiagramEntity, action.IsApplicable());
		}

		#endregion

		#region IsEnabled

		public void TestShouldCheckEnablednessForAllSelectedEntities_AndAllowIfAtLeastOneAllows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node3.Entity, node4.Entity });

			AssertChecksAccessibilityForAllSelectedEntities_AndAllowsIfAtLeastOneAllows(networkViewModel,
				nonAllowedEntity1: node1.Entity,
				nonAllowedEntity2: node2.Entity,
				allowedEntity1: node3.Entity,
				allowedEntity2: node4.Entity,
				delegateForExactEntity: (e) => action.IsEnabledForEntity(e),
				delegateForWholeNetwork: () => action.IsEnabled());
		}

		public void TestShoudlCheckEnablednessForDiagram_IfNothingIsSelected_AndAllowIfDiagramAllows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { network.Object.DiagramEntity },
				entitiesActionIsEnabledFor: new[] { network.Object.DiagramEntity });
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
		}

		public void TestShoudlCheckEnablednessForDiagram_IfNothingIsSelected_AndDisallowIfDiagramDisallows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { network.Object.DiagramEntity });
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(networkViewModel.Network.DiagramEntity, action.IsEnabled());
		}

		#endregion

		#region CheckCanStartExecution

		public void TestCheckCanStartExecution_ShouldCheckEnablednessForAllSelectedEntities_AndAllowIfAtLeastOneAllows()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node3.Entity, node4.Entity });

			AssertChecksAccessibilityForAllSelectedEntities_AndAllowsIfAtLeastOneAllows(networkViewModel,
				nonAllowedEntity1: node1.Entity,
				nonAllowedEntity2: node2.Entity,
				allowedEntity1: node3.Entity,
				allowedEntity2: node4.Entity,
				delegateForExactEntity: (e) => action.IsEnabledForEntity(e),
				delegateForWholeNetwork: () => action.CheckCanStartExecution());
		}

		public void TestCheckCanStartExecution_ShouldCheckIfCanStartExecutionForNetwork_AfterCheckingEnablednessForAllSelectedEntities()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node3.Entity, node4.Entity });

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", node1.Entity, action.IsEnabledForEntity(node1.Entity));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", node2.Entity, action.IsEnabledForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsEnabledForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsEnabledForEntity(node4.Entity));
			AssertEquals("Should not call to PerformPreExecutionChecksForNetwork when performing checks for a particular entity", false, action.CanStartExecutionChecksForNetworkInvoked);

			networkViewModel.SelectEntities(new[] { node1.Entity, node2.Entity });
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new[] { node1.Entity, node2.Entity }, action.CheckCanStartExecution());
			AssertEquals("Should not call to PerformPreExecutionChecksForNetwork when performing checks for the whole network if cannot execute for the all selected entities", false, action.CanStartExecutionChecksForNetworkInvoked);

			networkViewModel.SelectEntities(new[] { node1.Entity, node3.Entity });
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			AssertEquals("Should call to PerformPreExecutionChecksForNetwork when performing checks for the whole network if can execute for at least one of the selected entities", true, action.CanStartExecutionChecksForNetworkInvoked);
		}

		#endregion

		#region Execute

		public void TestShouldExecuteForAllSelectedEntitiesIteratively()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node3.Entity, node4.Entity });
			action.Execute();

			AssertContainsExactElementsInAnyOrder(new[] { node3.Entity, node4.Entity }, action.ProcessedEntities);
		}

		public void TestShouldExecuteForDiagramEntity_WhenNothingIsSelected()
		{
			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { network.Object.DiagramEntity },
				entitiesActionIsEnabledFor: new[] { network.Object.DiagramEntity },
				entitiesActionCanExecuteFor: new[] { network.Object.DiagramEntity });

			action.Execute();

			AssertContainsExactElementsInAnyOrder(new[] { network.Object.DiagramEntity }, action.ProcessedEntities);
		}

		public void TestShouldExecuteWithoutNotification_WhenCanExecuteForAllSelectedEntities_AndNetworkAllowsExecution()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node3.Entity, node4.Entity });

			action.Execute();

			AssertContainsExactElementsInAnyOrder("Should execute", new[] { node3.Entity, node4.Entity }, action.ProcessedEntities);
			AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node4.Entity));

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldNotExecuteAndShouldNotify_WhenDisabledForAllSelectedEntities()
		{
			controller
				.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(
						new[] { node1.Entity, node2.Entity }, accessibility);
					NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(
						new[] { "Action is not enabled for this entity.", "Action is not enabled for this entity." },
						accessibility);
				}));
			controller.Setup(c => c.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: Array.Empty<INetworkEntity>(), // disabled
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node1.Entity, node2.Entity });

			action.Execute();

			AssertEquals("Should not execute as not enabled for all the selected entities", 0, action.ProcessedEntities.Count);
			AssertEquals("Should not check if can execute for network", false, action.CanStartExecutionChecksForNetworkInvoked);

			// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node1.Entity, action.IsEnabledForEntity(node1.Entity));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node2.Entity, action.IsEnabledForEntity(node2.Entity));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new[] { node1.Entity, node2.Entity }, action.IsEnabled());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(c => c.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldNotNotifyBeforeExecution_ShouldExecute_AndNotifyAfterwards_WhenEnabledForSomeOfSelectedEntities_AndCanExecuteForAllOfThem()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller
				.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(
						"Action is not enabled for this entity.", node2.Entity, accessibility);
				}));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node3.Entity, node4.Entity }, // enabled partially
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node2.Entity, node3.Entity, node4.Entity }); // contains one that the action is disabled for

			action.Execute();

			AssertContainsExactElementsInAnyOrder("Should execute as enabled for some of the selected entities",
				new[] { node3.Entity, node4.Entity }, action.ProcessedEntities);
			AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

			// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node2.Entity, action.IsEnabledForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node3.Entity));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldNotNotifyBeforeExecution_ShouldExecute_AndNotifyAfterwards_WhenEnabledForSomeOfSelectedEntities_AndCanExecuteForEvenFewerOfThem()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller
				.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(
						new[] { node2.Entity, node4.Entity }, accessibility);
					NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(
						new[]
						{
							"Action is not enabled for this entity.", "Action cannot execute for this entity."
						}, accessibility);
				}));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node3.Entity, node4.Entity }, // enabled partially
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity }); // cannot execute for node4

			networkViewModel.SelectEntities(new[] { node2.Entity, node3.Entity, node4.Entity }); // contains ones that the action is disabled / cannot execute for

			action.Execute();

			AssertContainsExactElementsInAnyOrder("Should execute as enabled for some of the selected entities", new[] { node3.Entity }, action.ProcessedEntities);
			AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

			// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node2.Entity, action.IsEnabledForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node4.Entity));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node4.Entity, action.PerformPreExecutionChecksForEntity(node4.Entity));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldNotNotifyBeforeExecution_ShouldExecute_AndNotifyAfterwards_WhenEnabledForAllOfSelectedEntities_AndCanExecuteForSomeOfThem()
		{
			controller.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()));
			controller
				.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(
						"Action cannot execute for this entity.", node2.Entity, accessibility);
				}));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionCanExecuteFor: new[] { node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node2.Entity, node3.Entity, node4.Entity }); // contains ones that the action cannot execute for

			action.Execute();

			AssertContainsExactElementsInAnyOrder("Should execute as enabled for some of the selected entities", new[] { node3.Entity, node4.Entity }, action.ProcessedEntities);
			AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node2.Entity, action.PerformPreExecutionChecksForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(node4.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node4.Entity));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()), Times.Never);
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
			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity },
				entitiesActionIsEnabledFor: new[] { node1.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity },
				allowedForNetwork: false);

			networkViewModel.SelectEntities(new[] { node1.Entity });

			action.Execute();

			AssertEquals("Should not execute as not allowed for network", 0, action.ProcessedEntities.Count);
			AssertEquals("Should check if can execute for network", true, action.CanStartExecutionChecksForNetworkInvoked);

			// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node1.Entity));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Not allowed for network.", action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Not allowed for network.", action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never());
		}

		public void TestShouldNotExecuteAndShouldNotify_WhenIsNotApplicableToAtLeastOneOfSelectedEntities()
		{
			controller
				.Setup(m => m.NotifyActionCannotBeExecuted(It.IsAny<INetworkActionAccessibility>()))
				.Callback(new Action<INetworkActionAccessibility>(accessibility =>
				{
					NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(
						"Action is not applicable to this entity.", node4.Entity, accessibility);
				}));
			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var action = new ActionToTestExecutionStrategies(networkViewModel, GetStrategy(),
				entitiesActionIsApplicableTo: new[] { node1.Entity, node2.Entity, node3.Entity }, // not applicable to node4
				entitiesActionIsEnabledFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity },
				entitiesActionCanExecuteFor: new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			networkViewModel.SelectEntities(new[] { node1.Entity, node2.Entity, node3.Entity, node4.Entity });

			action.Execute();

			AssertEquals("Should not execute as not applicable to one of the selected entities", 0, action.ProcessedEntities.Count);
			AssertEquals("Should not check if can execute for network", false, action.CanStartExecutionChecksForNetworkInvoked);

			// Preconditions but we check them here to avoid affecting the CanStartExecutionChecksForNetworkInvoked flag
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node1.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node2.Entity));
			NetworkActionAccessibilityTest.AssertAllowed(action.PerformPreExecutionChecksForEntity(node3.Entity));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node4.Entity, action.IsApplicableToEntity(node4.Entity));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason(node4.Entity, action.CheckCanStartExecution());
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionForNetwork());

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		#endregion

		#region Implementation

		protected override INetworkActionExecutionStrategy GetStrategy() => new MultipleSelectedEntitiesExecutionStrategy();

		void AssertChecksAccessibilityForAllSelectedEntities_AndAllowsIfAllOfThemAllow(NetworkViewModel networkViewModel,
			INetworkEntity nonAllowedEntity1,
			INetworkEntity nonAllowedEntity2,
			INetworkEntity allowedEntity1,
			INetworkEntity allowedEntity2,
			Func<INetworkEntity, INetworkActionAccessibility> delegateForExactEntity,
			Func<INetworkActionAccessibility> delegateForWholeNetwork)
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", nonAllowedEntity1, delegateForExactEntity(nonAllowedEntity1));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", nonAllowedEntity2, delegateForExactEntity(nonAllowedEntity2));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", delegateForExactEntity(allowedEntity1));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", delegateForExactEntity(allowedEntity2));

			CombineAssertions("Checking for entities which do not allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { nonAllowedEntity1, nonAllowedEntity2 });
				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new[] { nonAllowedEntity1, nonAllowedEntity2 }, delegateForWholeNetwork());
			});

			CombineAssertions("Checking for entities which allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { allowedEntity1, allowedEntity2 });
				NetworkActionAccessibilityTest.AssertAllowed(delegateForWholeNetwork());
			});

			CombineAssertions("Checking for entities which have ones that do not allow and ones that allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { nonAllowedEntity1, allowedEntity1 });
				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new[] { nonAllowedEntity1 }, delegateForWholeNetwork());
			});
		}

		void AssertChecksAccessibilityForAllSelectedEntities_AndAllowsIfAtLeastOneAllows(NetworkViewModel networkViewModel,
			INetworkEntity nonAllowedEntity1,
			INetworkEntity nonAllowedEntity2,
			INetworkEntity allowedEntity1,
			INetworkEntity allowedEntity2,
			Func<INetworkEntity, INetworkActionAccessibility> delegateForExactEntity,
			Func<INetworkActionAccessibility> delegateForWholeNetwork)
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", nonAllowedEntity1, delegateForExactEntity(nonAllowedEntity1));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", nonAllowedEntity2, delegateForExactEntity(nonAllowedEntity2));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", delegateForExactEntity(allowedEntity1));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", delegateForExactEntity(allowedEntity2));

			CombineAssertions("Checking for entities which do not allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { nonAllowedEntity1, nonAllowedEntity2 });
				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new[] { nonAllowedEntity1, nonAllowedEntity2 }, delegateForWholeNetwork());
			});

			CombineAssertions("Checking for entities which allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { allowedEntity1, allowedEntity2 });
				NetworkActionAccessibilityTest.AssertAllowed(delegateForWholeNetwork());
			});

			CombineAssertions("Checking for entities which have ones that do not allow and ones that allow execution", () =>
			{
				networkViewModel.SelectEntities(new[] { nonAllowedEntity1, allowedEntity1 });
				NetworkActionAccessibilityTest.AssertAllowed("Should allow if allowed for at least on of the selected entities", delegateForWholeNetwork());
			});
		}

		#endregion
	}
}
