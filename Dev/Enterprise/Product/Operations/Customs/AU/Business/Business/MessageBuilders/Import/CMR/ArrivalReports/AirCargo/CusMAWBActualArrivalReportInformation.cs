using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBActualArrivalReportInformation : CusMAWBArrivalReportInformation, IAirActualArrivalReportInformation
	{
		public CusMAWBActualArrivalReportInformation(CusMAWB mAWB)
			: base(mAWB)
		{
		}

		public ZDateTime ActualArrivalDateTimeUTC
		{
			get
			{
				return mAWB.CM_ArrivalDate.AddHours(-10);//ZDateTime.UtcNow.AddHours(-1);
			}
		}

		public ZString LoadPort
		{
			get { return mAWB.CM_RL_NKLoadPort; }
		}

		public ZDateTime EstimatedArrivalDate
		{
			get { return mAWB.CM_ArrivalDate; }
		}

		public ZString FlightNo
		{
			get { return mAWB.CM_FlightNo; }
		}
	}
}
