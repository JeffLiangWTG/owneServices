using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(UsageMinimumFeeSettingsRegistryEditor))]
	public class UsageMinimumFeeSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new UsageMinimumFeeSettingsRegistryEditor(new UsageMinimumFeeSettingsRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((UsageMinimumFeeSettingsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType() => typeof(UsageMinimumFeeSettingsRegistryControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UsageMinimumFeeSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "PL0";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var usageMinimumFeeSettings = new UsageMinimumFeeSettings();
			var minimumFee1 = usageMinimumFeeSettings.MinimumFeeList.AddNew();
			minimumFee1.ProductCode = "ABC";
			minimumFee1.PriceListCode = "PL0";
			minimumFee1.MinimumFeeCode = "AXT";

			return new object[] { usageMinimumFeeSettings };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
