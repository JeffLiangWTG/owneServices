namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerOutturnReportHeaderInformationTest : CusSCAOceanBillOutturnReportHeaderInformationAbstractTest
	{
		public void TestLines()
		{
			var house = OceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = Container.PK;
			var outturn1 = Underbond.Outturns.AddNew();
			var outturn2 = Underbond.Outturns.AddNew();
			outturn1.Parent = Container;
			outturn2.Parent = pivot;

			int linesGenerated = 0;
			foreach (var line in ((CusSCAContainerOutturnReportHeaderInformation)GetHeaderInfo()).Lines)
			{
				linesGenerated++;
				if (linesGenerated == 1)
				{
					AssertEquals("Type1", typeof(CusSCAContainerOutturnReportLineInformation), line.GetType());
				}
				else
				{
					AssertEquals("Type2", typeof(CusSCAPivotOutturnReportLineInformation), line.GetType());
				}
			}

			AssertEquals("Two lines should be found", 2, linesGenerated);
		}

		protected override CusUnderbondOutturnReportHeaderInformation GetHeaderInfo() => new CusSCAContainerOutturnReportHeaderInformation(Container, Underbond);

		CusSCAContainer container;
		CusSCAContainer Container => container ?? (container = OceanBill.Containers.AddNew());
	}
}
