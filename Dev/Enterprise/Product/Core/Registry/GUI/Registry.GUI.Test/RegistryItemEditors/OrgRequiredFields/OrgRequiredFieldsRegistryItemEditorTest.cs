using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgRequiredFieldsRegistryItemEditor))]
	sealed class OrgRequiredFieldsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgRequiredFieldsRegistryItemEditor(false, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgRequiredFieldsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			BinaryRegistryItem result = new BinaryRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
			return result;
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			AssertEquals("GetValueFromEditorPane()", (byte[])setValue, (byte[])getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				new byte[]
				{
					60, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62, 13, 10, 32, 32, 60, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 32, 32, 60, 82,
					101, 113, 117, 105, 114, 101, 65, 100, 100, 114, 101, 115, 115, 50, 62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114,
					101, 65, 100, 100, 114, 101, 115, 115, 50, 62, 13, 10, 32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 66, 114, 97, 110, 99, 104,
					62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 66, 114, 97, 110, 99, 104, 62, 13, 10, 32, 32, 32, 32, 60, 82,
					101, 113, 117, 105, 114, 101, 67, 105, 116, 121, 62, 116, 114, 117, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 67, 105, 116, 121,
					62, 13, 10, 32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 80, 104, 111, 110, 101, 78, 117, 109, 98, 101, 114, 62, 116, 114,
					117, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 80, 104, 111, 110, 101, 78, 117, 109, 98, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60,
					82, 101, 113, 117, 105, 114, 101, 66, 117, 115, 105, 110, 101, 115, 115, 78, 117, 109, 98, 101, 114, 62, 116, 114, 117, 101, 60, 47,
					82, 101, 113, 117, 105, 114, 101, 66, 117, 115, 105, 110, 101, 115, 115, 78, 117, 109, 98, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60,
					82, 101, 113, 117, 105, 114, 101, 80, 104, 111, 110, 101, 79, 114, 66, 117, 115, 105, 110, 101, 115, 115, 78, 117, 109, 98, 101, 114,
					62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 80, 104, 111, 110, 101, 79, 114, 66, 117, 115, 105, 110, 101,
					115, 115, 78, 117, 109, 98, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 70, 97, 120, 78, 117, 109, 98,
					101, 114, 62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 70, 97, 120, 78, 117, 109, 98, 101, 114, 62, 13, 10,
					32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 69, 109, 97, 105, 108, 65, 100, 100, 114, 101, 115, 115, 62, 102, 97, 108, 115,
					101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 69, 109, 97, 105, 108, 65, 100, 100, 114, 101, 115, 115, 62, 13, 10, 32, 32, 32, 32, 60,
					82, 101, 113, 117, 105, 114, 101, 87, 101, 98, 65, 100, 100, 114, 101, 115, 115, 62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117,
					105, 114, 101, 87, 101, 98, 65, 100, 100, 114, 101, 115, 115, 62, 13, 10, 32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 70, 97,
					120, 69, 109, 97, 105, 108, 79, 114, 87, 101, 98, 62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 70, 97, 120,
					69, 109, 97, 105, 108, 79, 114, 87, 101, 98, 62, 13, 10, 32, 32, 32, 32, 60, 82, 101, 113, 117, 105, 114, 101, 65, 82, 67, 111, 110,
					116, 97, 99, 116, 62, 102, 97, 108, 115, 101, 60, 47, 82, 101, 113, 117, 105, 114, 101, 65, 82, 67, 111, 110, 116, 97, 99, 116, 62, 13,
					10, 32, 32, 60, 47, 84, 97, 98, 108, 101, 49, 62, 13, 10, 60, 47, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62
				}
			};
		}

		#endregion
	}
}
