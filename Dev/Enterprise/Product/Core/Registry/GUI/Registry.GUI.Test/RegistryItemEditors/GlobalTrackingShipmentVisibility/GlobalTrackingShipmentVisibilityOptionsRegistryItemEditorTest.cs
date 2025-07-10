using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor))]
	sealed class GlobalTrackingShipmentVisibilityOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor(new GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType(new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false }), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GlobalTrackingShipmentVisibilityOptionsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GlobalTrackingShipmentVisibilityOptionsRegistryItemControl);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override Integration.IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GlobalTrackingShipmentVisibilityOptionsRegistryItem(
						"GlobalTrackingShipmentVisibility",
						FreightDataRegistry.Categories.Freight_GlobalTracking,
						ResString.GetMultilingualString("ADF9E11B-354A-4231-A360-0F2401A2EB58", "Shipment Visibility"),
					ResString.GetMultilingualString("ADD9E11B-351A-4231-A360-0F1405A2EB59", "Enable Shipment Visibility?"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false });
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false } };
		}
	}
}
