using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CommissionPeriodListRegistryItemEditor))]
	sealed class CommissionPeriodListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CommissionPeriodListRegistryItemEditor(new CommissionPeriodListRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CommissionPeriodListRegistryControl)editorPane).Grid.ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(CommissionPeriodListRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CommissionPeriodListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, new CommissionPeriodListRegistryEditorInfo(), new CommissionPeriodCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CommissionPeriodCollection();
			collection.AddNew("0-12", (NoResString)"First year only", 0, 12);
			collection.AddNew("0-24", (NoResString)"First 2 years only", 0, 24);
			collection.AddNew("12-24", (NoResString)"Second year only", 12, 24);
			collection.AddNew("IND", (NoResString)"Indefinite", 0, 0);
			return new[] { collection };
		}
	}
}
