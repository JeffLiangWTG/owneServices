using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(AutomatedModificationRegistryItemEditor))]
	class AutomatedModificationRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AutomatedModificationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((AutomatedModificationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutomatedModificationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutomatedModificationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var automaticAdvanceDate1 = new AutomatedModification();
			automaticAdvanceDate1.TimeByDefault = ZDateTime.Today;

			var automaticAdvanceDate2 = new AutomatedModification();
			automaticAdvanceDate2.TimeByDefault = ZDateTime.Empty;

			return new object[] { automaticAdvanceDate1, automaticAdvanceDate2 };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
