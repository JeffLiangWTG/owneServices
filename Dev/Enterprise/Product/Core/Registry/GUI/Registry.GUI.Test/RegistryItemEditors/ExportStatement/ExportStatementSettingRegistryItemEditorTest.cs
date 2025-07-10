using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ExportStatementSettingRegistryItemEditor))]
	sealed class ExportStatementSettingRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExportStatementSettingRegistryItem("", null, null, null, RegistryStorageFlags.System, new CountryExportStatementSettingCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ExportStatementSettingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExportStatementSettingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CountryExportStatementSettingCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ExportStatementSettingControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
