namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotOutturnReportHeaderInformationTest : CusSCAOceanBillOutturnReportHeaderInformationAbstractTest
	{
		public void TestLines()
		{
			var outturn = Underbond.Outturns.AddNew();
			outturn.Parent = Pivot;
			int linesGenerated = 0;
			foreach (var line in ((CusSCAPivotOutturnReportHeaderInformation)GetHeaderInfo()).Lines)
			{
				linesGenerated++;
				AssertEquals("Type2", typeof(CusSCAPivotOutturnReportLineInformation), line.GetType());
			}

			AssertEquals("One line should have been found", 1, linesGenerated);
		}

		protected override CusUnderbondOutturnReportHeaderInformation GetHeaderInfo() => new CusSCAPivotOutturnReportHeaderInformation(Pivot, Underbond);

		CusSCAPivot pivot;
		CusSCAPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var house = OceanBill.HouseBills.AddNew();
					var container = OceanBill.Containers.AddNew();
					pivot = house.Pivot.AddNew();
					pivot.CV_CN = container.PK;
				}
				return pivot;
			}
		}
	}
}
