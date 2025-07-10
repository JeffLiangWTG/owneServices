using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSREP;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPARTMessageBuilder : CMRCUSREPMessageBuilder
	{
		public DEPARTMessageBuilder(ForwardingConsol consol)
			: this(new FreightConsolDepartDataWrapper(consol))
		{
			Messages = consol.Messages;
		}

		public DEPARTMessageBuilder(ExportCustomsManifestHeader header)
			: this(new ExportCustomsManifestHeaderDepartDataWrapper(header))
		{
			Messages = header.Messages;
		}

		#region Implementation

		protected DEPARTMessageBuilder(IDepartDataWrapper data)
		{
			this.data = data;
		}

		protected internal override void GenerateMessageText()
		{
			if (cUSREP == null)
			{
				cUSREP = new CUSREPMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateDateAndTimeOfDeparture();
				PopulateDepartureLOC();
				PopulateTDT();
				PopulateDestinationLOC();
				PopulateUNT();
			}
		}

		protected internal override ZString DocumentName => "DEPART";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.TransportDepartureReport;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.DEPART;

		protected internal override Type TypeOfMessage => typeof(CMRDEPARTMessage);

		protected void PopulateTDT()
		{
			if (data.IsAir)
			{
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, data.FlightNumber, TransportMeansDescriptionCodeList.Aircraft, data.AirlineCode, CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			}
			else if (data.IsSea)
			{
				ZString carrierPartyID = data.CarrierPartyID;
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, data.VoyageNumber, TransportMeansDescriptionCodeList.Ship, carrierPartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, data.VesselID);
			}
		}

		protected void PopulateDepartureLOC()
		{
			ZString departureCTOEstablishmentID = data.CTOEstablishmentID;
			if (!departureCTOEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(cUSREP.Group2[0].LOC[0], LocationFunctionCodeQualifierList.PlaceOfDeparture, departureCTOEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected void PopulateDestinationLOC()
		{
			MessageUtilities.PopulateLOC(cUSREP.Group8[0].Group9[0].LOC[0], LocationFunctionCodeQualifierList.PlaceOfDestination, data.PortOfDestination, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
		}

		protected void PopulateDateAndTimeOfDeparture()
		{
			string departureDate = data.DepartureDateTime.ToString("yyyyMMdd");
			string departureTime = data.DepartureDateTime.ToString("HHmm");
			MessageUtilities.PopulateDTM(cUSREP.DTM[0], DateTimePeriodFunctionCodeQualifierList.DepartureDateTime, departureDate, DateTimePeriodFormatCodeList.Ccyymmdd);
			MessageUtilities.PopulateDTM(cUSREP.DTM[1], DateTimePeriodFunctionCodeQualifierList.DepartureDateTime, departureTime, DateTimePeriodFormatCodeList.Hhmm);
		}

		protected IDepartDataWrapper data;

		#endregion
	}
}
