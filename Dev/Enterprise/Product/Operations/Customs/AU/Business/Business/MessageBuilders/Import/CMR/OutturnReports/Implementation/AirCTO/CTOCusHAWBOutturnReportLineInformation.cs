using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBOutturnReportLineInformation : CusOutturnOutturnReportLineInformation, IAirOutturnReportLineInformation
	{
		public CTOCusHAWBOutturnReportLineInformation(CTOCusHAWB hAWB, CusOutturn outturn)
			: base(outturn)
		{
			this.hAWB = hAWB;
		}

		public ZString HouseAirWaybillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString MasterAirWaybillNumber
		{
			get { return hAWB.CS_HAWB; }
		}

		#region Implementation

		readonly CTOCusHAWB hAWB;

		#endregion
	}
}
