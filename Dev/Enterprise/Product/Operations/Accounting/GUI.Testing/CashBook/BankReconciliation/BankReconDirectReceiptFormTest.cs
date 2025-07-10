using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation.Testing
{
	[TestedType(typeof(BankReconDirectReceiptForm))]
	public class BankReconDirectReceiptFormTest : CashBook.Testing.DirectReceiptFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
			Factory.Save();
			return new BankReconDirectReceiptForm(directReceipt);
		}

		public void TestCloseFormWillNotPostWhenEditInBankTransactionForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.BankReconDirectReceipt);
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();

			using (var form = controller.ShowEditForm(directReceipt) as BankReconDirectReceiptForm)
			{
				(form as IPostingButtonsProvider).CommandButtonCancel.PerformClick();
				AssertEquals(true, form.IsDisposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(new BusinessObjectFactory().Load<BankReconDirectReceipt>(directReceipt.PK));
			}

			using (var form = controller.ShowEditForm(directReceipt) as BankReconDirectReceiptForm)
			{
				form.Close();
				AssertEquals(true, form.IsDisposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(new BusinessObjectFactory().Load<BankReconDirectReceipt>(directReceipt.PK));
			}
		}

		public void TestPostingButtonsWhenEditInBankTransactionForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.BankReconDirectReceipt);
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();

			using (var form = controller.ShowEditForm(directReceipt) as AccountingZForm)
			{
				var postButton = (form as IPostingButtonsProvider).CommandButtonPost;
				AssertEquals(false, postButton.Visible);
				AssertEquals(false, postButton.Enabled);

				var applyButton = (form as IPostingButtonsProvider).CommandButtonApply;
				AssertEquals(false, applyButton.Visible);
				AssertEquals(false, applyButton.Enabled);

				var cancelButton = (form as IPostingButtonsProvider).CommandButtonCancel;
				AssertEquals(true, cancelButton.Visible);
				AssertEquals(true, cancelButton.Enabled);
				AssertEquals("Close", cancelButton.Text);
			}
		}

		#region Implementation

		public override void TestFormVerb()
		{
			using (var form = (BankReconDirectReceiptForm)GetFormToBashCore())
			{
				form.DisplayMode = ODisplayMode.New;
				AssertEquals($"{form.Name} verb should be 'View'", "View", form.FormVerb);
			}
		}

		#endregion
	}
}
