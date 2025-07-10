using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOutturnStandAloneReportLine : CusOutturnOutturnReportLineInformation, IAirOutturnReportLineInformation
	{
		public AirOutturnStandAloneReportLine(CusOutturn outturn)
			: base(outturn)
		{
		}

		#region IAirOutturnReportLineInformation Members

		ZString IAirOutturnReportLineInformation.HouseAirWaybillNumber
		{
			get { return Outturn.C5_HouseBill; }
		}

		ZString IAirOutturnReportLineInformation.MasterAirWaybillNumber
		{
			get { return Outturn.Underbond.C4_MAWB; }
		}

		#endregion
	}
}
