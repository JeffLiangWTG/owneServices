using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusMAWBOutturnReportLineInformationTest : CusOutturnOutturnReportLineInformationAbstractTest
	{
		public virtual void TestDescription()
		{
			AssertEquals("Description", ZString.Empty, HeaderInfo.GoodsDescription);
		}

		public void TestOutturnGoodsDescription()
		{
			var hAWB = MAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "12345";
			hAWB.CS_GoodsDescription = "Cuckoo Squeakers";
			var underbond = MAWB.Underbonds.AddNew();
			underbond.C4_ParentID = MAWB.PK;
			MAWB.AllUnderbonds.Load();
			var outturn = underbond.Outturns.AddNew();
			outturn.C5_ParentID = hAWB.PK;
			outturn.C5_GoodsDescription = "Gibbiceps";
			var lineInfo = new CusMAWBOutturnReportLineInformation(MAWB, outturn);
			AssertEquals("Gibbiceps", lineInfo.GoodsDescription);
		}

		public void TestMasterAirWaybillNumber()
		{
			MAWB.CM_MAWB = "123";
			AssertEquals("MasterAirWaybillNumber", "123", HeaderInfo.MasterAirWaybillNumber);
		}

		public virtual void TestHouseAirWaybillNumber()
		{
			MAWB.CM_MasterHouseBill = "222";
			AssertEquals("HouseAirWaybillNumber", "222", HeaderInfo.HouseAirWaybillNumber);
		}

		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo() => new CusMAWBOutturnReportLineInformation(MAWB, Outturn);

		CusMAWBOutturnReportLineInformation HeaderInfo => (CusMAWBOutturnReportLineInformation)GetHeaderInfo();

		CusMAWB mawb;
		protected CusMAWB MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());
	}
}
