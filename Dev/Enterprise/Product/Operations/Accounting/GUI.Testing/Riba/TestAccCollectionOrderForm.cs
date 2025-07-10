using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.AccountingZForm;

namespace Enterprise.Accounting.GUI.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrderForm))]
	public class TestAccCollectionOrderForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var testOrder = Factory.New<AccCollectionOrder>();

			var result = new AccCollectionOrderForm(testOrder);
			result.ControllerID = ControllerIDs.AccCollectionOrder;
			return result;
		}

		public void TestAddTransactionsToOrderAcctionMenu()
		{
			SetupData();

			using (var form = new AccCollectionOrderForm(order))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				var actionMenuItems = form.ActionsMenuItem_ForTestOnly.MenuItems;
				Assert("Should contain Add Transactions To Order Menu Item", actionMenuItems.Contains(form.AddTransactionsToOrderMenuItem));

				Env.Security.CollectionOrderEdit.IsAllowed = false;
				form.HandleAddTransactionsToOrder(null, null);
				AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CollectionOrderEdit.IsAllowed = true;

				order.IsCancelled = true;
				Factory.Save();
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals("You are not allowed to add new transactions to a rejected order.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				order.IsCancelled = false;
				Factory.Save();
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals(typeof(OrderTransactionsFilterHolder), ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateReceiptAcctionMenu()
		{
			SetupData();

			using (var form = new AccCollectionOrderForm(order))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				var actionMenuItems = form.ActionsMenuItem_ForTestOnly.MenuItems;
				Assert("Should contain Create Receipt Menu Item", actionMenuItems.Contains(form.CreateReceiptMenuItem));

				Env.Security.CollectionOrderCreateReceipt.IsAllowed = false;
				form.HandleCreateReceipts(null, null);
				AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CollectionOrderCreateReceipt.IsAllowed = true;

				Factory.Save();
				form.HandleCreateReceipts(null, null);
				AssertEquals("expect successful message", "Receipts and Deposit Batches are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRejectOrderAcctionMenu()
		{
			SetupData();

			using (var form = new AccCollectionOrderForm(order))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				var actionMenuItems = form.ActionsMenuItem_ForTestOnly.MenuItems;
				Assert("Should contain reject order Menu Item", actionMenuItems.Contains(form.RejectOrderMenuItem));

				order.IsCancelled = true;
				Factory.Save();
				form.HandleRejectOrder(null, null);
				AssertEquals("This order has already been rejected.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				order.IsCancelled = false;
				Factory.Save();
				Env.Security.CollectionOrderReject.IsAllowed = false;
				form.HandleRejectOrder(this, null);
				AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CollectionOrderReject.IsAllowed = true;
				form.HandleRejectOrder(null, null);
				AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				//TODO: Assert("This order has not been rejected.", order.IsCancelled);
			}
		}

		void SetupData()
		{
			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 50, 0M, 50, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 50m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";
			batch.ACB_Type = "STD";

			order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_OH_Debtor = (new TestObjectCreator(Factory)).AALSHI.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;

			orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = invoice1.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;

			Factory.Save();
		}

		AccCollectionBatch batch;
		AccCollectionOrder order;
		AccCollectionOrderLine orderline;

		public void TestConstruction_PlugIns()
		{
			SetupData();
			using (var form = new AccCollectionOrderForm(order))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		public void TestControllerID()
		{
			SetupData();
			using (var form = new AccCollectionOrderForm(order))
			{
				AssertEquals(ControllerIDs.AccCollectionOrder, form.ControllerID);
			}
		}

		public void TestFormVerb()
		{
			SetupData();
			using (var form = new AccCollectionOrderForm(order))
			{
				order.IsCancelled = false;
				AssertEquals(FormVerbs_ForTestOnly.Edit, form.FormVerb);

				order.IsCancelled = true;
				AssertEquals(FormVerbs_ForTestOnly.View, form.FormVerb);
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}
}
