using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing.Riba
{
	public class TestCollectionOrderBatchHelper : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			fTestObjectCreator = new TestObjectCreator(Factory);

			invoice1 = (ARInvoice)fTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", fTestObjectCreator.AUD, 1M, 50, 0M, 50, 0M, fTestObjectCreator.ABIGAS, fTestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice2 = (ARInvoice)fTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", fTestObjectCreator.AUD, 1M, 50, 0M, 50, 0M, fTestObjectCreator.ABIGAS, fTestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			batch = Factory.New<AccCollectionBatch>();
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			order1 = Factory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = ZDateTime.Today.Date;
			order1.ACO_OH_Debtor = fTestObjectCreator.AALSHI.PK;
			order1.ACO_OrderNumber = "000001";
			order1.IncludeInBatch = true;

			orderline1 = Factory.New<AccCollectionOrderLine>();
			orderline1.AOL_ACO = order1.PK;
			orderline1.AOL_AH = invoice1.PK;
			orderline1.AOL_IsCancelled = false;
			orderline1.IncludeInOrder = true;

			order2 = Factory.New<AccCollectionOrder>();
			order2.ACO_ACB = batch.PK;
			order2.ACO_CollectionDate = ZDateTime.Today.Date;
			order2.ACO_OH_Debtor = fTestObjectCreator.AALSHI.PK;
			order2.ACO_OrderNumber = "000002";
			order2.IncludeInBatch = true;

			orderline2 = Factory.New<AccCollectionOrderLine>();
			orderline2.AOL_ACO = order2.PK;
			orderline2.AOL_AH = invoice2.PK;
			orderline2.AOL_IsCancelled = false;
			orderline2.IncludeInOrder = true;

			Factory.Save();
		}

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccCollectionOrderLine orderline1;
		InvoicingBase invoice1;
		AccCollectionOrder order2;
		AccCollectionOrderLine orderline2;
		InvoicingBase invoice2;

		public void TestAddTransactionsToOrder_IncludeInBatch_True()
		{
			AssertAddTransactionsToOrder(Env.Security.CollectionBatchEdit, true);
		}

		public void TestAddTransactionsToOrder_IncludeInBatch_False()
		{
			AssertAddTransactionsToOrder(Env.Security.CollectionBatchEdit, false);
		}

		void AssertAddTransactionsToOrder(SecurityCheckpoint securityCheckPoint, bool includeInBatch)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.AddTransactionsToOrder(order2, securityCheckPoint, includeInBatch);
			AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			securityCheckPoint.IsAllowed = true;

			order1.IsCancelled = true;
			CollectionOrderBatchHelper.AddTransactionsToOrder(order1, securityCheckPoint, includeInBatch);
			AssertEquals("You are not allowed to add new transactions to a rejected order.", UnitTestUserNotification.Instance.LastMessage.Text);
			order1.IsCancelled = false;

			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			invoice1.AH_OutstandingAmount = 0;
			CollectionOrderBatchHelper.AddTransactionsToOrder(order1, securityCheckPoint, includeInBatch);
			AssertEquals("This order has already been fully paid with a receipt.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			order2.IncludeInBatch = includeInBatch;
			order2.ACO_Amount = 60M; //simulate order amount is changed after add transactions to order
			CollectionOrderBatchHelper.AddTransactionsToOrder(order2, securityCheckPoint, includeInBatch);
			AssertEquals(typeof(OrderTransactionsFilterHolder), ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType());
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			if (includeInBatch)
			{
				AssertEquals("batch amount(110) = order1 amount(50) + order2 amount(60)", 110M, order2.CollectionBatch.ACB_TotalAmount);
			}
			else
			{
				AssertEquals("batch amount(50) = order1 amount(50)", 50M, order2.CollectionBatch.ACB_TotalAmount);
			}
		}

		public void TestRejectOrder()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var securityCheckPoint = Env.Security.CollectionOrderReject;

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.RejectOrder(order2, securityCheckPoint, false);
			AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			securityCheckPoint.IsAllowed = true;

			order1.IsCancelled = true;
			CollectionOrderBatchHelper.RejectOrder(order1, securityCheckPoint, false);
			AssertEquals("This order has already been rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
			order1.IsCancelled = false;

			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			invoice1.AH_OutstandingAmount = 0;
			CollectionOrderBatchHelper.RejectOrder(order1, securityCheckPoint, false);
			AssertEquals("This order has already been fully paid with a receipt.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CollectionOrderBatchHelper.RejectOrder(order2, securityCheckPoint, false);
			AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("This order has not been rejected.", !order2.IsCancelled);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			order2.IsCancelled = false;
			CollectionOrderBatchHelper.RejectOrder(order2, securityCheckPoint, true);
			AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			//TODO: Assert("This order has not been rejected.", order2.IsCancelled);
		}

		public void TestCreateReceipts()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var testCollection = new List<AccCollectionOrder>();
			var securityCheckPoint = Env.Security.CollectionOrderCreateReceipt;

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.CreateReceipts(testCollection, securityCheckPoint);
			AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			securityCheckPoint.IsAllowed = true;

			CollectionOrderBatchHelper.CreateReceipts(testCollection, securityCheckPoint);
			AssertEquals("Please select at least one order first.", UnitTestUserNotification.Instance.LastMessage.Text);

			testCollection.Add(order1);
			order1.IsCancelled = true;
			CollectionOrderBatchHelper.CreateReceipts(testCollection, securityCheckPoint);
			AssertEquals("Some orders have already been rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
			order1.IsCancelled = false;

			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			invoice1.AH_OutstandingAmount = 0;
			CollectionOrderBatchHelper.CreateReceipts(testCollection, securityCheckPoint);
			AssertEquals("Some order have already been fully paid with a receipt.", UnitTestUserNotification.Instance.LastMessage.Text);

			testCollection.Add(order2);
			invoice1.AH_FullyPaidDate = ZDateTime.Empty;
			invoice1.AH_OutstandingAmount = 50;
			invoice2.AH_OutstandingAmount = 50;
			CollectionOrderBatchHelper.CreateReceipts(testCollection, securityCheckPoint);
			AssertEquals("Receipts and Deposit Batch are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateReceiptsAndOneDepositBatch()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var securityCheckPoint = Env.Security.CollectionBatchCreateReceipt;

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.CreateReceiptsAndOneDepositBatch(batch, securityCheckPoint);
			AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			securityCheckPoint.IsAllowed = true;

			CollectionOrderBatchHelper.CreateReceiptsAndOneDepositBatch(batch, securityCheckPoint);
			AssertEquals("Receipts and Deposit Batch are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateReceiptsAndIndividualDepositBatch()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var securityCheckPoint = Env.Security.CollectionBatchCreateReceipt;

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.CreateReceiptsAndIndividualDepositBatch(batch, securityCheckPoint);
			AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			securityCheckPoint.IsAllowed = true;

			CollectionOrderBatchHelper.CreateReceiptsAndIndividualDepositBatch(batch, securityCheckPoint);
			AssertEquals("Receipts and Deposit Batches are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrintCollectionOrders()
		{
			var collectionOrders = new[] { order1, order2 };
			var securityCheckPoint = Env.Security.CollectionOrderPrint;

			securityCheckPoint.IsAllowed = false;
			CollectionOrderBatchHelper.PrintCollectionOrders(collectionOrders, securityCheckPoint);
			AssertEquals(securityCheckPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			securityCheckPoint.IsAllowed = true;
			CollectionOrderBatchHelper.PrintCollectionOrders(collectionOrders, securityCheckPoint);
			AssertEquals("DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, "Collection Advice");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "CollectionOrder");
			var menuItem = Factory.LoadTop1<StmMenuItem>(query);
			menuItem.SU_MenuName = "Collection Advice Error";

			AssertExceptionThrown<ReportException>(() => CollectionOrderBatchHelper.PrintCollectionOrders(collectionOrders, securityCheckPoint));
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		TestObjectCreator fTestObjectCreator;
	}
}