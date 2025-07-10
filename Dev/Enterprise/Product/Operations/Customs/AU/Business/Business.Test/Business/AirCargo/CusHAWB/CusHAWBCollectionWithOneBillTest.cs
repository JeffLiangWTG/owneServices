using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBCollectionWithOneBill))]
	sealed class CusHAWBCollectionWithOneBillTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadFromShipment()
		{
			var mawb = Factory.New<CusMAWB>();
			var houseBill1 = mawb.ChildBills.AddNew();
			houseBill1.CS_CM = masterBill.PK;
			houseBill1.CS_JS = shipment.PK;

			var houseBill2 = mawb.ChildBills.AddNew();
			var shipment2 = CommonShipment.New(Factory);
			houseBill2.CS_CM = masterBill.PK;
			houseBill2.CS_JS = shipment2.PK;

			var houseBills = new CusHAWBCollection(masterBill, Factory);
			houseBills.Load();
			AssertEquals("All House Bills for this MasterBill", 2, houseBills.Count);

			var currentBill = new CusHAWBCollectionWithOneBill(masterBill, Factory);
			currentBill.HouseBill = houseBill1;
			currentBill.Load();
			AssertEquals("Current HouseBill", 1, currentBill.Count);
			AssertEquals("Current HouseBill", houseBill1, currentBill[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusHAWBCollectionWithOneBill(Factory.New<CusMAWB>(), Factory);

		CommonShipment shipment;
		CusMAWB masterBill;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = CommonShipment.New(Factory);
			masterBill = Factory.New<CusMAWB>();
		}
	}
}
