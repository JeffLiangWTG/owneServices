using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotOutturnReportLineInformationTest : CusSCAContainerOutturnReportLineInformationTest
	{
		public override void TestHouseBillOfLading()
		{
			House.CA_HouseBill = "123";
			AssertEquals("HouseBillOfLading", "123", ReportInfo.HouseBillOfLading);
		}

		public override void TestOceanBillOfLading()
		{
			OceanBill.CB_OceanBill = "321";
			AssertEquals("OceanBillOfLading", "321", ReportInfo.OceanBillOfLading);
		}

		public override void TestImportCargoType()
		{
			Container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("ImportCargoType", Core.Constants.ContainerModes.LCL, ReportInfo.ImportCargoType);
		}

		public override void TestMarksAndNumbers()
		{
			House.Pivot[0].CV_MarksAndNumbers = "123";
			AssertEquals("MarksAndNumbers", "123", ReportInfo.MarksAndNumbers);
		}

		public override void TestGoodsDescription()
		{
			House.Pivot[0].CV_GoodsDescription = "321";
			AssertEquals("GoodsDescription", "321", ReportInfo.GoodsDescription);
		}

		public void TestOceanBillOfLadingIfMultiOceanUnpack()
		{
			OceanBill.CB_MultiOBLUnpack = true;
			OceanBill.CB_OceanBill = "111";
			House.CA_HouseBill = "222";
			AssertEquals("OceanBillOfLading", "222", ReportInfo.OceanBillOfLading);
		}

		public void TestHouseBillOfLadingIfMultiOceanUnpack()
		{
			OceanBill.CB_MultiOBLUnpack = true;
			OceanBill.CB_OceanBill = "111";
			House.CA_HouseBill = "222";
			AssertEquals("HouseBillOfLading", ZString.Empty, ReportInfo.HouseBillOfLading);
		}

		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo() => new CusSCAPivotOutturnReportLineInformation(House.Pivot[0], Outturn);

		CusSCAPivotOutturnReportLineInformation ReportInfo => (CusSCAPivotOutturnReportLineInformation)GetHeaderInfo();
	}
}
