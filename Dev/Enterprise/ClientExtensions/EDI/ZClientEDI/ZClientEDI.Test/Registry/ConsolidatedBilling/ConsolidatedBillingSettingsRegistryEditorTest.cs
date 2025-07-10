using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolidatedBillingSettingsRegistryEditor))]
	public class ConsolidatedBillingSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ConsolidatedBillingSettingsRegistryEditor(new ConsolidatedBillingSettingsRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ConsolidatedBillingSettingsRegistryUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ConsolidatedBillingSettingsRegistryUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ConsolidatedBillingSettingsRegistryItem(
				"ConsolidatedBillingSettings",
				(NoResString)"LicenceBillingCategory",
				(NoResString)"Products (non-CW1) Enabled for Consolidated Billing",
				(NoResString)$"This will deliver the 'Products (non-CW1) Enabled for Consolidated Billing' registry.\r\nThis will allow multiplie databases to be consolidated into a single billing summary.",
				Integration.RegistryStorageFlags.System
				);
		}

		protected override object[] GetValidRegistryValues()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new ConsolidatedBillingSettingCollection(null, Factory);

			var product1 = settings.AddNew();
			product1.ProductCode = "ABC";
			product1.Description = "ABC - Price List #0";

			var priceList2 = settings.AddNew();
			priceList2.ProductCode = "ABC";
			priceList2.Description = "ABC - Price List #1";

			return new object[] { settings };
		}
	}
}
