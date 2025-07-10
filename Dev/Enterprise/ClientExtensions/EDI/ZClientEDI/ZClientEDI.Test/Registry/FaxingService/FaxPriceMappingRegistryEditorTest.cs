using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(FaxPriceRegistryEditor))]
	public class FaxPriceMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FaxPriceRegistryItem("");
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new FaxPriceRegistryEditor(new FaxPriceDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FaxPriceControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			FaxPriceCollection[] items = new FaxPriceCollection[1];
			items[0] = new FaxPriceCollection();
			FaxPrice item = items[0].AddNew();
			item.Code = "AUD";
			item.Price = 0.20m;
			return items;
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
			return !((FaxPriceControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
