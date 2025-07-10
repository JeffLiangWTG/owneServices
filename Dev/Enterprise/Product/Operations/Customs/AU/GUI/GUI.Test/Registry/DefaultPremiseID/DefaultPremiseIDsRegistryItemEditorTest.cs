using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(DefaultPremiseIDsRegistryItemEditor))]
	sealed class DefaultPremiseIDsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultPremiseIDsRegistryItemEditor((DefaultPremiseIDsRegistryDataType)RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DefaultPremiseIDControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultPremiseIDControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultPremiseIDsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			DefaultPremiseIDCollection collection = new DefaultPremiseIDCollection();
			DefaultPremiseID item = collection.AddNew();
			item.AirlineCode = "QF";
			item.PortOfDischarge = "AUSYD";
			item.PremiseID = "9914N";

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			DefaultPremiseIDCollection collection1 = (DefaultPremiseIDCollection)setValue;
			DefaultPremiseIDCollection collection2 = (DefaultPremiseIDCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].AirlineCode", i.ToString()), collection1[i].AirlineCode, collection2[i].AirlineCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PortOfDischarge", i.ToString()), collection1[i].PortOfDischarge, collection2[i].PortOfDischarge);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PremiseID", i.ToString()), collection1[i].PremiseID, collection2[i].PremiseID);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
