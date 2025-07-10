using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirOutturnStandAloneReportLineTest : TestCaseWithFactory
	{
		public void TestHouseAirWaybillNumber()
		{
			Outturn.C5_HouseBill = "Cuckoo Squeaker";
			AssertEquals("Cuckoo Squeaker", ((IAirOutturnReportLineInformation)Line).HouseAirWaybillNumber);
		}

		public void TestGoodsDescription()
		{
			Outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			AssertEquals("Cuckoo Squeakers", ((IAirOutturnReportLineInformation)Line).GoodsDescription);
		}

		public void TestMasterAirWaybillNumber()
		{
			Outturn.Underbond.C4_MAWB = "Gibbiceps";
			AssertEquals("Gibbiceps", ((IAirOutturnReportLineInformation)Line).MasterAirWaybillNumber);
		}

		AirOutturnStandAloneReportLine line;
		AirOutturnStandAloneReportLine Line => line ?? (line = new AirOutturnStandAloneReportLine(Outturn));

		CusOutturn outturn;
		CusOutturn Outturn
		{
			get
			{
				if (outturn == null)
				{
					var underbond = Factory.New<CusUnderbond>();
					outturn = underbond.Outturns.AddNew();
				}
				return outturn;
			}
		}
	}
}
