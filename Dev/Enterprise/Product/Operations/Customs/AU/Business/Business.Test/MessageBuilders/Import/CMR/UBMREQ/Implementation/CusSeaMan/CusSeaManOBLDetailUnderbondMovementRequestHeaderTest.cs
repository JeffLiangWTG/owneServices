using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLDetailUnderbondMovementRequestHeaderTest : CusUnderbondUnderbondMovementRequestHeaderAbstractTest
	{
		public void TestTranshipmentPort()
		{
			Underbond.C4_MovementReason = "TSH";
			Detail.Header.BO_RL_NKDestinationPort = "NZAKL";
			AssertEquals("Transhipment port when reason TSH and destination port of NZAKL", "NZAKL", RequestHeader.TranshipmentOverseasDestinationPort);

			Underbond.C4_MovementReason = "MOV";
			AssertEquals("Transhipment port when reason not TSH and destination port of NZAKL", ZString.Empty, RequestHeader.TranshipmentOverseasDestinationPort);

			Detail.Header.BO_RL_NKDestinationPort = "AUSYD";
			AssertEquals("Transhipment port when reason not TSH and destination port of AUSYD", ZString.Empty, RequestHeader.TranshipmentOverseasDestinationPort);

			Underbond.C4_MovementReason = "TSH";
			AssertEquals("Transhipment port when reason TSH and destination port of AUSYD", ZString.Empty, RequestHeader.TranshipmentOverseasDestinationPort);
		}

		public void TestEstimatedDateOfArrival()
		{
			AssertEquals("EstimatedDateOfArrival", ZDateTime.Empty, RequestHeader.EstimatedDateOfArrival);
		}

		public void TestFlightNumber()
		{
			AssertEquals("FlightNumber", ZString.Empty, RequestHeader.FlightNumber);
		}

		public void TestVesselID()
		{
			Detail.Header.TransportHeader.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("VesselID", "8811924", RequestHeader.VesselID);
		}

		public void TestVoyageNumber()
		{
			Detail.Header.TransportHeader.BT_VoyageNum = "123";
			AssertEquals("VoyageNumber", "123", RequestHeader.VoyageNumber);
		}

		public void TestLine()
		{
			AssertEquals("Line", typeof(CusSeaManOBLDetailUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CusSeaManOBLDetailUnderbondMovementRequestHeader(Underbond, Detail);

		CusSeaManOBLDetail detail;
		CusSeaManOBLDetail Detail
		{
			get
			{
				if (detail == null)
				{
					var transportHeader = Factory.New<CusSeaManTranHead>();
					detail = transportHeader.OceanBills.AddNew().Details.AddNew();
				}
				return detail;
			}
		}
	}
}
