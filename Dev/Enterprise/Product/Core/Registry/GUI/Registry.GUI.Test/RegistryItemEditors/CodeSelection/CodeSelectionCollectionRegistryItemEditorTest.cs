using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeSelectionCollectionRegistryItemEditor))]
	sealed class CodeSelectionCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeSelectionCollectionRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeSelectionCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeSelectionCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeSelectionCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, CodeSelectionTest.GetCodesProviderForTesting());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting()) };
		}
	}
}
