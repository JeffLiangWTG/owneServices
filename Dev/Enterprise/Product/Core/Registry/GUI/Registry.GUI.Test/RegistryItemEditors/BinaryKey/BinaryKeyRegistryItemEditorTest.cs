using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BinaryKeyRegistryItemEditor))]
	sealed class BinaryKeyRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		const int KeySizeForTest = 64;

		#region Implementation

		BinaryKeyRegistryDataType GetDataType()
		{
			return new BinaryKeyRegistryDataType(KeySizeForTest);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BinaryKeyRegistryItemEditor(GetDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BinaryKeyControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.DataType = GetDataType();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "547C89AD1019E2224CA9632E1689C9C015018828A9961EDB3E41E48B98926859968A5D1F5F6B64E73291962CB669CA3BA9664560AF8240EAD06DB86C0891ED89" };
		}

		#endregion
	}
}
