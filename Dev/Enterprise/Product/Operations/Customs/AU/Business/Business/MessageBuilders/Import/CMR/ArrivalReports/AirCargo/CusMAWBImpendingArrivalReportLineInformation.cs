using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBImpendingArrivalReportLineInformation : IImpendingArrivalReportLineInformation
	{
		public CusMAWBImpendingArrivalReportLineInformation(CusMAWB mAWB)
		{
			this.mAWB = mAWB;
		}

		#region IImpendingArrivalReportLineInformation Members

		public ZDateTime EstimatedDateTimeOfArrivalUTC
		{
			get { return mAWB.CM_ArrivalDate.AddHours(-10); }
		}

		public ZString DischargeCTOEstablishmentID
		{
			get { return mAWB.DischargeCTOID; }
		}

		public bool DischargeIndicator
		{
			get { return mAWB.CM_RL_NKFirstArrivalPort.IsEmpty; }
		}

		public ZString PortOfArrival
		{
			get { return mAWB.CM_RL_NKDischargePort; }
		}

		public ZString StevedoreID
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Implementation

		readonly CusMAWB mAWB;

		#endregion
	}
}
