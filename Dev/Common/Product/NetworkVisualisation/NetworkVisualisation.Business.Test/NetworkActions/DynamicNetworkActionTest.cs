using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class DynamicNetworkActionTest : TestCase
	{
		#region Default Values

		public void TestShouldUseDefaultValues_WhenNotOverridden()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var action = new DynamicActionsWithDefaultValuesOnly(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicable());

			AssertEquals("Default name", action.GetName());
			AssertEquals("Default description", action.GetDescription());
			AssertEquals(true, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null }, action.GetChildActions());
		}

		public void TestShouldUseDefaultValues_WhenNotApplicable()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var isApplicable = NetworkActionAccessibility.Denied_ForTesting;
			var action = new DynamicActionToTestDefaultValues(networkViewModel, applicabilityGetter: () => isApplicable);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Precondition", "Not allowed.", action.IsApplicable());

			AssertEquals("Default name", action.GetName());
			AssertEquals("Default description", action.GetDescription());
			AssertEquals(true, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null }, action.GetChildActions());

			isApplicable = NetworkActionAccessibility.Allowed;
			action.Refresh(new RefreshArgs(RefreshType.None));
			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicable());

			AssertEquals("Dynamic name", action.GetName());
			AssertEquals("Dynamic description", action.GetDescription());
			AssertEquals(false, action.IsActivated());
			AssertContainsExactElementsInAnyOrder(new INetworkAction[] { null, null }, action.GetChildActions());
		}

		#endregion

		#region Evaluation for Deleted Entities

		public void TestShouldMakeActionsNotApplicableWhenEntityIsRemovedFromDiagram()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);
			var action = new TestDynamicNetworkAction(networkViewModel);

			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity to delete");

			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicableToEntity(node.Entity));

			networkViewModel.RemoveFromDiagram(node);
			AssertNull(networkViewModel.GetNodeForEntity(node.Entity));
			action.ResetCounters();

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute as the entity was deleted.", action.IsApplicableToEntity(node.Entity));
			AssertEquals("Should not run applicability core calculation for the entity removed from diagram - should make it not applicable instead", 0, action.IsApplicableRequestsCounter);
		}

		public void TestShouldMakeActionsNotApplicableForDeletedEntities()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);
			var action = new TestDynamicNetworkAction(networkViewModel);

			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity to delete");

			NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicableToEntity(node.Entity));

			(node.Entity as Entity).IsDeleted = true;
			AssertNotNull(networkViewModel.GetNodeForEntity(node.Entity));
			action.ResetCounters();

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute as the entity was deleted.", action.IsApplicableToEntity(node.Entity));
			AssertEquals("Should not run applicability core calculation for the deleted entity - should make it not applicable instead", 0, action.IsApplicableRequestsCounter);
		}

		public void TestShouldMakeActionNotApplicableImmediatelyAfterRemovingNode_AndShouldNotReevaluateApplicabilityBeforeRemoving()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);
			var action = new TestDynamicNetworkAction(networkViewModel);

			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity to delete");
			var entity = node.Entity;

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entity))
			{
				NetworkActionAccessibilityTest.AssertAllowed("Precondition", action.IsApplicable());
				AssertNotNull("Precondition", networkViewModel.GetNodeForEntity(entity));

				var startingIsApplicableCalls = action.IsApplicableRequestsCounter;

				networkViewModel.RemoveFromDiagram(node);
				AssertEquals("IsApplicable should not be checked after the node is removed. SAD!", startingIsApplicableCalls, action.IsApplicableRequestsCounter);
			}
		}

		#endregion

		#region Properties

		public void TestShouldRecalculateProperties()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Precondition", 0, action.NameRequestsCounter);
			AssertEquals("Precondition", 0, action.DescriptionRequestsCounter);
			AssertEquals("Precondition", 0, action.IsActivatedRequestsCounter);
			AssertEquals("Precondition", 0, action.ChildActionsRequestsCounter);
			AssertEquals("Precondition", 0, action.IsApplicableRequestsCounter);
			AssertEquals("Precondition", 0, action.IsEnabledRequestsCounter);

			CombineAssertions("Should recalculate on the first call to IsApplicable", () =>
			{
				action.ResetCounters();
				action.IsApplicable();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 1, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate on the first call to IsEnabled", () =>
			{
				action.ResetCounters();
				action.IsEnabled();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 1, action.IsApplicableRequestsCounter); // as enabledness depends on applicability
				AssertEquals("IsEnabled", 1, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate on the first call to GetName", () =>
			{
				action.ResetCounters();
				action.GetName();

				AssertEquals("Name", 1, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 0, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate on the first call to GetDescription", () =>
			{
				action.ResetCounters();
				action.GetDescription();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 1, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 0, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate on the first call to IsActivated", () =>
			{
				action.ResetCounters();
				action.IsActivated();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 1, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 0, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate on the first call to GetChildActions", () =>
			{
				action.ResetCounters();
				action.GetChildActions();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 1, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 0, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should recalculate everything when network changes - and everything should be recalculated just once", () =>
			{
				action.ResetCounters();
				action.Refresh(new RefreshArgs(RefreshType.Affinities));

				AssertEquals("Name", 1, action.NameRequestsCounter);
				AssertEquals("Description", 1, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 1, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 1, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 2, action.IsApplicableRequestsCounter); // two requests as enabledness depends on applicability (will be reduced down to 1 within WI00230632)
				AssertEquals("IsEnabled", 1, action.IsEnabledRequestsCounter);
			});

			CombineAssertions("Should not recalculate if network has not changed", () =>
			{
				action.ResetCounters();
				action.GetName();
				action.GetDescription();
				action.IsActivated();
				action.GetChildActions();
				action.IsApplicable();
				action.IsEnabled();

				AssertEquals("Name", 0, action.NameRequestsCounter);
				AssertEquals("Description", 0, action.DescriptionRequestsCounter);
				AssertEquals("IsActivated", 0, action.IsActivatedRequestsCounter);
				AssertEquals("ChildActions", 0, action.ChildActionsRequestsCounter);
				AssertEquals("IsApplicable", 0, action.IsApplicableRequestsCounter);
				AssertEquals("IsEnabled", 0, action.IsEnabledRequestsCounter);
			});
		}

		public void TestShouldRecalculateApplicabilityOnFirstPropertiesRequestJustOnce_RegardlessTheOrderThePropertiesAreRequested()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);

			var action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Precondition", 0, action.NameRequestsCounter);
			AssertEquals("Precondition", 0, action.DescriptionRequestsCounter);
			AssertEquals("Precondition", 0, action.IsActivatedRequestsCounter);
			AssertEquals("Precondition", 0, action.ChildActionsRequestsCounter);
			AssertEquals("Precondition", 0, action.IsApplicableRequestsCounter);
			AssertEquals("Precondition", 0, action.IsEnabledRequestsCounter);

			action.IsApplicable();
			action.GetName();
			action.GetDescription();

			AssertEquals(1, action.NameRequestsCounter);
			AssertEquals(1, action.DescriptionRequestsCounter);
			AssertEquals(0, action.IsActivatedRequestsCounter);
			AssertEquals(0, action.ChildActionsRequestsCounter);
			AssertEquals(1, action.IsApplicableRequestsCounter);
			AssertEquals(0, action.IsEnabledRequestsCounter);

			action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Precondition", 0, action.NameRequestsCounter);
			AssertEquals("Precondition", 0, action.DescriptionRequestsCounter);
			AssertEquals("Precondition", 0, action.IsActivatedRequestsCounter);
			AssertEquals("Precondition", 0, action.ChildActionsRequestsCounter);
			AssertEquals("Precondition", 0, action.IsApplicableRequestsCounter);
			AssertEquals("Precondition", 0, action.IsEnabledRequestsCounter);

			action.GetName();
			action.GetDescription();
			action.IsApplicable();

			AssertEquals(1, action.NameRequestsCounter);
			AssertEquals(1, action.DescriptionRequestsCounter);
			AssertEquals(0, action.IsActivatedRequestsCounter);
			AssertEquals(0, action.ChildActionsRequestsCounter);
			AssertEquals(1, action.IsApplicableRequestsCounter);
			AssertEquals(0, action.IsEnabledRequestsCounter);

			action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Precondition", 0, action.NameRequestsCounter);
			AssertEquals("Precondition", 0, action.DescriptionRequestsCounter);
			AssertEquals("Precondition", 0, action.IsActivatedRequestsCounter);
			AssertEquals("Precondition", 0, action.ChildActionsRequestsCounter);
			AssertEquals("Precondition", 0, action.IsApplicableRequestsCounter);
			AssertEquals("Precondition", 0, action.IsEnabledRequestsCounter);

			action.IsApplicable();
			action.IsEnabled();

			AssertEquals(0, action.NameRequestsCounter);
			AssertEquals(0, action.DescriptionRequestsCounter);
			AssertEquals(0, action.IsActivatedRequestsCounter);
			AssertEquals(0, action.ChildActionsRequestsCounter);
			AssertEquals(2, action.IsApplicableRequestsCounter); // two requests as enabledness depends on applicability (will be reduced down to 1 within WI00230632)
			AssertEquals(1, action.IsEnabledRequestsCounter);

			action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Precondition", 0, action.NameRequestsCounter);
			AssertEquals("Precondition", 0, action.DescriptionRequestsCounter);
			AssertEquals("Precondition", 0, action.IsActivatedRequestsCounter);
			AssertEquals("Precondition", 0, action.ChildActionsRequestsCounter);
			AssertEquals("Precondition", 0, action.IsApplicableRequestsCounter);
			AssertEquals("Precondition", 0, action.IsEnabledRequestsCounter);

			action.IsEnabled();
			action.IsApplicable();

			AssertEquals(0, action.NameRequestsCounter);
			AssertEquals(0, action.DescriptionRequestsCounter);
			AssertEquals(0, action.IsActivatedRequestsCounter);
			AssertEquals(0, action.ChildActionsRequestsCounter);
			AssertEquals(2, action.IsApplicableRequestsCounter); // two requests as enabledness depends on applicability (will be reduced down to 1 within WI00230632)
			AssertEquals(1, action.IsEnabledRequestsCounter);
		}

		public void TestShouldChangeDependentProperties_WhenApplicabilityChanges()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new TestActionWhichPropertiesDoNotReactOnModelChangesDirectly(networkViewModel);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(true, action.IsApplicable().IsAllowed);
				AssertEquals(true, action.IsEnabled().IsAllowed);
				AssertEquals("Name", action.GetName());
				AssertEquals("Description", action.GetDescription());
				AssertEquals(true, action.IsActivated());
				AssertEquals(1, action.GetChildActions().Count());
			});

			action.ReturnValueForIsApplicableToEntityCore = NetworkActionAccessibility.Denied_ForTesting;
			action.Refresh(new RefreshArgs(RefreshType.Affinities));

			CombineAssertions("Should switch to default values even if the properties do not react to model changes directly", () =>
			{
				AssertEquals(false, action.IsApplicable().IsAllowed);
				AssertEquals(false, action.IsEnabled().IsAllowed);
				AssertEquals(null, action.GetName());
				AssertEquals(null, action.GetDescription());
				AssertEquals(false, action.IsActivated());
				AssertEquals(0, action.GetChildActions().Count());
			});
		}

		public void TestShouldRecalculateApplicabilityFirstAndOtherPropertiesNext_NotViceVersa_WhenNetworkChanges()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new TestDynamicNetworkAction(networkViewModel);
			AssertEquals("Name", action.GetName());
			AssertEquals(true, action.IsApplicable().IsAllowed);
			AssertEquals(1, action.IsApplicableRequestsCounter);

			action.ReturnValueForIsApplicableToEntityCore = NetworkActionAccessibility.Denied_ForTesting;
			action.ResetCounters();
			action.Refresh(new RefreshArgs(RefreshType.Affinities));

			AssertEquals("Should be the default name as should be no longer applicable", null, action.GetName());
			AssertEquals("Should be not applicable", false, action.IsApplicable().IsAllowed);
			AssertEquals(1, action.IsApplicableRequestsCounter);
		}

		#endregion

		#region Preventing Memory Leaks
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1056:GC.Collect()", Justification = "This code is testing for memory leaks.")]
		public void TestChildActionsShouldNotLeak()
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork() { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var networkViewModel = new NetworkViewModel(network);

			var action = new TestDynamicNetworkAction(networkViewModel);
			var childReference = GetReferenceToChildAction(action);
			AssertEquals("Precondition", true, childReference.IsAlive);

			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertEquals("Child action should stay in memory as child actions trigger has not recalculated", true, childReference.IsAlive);

			network.Refresh(RefreshType.Affinities);
			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertEquals("Child action should stay in memory as child actions trigger has recalculated but its value has not changed", true, childReference.IsAlive);

			var newChildAction = new TestDynamicNetworkAction(networkViewModel);
			action.ReturnValueForGetChildActionsCore = new INetworkAction[] { newChildAction };
			action.Refresh(new RefreshArgs(RefreshType.Affinities));
			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertEquals("Child action should go out of the scope and be garbage collected as child actions trigger has changed its value", false, childReference.IsAlive);
		}

		WeakReference GetReferenceToChildAction(INetworkAction action)
		{
			var childAction = action.GetChildActions().SingleOrDefault();
			AssertNotNull(childAction);
			return new WeakReference(childAction);
		}

		#endregion
	}

	#region Test Classes

	#region Classes to Test Default Values

	class DynamicActionsWithDefaultValuesOnly : DynamicNetworkAction
	{
		public DynamicActionsWithDefaultValuesOnly(INetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("1B16BB41-1466-47A3-8D6E-F51623589B5F", "Default name");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("C87DDD2E-336D-4B57-9248-CC45A5D5007E", "Default description");

		protected override bool GetDefaultIsActivatedCore() => true;

		protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore() => new INetworkAction[] { null };

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			throw new NotImplementedException();
		}
	}

	class DynamicActionToTestDefaultValues : DynamicActionsWithDefaultValuesOnly
	{
		public DynamicActionToTestDefaultValues(INetworkViewModel networkViewModel, Func<INetworkActionAccessibility> applicabilityGetter)
			: base(networkViewModel)
		{
			this.applicabilityGetter = applicabilityGetter;
		}

		readonly Func<INetworkActionAccessibility> applicabilityGetter;

		protected override ResourceString GetNameCore(INetworkEntity activeEntity)
		{
			if (applicabilityGetter().IsAllowed)
			{
				return ResString.GetMultilingualString("0F222B0F-8FF7-4291-A513-CC7152EAFA27", "Dynamic name");
			}
			throw new InvalidOperationException();
		}

		protected override ResourceString GetDescriptionCore(INetworkEntity activeEntity)
		{
			if (applicabilityGetter().IsAllowed)
			{
				return ResString.GetMultilingualString("F0F02514-6C76-45D4-BECE-C8D5C5E2A933", "Dynamic description");
			}
			throw new InvalidOperationException();
		}

		protected override bool IsActivatedCore(INetworkEntity activeEntity)
		{
			if (applicabilityGetter().IsAllowed)
			{
				return false;
			}
			throw new InvalidOperationException();
		}

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			if (applicabilityGetter().IsAllowed)
			{
				return new INetworkAction[] { null, null };
			}
			throw new InvalidOperationException();
		}

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => applicabilityGetter();

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region Classes to Test Triggers

	#region Classes to Test Name and Description

	public class ResponsiveActionToTestNameAndDescription : TestDynamicNetworkAction
	{
		public ResponsiveActionToTestNameAndDescription(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => true;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class PickyActionToTestNameAndDescription : TestDynamicNetworkAction
	{
		public PickyActionToTestNameAndDescription(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => args is RefreshArgs refreshArgs && refreshArgs.RefreshType == RefreshType.Affinities;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class IndifferentActionToTestNameAndDescription : TestDynamicNetworkAction
	{
		public IndifferentActionToTestNameAndDescription(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	#endregion

	#region Classes to Test IsActivated

	public class ResponsiveActionToTestIsActivated : TestDynamicNetworkAction
	{
		public ResponsiveActionToTestIsActivated(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class PickyActionToTestIsActivated : TestDynamicNetworkAction
	{
		public PickyActionToTestIsActivated(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => args.RefreshType == RefreshType.Affinities;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class IndifferentActionToTestIsActivated : TestDynamicNetworkAction
	{
		public IndifferentActionToTestIsActivated(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => false;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	#endregion

	#region Classes to Test Child Actions

	public class ResponsiveActionToTestChildActions : TestDynamicNetworkAction
	{
		public ResponsiveActionToTestChildActions(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => true;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class PickyActionToTestChildActions : TestDynamicNetworkAction
	{
		public PickyActionToTestChildActions(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => args is RefreshArgs refreshArgs && refreshArgs.RefreshType == RefreshType.Affinities;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	public class IndifferentActionToTestChildActions : TestDynamicNetworkAction
	{
		public IndifferentActionToTestChildActions(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;
		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	#endregion

	#region Classes to Test Accessibility

	public class ResponsiveActionToTestAccessibility : TestDynamicNetworkAction
	{
		public ResponsiveActionToTestAccessibility(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => true;
	}

	public class PickyActionToTestAccessibility : TestDynamicNetworkAction
	{
		public PickyActionToTestAccessibility(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => args.RefreshType == RefreshType.Affinities;
	}

	public class IndifferentActionToTestAccessibility : TestDynamicNetworkAction
	{
		public IndifferentActionToTestAccessibility(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;
	}

	#endregion

	#region Classes to Test Properties Update

	class TestActionWhichPropertiesDoNotReactOnModelChangesDirectly : TestDynamicNetworkAction
	{
		public TestActionWhichPropertiesDoNotReactOnModelChangesDirectly(NetworkViewModel networkViewModel)
			: base(networkViewModel)
		{
		}

		protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => false;

		protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;
	}

	#endregion

	#endregion

	#endregion
}
