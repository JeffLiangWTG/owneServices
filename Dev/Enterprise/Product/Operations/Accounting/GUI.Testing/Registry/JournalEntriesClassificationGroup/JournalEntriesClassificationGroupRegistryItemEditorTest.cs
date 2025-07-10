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
	[TestedType(typeof(JournalEntriesClassificationGroupRegistryItemEditor))]
	public class JournalEntriesClassificationGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JournalEntriesClassificationGroupRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JournalEntriesClassificationGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JournalEntriesClassificationGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JournalEntriesClassificationGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new JournalEntriesClassificationGroupCollection();
			var item = collection.AddNew();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion

	}
}
