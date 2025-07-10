using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccPayableOrderController))]
	class AccPayableOrderControllerBasherTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccPayableOrderHeader orderBizo = Factory.New<AccPayableOrderHeader>();
			orderBizo.APH_OrderNumber = "Zubstest";
			orderBizo.APH_OA_Buyer = Factory.LoadTop1(typeof(OrgAddress), new ZQuery()).PK;
			Factory.Save();
			return orderBizo;
		}

		public void TestNewOrderSplitHasSaveEnabled()
		{
			// TO DO
			Assert(true);
		}

		public void TestNewSplitUsesDifferentFactory()
		{
			// TO DO
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccPayableOrder;
		}
	}
}
