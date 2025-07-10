using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class RibbonButtonViewModelTest : TestCase
	{
		#region Model Properties

		public void TestShouldSetLabelAndTooltipToActionNameAndDescription_WhenAttachedToStaticAction()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var network = new DummyNetwork();
				var networkViewModel = new NetworkViewModel(network);

				var action = new StaticNetworkAction(name: ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF58", "Name"),
					description: ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF59", "Description"));

				var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertEquals("Name", buttonModel.Label);
				AssertEquals("Description", buttonModel.Tooltip);
			}
		}

		public void TestShouldUseLabelAndTooltipOverrides_WhenOverridesProvided_AndAttachedToStaticAction()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var network = new DummyNetwork();
				var networkViewModel = new NetworkViewModel(network);

				var action = new StaticNetworkAction(name: ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF58", "Name"),
					description: ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF59", "Description"));

				var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, label: ResString.GetMultilingualString("C66D51C6-4AE0-4DA6-826C-0649C966EF58", "Overridden name"),
					tooltip: ResString.GetMultilingualString("C66D51C6-4AE0-4DA6-826C-0649C966EF59", "Overridden description"), action);
				AssertEquals("Overridden name", buttonModel.Label);
				AssertEquals("Overridden description", buttonModel.Tooltip);
			}
		}

		public void TestShouldSetIsActivatedToActionIsActivated_WhenAttachedToStaticAction()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var network = new DummyNetwork();
				var networkViewModel = new NetworkViewModel(network);

				var action = new StaticNetworkAction(isActivated: true);

				var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertEquals(true, buttonModel.IsChecked);

				action = new StaticNetworkAction(isActivated: false);
				buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertEquals(false, buttonModel.IsChecked);
			}
		}

		public void TestShouldSetChildActionsToActionChildActions_WhenAttachedToStaticAction()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var network = new DummyNetwork();
				var networkViewModel = new NetworkViewModel(network);

				var childAction = new StaticNetworkAction();

				var action = new StaticNetworkAction(childActions: new INetworkAction[] { childAction });

				var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertContainsExactElementsInAnyOrder(new INetworkAction[] { childAction }, buttonModel.ChildButtons.Select(b => b.Action));

				action = new StaticNetworkAction(childActions: System.Array.Empty<INetworkAction>());
				buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<INetworkAction>(), buttonModel.ChildButtons.Select(b => b.Action));
			}
		}

		public void TestShouldSetIsEnabledToActionIsEnabled_WhenAttachedToStaticAction()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var action = new StaticNetworkAction(isEnabled: NetworkActionAccessibility.Allowed);

				var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertEquals(true, buttonModel.IsEnabled);

				action = new StaticNetworkAction(isEnabled: new NetworkActionAccessibility(new Entity() { Name = "Baggins" }, "Thief, thief, thief! Baggins! We hates it, we hates it, we hates it forever!"));
				buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
				AssertEquals(false, buttonModel.IsEnabled);
			}
		}

		public void TestShouldDynamicallyChangePropertiesToUpdatedActionProperties_WhenAttachedToDynamicAction_AndModelChanges()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var childAction = new StaticNetworkAction();

				var action = new TestDynamicNetworkAction(config.NetworkViewModel)
				{
					ReturnValueForGetNameCore = ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF58", "Name1"),
					ReturnValueForGetDescriptionCore = ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF59", "Description1"),
					ReturnValueForIsActivatedCore = true,
					ReturnValueForGetChildActionsCore = new INetworkAction[] { childAction },
					ReturnValueForIsEnabledForEntityCore = NetworkActionAccessibility.Allowed
				};

				var buttonModel = CreateButtonOnRibbon(config, action);
				AssertEquals("Name1", buttonModel.Label);
				AssertEquals("Description1", buttonModel.Tooltip);
				AssertEquals(true, buttonModel.IsChecked);
				AssertContainsExactElementsInAnyOrder(new INetworkAction[] { childAction }, buttonModel.ChildButtons.Select(b => b.Action));
				AssertEquals(true, buttonModel.IsEnabled);

				action.ReturnValueForGetNameCore = ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF68", "Name2");
				action.ReturnValueForGetDescriptionCore = ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966EF69", "Description2");
				action.ReturnValueForIsActivatedCore = false;
				action.ReturnValueForGetChildActionsCore = System.Array.Empty<INetworkAction>();
				action.ReturnValueForIsEnabledForEntityCore = new NetworkActionAccessibility(new Entity() { Name = "Baggins" }, "Thief, thief, thief! Baggins! We hates it, we hates it, we hates it forever!");

				config.Network.Refresh(RefreshType.Affinities);

				AssertEquals("Name2", buttonModel.Label);
				AssertEquals(@"Description2

This action cannot be executed for the given shape(s) due to the following reasons:
Baggins: Thief, thief, thief! Baggins! We hates it, we hates it, we hates it forever!", buttonModel.Tooltip);
				AssertEquals(false, buttonModel.IsChecked);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<INetworkAction>(), buttonModel.ChildButtons.Select(b => b.Action));
				AssertEquals(false, buttonModel.IsEnabled);
			}
		}

		public void TestShouldDynamicallyChangeTooltip_WhenBecomesDisabled()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var action = new TestDynamicNetworkAction(config.NetworkViewModel)
				{
					ReturnValueForGetDescriptionCore = ResString.GetMultilingualString("B66D51C6-4AE0-4DA6-826C-0649C966ED59", "Description"),
					ReturnValueForIsEnabledForEntityCore = NetworkActionAccessibility.Allowed
				};

				var buttonModel = CreateButtonOnRibbon(config, action);
				AssertEquals(true, buttonModel.IsEnabled);
				AssertEquals("Description", buttonModel.Tooltip);

				action.ReturnValueForIsEnabledForEntityCore = new NetworkActionAccessibility(new Entity() { Name = "Baggins" }, "Thief, thief, thief! Baggins! We hates it, we hates it, we hates it forever!");

				config.Network.Refresh(RefreshType.Affinities);

				AssertEquals(false, buttonModel.IsEnabled);
				AssertEquals(@"Description

This action cannot be executed for the given shape(s) due to the following reasons:
Baggins: Thief, thief, thief! Baggins! We hates it, we hates it, we hates it forever!", buttonModel.Tooltip);
			}
		}

		#endregion

		#region Implementation

		static RibbonButtonViewModel CreateButtonOnRibbon(NetworkGuiTestConfig config, INetworkAction action)
		{
			var buttonModel = new RibbonButtonViewModel(config.RibbonViewModel, action);
			config.Control.RibbonControlExposed_ForTesting.Model.Tabs[0].Groups[0].Items.Add(buttonModel);

			return buttonModel;
		}

		#endregion
	}
}
