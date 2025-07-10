using System;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEQMessageBuilder : CMRCUSCARMessageBuilder
	{
		public SEQMessageBuilder(ISeaCargoEstablishmentQueryInformation queryInfo)
		{
			this.queryInfo = queryInfo;
		}
		readonly ISeaCargoEstablishmentQueryInformation queryInfo;

		protected internal override int Version => 1;

		protected internal override CargoWise.Types.ZString DocumentName => "SEQ";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.TransportStatusRequest;

		protected internal override CargoWise.Types.ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEQ;

		protected internal override Type TypeOfMessage => typeof(CMRSEQMessage);

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateGroup1();
				PopulateGroup2();
				PopulateGroup4();
				PopulateUNT();
			}
		}

		void PopulateGroup1()
		{
			if (!queryInfo.ContainerNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, queryInfo.ContainerNumber, null);
			}
			if (!queryInfo.HouseBill.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber, queryInfo.HouseBill, null);
			}
			if (!queryInfo.OceanBill.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber, queryInfo.OceanBill, null);
			}
		}

		void PopulateGroup2()
		{
			if (!queryInfo.ResponsiblePartyID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, queryInfo.ResponsiblePartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		protected void PopulateGroup4()
		{
			SegmentGroup4 group4 = CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection();
			if (!queryInfo.VesselID.IsEmpty)
			{
				TDTSegment tDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
				tDT.TransportMeans.TransportMeansDescriptionCode = TransportMeansDescriptionCodeList.Ship;
				tDT.ConveyanceReferenceNumber = queryInfo.VoyageNumber;
				tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier = queryInfo.VesselID;
				tDT.TransportIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping;
			}
			if (!queryInfo.EstablishmentID.IsEmpty)
			{
				LOCSegment lOC = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.Terminal;
				lOC.LocationIdentification.LocationNameCode = queryInfo.EstablishmentID;
				lOC.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}
	}
}
