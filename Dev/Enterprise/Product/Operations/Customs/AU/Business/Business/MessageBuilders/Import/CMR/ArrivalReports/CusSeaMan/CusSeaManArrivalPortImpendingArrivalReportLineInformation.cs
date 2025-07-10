using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortImpendingArrivalReportLineInformation : IImpendingArrivalReportLineInformation
	{
		public CusSeaManArrivalPortImpendingArrivalReportLineInformation(CusSeaManArrivalPort arrival)
		{
			this.arrival = arrival;
		}

		public ZString DischargeCTOEstablishmentID
		{
			get { return arrival.CTOEstablishmentID; }
		}

		public bool DischargeIndicator
		{
			get { return arrival.BA_DischargeIndicator; }
		}

		public ZDateTime EstimatedDateTimeOfArrivalUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				RefUNLOCO arrivalPort = arrival.ArrivalPort;
				if (arrivalPort != null && !arrival.BA_ArrivalPortETA.IsEmpty && arrival.BA_ArrivalPortETA.IsValid)
				{
					result = arrivalPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(arrival.BA_ArrivalPortETA.ToDateTime());
				}
				return result;
			}
		}

		public EDIMessageCollection Messages
		{
			get { return arrival.Messages; }
		}

		public ZString PortOfArrival
		{
			get { return arrival.BA_RL_NKArrivalPort; }
		}

		public ZString StevedoreID
		{
			get { return arrival.StevedoreID; }
		}

		#region Implementation

		readonly CusSeaManArrivalPort arrival;

		#endregion
	}
}
