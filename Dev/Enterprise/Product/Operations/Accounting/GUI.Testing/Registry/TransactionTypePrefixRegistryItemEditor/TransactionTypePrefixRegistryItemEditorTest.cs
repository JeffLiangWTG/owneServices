using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(TransactionTypePrefixRegistryItemEditor))]
	public class TransactionTypePrefixRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransactionTypePrefixRegistryItem("TransactionTypePrefix", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TransactionTypePrefixRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TransactionTypePrefixControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix = collection.AddNew();
			prefix.Ledger = "AR";
			prefix.TransactionType = "DSC";
			prefix.Prefix = "XX";

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TransactionTypePrefixControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
