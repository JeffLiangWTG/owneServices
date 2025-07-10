using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(BankAccountSelectionObject))]
	public class BankAccountSelectionObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetNothingSelected()
		{
			AccBankAccount newBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			BankAccountSelectionObject testBizObj = (BankAccountSelectionObject)GetNewBusinessObject();
			testBizObj.SelectedBankAccount = newBankAccount.PK;
			AssertEquals("SelectedBankAccount should be set correctly", newBankAccount.PK, testBizObj.SelectedBankAccount);
			testBizObj.SetNothingSelected();
			AssertEquals("Bank Account should be reseted", ZGuid.Empty, testBizObj.SelectedBankAccount);
		}

		public void TestConstructor()
		{
			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory);
			AccBankAccount newBankAccount = bankAccounts.AddNew();
			BankAccountSelectionObject testBizObj = new BankAccountSelectionObject(bankAccounts);
			AssertEquals("DefaultBankAccounts should return BankAccounts", bankAccounts, testBizObj.DefaultBankAccounts);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BankAccountSelectionObject(Factory);
		}

		#endregion

	}
}
