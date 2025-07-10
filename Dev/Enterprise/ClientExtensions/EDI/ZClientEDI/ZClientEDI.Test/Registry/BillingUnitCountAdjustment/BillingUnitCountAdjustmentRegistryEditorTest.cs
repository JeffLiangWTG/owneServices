using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(BillingUnitCountAdjustmentRegistryEditor))]
	public class BillingUnitCountAdjustmentRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillingUnitCountAdjustmentRegistryItem("", null, null, null, new BillingUnitCountAdjustmentRegistryEditorInfo(), RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BillingUnitCountAdjustmentRegistryEditor(new BillingUnitCountAdjustmentRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType() => typeof(BillingUnitCountAdjustmentRegistryControl);
		protected override object[] GetValidRegistryValues()
		{
			var collection = new BillingUnitCountAdjustmentCollection();
			var adjustment = collection.AddNew();
			adjustment.PriceCode = "P01";
			adjustment.AdjustmentSettings.AddNew(1, 1.5);
			var adjustment2 = collection.AddNew();
			adjustment2.PriceCode = "P02";
			adjustment2.AdjustmentSettings.AddNew(1, 0.5);
			adjustment2.AdjustmentSettings.AddNew(2, 1.5);
			adjustment2.AdjustmentSettings.AddNew(3, 2.5);
			return new object[] { collection };
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
			return !((BillingUnitCountAdjustmentRegistryControl)editorPane).ReadOnly;
		}
	}
}
