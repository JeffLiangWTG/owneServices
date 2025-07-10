using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(DefaultFreightPercentagesRegistryItemEditor))]
	sealed class DefaultFreightPercentagesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
			=> new DefaultFreightPercentagesRegistryItemEditor((DefaultFreightPercentagesRegistryDataType)RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DefaultFreightPercentageControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DefaultFreightPercentageControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DefaultFreightPercentagesRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 0.12m;

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (DefaultFreightPercentageCollection)setValue;
			var collection2 = (DefaultFreightPercentageCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].ModeofTransport", i.ToString()), collection1[i].ModeofTransport, collection2[i].ModeofTransport);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].FreightPercentage", i.ToString()), collection1[i].FreightPercentage, collection2[i].FreightPercentage);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
