using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CTOCusHAWBOutturnReportLineInformationTest : CusOutturnOutturnReportLineInformationAbstractTest
	{
		public void TestDescription()
		{
			HAWB.CS_GoodsDescription = "DESCRIPTION";
			Outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			AssertEquals("Goods Description", "Cuckoo Squeakers", HeaderInfo.GoodsDescription);
		}

		public void TestMasterAirWaybillNumber()
		{
			HAWB.CS_HAWB = "123";
			AssertEquals("MasterAirWaybillNumber", "123", HeaderInfo.MasterAirWaybillNumber);
		}

		public void TestHouseAirWaybillNumber()
		{
			AssertEquals("HouseAirWaybillNumber", ZString.Empty, HeaderInfo.HouseAirWaybillNumber);
		}

		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo() => new CTOCusHAWBOutturnReportLineInformation(HAWB, Outturn);

		CTOCusHAWBOutturnReportLineInformation HeaderInfo => (CTOCusHAWBOutturnReportLineInformation)GetHeaderInfo();

		CTOCusHAWB hawb;
		internal CTOCusHAWB HAWB
		{
			get
			{
				if (hawb == null)
				{
					var mawb = Factory.New<CTOCusMAWB>();
					hawb = mawb.ChildBills.AddNew();
				}
				return hawb;
			}
		}
	}
}
