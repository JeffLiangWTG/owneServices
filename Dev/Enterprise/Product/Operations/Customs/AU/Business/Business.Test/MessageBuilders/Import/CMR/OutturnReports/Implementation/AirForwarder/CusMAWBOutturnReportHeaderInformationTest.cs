namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBOutturnReportHeaderInformationTest : CusMAWBBaseOutturnReportHeaderInformationAbstractTest
	{
		public void TestLines()
		{
			Underbond.LinkedObject = MAWB;
			var hAWB = MAWB.ChildBills.AddNew();
			var outturn1 = Underbond.Outturns.AddNew();
			outturn1.Parent = MAWB;
			var outturn2 = Underbond.Outturns.AddNew();
			outturn2.Parent = hAWB;
			AssertEquals("Length", 2, HeaderInfo.Lines.Length);
		}

		public void TestDatebaseLines()
		{
			Underbond.LinkedObject = MAWB;
			var hAWB = MAWB.ChildBills.AddNew();
			var outturn = Underbond.Outturns.AddNew();
			outturn.Parent = hAWB;
			AssertEquals("Length", 0, HeaderInfo.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("Length", 1, HeaderInfo.DatabaseLines.Length);
		}

		protected override CusMAWBBase GetCusMAWB() => Factory.New<CusMAWB>();

		protected override CusUnderbondOutturnReportHeaderInformation GetHeaderInfo() => new CusMAWBOutturnReportHeaderInformation(MAWB, Underbond);

		new CusMAWB MAWB => (CusMAWB)base.MAWB;

		CusMAWBOutturnReportHeaderInformation HeaderInfo => (CusMAWBOutturnReportHeaderInformation)GetHeaderInfo();
	}
}
