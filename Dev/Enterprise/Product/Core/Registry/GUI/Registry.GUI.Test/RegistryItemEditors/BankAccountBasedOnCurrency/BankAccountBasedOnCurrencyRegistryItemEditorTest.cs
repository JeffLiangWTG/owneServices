using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BankAccountBasedOnCurrencyRegistryItemEditor))]
	sealed class BankAccountBasedOnCurrencyRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new BankAccountBasedOnCurrencyRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BankAccountBasedOnCurrencyControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BankAccountBasedOnCurrencyControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BankAccountBasedOnCurrencyRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();

			BankAccountBasedOnCurrency bankAccountBasedOnCurrency = collection.AddNew();

			bankAccountBasedOnCurrency.Currency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			bankAccountBasedOnCurrency.BankAccount = Factory.NewWithValidTestData<AccBankAccount>().PK;
			Factory.Save();

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
