namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBOutturnReportLineInformationTest : CusMAWBOutturnReportLineInformationTest
	{
		public override void TestDescription()
		{
			HAWB.CS_GoodsDescription = "456";
			Outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			AssertEquals("Description", "Cuckoo Squeakers", HeaderInfo.GoodsDescription);
		}

		public override void TestHouseAirWaybillNumber()
		{
			HAWB.CS_HAWB = "135";
			AssertEquals("HouseAirWaybillNumber", "135", HeaderInfo.HouseAirWaybillNumber);
		}

		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo() => new CusHAWBOutturnReportLineInformation(HAWB, Outturn);

		CusHAWBOutturnReportLineInformation HeaderInfo => (CusHAWBOutturnReportLineInformation)GetHeaderInfo();

		CusHAWB hawb;
		CusHAWB HAWB => hawb ?? (hawb = MAWB.ChildBills.AddNew());
	}
}
