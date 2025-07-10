using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusPartShipUnderbondMovementRequestHeaderTest : CTOCusHAWBUnderbondMovementRequestHeaderTest
	{
		public override void TestFlightNumber()
		{
			PartShip.CG_FlightNo = "AA123";
			AssertEquals("FlightNumber", "AA123", RequestHeader.FlightNumber);
		}

		public override void TestEstimatedDateOfArrival()
		{
			PartShip.CG_ArrivalDate = new ZDateTime(2005, 7, 8);
			AssertEquals("EstimatedDateOfArrival", new ZDateTime(2005, 7, 8), RequestHeader.EstimatedDateOfArrival);
		}

		public override void TestLine()
		{
			AssertEquals("LineType", typeof(CTOCusPartShipUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CTOCusPartShipUnderbondMovementRequestHeader(Underbond, HAWB, PartShip);

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = HAWB.PartShips.AddNew());
	}
}
