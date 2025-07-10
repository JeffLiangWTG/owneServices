using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CreateMissingProductsItemEditor))]
	sealed class CreateMissingProductsItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CreateMissingProductsItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CreateMissingProductsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CreateMissingProductsRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CreateMissingProductsRegistryItem("CreateMissingProductWithRelationship", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CreateMissingProductsInfo());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CreateMissingProductsInfo() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeft; }
		}
	}
}
