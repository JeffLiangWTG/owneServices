using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusPartShipAirCargoReportHeaderTest : CusHAWBAirCargoReportHeaderTest
	{
		public override void TestFlightNo()
		{
			HAWB.MAWB.CM_FlightNo = "";
			PartShip.CG_FlightNo = "ABC";
			AssertEquals("FlightNo ", "ABC", Header.FlightNo);
		}

		public override void TestArivalDate()
		{
			HAWB.MAWB.CM_ArrivalDate = ZDateTime.Empty;
			PartShip.CG_ArrivalDate = new ZDateTime(2005, 1, 1);
			AssertEquals("ArivalDate ", new ZDateTime(2005, 1, 1), Header.ArivalDate);
		}

		public override void TestLoading()
		{
			HAWB.MAWB.CM_RL_NKLoadPort = "";
			PartShip.CG_RL_NKLoadPort = "ABC";
			AssertEquals("Loading", "ABC", Header.Loading);
		}

		public override void TestDischarge()
		{
			HAWB.MAWB.CM_RL_NKDischargePort = "";
			PartShip.CG_RL_NKDischargePort = "ABC";
			AssertEquals("Discharge", "ABC", Header.Discharge);
		}

		public override void TestRoutings()
		{
			HAWB.MAWB.CM_RL_NKRoutePort1 = "SGSIN";
			HAWB.MAWB.CM_RL_NKRoutePort2 = "AUMEL";
			AssertEquals("Routings", 2, Header.Routings.Length);
			AssertCollectionContains("SGSIN", Header.Routings);
			AssertCollectionContains("AUMEL", Header.Routings);
		}

		IAirCargoReportHeader header;
		protected override IAirCargoReportHeader Header => header ?? (header = new CusPartShipAirCargoReportHeader(PartShip));

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = ((CusHAWB)HAWB).PartShips.AddNew());
	}
}
