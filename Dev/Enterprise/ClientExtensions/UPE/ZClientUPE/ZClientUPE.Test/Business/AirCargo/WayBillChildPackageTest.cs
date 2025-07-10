using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Testing;

namespace Enterprise.Client.UPE.Business.AirCargo.Testing
{
	internal class WayBillChildPackageTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestCreateJobRelatedWaybill()
		{
			Assert("No waybills", Factory.GetDatabaseCount(typeof(UPEJobRelatedWayBill)) == 0);

			TestHelper.Create2SplitShipmentGroups();

			Assert("4 waybills", Factory.GetDatabaseCount(typeof(UPEJobRelatedWayBill)) == 4);
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;
	}
}
