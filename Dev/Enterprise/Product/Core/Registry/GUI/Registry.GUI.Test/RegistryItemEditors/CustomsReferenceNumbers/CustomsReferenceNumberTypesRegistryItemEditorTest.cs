using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CustomsReferenceNumberTypesRegistryItemEditor))]
	sealed class CustomsReferenceNumberTypesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CustomsReferenceNumberTypesRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, new CustomsReferenceNumberTypeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CustomsReferenceNumberTypesRegistryItemEditor(new CustomsReferenceNumberTypesDataType(), null, null);
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(CustomsReferenceNumberTypesRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CustomsReferenceNumberTypeCollection();
			collection.Add("XXX", (NoResString)"DESC1");
			collection.Add("YYY", (NoResString)"DESC2");
			collection.Add("ZZZ", (NoResString)"DESC3");
			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CustomsReferenceNumberTypesRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
