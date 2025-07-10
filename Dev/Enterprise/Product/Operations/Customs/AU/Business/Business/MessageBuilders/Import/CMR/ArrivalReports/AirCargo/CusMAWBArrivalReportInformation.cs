using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBArrivalReportInformation : IArrivalReportInformation
	{
		public CusMAWBArrivalReportInformation(CusMAWB mAWB)
		{
			this.mAWB = mAWB;
		}

		public EDIMessageCollection Messages
		{
			get
			{
				return mAWB.Messages;
			}
		}

		public ZString LastOverseasPortOfDeparture
		{
			get
			{
				return mAWB.CM_RL_NKLoadPort;
			}
		}

		public ZString PortOfArrival
		{
			get
			{
				if (!mAWB.CM_RL_NKFirstArrivalPort.IsEmpty)
				{
					return mAWB.CM_RL_NKFirstArrivalPort;
				}
				else
				{
					return mAWB.CM_RL_NKDischargePort;
				}
			}
		}

		public ZDateTime DateTimeOfDepartureUTC
		{
			get
			{
				return mAWB.CM_ArrivalDate.AddHours(-12).AddHours(-2);
			}
		}

		public ZString ResponsiblePartyID
		{
			get { return mAWB.CM_ResponsiblePartyID; }
		}

		protected CusMAWB mAWB;
	}
}
