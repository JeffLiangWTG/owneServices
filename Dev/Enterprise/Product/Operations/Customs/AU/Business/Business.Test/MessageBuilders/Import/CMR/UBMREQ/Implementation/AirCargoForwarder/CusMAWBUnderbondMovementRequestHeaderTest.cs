using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusMAWBUnderbondMovementRequestHeaderTest : CusUnderbondUnderbondMovementRequestHeaderAbstractTest
	{
		public virtual void TestFlightNumber()
		{
			MAWB.CM_FlightNo = "QF123";
			AssertEquals("AirlineCode", "QF123", RequestHeader.FlightNumber);
		}

		public virtual void TestEstimatedDateOfArrival()
		{
			MAWB.CM_ArrivalDate = new ZDateTime(2005, 6, 2);
			AssertEquals("EstimatedDateOfArrival", new ZDateTime(2005, 6, 2), RequestHeader.EstimatedDateOfArrival);
		}

		public void TestVoyageNumber()
		{
			AssertEquals("VoyageNumber", ZString.Empty, RequestHeader.VoyageNumber);
		}

		public void TestVesselID()
		{
			AssertEquals("VesselID", ZString.Empty, RequestHeader.VesselID);
		}

		public virtual void TestTranshipmentOverseasDestinationPort()
		{
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			Underbond.C4_RL_NKTranshipDestPort = "CNSHA";
			AssertEquals("TranshipmentOverseasDestinationPort", "CNSHA", RequestHeader.TranshipmentOverseasDestinationPort);
		}

		public virtual void TestLine()
		{
			AssertEquals("Line", typeof(CusMAWBUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CusMAWBUnderbondMovementRequestHeader(Underbond, MAWB);

		CusMAWBBase mawb;
		protected virtual CusMAWBBase MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());
	}
}
