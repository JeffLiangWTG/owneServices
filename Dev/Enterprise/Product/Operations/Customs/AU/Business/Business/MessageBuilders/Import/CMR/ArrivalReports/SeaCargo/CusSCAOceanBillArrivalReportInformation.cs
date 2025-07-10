using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillArrivalReportInformation : IArrivalReportInformation
	{
		public CusSCAOceanBillArrivalReportInformation(CusSCAOceanBill oceanBill)
		{
			this.oceanBill = oceanBill;
		}

		public EDIMessageCollection Messages
		{
			get
			{
				return oceanBill.MessageCollection;
			}
		}

		public ZString LastOverseasPortOfDeparture
		{
			get
			{
				return oceanBill.CB_RL_NKPortOfLoading;
			}
		}

		public ZString PortOfArrival
		{
			get
			{
				if (!oceanBill.CB_RL_NKPortOfFirstArrival.IsEmpty)
				{
					return oceanBill.CB_RL_NKPortOfFirstArrival;
				}
				else
				{
					return oceanBill.CB_RL_NKPortOfDischarge;
				}
			}
		}

		//		public ZDateTime DateTimeOfDeparture
		//		{
		//			get
		//			{
		//				return ZDateTime.Now.AddDays(-2);
		//			}
		//		}
		//
		//		public ZDateTime EstimatedDateOfArrival
		//		{
		//			get
		//			{
		//				return ZDateTime.Now.AddDays(1);
		//			}
		//		}

		public ZString CTOID
		{
			get
			{
				return oceanBill.DischargeCTOID;
			}
		}

		protected CusSCAOceanBill oceanBill;

		public ZString ResponsiblePartyID
		{
			get { return oceanBill.CB_ResponsiblePartyID; }
		}
	}
}
