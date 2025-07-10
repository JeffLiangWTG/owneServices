using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusPartShipCollection))]
	sealed class CusPartShipCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPartShipToSend()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			CusPartShip partShip1 = houseBill.PartShips.AddNew();
			partShip1.CG_CustomsStatus = "H600";

			CusPartShip partShip2 = houseBill.PartShips.AddNew();
			partShip2.CG_CustomsStatus = AirCargoMessage.NewStatus.NotSent;

			AssertEquals("Next Part Shipment to send", partShip2.PK, houseBill.PartShips.PartShipToSend.PK);
		}

		public void TestLastFlightDetails()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_ArrivalDate = ZDateTime.Today;

			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_PiecesManifested = 10;

			AssertEquals("Last Flight No is a master flight no as there is no part shipment", "QF2", houseBill.PartShips.LastFlightNo);
			AssertEquals("Last Arrival Date is a master arrival date as there is no part shipment", ZDateTime.Today, houseBill.PartShips.LastArrivalDate);
		}

		public void TestGetPrevSuccPartShip()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			Assert("No PrevSuccPartShip", houseBill.PartShips.PrevSuccPartShip == null);

			CusPartShip partShip1 = houseBill.PartShips.AddNew();
			partShip1.CG_MessageReference = "00001";
			partShip1.CG_CustomsStatus = "H600";
			AssertEquals("PrevSuccPartShip", partShip1, houseBill.PartShips.PrevSuccPartShip);

			CusPartShip partShip2 = houseBill.PartShips.AddNew();
			partShip2.CG_MessageReference = "00002";
			partShip2.CG_CustomsStatus = "REJ";
			AssertEquals("PrevSuccPartShip", partShip1, houseBill.PartShips.PrevSuccPartShip);

			CusPartShip partShip3 = houseBill.PartShips.AddNew();
			partShip3.CG_MessageReference = "00003";
			partShip3.CG_CustomsStatus = "NOT";
			AssertEquals("PrevSuccPartShip", partShip1, houseBill.PartShips.PrevSuccPartShip);

			partShip3.CG_CustomsStatus = "H600";
			AssertEquals("PrevSuccPartShip", partShip3, houseBill.PartShips.PrevSuccPartShip);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var mawb = Factory.New<CusMAWB>();
			var houseBill = mawb.ChildBills.AddNew();
			return new CusPartShipCollection(houseBill, Factory);
		}
	}
}
