using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBOutturnReportLineInformation : CusOutturnOutturnReportLineInformation, IAirOutturnReportLineInformation
	{
		public CusMAWBOutturnReportLineInformation(CusMAWB mAWB, CusOutturn outturn)
			: base(outturn)
		{
			this.mAWB = mAWB;
		}

		public ZString MasterAirWaybillNumber
		{
			get { return mAWB == null ? ZString.Empty : mAWB.CM_MAWB; }
		}

		public ZString HouseAirWaybillNumber
		{
			get { return GetHouseAirWaybillNumber(); }
		}

		protected virtual ZString GetHouseAirWaybillNumber()
		{
			return mAWB == null ? ZString.Empty : mAWB.CM_MasterHouseBill;
		}

		readonly CusMAWB mAWB;
	}
}
