using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccCollectionBatchController))]
	class AccCollectionBatchControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new AccCollectionBatchController();
			AssertEquals("ModuleID", ModuleIDs.AccCollectionBatch, controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(AccCollectionBatch);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccCollectionBatch;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 100m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDate.Today;
			TestObjectCreator creator = new TestObjectCreator(Factory);
			order.ACO_OH_Debtor = creator.AALSHI.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;
			order.ACO_Amount = 100m;

			var orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = invoice.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;
			Factory.Save();
			return batch;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("Collection batch can only be created from the receivable module", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
