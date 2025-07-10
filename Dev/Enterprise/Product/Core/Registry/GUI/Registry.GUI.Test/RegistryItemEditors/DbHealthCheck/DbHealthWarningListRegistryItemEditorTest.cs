using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DbHealthWarningListRegistryItemEditor))]
	sealed class DbHealthWarningListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DbHealthWarningListRegistryItem("", null, null, null, new DbHealthWarningRegistryCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DbHealthWarningListRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DbHealthWarningListControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new DbHealthWarningRegistryCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((DbHealthWarningListControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
