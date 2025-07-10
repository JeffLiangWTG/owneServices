using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(UsageBillingSettingsRegistryEditor))]
	public class UsageBillingSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UsageBillingSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new UsageBillingSettingsRegistryEditor(new UsageBillingSettingsRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType() => typeof(UsageBillingSettingsRegistryControl);
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

			var priceList2 = priceLists.AddNew();
			priceList2.ProductCode = "ABC";
			priceList2.RawUsageCategory = "3GT";
			priceList2.PriceListCode = "PL1";
			priceList2.Description = "ABC - Price List #1";

			var branchRestriction = settings.BranchRestrictions.AddNew();
			branchRestriction.ProductCode = "ABC";
			branchRestriction.InvoicingBranch = Env.CurrentBranchPK;

			return new object[] { settings };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((UsageBillingSettingsRegistryControl)editorPane).ReadOnly;
		}
	}
}
