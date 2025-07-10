using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccCollectionOrderController))]
	class AccCollectionOrderControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new AccCollectionOrderController();
			AssertEquals("ModuleID", ModuleIDs.AccCollectionOrder, controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(AccCollectionOrder);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccCollectionOrder;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("Collection order can only be created from the receivable module", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizo = Factory.NewWithValidTestData(GetBusinessObjectType()) as AccCollectionOrder;
			bizo.ACO_Amount = 1; //must be > 0 to correctly save into DB
			Factory.Save();
			return bizo;
		}

		public void TestIncludeInBatchOfAccCollectionOrderForm()
		{
			var order = Factory.NewWithValidTestData(GetBusinessObjectType()) as AccCollectionOrder;
			Assert("Not true by default", !order.IncludeInBatch);

			var controller = new AccCollectionOrderController();
			using (var orderForm = controller.GetForm_ForTestOnly(order))
			{
				Assert(order.IncludeInBatch);
			}
		}
	}
}
