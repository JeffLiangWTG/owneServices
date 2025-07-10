using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccCollectionOrderModule))]
	public class AccCollectionOrderModuleTest : ZModuleBasherTest
	{
		public void TestHandlePrint()
		{
			SetUpBatch();

			using (var module = new AccCollectionOrderModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					var userNotif = UnitTestUserNotification.Instance;

					var menuItems = module.GetNewAdditionalMenuItems_ForTestOnly();
					var printMenuItem = menuItems.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);

					Env.Security.CollectionOrderPrint.IsAllowed = true;
					printMenuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", userNotif.LastMessage.Text);
					userNotif.ClearMessagesAndAnswers();

					var filterBO = (AccCollectionOrderFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();
					AssertEquals(2, module.GridCollection.Count);

					module.DisplayGrid.Select(0);
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					printMenuItem.PerformClick();
					AssertNullOrEmpty(userNotif.LastMessage.Text);
					userNotif.ClearMessagesAndAnswers();
					AssertEquals("DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
				}
			}
		}

		public void TestRejectOrder()
		{
			SetUpBatch();

			using (var module = new AccCollectionOrderModule())
			{
				var actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
				AssertNotNull("There should be a 'reject order' menu item", actionMenuItems.FindByText("Reject Order"));

				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					var userNotif = UnitTestUserNotification.Instance;

					var filterBO = (AccCollectionOrderFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();

					AssertEquals(2, module.GridCollection.Count);

					userNotif.ClearMessagesAndAnswers();
					module.HandleRejectOrder_ForTestOnly(this, new EventArgs());
					AssertEquals("Please select one order first.", userNotif.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					module.HandleRejectOrder_ForTestOnly(this, new EventArgs());
					AssertEquals("Please select one order first.", userNotif.LastMessage.Text);
					module.DisplayGrid.UnSelectAll();

					module.DisplayGrid.Select(0);
					Env.Security.CollectionOrderReject.IsAllowed = false;
					module.HandleRejectOrder_ForTestOnly(this, new EventArgs());
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", userNotif.LastMessage.Text);
					Env.Security.CollectionOrderReject.IsAllowed = true;

					userNotif.ClearMessagesAndAnswers();
					module.HandleRejectOrder_ForTestOnly(this, new EventArgs());
					AssertEquals(typeof(OrderRejectReasonForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertNull(userNotif.LastMessage.Text);
					//TODO: Assert("This order has not been rejected.", order1.IsCancelled);
				}
			}
		}

		public void TestCreateReceiptsAndIndividualDepositBatch()
		{
			SetUpBatch();

			using (var module = new AccCollectionOrderModule())
			{
				var actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
				AssertNotNull("There should be a 'create receipt and individual deposit batch' menu item", actionMenuItems.FindByText("Create Receipts and Individual Deposit Batch"));

				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					var userNotif = UnitTestUserNotification.Instance;

					var filterBO = (AccCollectionOrderFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();

					AssertEquals(2, module.GridCollection.Count);

					userNotif.ClearMessagesAndAnswers();
					module.HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Please select a record in the grid.", userNotif.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					Env.Security.CollectionOrderCreateReceipt.IsAllowed = false;
					module.HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(this, new EventArgs());
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", userNotif.LastMessage.Text);
					Env.Security.CollectionOrderCreateReceipt.IsAllowed = true;

					userNotif.ClearMessagesAndAnswers();
					module.HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Receipts and Deposit Batch are created successfully.", userNotif.LastMessage.Text);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.AccCollectionOrder;

		public void TestSecurityCheckpoint()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertEquals(Env.Security.CollectionOrder, module.SecurityCheckpoint);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestWorkflowType()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertEquals(WorkflowDescriptors.CollectionOrderCode, module.WorkflowType);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				Assert("Not Allow Delete", !module.AllowDelete);
			}
		}

		public void TestGetNewController()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertEquals(typeof(AccCollectionOrderController), module.GetNewController().GetType());
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			using (var filterControl = module.GetNewFilterControlExposedForTest())
			{
				AssertEquals("FilterControl type", typeof(AccCollectionOrderFilterControl), filterControl.GetType());
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				IBusinessObjectCollection collectionOrdersCollection = module.GetNewGridCollectionExposedForTest();
				AssertEquals("Grid Collection type", typeof(AccCollectionOrderCollection), collectionOrdersCollection.GetType());
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.GetNewFilterBusinessObjectExposedForTest();
				AssertEquals("FilterBusinessObject type", typeof(AccCollectionOrderFilterBusinessObject), filterBusinessObject.GetType());
			}
		}

		public void TestLicenseCheckpoints()
		{
			using (var module = new AccCollectionOrderModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestViewMenuItemText()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("View"));
			}
		}

		public void TestNewMenuItemText()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("New"));
			}
		}

		public void TestEditMenuItemText()
		{
			using (var module = new AccCollectionOrderModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Edit"));
			}
		}

		TestObjectCreator fTestObjectCreator;
		AccCollectionOrder order1;
		AccCollectionOrder order2;

		void SetUpBatch()
		{
			AccCollectionBatch batch;
			AccCollectionOrderLine line1_1;
			AccCollectionOrderLine line1_2;
			AccCollectionOrderLine orderline2;
			ARInvoice invoice1;
			ARInvoice invoice2;
			ARInvoice invoice3;

			fTestObjectCreator = new TestObjectCreator(Factory);
			batch = Factory.NewWithValidTestData<AccCollectionBatch>();

			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_TotalAmount = 100m;

			var now = ZDateTime.Now;
			invoice1 = (ARInvoice)fTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", fTestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, fTestObjectCreator.ABIGAS, fTestObjectCreator.CC1.PK,
				now, ZDateTime.Empty, now, false);
			invoice2 = (ARInvoice)fTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", fTestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, fTestObjectCreator.ABIGAS, fTestObjectCreator.CC1.PK,
				now, ZDateTime.Empty, now, false);
			invoice3 = (ARInvoice)fTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", fTestObjectCreator.AUD, 1M, 40, 0M, 40, 0M, fTestObjectCreator.AALSHI, fTestObjectCreator.CC1.PK,
				now, ZDateTime.Empty, now, false);

			order1 = Factory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = now.Date;
			order1.ACO_IsCancelled = false;
			order1.ACO_OH_Debtor = fTestObjectCreator.ABIGAS.PK;
			order1.ACO_OrderNumber = ZString.Empty;
			order1.ACO_Amount = 50m;

			line1_1 = Factory.New<AccCollectionOrderLine>();
			line1_1.AOL_ACO = order1.PK;
			line1_1.AOL_AH = invoice1.PK;
			line1_1.IsCancelled = false;
			line1_2 = Factory.New<AccCollectionOrderLine>();
			line1_2.AOL_ACO = order1.PK;
			line1_2.AOL_AH = invoice2.PK;
			line1_2.IsCancelled = false;

			order2 = Factory.New<AccCollectionOrder>();
			order2.ACO_ACB = batch.PK;
			order2.ACO_CollectionDate = now.Date;
			order2.ACO_OH_Debtor = fTestObjectCreator.AALSHI.PK;
			order2.ACO_OrderNumber = ZString.Empty;
			order2.ACO_Amount = 40m;

			orderline2 = Factory.New<AccCollectionOrderLine>();
			orderline2.AOL_ACO = order2.PK;
			orderline2.AOL_AH = invoice3.PK;
			orderline2.AOL_IsCancelled = false;

			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = factory.NewWithValidTestData<AccCollectionOrder>();
			bizo.CollectionBatch.ACB_GC = Env.CurrentCompany.PK;
			return bizo;
		}

		class AccCollectionOrderModuleForTest : AccCollectionOrderModule
		{
			public AccCollectionOrderModuleForTest()
			{ }

			public IFilterControl GetNewFilterControlExposedForTest()
			{
				return GetNewFilterControl();
			}

			public IBusinessObjectCollection GetNewGridCollectionExposedForTest()
			{
				return GetNewGridCollection();
			}

			public FilterBusinessObject GetNewFilterBusinessObjectExposedForTest()
			{
				return GetNewFilterBusinessObject();
			}
		}
	}
}
