using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusPartShipCargoReportHeaderTest : CTOCusHAWBCargoReportHeaderTest
	{
		public override void TestFlightNo()
		{
			PartShip.CG_FlightNo = "QF222";
			AssertEquals("FlightNo", "QF222", Header.FlightNo);
		}

		public override void TestArivalDate()
		{
			PartShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
			AssertEquals("ArivalDate", new ZDateTime(2005, 7, 4), Header.ArivalDate);
		}

		public override void TestLoading()
		{
			PartShip.CG_RL_NKLoadPort = "HKHKG";
			AssertEquals("Loading", "HKHKG", Header.Loading);
		}

		public override void TestDischarge()
		{
			PartShip.CG_RL_NKDischargePort = "AUNTL";
			AssertEquals("Loading", "AUNTL", Header.Discharge);
		}

		public override void TestPackageCount()
		{
			PartShip.CG_PiecesLanded = 123;
			AssertEquals("PackageCount", 123, Header.PackageCount);
		}

		public override void TestRoutings()
		{
			AssertNull(Header.Routings);
		}

		public override void TestIsHVLVSpecialReporter()
		{
			Assert(!Header.IsHVLVSpecialReporter);
		}

		public override void TestIsRemailSpecialReporter()
		{
			Assert(!Header.IsRemailSpecialReporter);
		}

		IAirCargoReportHeader fHeader;
		protected override IAirCargoReportHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = new CTOCusPartShipCargoReportHeader(PartShip, (CTOCusHAWB)HAWB);
				}
				return fHeader;
			}
		}

		CusPartShip fPartShip;
		CusPartShip PartShip
		{
			get
			{
				if (fPartShip == null)
				{
					fPartShip = HAWB.PartShips.AddNew();
				}
				return fPartShip;
			}
		}
	}
}
