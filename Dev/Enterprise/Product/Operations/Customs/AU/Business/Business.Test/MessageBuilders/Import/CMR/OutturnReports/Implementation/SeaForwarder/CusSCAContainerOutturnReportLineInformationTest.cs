namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSCAContainerOutturnReportLineInformationTest : SeaOutturnReportLineInformationAbstractTest
	{
		public void TestContainerNumber()
		{
			Container.CN_ContainerNumber = "123";
			AssertEquals("ContainerNumber", "123", ReportInfo.ContainerNumber);
		}

		public virtual void TestHouseBillOfLading()
		{
			AssertEquals("HouseBillOfLading", "", ReportInfo.HouseBillOfLading);
		}

		public virtual void TestOceanBillOfLading()
		{
			AssertEquals("OceanBillOfLading", "", ReportInfo.OceanBillOfLading);
		}

		public void TestSealNumber()
		{
			Container.CN_SealNumber = "321";
			AssertEquals("SealNumber", "321", ReportInfo.SealNumber);
		}

		public virtual void TestImportCargoType()
		{
			AssertEquals("ImportCargoType", Core.Constants.ContainerModes.FCL, ReportInfo.ImportCargoType);
		}

		public void TestPackageType()
		{
			House.Pivot[0].CV_PackageType = "BOX";
			AssertEquals("PackageType", "BOX", ReportInfo.PackageType);
		}

		public virtual void TestMarksAndNumbers()
		{
			AssertEquals("MarksAndNumbers", "", ReportInfo.MarksAndNumbers);
		}

		public virtual void TestGoodsDescription()
		{
			Outturn.C5_GoodsDescription = "Wifezilla";
			AssertEquals("GoodsDescription", "Wifezilla", ReportInfo.GoodsDescription);
		}

		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo() => new CusSCAContainerOutturnReportLineInformation(Container, Outturn);

		CusSCAContainerOutturnReportLineInformation ReportInfo => (CusSCAContainerOutturnReportLineInformation)GetHeaderInfo();

		CusSCAContainer container;
		protected CusSCAContainer Container => container ?? (container = OceanBill.Containers.AddNew());

		CusSCAHouse house;
		protected CusSCAHouse House
		{
			get
			{
				if (house == null)
				{
					house = OceanBill.HouseBills.AddNew();
					var pivot = house.Pivot.AddNew();
					pivot.CV_CN = Container.PK;
				}
				return house;
			}
		}

		CusSCAOceanBill oceanBill;
		protected CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());
	}
}
