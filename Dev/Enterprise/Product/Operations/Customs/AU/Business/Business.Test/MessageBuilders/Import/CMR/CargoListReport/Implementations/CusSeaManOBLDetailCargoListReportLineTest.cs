using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLDetailCargoListReportLineTest : TestCaseWithFactory
	{
		public void TestCargoCode()
		{
			Header.BO_HeaderCargoType = "X";
			AssertEquals("CargoCode", "X", ReportLine.CargoCode);
		}

		public void TestCargoIdentifier()
		{
			Detail.BD_ContainerNumber = "123";
			AssertEquals("CargoIdentifier", "123", ReportLine.CargoIdentifier);
		}

		public void TestPortOfDestination()
		{
			Header.BO_RL_NKDestinationPort = "AUSYD";
			AssertEquals("PortOfDestination", "AUSYD", ReportLine.PortOfDestination);
		}

		public void TestPortOfLoading()
		{
			Header.BO_RL_NKLoadPort = "NZAKL";
			AssertEquals("PortOfLoading", "NZAKL", ReportLine.PortOfLoading);
		}

		public void TestImportCargoType()
		{
			Detail.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			AssertEquals("ImportCargoType", CMRCargoTypes.Codes.FullContainerLoad, ReportLine.ImportCargoType);
		}

		public void TestNumberOfPackages()
		{
			Detail.BD_NoOfPacks = 1;
			AssertEquals("NumberOfPackages", 1, ReportLine.NumberOfPackages);
		}

		public void TestPackageType()
		{
			Detail.BD_PackType = "BOX";
			AssertEquals("PackageType", "BOX", ReportLine.PackageType);
		}

		CusSeaManOBLDetailCargoListReportLine ReportLine => new CusSeaManOBLDetailCargoListReportLine(Detail);

		CusSeaManOBLHeaderCargoLine header;
		CusSeaManOBLHeaderCargoLine Header => header ?? (header = Factory.New<CusSeaManOBLHeaderCargoLine>());

		CusSeaManOBLDetailCargoLine detail;
		CusSeaManOBLDetailCargoLine Detail => detail ?? (detail = Header.Details.AddNew());
	}
}
