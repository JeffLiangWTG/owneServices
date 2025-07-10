using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(AccountingTransactionsNumberSequenceCustomisationRegistryItemEditor))]
	public class AccountingTransactionsNumberSequenceCustomisationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AccountingTransactionsNumberSequenceCustomisationRegistryItemEditor(new TransactionsNumberSequenceCustomisationRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AccountingTransactionsNumberSequenceCustomisationControl)editorPane).ReadOnly;
		}

		protected override object[] GetValidRegistryValues()
		{
			TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();

			TransactionNumberSequenceCustomisation item = collection.AddNew();
			//TODO: set properties

			return new object[] { collection };
		}
		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AccountingTransactionsNumberSequenceCustomisationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			//TODO: check
			return new TransactionsNumberSequenceCustomisationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		#endregion
	}
}
