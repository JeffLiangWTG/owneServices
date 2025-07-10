namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusPartShipOutturnReportLineInformationTest : CTOCusHAWBOutturnReportLineInformationTest
	{
		protected override CusOutturnOutturnReportLineInformation GetHeaderInfo()
			=> new CTOCusPartShipOutturnReportLineInformation(HAWB.PartShips.AddNew(), HAWB, Outturn);
	}
}
