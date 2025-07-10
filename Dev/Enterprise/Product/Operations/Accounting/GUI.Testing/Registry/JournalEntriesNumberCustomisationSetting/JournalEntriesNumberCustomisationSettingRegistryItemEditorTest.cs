using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisationSettingRegistryItemEditor))]
	public class JournalEntriesNumberCustomisationSettingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JournalEntriesNumberCustomisationSettingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JournalEntriesNumberCustomisationSettingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JournalEntriesNumberCustomisationSettingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JournalEntriesNumberCustomisationSettingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new JournalEntriesNumberCustomisationSetting() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
