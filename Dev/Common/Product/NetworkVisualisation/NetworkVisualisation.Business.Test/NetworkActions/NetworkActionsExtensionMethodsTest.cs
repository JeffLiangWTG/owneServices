using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkActionsExtensionMethodsTest : TestCase
	{
		#region ToMenuItem

		public void TestTooltipForEnabledStaticAction()
		{
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("TestTooltipForEnabledStaticAction: action: name", "Stop get world"),
								description: ResString.GetMultilingualString("TestTooltipForEnabledStaticAction: action: description", "I want to get off"),
								isEnabled: NetworkActionAccessibility.Allowed,
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals("I want to get off", menuItem.Tooltip);
		}

		public void TestTooltipForEnabledDynamicAction()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var action = GetMockAction(mocks,
								name: ResString.GetMultilingualString("TestTooltipForEnabledDynamicAction: action: name", "Stop get world"),
								description: ResString.GetMultilingualString("TestTooltipForEnabledDynamicAction: action: description", "I want to get off"),
								isApplicable: NetworkActionAccessibility.Allowed,
								isEnabled: NetworkActionAccessibility.Allowed,
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals("I want to get off", menuItem.Tooltip);
		}

		public void TestTooltipForDisabledStaticAction_SingleReason()
		{
			var entity = new Entity() { Name = "Entity" };
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("TestTooltipForDisabledStaticAction_SingleReason: action: name", "Never again Is what you swore"),
								description: ResString.GetMultilingualString("TestTooltipForDisabledStaticAction_SingleReason: action: description", "The time before"),
								isEnabled: new NetworkActionAccessibility(entity, "This can't be done"),
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals(@"The time before

This action cannot be executed for the given shape(s) due to the following reasons:
Entity: This can't be done", menuItem.Tooltip);
		}

		public void TestTooltipForDisabledDynamicAction_SingleReason()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var entity = new Entity() { Name = "Entity" };
			var action = GetMockAction(mocks,
								name: ResString.GetMultilingualString("TestTooltipForDisabledDynamicAction_SingleReason: action: name", "Never again Is what you swore"),
								description: ResString.GetMultilingualString("TestTooltipForDisabledDynamicAction_SingleReason: action: description", "The time before"),
								isApplicable: NetworkActionAccessibility.Allowed,
								isEnabled: new NetworkActionAccessibility(entity, "This can't be done"),
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals(@"The time before

This action cannot be executed for the given shape(s) due to the following reasons:
Entity: This can't be done", menuItem.Tooltip);
		}

		public void TestTooltipForDisabledStaticAction_MultipleReasons()
		{
			var entity1 = new Entity() { Name = "Entity1" };
			var entity2 = new Entity() { Name = "Entity2" };
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("TestTooltipForDisabledStaticAction_MultipleReasons: action: name", "Never again Is what you swore"),
								description: ResString.GetMultilingualString("TestTooltipForDisabledStaticAction_MultipleReasons: action: description", "The time before"),
								isEnabled: new NetworkActionAccessibility(entity1, "This can't be done").Union(new NetworkActionAccessibility(entity2, "Because it's against the law")),
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals(@"The time before

This action cannot be executed for the given shape(s) due to the following reasons:
Entity1: This can't be done
Entity2: Because it's against the law", menuItem.Tooltip);
		}

		public void TestTooltipForDisabledDynamicAction_MultipleReasons()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var entity1 = new Entity() { Name = "Entity1" };
			var entity2 = new Entity() { Name = "Entity2" };
			var action = GetMockAction(mocks,
								name: ResString.GetMultilingualString("TestTooltipForDisabledDynamicAction_MultipleReasons: action: name", "Never again Is what you swore"),
								description: ResString.GetMultilingualString("TestTooltipForDisabledDynamicAction_MultipleReasons: action: description", "The time before"),
								isApplicable: NetworkActionAccessibility.Allowed,
								isEnabled: new NetworkActionAccessibility(entity1, "This can't be done").Union(new NetworkActionAccessibility(entity2, "Because it's against the law")),
								isActivated: false,
								childActions: Enumerable.Empty<INetworkAction>());

			var menuItem = action.ToMenuItem();
			AssertEquals(@"The time before

This action cannot be executed for the given shape(s) due to the following reasons:
Entity1: This can't be done
Entity2: Because it's against the law", menuItem.Tooltip);
		}

		#endregion

		#region ToMenuItemsGrouped

		public void TestToMenuItemsGrouped()
		{
			var actions = new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped: 100_20", "100_20")) { Group = 100, GroupIndex = 20 },

				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped: 10_0", "10_0")) { Group = 10, GroupIndex = 0 },
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped: 10_10", "10_10")) { Group = 10, GroupIndex = 10 },

				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped: 20_0", "20_0")) { Group = 20, GroupIndex = 0 },

				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped: 50_20", "50_20")) { Group = 50, GroupIndex = 20 },
			};

			var menuItems = actions.ToMenuItemsGrouped();
			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Name == "10_0",
				i => i != null && i.Name == "10_10",
				i => i == null,
				i => i != null && i.Name == "20_0",
				i => i == null,
				i => i != null && i.Name == "50_20",
				i => i == null,
				i => i != null && i.Name == "100_20"
			}, menuItems);
		}

		public void TestToMenuItemsGrouped_NullAction_Foolproof()
		{
			var actions = new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_NullAction_Foolproof: 10_0", "10_0")) { Group = 10, GroupIndex = 0 },
				null,
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_NullAction_Foolproof: 10_10", "10_10")) { Group = 10, GroupIndex = 10 },
			};

			var menuItems = actions.ToMenuItemsGrouped();
			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Name == "10_0",
				i => i != null && i.Name == "10_10"
			}, menuItems);
		}

		public void TestToMenuItemsGrouped_ShouldNotIncludeNotApplicableItems()
		{
			var mocks = new MockRepository(MockBehavior.Loose);

			var actions = new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldNotIncludeNotApplicableItems: 10_0", "10_0")) { Group = 10, GroupIndex = 0 },
				GetNotApplicableMockAction(mocks),
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldNotIncludeNotApplicableItems: 10_10", "10_10")) { Group = 10, GroupIndex = 10 },
			};

			var menuItems = actions.ToMenuItemsGrouped();
			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Name == "10_0",
				i => i != null && i.Name == "10_10"
			}, menuItems);
		}

		public void TestToMenuItemsGrouped_NullChildActions_Foolproof()
		{
			var actions = new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_NullChildActions_Foolproof: Action", "Action"), childActions: null)
			};

			var menuItems = actions.ToMenuItemsGrouped();
			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Items != null && !i.Items.Any()
			}, menuItems);
		}

		public void TestToMenuItemsGrouped_ShouldSeparateItemsForChildActionsWithDelimiters()
		{
			var actions = new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldSeparateItemsForChildActionsWithDelimiters: Action", "Action"), childActions: new INetworkAction[]
				{
					new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldSeparateItemsForChildActionsWithDelimiters: Child_10_0", "Child_10_0")) { Group = 10, GroupIndex = 0 },
					new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldSeparateItemsForChildActionsWithDelimiters: Child_10_10", "Child_10_10")) { Group = 10, GroupIndex = 10 },

					new StaticNetworkAction(ResString.GetMultilingualString("TestToMenuItemsGrouped_ShouldSeparateItemsForChildActionsWithDelimiters: Child_20_0", "Child_20_0")) { Group = 20, GroupIndex = 0 },
				})
			};
			var menuItems = actions.ToMenuItemsGrouped();
			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Items != null
			}, menuItems);

			AssertMenuItems(new Func<NetworkActionMenuItem, bool>[]
			{
				i => i != null && i.Name == "Child_10_0",
				i => i != null && i.Name == "Child_10_10",
				i => i == null,
				i => i != null && i.Name == "Child_20_0",
			}, menuItems.First().Items);
		}

		#endregion

		#region Implementation

		IDynamicNetworkAction GetMockAction(MockRepository mocks, ResourceString name, ResourceString description, INetworkActionAccessibility isApplicable, INetworkActionAccessibility isEnabled, bool isActivated = false, IEnumerable<INetworkAction> childActions = null, int group = 0, int groupIndex = 0)
		{
			var dynamicAction = mocks.Create<IDynamicNetworkAction>();

			dynamicAction.Setup(m => m.GetName()).Returns(name);
			dynamicAction.Setup(m => m.GetDescription()).Returns(description);
			dynamicAction.Setup(m => m.IsApplicable()).Returns(isApplicable);
			dynamicAction.Setup(m => m.IsEnabled()).Returns(isEnabled);
			dynamicAction.Setup(m => m.IsActivated()).Returns(isActivated);
			dynamicAction.Setup(m => m.GetChildActions()).Returns(childActions);
			dynamicAction.Setup(m => m.Group).Returns(group);
			dynamicAction.Setup(m => m.GroupIndex).Returns(groupIndex);

			return dynamicAction.Object;
		}

		IDynamicNetworkAction GetNotApplicableMockAction(MockRepository mocks)
		{
			var dynamicAction = mocks.Create<IDynamicNetworkAction>();
			dynamicAction.Setup(m => m.IsApplicable()).Returns(NetworkActionAccessibility.Denied_ForTesting);

			dynamicAction.Setup(m => m.GetName());
			dynamicAction.Setup(m => m.GetDescription());
			dynamicAction.Setup(m => m.IsActivated());
			dynamicAction.Setup(m => m.GetChildActions());
			dynamicAction.Setup(m => m.Group);
			dynamicAction.Setup(m => m.GroupIndex);

			return dynamicAction.Object;
		}

		void AssertMenuItems(IEnumerable<Func<NetworkActionMenuItem, bool>> predicates, IEnumerable<NetworkActionMenuItem> menuItems)
		{
			var predicatesArray = predicates.ToArray();
			AssertEquals($"Should include {predicatesArray.Length} items including delimiters", predicatesArray.Length, menuItems.Count());

			var items = new Queue<NetworkActionMenuItem>(menuItems);
			var number = 0;
			foreach (var predicate in predicates)
			{
				var item = items.Dequeue();

				if (predicate(item))
				{
					Assert(true);
				}
				else
				{
					Assert($@"Predicate number {number} failed.
Corresponding menu item:
{item}", false);
				}

				number++;
			}
		}

		#endregion
	}
}
