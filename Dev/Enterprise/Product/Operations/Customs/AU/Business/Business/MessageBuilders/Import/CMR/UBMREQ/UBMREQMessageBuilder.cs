using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using BaseCustomsMessageBuilders = Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Responsible to populate information coming from dbo.CusUnderbond only
	/// </summary>
	public sealed class UBMREQMessageBuilder : CMRCUSCARMessageBuilder
	{
		public UBMREQMessageBuilder(IUnderbondMovementRequestHeader header, ZString messageOwnerSiteID)
			: base(messageOwnerSiteID)
		{
			if (header == null)
			{
				throw new ArgumentException("Header cannot be NULL");
			}

			this.header = header;
		}

		public UBMREQMessageBuilder(IUnderbondMovementRequestHeader header)
			: this(header, ZString.Empty)
		{
		}

		protected override bool IsBureau
		{
			get
			{
				return header.IsBureau;
			}
		}

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateRFF();
				PopulateSegmentGroup2();
				PopulateSegmentGroup4();
				PopulateGroup7();
				PopulateUNT();
			}
		}

		void PopulateRFF()
		{
			if (!header.RequestReasonCode.IsEmpty && !IsWithdrawal && !IsChange)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF[0], ReferenceFunctionCodeQualifierList.AdditionalReferenceNumber, header.RequestReasonCode, null);
			}
		}

		void PopulateSegmentGroup2()
		{
			ZString responsiblePartyClientID = header.ResponsiblePartyID;
			if (!responsiblePartyClientID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, responsiblePartyClientID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		void PopulateSegmentGroup4()
		{
			if (((!header.UnderbondBySeaVoyageNumber.IsEmpty && !header.UnderbondBySeaVesselID.IsEmpty) || !header.ModeOfTransport.IsEmpty) && !IsWithdrawal && !IsChange)
			{
				MessageUtilities.PopulateTDT(CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection().TDT[0], TransportStageCodeQualifierList.InlandTransport, header.UnderbondBySeaVoyageNumber, header.ModeOfTransport, null, null, header.UnderbondBySeaVesselID, CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping);
			}

			PopulateFlightOrVoyageDetails();

			if (CUSCAR.Group4.Count > 0)
			{
				SegmentGroup4 group4 = CUSCAR.Group4[CUSCAR.Group4.Count - 1];
				PopulateLocations(group4);
				PopulateDates(group4);
			}
		}

		void PopulateFlightOrVoyageDetails()
		{
			if (!header.FlightNumber.IsEmpty)
			{
				ZString airlineCode = header.FlightNumber.Replace(" ", "").SubstringSafe(0, 2);
				ZString flightNo = header.FlightNumber.Replace(" ", "").SubstringSafe(2);
				MessageUtilities.PopulateTDT(CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection().TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, flightNo, TransportMeansDescriptionCodeList.Aircraft, airlineCode, CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			}
			if (!header.VesselID.IsEmpty && !header.VoyageNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection().TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, header.VoyageNumber, TransportMeansDescriptionCodeList.Ship, null, null, header.VesselID);
			}
		}

		void PopulateLocations(SegmentGroup4 group4)
		{
			if (!header.DestaintionEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(group4.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.GoodsReceiptPlace, header.DestaintionEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (!header.OriginatingEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(group4.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDeparture, header.OriginatingEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (!header.DischargeEstablishmentID.IsEmpty && !IsWithdrawal && !IsChange)
			{
				MessageUtilities.PopulateLOC(group4.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlacePortOfDischarge, header.DischargeEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (!header.TranshipmentOverseasDestinationPort.IsEmpty)
			{
				MessageUtilities.PopulateLOC(group4.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfUltimateDestinationOfGoods, header.TranshipmentOverseasDestinationPort, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
		}

		void PopulateDates(SegmentGroup4 group4)
		{
			if (!header.EstimatedDateOfArrival.IsEmpty)
			{
				MessageUtilities.PopulateDTM(group4.DTM[0], DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, header.EstimatedDateOfArrival.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
			}
		}

		void PopulateGroup7()
		{
			if (!IsWithdrawal)
			{
				new UBMREQMessageLine(header.Line, header.NumberOfPackages, header.PackageType).Populate(CUSCAR.Group7[0], "I");
			}
		}

		#region Implementation

		bool IsChange => MessageSubType == BaseCustomsMessageBuilders.MessageSubTypes.Change;

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.UnderbondRequest;

		protected internal override ZString DocumentName => "UBMREQ";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.UBMREQ;

		protected internal override Type TypeOfMessage => typeof(CMRUBMREQMessage);

		readonly IUnderbondMovementRequestHeader header;

		#endregion
	}
}
