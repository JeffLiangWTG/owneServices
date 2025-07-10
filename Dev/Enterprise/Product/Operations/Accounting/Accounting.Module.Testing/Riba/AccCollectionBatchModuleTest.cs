using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
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
	[TestedType(typeof(AccCollectionBatchModule))]
	public class AccCollectionBatchModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccCollectionBatch;
		}

		public void TestLicenseCheckpoints()
		{
			using (var module = new AccCollectionBatchModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				AssertEquals(Env.Security.CollectionBatch, module.SecurityCheckpoint);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccCollectionBatchFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				IBusinessObjectCollection collectionBatchsCollection = module.NewGridCollection;
				Assert("Invalid type", collectionBatchsCollection is AccCollectionBatchCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccCollectionBatchFilterBusinessObject);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (var module = new AccCollectionBatchModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Cancel Batch"));
			}
		}

		public void TestHandleCreateReceiptsAndOneDepositBatch()
		{
			SetUpBatch();

			using (var module = new AccCollectionBatchModule())
			{
				MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
				AssertNotNull("There should be a 'create receipt' menu item", actionMenuItems.FindByText("Create Receipts and Deposit Batch"));

				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var filterBO = (AccCollectionBatchFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();

					var testCollection = module.GetNewGridCollection_ForTestOnly();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Please select a batch first", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					AssertEquals("Grid_ForTestOnlyCollection Should have 1 batch selected", 1, module.DisplayGrid.SelectedElements.Length);

					Env.Security.CollectionBatchCreateReceipt.IsAllowed = false;
					module.HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(this, new EventArgs());
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.CollectionBatchCreateReceipt.IsAllowed = true;
					module.HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Receipts and Deposit Batch are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCreateReceiptsAndIndividualDepositBatch()
		{
			SetUpBatch();

			using (var module = new AccCollectionBatchModule())
			{
				MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
				AssertNotNull("There should be a 'create receipt and individual deposit batch' menu item", actionMenuItems.FindByText("Create Receipts and Individual Deposit Batch"));

				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var filterBO = (AccCollectionBatchFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();

					var testCollection = module.GetNewGridCollection_ForTestOnly();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Please select a batch first", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					AssertEquals("Grid_ForTestOnlyCollection Should have 1 batch selected", 1, module.DisplayGrid.SelectedElements.Length);

					Env.Security.CollectionBatchCreateReceipt.IsAllowed = false;
					module.HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(this, new EventArgs());
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.CollectionBatchCreateReceipt.IsAllowed = true;
					module.HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(this, new EventArgs());
					AssertEquals("Receipts and Deposit Batches are created successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void SetUpBatch()
		{
			AccCollectionBatch batch;
			AccCollectionOrder order1;
			AccCollectionOrder order2;
			AccCollectionOrderLine line1_1;
			AccCollectionOrderLine line1_2;
			AccCollectionOrderLine line2_1;
			AccCollectionOrderLine line2_2;
			TestObjectCreator creator;
			AccBankAccount bankAccount;
			ARInvoice invoice1;
			ARInvoice invoice2;
			ARInvoice invoice3;
			ARInvoice invoice4;

			creator = new TestObjectCreator(Factory);
			batch = Factory.NewWithValidTestData<AccCollectionBatch>();

			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			//batch.ACB_AB = bankAccount.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_TotalAmount = 100m;

			invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice3 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice4 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", TestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			order1 = Factory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = ZDateTime.Today.Date;
			order1.ACO_IsCancelled = false;
			order1.ACO_OH_Debtor = creator.ABIGAS.PK;
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
			order2.ACO_CollectionDate = ZDateTime.Today.Date.AddDays(1);
			order2.ACO_IsCancelled = false;
			order2.ACO_OH_Debtor = creator.ABIGAS.PK;
			order2.ACO_OrderNumber = ZString.Empty;
			order2.ACO_Amount = 50m;

			line2_1 = Factory.New<AccCollectionOrderLine>();
			line2_1.AOL_ACO = order2.PK;
			line2_1.AOL_AH = invoice3.PK;
			line2_1.IsCancelled = false;
			line2_2 = Factory.New<AccCollectionOrderLine>();
			line2_2.AOL_ACO = order2.PK;
			line2_2.AOL_AH = invoice4.PK;
			line2_2.IsCancelled = false;

			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = factory.NewWithValidTestData<AccCollectionBatch>();
			bizo.ACB_GC = Env.CurrentCompany.PK;
			return bizo;
		}

		class AccCollectionBatchModuleForTest : AccCollectionBatchModule
		{
			public AccCollectionBatchModuleForTest()
			{ }

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
