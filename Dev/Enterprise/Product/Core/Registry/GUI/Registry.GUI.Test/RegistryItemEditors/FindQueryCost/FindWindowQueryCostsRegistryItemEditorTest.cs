using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FindWindowQueryCostsRegistryItemEditor))]
	sealed class FindWindowQueryCostsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new FindWindowQueryCostsRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((FindWindowQueryCostsRegistryItemEditorUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(FindWindowQueryCostsRegistryItemEditorUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new FindWindowQueryCostsRegistryItem("", null, null, null, RegistryStorageFlags.System, new FindWindowQueryCosts());
		protected override object[] GetValidRegistryValues() => new object[] { new FindWindowQueryCosts() };
	}
}
