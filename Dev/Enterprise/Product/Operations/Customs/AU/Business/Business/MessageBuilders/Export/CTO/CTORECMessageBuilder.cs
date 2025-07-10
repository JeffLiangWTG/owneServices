using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTORECMessageBuilder : CTOMessageBuilder
	{
		public CTORECMessageBuilder(ExportCustomsManifestLines line)
			: this(new ExportCustomsManifestLinesWrapper(line))
		{
		}

		public CTORECMessageBuilder(ICTOMessageLine line)
			: base(line)
		{
		}

		#region Implementation

		protected override void PopulateSegmentGroup8(SegmentGroup8 group8, ICTOMessageLine line, int lineNumber)
		{
			PopulateTDTCarrierVesselVoyage(group8, line.VoyageNumber, line.CarrierPartyID, line.VesselID);
			PopulateLOCDestinationCountry(group8, line.CountryOfDestination);
			PopulateGISOffload(group8, line.OffloadIndicator);
			PopulateNADGoodsOwner(group8, line.GoodsOwnerPartyID, line.OwnerName);
			PopulateDTMDeparture(group8, line.ProposedDateOfDeparture);
			PopulateGIDFTXGoodsDescription(group8, line.GoodsDescription);
		}

		void PopulateGIDFTXGoodsDescription(SegmentGroup8 group8, ZString goodsDescription)
		{
			if (!goodsDescription.IsEmpty)
			{
				MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
				MessageUtilities.PopulateFTX(group8.Group14[0].FTX[0], TextSubjectCodeQualifierList.GoodsDescription, goodsDescription);
			}
		}

		void PopulateDTMDeparture(SegmentGroup8 group8, ZDate departureDate)
		{
			if (!departureDate.IsEmpty)
			{
				MessageUtilities.PopulateDTM(group8.Group11[0].DTM[0], DateTimePeriodFunctionCodeQualifierList.DepartureDateTimeEstimated, departureDate.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
			}
		}

		void PopulateNADGoodsOwner(SegmentGroup8 group8, ZString ownerID, ZString name)
		{
			if (!ownerID.IsEmpty || !name.IsEmpty)
			{
				MessageUtilities.PopulateNAD(group8.Group11[0].NAD[0], PartyFunctionCodeQualifierList.GoodsOwner, ownerID, ownerID.IsEmpty ? null : CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, name, null);
			}
		}

		void PopulateGISOffload(SegmentGroup8 group8, bool offload)
		{
			if (offload)
			{
				MessageUtilities.PopulateGIS(group8.GIS[0], ProcessingIndicatorDescriptionCodeList.GetFromString("OFF"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		void PopulateLOCDestinationCountry(SegmentGroup8 group8, ZString country)
		{
			if (!country.IsEmpty)
			{
				MessageUtilities.PopulateLOC(group8.LOC[0], LocationFunctionCodeQualifierList.CountryOfDestinationOfGoods, country, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
			}
		}

		void PopulateTDTCarrierVesselVoyage(SegmentGroup8 group8, ZString voyage, ZString carrierID, ZString vesselID)
		{
			if (line.IsSea && (!voyage.IsEmpty || !carrierID.IsEmpty || !vesselID.IsEmpty))
			{
				MessageUtilities.PopulateTDT(group8.Group9[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, voyage.IsEmpty ? null : voyage, null, carrierID.IsEmpty ? null : carrierID, carrierID.IsEmpty ? null : CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, vesselID.IsEmpty ? null : vesselID, vesselID.IsEmpty ? null : CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping);
			}
		}

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.CTOREC;

		protected internal override ZString DocumentName => "CTOREC";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.TransportStatusReport;

		protected internal override Type TypeOfMessage => typeof(CMRCTORECMessage);

		#endregion
	}
}
