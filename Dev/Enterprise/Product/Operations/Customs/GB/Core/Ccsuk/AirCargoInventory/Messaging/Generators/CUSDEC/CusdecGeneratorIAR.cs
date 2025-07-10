using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	class CusdecGeneratorIAR : CusdecGeneratorBase
	{
		public CusdecGeneratorIAR(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond iarUnderbond)
			: base(awb, ec, iarUnderbond)
		{
			this.iarUnderbond = (InterAirportRemoval)iarUnderbond;
		}

		protected override string AssociationAssignedCode
		{
			get { return "109603"; }
		}

		protected override string BgmDocumentName
		{
			get { return "IAR"; }
		}

		protected override void MakeGroup1ReferenceOnwardAwb(SegmentGroup1 grp1)
		{
			var onwardAwb = iarUnderbond.OnwardAirWaybillNumber.KeepAlphanumericCharacters();
			if (!onwardAwb.IsEmpty)
			{
				var rffOnwardAwb = grp1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffOnwardAwb.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString(FsaDocumentTypes.Codes.AirWaybill);
				rffOnwardAwb.Reference.ReferenceIdentifier = onwardAwb;
			}
		}

		protected override void MakesGISforLicenceRestricted()
		{
			MakesGISforLicenceRestrictedCore(iarUnderbond.LicenseRestrictionInd);
		}

		protected override void MakeTDT()
		{
			var tdt = result.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageQualifier = TransportStageCodeQualifierList.GetFromString("12");
			tdt.XModeOfTransport = iarUnderbond.OnwardMode;
			MakeTDTOnwardCarrier(tdt, iarUnderbond);
		}

		internal static void MakeTDTOnwardCarrier(TDTSegmentWithDatetime tdt, IOnwardCarrierProvider onwardCarrierProviderUnderbond)
		{
			if (onwardCarrierProviderUnderbond.OnwardMode == ModesOfTransportCodes.Codes.Air && !onwardCarrierProviderUnderbond.OnwardCarrier.IsEmpty)
			{
				tdt.Carrier.CarrierIdentifier = onwardCarrierProviderUnderbond.OnwardCarrier;
				tdt.Carrier.CodeListIdentificationCode = CodeListIdentificationCodeList.Carriers;  // 172
				tdt.Carrier.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;  // IATA=3
			}
		}

		protected override void AddShedToLOC85DestinationAirport(LocationAndRelationsAsASingleElement parentLocation)
		{
			if (!iarUnderbond.NewShedId.IsEmpty)
			{
				parentLocation.SubLocationIdentification3439 = iarUnderbond.NewShedId;
				parentLocation.CodeListQualifier1131_2 = "129";
				parentLocation.CodeListResponsibleAgencyCoded3055_2 = "ZZZ";
			}
		}
		readonly InterAirportRemoval iarUnderbond;
	}
}
