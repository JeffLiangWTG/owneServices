using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateOptionsRegistryItemEditor))]
	sealed class CalculateDeliveryDueDateOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CalculateDeliveryDueDateOptionsRegistryItemEditor(new CalculateDeliveryDueDateOptionsRegistryItemDataType(new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = true }), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CalculateDeliveryDueDateOptionsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CalculateDeliveryDueDateOptionsRegistryItemControl);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override Integration.IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var defaultList = new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false };
			return new CalculateDeliveryDueDateOptionsRegistryItem(
					"CalculateDeliveryDueDateByTransportMode",
						FreightDataRegistry.Categories.Freight_Shipment,
						ResString.GetMultilingualString("ADD9E11B-358D-4231-A360-0F5402A2EB5A", "Calculate Delivery Due Date"),
						ResString.GetMultilingualString("D09891D2-D487-4E94-9D63-2250853FD31A", "Enable Delivery Due Date calculation on Bookings and Shipments."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) };
		}
	}
}
