using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LastDbRestoreRegistryItemEditor))]
	sealed class LastDbRestoreRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new LastDbRestoreRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LastDbRestoreUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LastDbRestoreUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LastDbRestoreRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsReadOnly, new LastDbRestoreInfo());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new LastDbRestoreInfo() };
		}
	}
}
