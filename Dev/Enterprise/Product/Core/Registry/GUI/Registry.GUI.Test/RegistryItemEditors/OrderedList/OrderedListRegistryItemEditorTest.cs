using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrderedListRegistryItemEditor))]
	sealed class OrderedListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OrderedListRegistryItemEditor(new StringRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrderedListControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new OrderedListRegistryItemEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "PR|PC|RP|PP" };
		}

		#endregion
	}
}
