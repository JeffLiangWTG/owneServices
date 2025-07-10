using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(MinimumIntervalSubLedgerTakeUpRegistryItemEditor))]
	sealed class MinimumIntervalSubLedgerTakeUpRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new MinimumIntervalSubLedgerTakeUpRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MinimumIntervalSubLedgerTakeUpControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MinimumIntervalSubLedgerTakeUpControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MinimumIntervalSubLedgerTakeUpRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, new MinimumIntervalSubLedgerTakeUp());
		}

		protected override object[] GetValidRegistryValues()
		{
			MinimumIntervalSubLedgerTakeUp bizO = new MinimumIntervalSubLedgerTakeUp();
			return new object[] { bizO };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
