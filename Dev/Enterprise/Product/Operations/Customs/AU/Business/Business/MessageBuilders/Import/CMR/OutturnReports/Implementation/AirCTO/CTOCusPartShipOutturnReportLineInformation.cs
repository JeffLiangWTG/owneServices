namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusPartShipOutturnReportLineInformation : CTOCusHAWBOutturnReportLineInformation
	{
		public CTOCusPartShipOutturnReportLineInformation(CusPartShip partShip, CTOCusHAWB hAWB, CusOutturn outturn)
			: base(hAWB, outturn)
		{
		}
	}
}
