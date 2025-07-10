using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionListRegistryEditorEx))]
	public class CodeDescriptionListRegistryEditorExTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			return new CodeDescriptionListRegistryEditorEx(registryItem, new CodeDescriptionPairListRegistryDataType(10), new CodeDescriptionPairListEditorInfo());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionListEditControlForRegistryEx)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionListEditControlForRegistryEx);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 10, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code", "Description");
			return new CodeDescriptionPairList[] { list };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			ReadOnlyCodeDescriptionPairList list1 = (ReadOnlyCodeDescriptionPairList)setValue;
			ReadOnlyCodeDescriptionPairList list2 = (ReadOnlyCodeDescriptionPairList)getValue;
			AssertEquals("GetValueFromEditorPaneCore().Count", list1.Count, list2.Count);
			for (int x = 0; x < list1.Count; ++x)
			{
				AssertEquals("GetValueFromEditorPaneCore().Code", list1[x].Code, list2[x].Code);
				AssertEquals("GetValueFromEditorPaneCore().Description", list1[x].Description, list2[x].Description);
			}
		}

		protected override void AssertGetValueTypeIsRegistryValueType(object getValue)
		{
			Assert("Value from EditorPane should be a ReadOnlyCodeDescriptionPairList.", getValue is ReadOnlyCodeDescriptionPairList);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
		#endregion
	}
}
