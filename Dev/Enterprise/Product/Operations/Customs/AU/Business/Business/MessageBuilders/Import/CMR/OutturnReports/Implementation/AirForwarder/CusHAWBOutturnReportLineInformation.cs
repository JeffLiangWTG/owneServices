using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBOutturnReportLineInformation : CusMAWBOutturnReportLineInformation
	{
		public CusHAWBOutturnReportLineInformation(CusHAWB hAWB, CusOutturn outturn)
			: base(hAWB.MAWB, outturn)
		{
			this.hAWB = hAWB;
		}

		protected override ZString GetHouseAirWaybillNumber()
		{
			return hAWB == null ? ZString.Empty : hAWB.CS_HAWB;
		}

		readonly CusHAWB hAWB;
	}
}
