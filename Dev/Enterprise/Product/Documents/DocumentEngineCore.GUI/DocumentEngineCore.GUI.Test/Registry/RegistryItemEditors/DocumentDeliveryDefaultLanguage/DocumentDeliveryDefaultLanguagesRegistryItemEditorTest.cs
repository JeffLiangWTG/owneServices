using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguagesRegistryItemEditor))]
	sealed class DocumentDeliveryDefaultLanguagesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DocumentDeliveryDefaultLanguagesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocumentDeliveryDefaultLanguagesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocumentDeliveryDefaultLanguagesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DocumentDeliveryDefaultLanguagesRegistryItem("", null, null, null, RegistryStorageFlags.System, new DocumentDeliveryDefaultLanguagesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			var entry = collection.AddNew();
			entry.Fallback = "System";
			entry.Order = 1;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
