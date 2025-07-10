using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(CountryTierPriceCodeMappingRegistryEditor))]
	public class CountryTierPriceCodeMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CountryTierPriceCodeMappingRegistryItem("", null, null, null, new CountryTierPriceCodeMappingRegistryEditorInfo(), RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CountryTierPriceCodeMappingRegistryEditor(new CountryTierPriceCodeMappingRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType() => typeof(CountryTierPriceCodeMappingRegistryControl);
		protected override object[] GetValidRegistryValues()
		{
			var collection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = collection.AddNew();
			mapping1.PriceCode = "P01";
			mapping1.SystemCode = "X01";
			mapping1.MappingLines.AddNew("AU", "C1");
			var mapping2 = collection.AddNew();
			mapping2.PriceCode = "P02";
			mapping2.SystemCode = "X01";
			mapping2.MappingLines.AddNew("AU", "C1");
			mapping2.MappingLines.AddNew("NZ", "C2");
			mapping2.MappingLines.AddNew("US", "C3");
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
			return !((CountryTierPriceCodeMappingRegistryControl)editorPane).ReadOnly;
		}
	}
}
