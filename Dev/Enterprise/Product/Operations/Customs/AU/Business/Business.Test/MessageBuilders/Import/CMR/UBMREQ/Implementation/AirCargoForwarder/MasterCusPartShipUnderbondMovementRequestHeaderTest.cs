using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MasterCusPartShipUnderbondMovementRequestHeaderTest : CusMAWBUnderbondMovementRequestHeaderTest
	{
		public override void TestLine()
		{
			AssertEquals("LineType", typeof(MasterCusPartShipUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		public override void TestFlightNumber()
		{
			PartShip.CG_FlightNo = "QF999";
			AssertEquals("FlightNumber", "QF999", RequestHeader.FlightNumber);
		}

		public override void TestEstimatedDateOfArrival()
		{
			PartShip.CG_ArrivalDate = new ZDateTime(2005, 8, 10);
			AssertEquals("FlightNumber", new ZDateTime(2005, 8, 10), RequestHeader.EstimatedDateOfArrival);
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new MasterCusPartShipUnderbondMovementRequestHeader(Underbond, (CusMAWB)MAWB, PartShip);

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = MAWB.PartShips.AddNew());
	}
}
