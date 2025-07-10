using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalculateDeliveryDateWithExceptionsOptionsRegistryItemEditor))]
	sealed class CalculateDeliveryDateWithExceptionsOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CalculateDeliveryDateWithExceptionsOptionsRegistryItemEditor(new CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType(new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 10 }), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override Integration.IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CalculateDeliveryDateWithExceptionsOptionsRegistryItem(
						"CalculateDeliveryDateWithExceptions",
						FreightDataRegistry.Categories.Freight_Container,
						ResString.GetMultilingualString("BDF9E11B-354D-4231-A360-0F5401A2EB51", "Calculate Delivery Due Date with Exceptions"),
						ResString.GetMultilingualString("CDD9E11B-351D-4231-A360-0F5405A2EB52", "Override this registry to specify the maximum combined delay duration per calendar day."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 10 });
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 10 } };
		}
	}
}
