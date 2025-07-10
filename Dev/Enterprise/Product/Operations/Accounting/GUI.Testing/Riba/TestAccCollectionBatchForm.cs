using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Riba.Testing
{
	[TestedType(typeof(AccCollectionBatchForm))]
	public class TestAccCollectionBatchForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var testBatch = Factory.New<AccCollectionBatch>();
			var result = new AccCollectionBatchForm(testBatch);
			result.ControllerID = ControllerIDs.AccCollectionBatch;
			return result;
		}

		public void TestConstruction_PlugIns()
		{
			SetupData();
			using (var form = new AccCollectionBatchForm(batch))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		public void TestControllerID()
		{
			SetupData();
			Factory.Save();
			using (var form = new AccCollectionBatchForm(batch))
			{
				AssertEquals(ControllerIDs.AccCollectionBatch, form.ControllerID);
			}
		}

		public void TestPrintContextMenu()
		{
			SetupData();
			Factory.Save();

			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);
				var menuItems = form.OrdersGrid.ContextMenu.MenuItems;
				Assert(menuItems.Contains(form.PrintMenuItem));
				AssertEquals(3, menuItems.IndexOf(form.PrintMenuItem));

				form.HandlePrint(null, null);
				AssertEquals("Please select at least one order first.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.CollectionOrderPrint.IsAllowed = true;
				form.OrdersGrid.Select(0);
				form.HandlePrint(null, null);
				AssertEquals("DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRejectOrderGridContextMenu()
		{
			SetupData();
			Factory.Save();
			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);
				var menuItems = form.OrdersGrid.ContextMenu.MenuItems;
				AssertNull("Should NOT contain delete menu item", menuItems.FindByText("Delete"));
				Assert("Should contain reject order menu item", menuItems.Contains(form.RejectOrderMenuItem));
				AssertEquals("reject order menu item index", 1, menuItems.IndexOf(form.RejectOrderMenuItem));

				form.HandleRejectOrder(null, null);
				AssertEquals("Please select one order first.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.OrdersGrid.SelectAllElements();
				form.HandleRejectOrder(null, null);
				AssertEquals("Please select one order first.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.OrdersGrid.UnSelectAll();

				batch.CollectionOrders[0].IsCancelled = true;
				Factory.Save();
				form.OrdersGrid.Select(0);
				form.HandleRejectOrder(null, null);
				AssertEquals("This order has already been rejected.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				batch.CollectionOrders[0].IsCancelled = false;
				Factory.Save();
				form.OrdersGrid.Select(0);
				form.HandleRejectOrder(null, null);
				AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				batch.CollectionOrders[0].IsCancelled = false;
				batch.CollectionOrders[0].ACO_DepositedDate = ZDate.Today;
				form.OrdersGrid.Select(0);
				form.HandleRejectOrder(null, null);
				AssertEquals("The collection order has been completed with a deposited date saved. Are you sure you want to reject this order?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestAddTransactionsToOrderGridContextMenu()
		{
			SetupData();
			Factory.Save();
			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);
				var menuItems = form.OrdersGrid.ContextMenu.MenuItems;
				Assert("Should contain Add Transactions To Order Menu Item", menuItems.Contains(form.AddTransactionsToOrderMenuItem));
				AssertEquals("Add Transactions To Order menu item index", 0, menuItems.IndexOf(form.AddTransactionsToOrderMenuItem));

				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals("Please select one order first.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.OrdersGrid.SelectAllElements();
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals("Please select one order first.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.OrdersGrid.UnSelectAll();

				batch.CollectionOrders[0].IsCancelled = true;
				Factory.Save();
				form.OrdersGrid.Select(0);
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals("You are not allowed to add new transactions to a rejected order.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				batch.CollectionOrders[0].IsCancelled = false;
				Factory.Save();
				form.OrdersGrid.Select(0);
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals(typeof(OrderTransactionsFilterHolder), ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				batch.CollectionOrders[0].IsCancelled = false;
				batch.CollectionOrders[0].ACO_DepositedDate = ZDate.Today;
				form.OrdersGrid.Select(0);
				form.HandleAddTransactionsToOrder(null, null);
				AssertEquals("The collection order has been completed with a deposited date saved. No further changes are allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateReceiptGridContextMenu()
		{
			SetupData();
			Factory.Save();
			batch.CollectionOrders.ApplySort(new InstantiationTimeComparer());
			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);
				var menuItems = form.OrdersGrid.ContextMenu.MenuItems;
				Assert("Should contain Create Receipt Menu Item", menuItems.Contains(form.CreateReceiptMenuItem));
				AssertEquals("Create Receipt menu item index", 2, menuItems.IndexOf(form.CreateReceiptMenuItem));

				Env.Security.CollectionOrderCreateReceipt.IsAllowed = false;
				form.HandleCreateReceipts(null, null);
				AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CollectionOrderCreateReceipt.IsAllowed = true;

				form.HandleCreateReceipts(null, null);
				AssertEquals("Please select at least one order first.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.OrdersGrid.Select(0);
				order1.IsCancelled = true;
				form.HandleCreateReceipts(null, null);
				AssertEquals("expect save first error message", "Please save this batch before trying to create receipts and deposit batch.", UnitTestUserNotification.Instance.LastMessage.Text);

				order1.IsCancelled = false;
				orderline1.IsCancelled = true;
				Factory.Save();
				form.OrdersGrid.Select(0);
				form.HandleCreateReceipts(null, null);
				AssertEquals("expect fail message", @"Order 000001 is already canceled or fully paid.", UnitTestUserNotification.Instance.LastMessage.Text);

				orderline1.IsCancelled = false;
				Factory.Save();
				form.OrdersGrid.SelectAllElements();
				form.HandleCreateReceipts(null, null);
				AssertEquals("expect successful message", "Receipts and Deposit Batch are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Expect order line1 paid", orderline1.CollectionAmount, 0m);
				AssertEquals("Expect order line2 paid", orderline2.CollectionAmount, 0m);
			}
		}

		public void TestValidateAndSave_WhenGetCollectionBatchFileGeneratorProviderReturnsNull_ContinueWithSaveYes()
		{
			SetupData();
			Assert("batch is not in Database", !batch.IsInDatabase);

			SetupMocksForValidateAndSave();
			accountingDependencyFactoryMock.Setup(x => x.GetCollectionBatchFileGeneratorProvider(batch, Env.Instance)).Returns((ICollectionBatchFileGeneratorProvider)null);

			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var actualResult = form.ValidateAndSave_ForTestOnly();
				collectionBatchValidationMock.Verify(x => x.GetPreSaveValidationMessage(), Times.Never);
				AssertEquals("ValidateAndSave_ForTestOnly", ContinueWithSave.Yes, actualResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("batch is saved in Database", batch.IsInDatabase);
			}
		}

		public void TestValidateAndSave_WhenGetValidationReturnsNull_ContinueWithSaveYes()
		{
			SetupData();
			Assert("batch is not in Database", !batch.IsInDatabase);

			SetupMocksForValidateAndSave();
			accountingDependencyFactoryMock.Setup(x => x.GetCollectionBatchFileGeneratorProvider(batch, Env.Instance)).Returns(collectionBatchFileGeneratorProviderMock.Object);
			collectionBatchFileGeneratorProviderMock.Setup(x => x.GetValidation()).Returns((ICollectionBatchValidation)null);

			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var actualResult = form.ValidateAndSave_ForTestOnly();
				collectionBatchValidationMock.Verify(x => x.GetPreSaveValidationMessage(), Times.Never);
				AssertEquals("ValidateAndSave_ForTestOnly", ContinueWithSave.Yes, actualResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("batch is saved in Database", batch.IsInDatabase);
			}
		}

		public void TestValidateAndSave_WhenGetPreSaveValidationMessageReturnsEmpty_ContinueWithSaveYes()
		{
			SetupData();
			Assert("batch is not in Database", !batch.IsInDatabase);

			SetupMocksForValidateAndSave();
			accountingDependencyFactoryMock.Setup(x => x.GetCollectionBatchFileGeneratorProvider(batch, Env.Instance)).Returns(collectionBatchFileGeneratorProviderMock.Object);
			collectionBatchFileGeneratorProviderMock.Setup(x => x.GetValidation()).Returns(collectionBatchValidationMock.Object);
			collectionBatchValidationMock.Setup(x => x.GetPreSaveValidationMessage()).Returns(ZString.Empty);

			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var actualResult = form.ValidateAndSave_ForTestOnly();
				collectionBatchValidationMock.Verify(x => x.GetPreSaveValidationMessage(), Times.Once);
				AssertEquals("ValidateAndSave_ForTestOnly", ContinueWithSave.Yes, actualResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("batch is saved in Database", batch.IsInDatabase);
			}
		}

		public void TestValidateAndSave_WhenGetPreSaveValidationMessageReturnsString_ContinueWithSaveNo()
		{
			SetupData();
			Assert("batch is not in Database", !batch.IsInDatabase);

			SetupMocksForValidateAndSave();
			accountingDependencyFactoryMock.Setup(x => x.GetCollectionBatchFileGeneratorProvider(batch, Env.Instance)).Returns(collectionBatchFileGeneratorProviderMock.Object);
			collectionBatchFileGeneratorProviderMock.Setup(x => x.GetValidation()).Returns(collectionBatchValidationMock.Object);
			collectionBatchValidationMock.Setup(x => x.GetPreSaveValidationMessage()).Returns("Test PreSaveValidation Error Message");

			using (var form = new AccCollectionBatchForm(batch))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				form.OnShown_ForTestOnly(null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var actualResult = form.ValidateAndSave_ForTestOnly();
				collectionBatchValidationMock.Verify(x => x.GetPreSaveValidationMessage(), Times.Once);
				AssertEquals("ValidateAndSave_ForTestOnly", ContinueWithSave.No, actualResult);
				AssertEquals("Test PreSaveValidation Error Message", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("batch is not saved in Database", !batch.IsInDatabase);
			}
		}

		public void TestFileReloadMenuItemPresence()
		{
			AssertReloadThisFormMenuPresence(ODisplayMode.New);
			AssertReloadThisFormMenuPresence(ODisplayMode.Browse);

			void AssertReloadThisFormMenuPresence(ODisplayMode displayMode)
			{
				using (var form = new AccCollectionBatchForm(Factory.New<AccCollectionBatch>()))
				{
					form.Show();
					form.DisplayMode = displayMode;
					form.OnShown_ForTestOnly(null);

					var menuItems = form.Menu.MenuItems["FileMenuItem"].MenuItems;

					if (displayMode == ODisplayMode.New)
					{
						AssertNull("Should NOT contain 'Reload this form' menu item", menuItems.FindByText("Reload this form"));
					}
					else
					{
						AssertNotNull("Should contain 'Reload this form' menu item", menuItems.FindByText("Reload this form"));
					}
				}
			}
		}

		void SetupMocksForValidateAndSave()
		{
			accountingDependencyFactoryMock = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			collectionBatchFileGeneratorProviderMock = new Mock<ICollectionBatchFileGeneratorProvider>();
			collectionBatchValidationMock = new Mock<ICollectionBatchValidation>();
			ObjectFactory.Substitute(accountingDependencyFactoryMock.Object);

			accountingDependencyFactoryMock.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);
		}

		void SetupData()
		{
			InvoicingBase invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 50, 0M, 50, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			InvoicingBase invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 50, 0M, 50, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 50m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";
			batch.ACB_Type = "STD";

			order1 = Factory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = ZDateTime.Today.Date;
			order1.ACO_OH_Debtor = (new TestObjectCreator(Factory)).AALSHI.PK;
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
			order2.ACO_OH_Debtor = (new TestObjectCreator(Factory)).AALSHI.PK;
			order2.ACO_OrderNumber = "000002";
			order2.IncludeInBatch = true;

			orderline2 = Factory.New<AccCollectionOrderLine>();
			orderline2.AOL_ACO = order2.PK;
			orderline2.AOL_AH = invoice2.PK;
			orderline2.AOL_IsCancelled = false;
			orderline2.IncludeInOrder = true;
		}

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccCollectionOrderLine orderline1;
		AccCollectionOrder order2;
		AccCollectionOrderLine orderline2;
		Mock<IAccountingDependencyFactory> accountingDependencyFactoryMock;
		Mock<ICollectionBatchFileGeneratorProvider> collectionBatchFileGeneratorProviderMock;
		Mock<ICollectionBatchValidation> collectionBatchValidationMock;

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}
}
