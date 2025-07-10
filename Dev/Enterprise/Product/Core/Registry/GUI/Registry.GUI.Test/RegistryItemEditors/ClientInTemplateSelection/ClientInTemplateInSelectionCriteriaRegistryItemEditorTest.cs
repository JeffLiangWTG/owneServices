using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionRegistryItemEditor))]
	sealed class ClientInTemplateInSelectionCriteriaRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ClientInTemplateSelectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ClientInTemplateSelectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ClientInTemplateSelectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ClientInTemplateSelectionCriteriaCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
