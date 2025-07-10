using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(PaymentBatchBankSelectionForm))]
	public class PaymentBatchBankSelectionFormTestCase : ZFormBasherTest
	{
		public void TestOnSaveButton_Click()
		{
			using (PaymentBatchBankSelectionForm bankSelectionForm = new PaymentBatchBankSelectionForm(TestBizObj))
			{
				bankSelectionForm.Show();
				TestBizObj.SelectedBankAccount = TestBizObj.DefaultBankAccounts[0].PK;
				bankSelectionForm.SaveButton_ForTestOnly.PerformClick();
				AssertEquals("Bank should still be selected", TestBizObj.DefaultBankAccounts[0].PK, TestBizObj.SelectedBankAccount);
			}
		}

		public void TestNothingSelectedIfCloseButtonPressed()
		{
			using (PaymentBatchBankSelectionForm bankSelectionForm = new PaymentBatchBankSelectionForm(TestBizObj))
			{
				bankSelectionForm.Show();
				TestBizObj.SelectedBankAccount = TestBizObj.DefaultBankAccounts[0].PK;
				AssertEquals(TestBizObj.DefaultBankAccounts[0].PK, TestBizObj.SelectedBankAccount);
				bankSelectionForm.CloseButton_ForTestOnly.PerformClick();
				AssertEquals("Bank Account should be reset", ZGuid.Empty, TestBizObj.SelectedBankAccount);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory);
			AccBankAccount newBankAccount = bankAccounts.AddNew();
			TestBizObj = new BankAccountSelectionObject(bankAccounts);
		}

		BankAccountSelectionObject TestBizObj;

		protected override Form GetFormToBashCore()
		{
			BankAccountSelectionObject selectionObject = new BankAccountSelectionObject(Factory);
			return new PaymentBatchBankSelectionForm(selectionObject);
		}
	}
}
