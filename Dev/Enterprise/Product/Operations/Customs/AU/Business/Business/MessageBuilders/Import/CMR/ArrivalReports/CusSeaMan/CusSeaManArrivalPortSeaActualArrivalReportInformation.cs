using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortSeaActualArrivalReportInformation : ISeaActualArrivalReportInformation
	{
		public CusSeaManArrivalPortSeaActualArrivalReportInformation(CusSeaManArrivalPort arrival)
		{
			this.arrival = arrival;
		}

		public ZDateTime ActualArrivalDateTimeUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				RefUNLOCO arrivalPort = arrival.ArrivalPort;
				if (arrivalPort != null && !arrival.BA_ArrivalPortATA.IsEmpty && arrival.BA_ArrivalPortATA.IsValid)
				{
					result = arrivalPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(arrival.BA_ArrivalPortATA.ToDateTime());
				}
				return result;
			}
		}

		public ZString LastOverseasPortOfDeparture
		{
			get { return arrival.Header.BT_RL_NKPortOfLastForeignPort; }
		}

		public ZString PortOfArrival
		{
			get { return arrival.BA_RL_NKArrivalPort; }
		}

		public ZString DischargeCTOID
		{
			get { return arrival.CTOAddress != null ? arrival.CTOAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString LloydsNumber
		{
			get { return arrival.Header.Vessel != null ? arrival.Header.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		public ZString Voyage
		{
			get { return arrival.Header.BT_VoyageNum; }
		}

		public ZString BerthCode
		{
			get { return arrival.BA_BerthCode; }
		}

		public ZString StevedoreID
		{
			get
			{
				OrgAddress cTOAddress = arrival.CTOAddress;
				return cTOAddress != null ? cTOAddress.Header.PrimaryRegistrationNumber.Number : ZString.Empty;
			}
		}

		public ZString ResponsiblePartyID
		{
			get { return arrival.Header.BT_ResponsiblePartyID; }
		}

		#region Implementation

		readonly CusSeaManArrivalPort arrival;
		#endregion
	}
}
