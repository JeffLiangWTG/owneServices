namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBOutturnReportHeaderInformationTest : CusMAWBBaseOutturnReportHeaderInformationAbstractTest
	{
		public void TestLines()
		{
			Underbond.LinkedObject = HAWB;
			CusOutturn outturn1 = Underbond.Outturns.AddNew();
			outturn1.Parent = HAWB;
			AssertEquals("Length", 1, HeaderInfo.Lines.Length);
		}

		public void TestDatebaseLines()
		{
			Underbond.LinkedObject = HAWB;
			CusOutturn outturn = Underbond.Outturns.AddNew();
			outturn.Parent = HAWB;
			AssertEquals("Length", 0, HeaderInfo.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("Length", 1, HeaderInfo.DatabaseLines.Length);
		}

		protected override CusMAWBBase GetCusMAWB() => Factory.New<CusMAWB>();

		protected override CusUnderbondOutturnReportHeaderInformation GetHeaderInfo() => new CusHAWBOutturnReportHeaderInformation(HAWB, Underbond);

		CusHAWB hawb;
		CusHAWB HAWB => hawb ?? (hawb = ((CusMAWB)MAWB).ChildBills.AddNew());

		CusHAWBOutturnReportHeaderInformation HeaderInfo => (CusHAWBOutturnReportHeaderInformation)GetHeaderInfo();
	}
}
