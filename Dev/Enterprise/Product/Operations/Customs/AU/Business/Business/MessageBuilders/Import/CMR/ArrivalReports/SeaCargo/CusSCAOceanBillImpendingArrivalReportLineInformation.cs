using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillImpendingArrivalReportLineInformation : IImpendingArrivalReportLineInformation
	{
		public CusSCAOceanBillImpendingArrivalReportLineInformation(CusSCAOceanBill oceanBill)
		{
			this.oceanBill = oceanBill;
		}

		readonly CusSCAOceanBill oceanBill;

		public ZDateTime EstimatedDateTimeOfArrivalUTC
		{
			get
			{
				return ZDateTime.Now.AddDays(1);
			}
		}

		public bool DischargeIndicator
		{
			get
			{
				return oceanBill.CB_RL_NKPortOfFirstArrival.IsEmpty;
			}
		}

		public ZString DischargeCTOEstablishmentID
		{
			get { return oceanBill.DischargeCTOID; }
		}

		public ZString PortOfArrival
		{
			get { return oceanBill.CB_RL_NKPortOfDischarge; }
		}

		public ZString StevedoreID
		{
			get
			{
				return oceanBill.StevadoreID;
			}
		}
	}
}
