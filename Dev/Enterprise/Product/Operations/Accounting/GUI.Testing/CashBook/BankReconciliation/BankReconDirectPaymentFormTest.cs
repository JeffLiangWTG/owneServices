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
	[TestedType(typeof(BankReconDirectPaymentForm))]
	public class BankReconDirectPaymentFormTest : CashBook.Testing.DirectPaymentFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var directPayment = TestObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();
			Factory.Save();
			return new BankReconDirectPaymentForm(directPayment);
		}

		public void TestCloseFormWillNotPostWhenEditInBankTransactionForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.BankReconDirectPayment);
			var directPayment = TestObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();

			using (var form = controller.ShowEditForm(directPayment) as BankReconDirectPaymentForm)
			{
				(form as IPostingButtonsProvider).CommandButtonCancel.PerformClick();
				AssertEquals(true, form.IsDisposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(new BusinessObjectFactory().Load<BankReconDirectPayment>(directPayment.PK));
			}

			using (var form = controller.ShowEditForm(directPayment) as BankReconDirectPaymentForm)
			{
				form.Close();
				AssertEquals(true, form.IsDisposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(new BusinessObjectFactory().Load<BankReconDirectPayment>(directPayment.PK));
			}
		}

		public void TestPostingButtonsWhenEditInBankTransactionForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.BankReconDirectPayment);
			var directPayment = TestObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();

			using (var form = controller.ShowEditForm(directPayment) as AccountingZForm)
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
			using (var form = (BankReconDirectPaymentForm)GetFormToBashCore())
			{
				form.DisplayMode = ODisplayMode.New;
				AssertEquals($"{form.Name} verb should be 'View'", "View", form.FormVerb);
			}
		}

		#endregion
	}
}
