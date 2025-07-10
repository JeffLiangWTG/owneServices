using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class HouseCusPartShipUnderbondMovementRequestHeaderTest : CusHAWBUnderbondMovementRequestHeaderTest
	{
		public override void TestLine()
		{
			AssertEquals("LineType", typeof(HouseCusPartShipUnderbondMovementRequestLine), RequestHeader.Line.GetType());
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

		protected override IUnderbondMovementRequestHeader RequestHeader => new HouseCusPartShipUnderbondMovementRequestHeader(Underbond, HAWB, PartShip);

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = HAWB.PartShips.AddNew());
	}
}
