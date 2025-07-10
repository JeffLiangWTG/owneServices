using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(TransactionsTypesToExportRegistryItemEditor))]
	public class TransactionsTypesToExportRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new TransactionsTypesToExportRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TransactionsTypesToExportControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TransactionsTypesToExportControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransactionsTypesToExportRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			TransactionsTypesToExportBusinessObject bizObject = new TransactionsTypesToExportBusinessObject(Factory);
			bizObject.ARAdjustmentNote = true;
			bizObject.ARCreditNote = true;
			bizObject.ARInvoice = true;
			bizObject.ARNonJobRelated = true;
			bizObject.ARJobRelated = false;
			return new object[] { bizObject };
		}
		#endregion
	}
}
