using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRUBMREQRMessage : CMRCUSRESMessage, ICusHAWBInformationProvider, ICusSCAContainerInformationProvider, ICMRDepotMessage
	{
		public CMRUBMREQRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (EM_StatusInfo.HasChanges
				&& EM_Status == EDIMessage.Status.Received
				&& GetStatusCode() == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived
				&& IsAir
				&& EM_LinkedObject is CusUnderbond underbond)
			{
				var parent = (ITransitWarehouseSyncDataParent)(underbond?.MAWB) ?? underbond;
				parent.RegisterSyncData();
			}
		}

		#region GetWrappedObject

		protected ExpectedArrivalCusUnderbondFactory[] AllUnderbondFactories
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(new VoyageManifestExpectedArrivalCusUnderbondFactory());
				result.Add(new AirCTOExpectedArrivalCusUnderbondFactory());
				result.Add(new StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory());
				result.Add(new SeaCargoForwarderUnderbondApprovalFactory());
				return (ExpectedArrivalCusUnderbondFactory[])result.ToArray(typeof(ExpectedArrivalCusUnderbondFactory));
			}
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			foreach (ExpectedArrivalCusUnderbondFactory underbondFactory in AllUnderbondFactories)
			{
				CusUnderbond factoryProcessingResult = underbondFactory.ProcessIncomingUBMREQR(this);
				if (result == null)
				{
					result = factoryProcessingResult;
				}
			}

			CMRSeaDepotMessageProcessor depotMessageProcessor = new CMRSeaDepotMessageProcessor(this);
			depotMessageProcessor.Process();

			return result;
		}

		#endregion

		#region Lines

		public ZInt Lines
		{
			get { return CUSRES.Group6.Count; }
		}

		#endregion

		#region UBMSendersReference

		public ZString UBMSendersReference
		{
			get
			{
				ZString result = SendersReference;
				int endOfReference = result.LastIndexOf("/");
				if (endOfReference != -1)
				{
					result = result.Substring(0, endOfReference);
				}

				return result;
			}
		}

		#endregion

		#region PremiseIDs

		public ZString OriginID
		{
			get
			{
				LOCSegment lOC = GetLOCSegment(CUSRES.LOC, LocationFunctionCodeQualifierList.PlaceOfDeparture);
				return lOC != null ? (ZString)lOC.LocationIdentification.LocationNameCode : ZString.Empty;
			}
		}

		public ZString DestinationID
		{
			get
			{
				LOCSegment lOC = GetLOCSegment(CUSRES.LOC, LocationFunctionCodeQualifierList.GoodsReceiptPlace);
				return lOC != null ? (ZString)lOC.LocationIdentification.LocationNameCode : ZString.Empty;
			}
		}

		#endregion

		#region Destination Port

		public ZString DestinationPort
		{
			get
			{
				LOCSegment lOC = GetLOCSegment(CUSRES.LOC, LocationFunctionCodeQualifierList.PlaceOfUltimateDestinationOfGoods);
				return lOC != null ? (ZString)lOC.LocationIdentification.LocationNameCode : ZString.Empty;
			}
		}

		#endregion

		#region Request Reason

		public ZString RequestReason
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.AdditionalReferenceNumber);
			}
		}

		#endregion

		#region Mode Of Movement

		public ZString ModeOfMovement
		{
			get
			{
				foreach (TDTSegment tDT in CUSRES.TDT)
				{
					if (tDT.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandTransport)
					{
						string modeOfTransport = tDT.ModeOfTransport.TransportModeNameCode;
						if (modeOfTransport != null && modeOfTransport.Length > 0)
						{
							return modeOfTransport;
						}
					}
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region Underbond By Sea

		public ZString UnderbondBySeaVesselID
		{
			get
			{
				foreach (TDTSegment tDT in CUSRES.TDT)
				{
					if (tDT.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandTransport)
					{
						string vesselID = tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier;
						if (vesselID != null && vesselID.Length > 0)
						{
							return vesselID;
						}
					}
				}
				return ZString.Empty;
			}
		}

		public ZString UnderbondBySeaVoyage
		{
			get
			{
				foreach (TDTSegment tDT in CUSRES.TDT)
				{
					if (tDT.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandTransport)
					{
						string voyage = tDT.ConveyanceReferenceNumber;
						if (voyage != null && voyage.Length > 0)
						{
							return voyage;
						}
					}
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region Number of Packages

		public int GetNumberOfPackages(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (PACSegment pAC in CUSRES.Group6[lineNumber - 1].PAC)
				{
					ZString numberOfPackagesString = pAC.NumberOfPackages;
					if (!numberOfPackagesString.IsEmpty)
					{
						return int.Parse(numberOfPackagesString);
					}
				}
			}
			return 0;
		}

		#endregion

		#region Package Type

		public ZString GetPackageType(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (PACSegment pAC in CUSRES.Group6[lineNumber - 1].PAC)
				{
					if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.ItemType)
					{
						return pAC.PackageType.PackageTypeDescriptionCode;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region MAWB

		public ZString GetMAWB(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (RFFSegment rFF in CUSRES.Group6[lineNumber - 1].RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber)
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region HAWB

		public ZString GetHAWB(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (RFFSegment rFF in CUSRES.Group6[lineNumber - 1].RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.HouseWaybillNumber)
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Container Number

		public ZString GetContainerNumber(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (RFFSegment rFF in CUSRES.Group6[lineNumber - 1].RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Container Mode

		public ZString GetContainerMode(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (PACSegment pAC in CUSRES.Group6[lineNumber - 1].PAC)
				{
					if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.TypeOfPackage)
					{
						return pAC.PackageType.PackageTypeDescriptionCode;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Ocean Bill

		public ZString GetOceanBillOfLading(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (RFFSegment rFF in CUSRES.Group6[lineNumber - 1].RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber)
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region House Bill

		public ZString GetHouseBillOfLading(int lineNumber)
		{
			if (CUSRES.Group6.Count >= lineNumber)
			{
				foreach (RFFSegment rFF in CUSRES.Group6[lineNumber - 1].RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber)
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Goods Description

		public ZString GetGoodsDescription(ZInt lineNumber)
		{
			if (Lines >= lineNumber)
			{
				foreach (FTXSegment fTX in CUSRES.Group6[lineNumber - 1].FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsDescription)
					{
						return fTX.TextLiteral.FreeTextValue1;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Marks and Numbers

		public ZString GetMarksAndNumbers(ZInt lineNumber)
		{
			if (Lines >= lineNumber)
			{
				foreach (PCISegment pCI in CUSRES.Group6[lineNumber - 1].PCI)
				{
					if (pCI.MarkingInstructionsCoded == MarkingInstructionsCodedList.MarkFreeText)
					{
						return pCI.MarksLabels.ShippingMarks1;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Get Reports

		public override ZString GetReport()
		{
			StringBuilder builder = new StringBuilder();

			ZString statusType = GetStatus();
			ZString statusDescription = GetStatusDescription();

			builder.Append("Status: " + statusType + "\r\n");
			if (!statusDescription.IsEmpty)
			{
				builder.Append("Status Description: " + statusDescription + "\r\n");
			}
			if (!ProcessingDate.IsEmpty)
			{
				builder.Append("Customs Processing Date: " + ProcessingDate + "\r\n");
			}

			if (IsAir)
			{
				builder.Append(GetReportAirDetails());
			}
			else if (IsSea)
			{
				builder.Append(GetReportSeaDetails());
			}

			builder.Append(UnderbondMovementDetails());

			builder.Append(AdditionalInfoForHeaderSectionOfReport());
			builder.Append(GetErrorsSection(statusType));

			CusUnderbond parentUnderbond = EM_LinkedObject as CusUnderbond;
			if (parentUnderbond != null)
			{
				builder.Append(GetFullUnderbondDetails(parentUnderbond) + "\r\n");
			}

			return builder.ToString();
		}

		ZString GetReportAirDetails()
		{
			StringBuilder builder = new StringBuilder();
			ICusHAWBInformationProvider provider = this;
			if (provider != null)
			{
				builder.Append("Flight No  : " + provider.FlightNumber + "\r\n");
				builder.Append("Flight Date: " + provider.ArrivalDate.ToShortDateString() + "\r\n");
				builder.Append(" \r\n");
			}
			for (int i = 1; i <= CUSRES.Group6.Count; i++)
			{
				ZString mAWB = GetMAWB(i);
				ZString hAWB = GetHAWB(i);
				ZString cargoType = GetContainerMode(i);
				ZInt packageCount = GetNumberOfPackages(i);
				ZString packageUnit = GetPackageType(i);
				builder.Append("MAWB       : " + mAWB + "\r\n");
				if (!hAWB.IsEmpty)
				{
					builder.Append("HAWB       : " + hAWB + "\r\n");
				}

				builder.Append(PackageDetails(cargoType, packageCount, packageUnit));
			}
			return builder.ToString();
		}

		ZString PackageDetails(ZString cargoType, int packageCount, ZString packageUnit)
		{
			StringBuilder builder = new StringBuilder();
			if (!cargoType.IsEmpty)
			{
				builder.Append("Cargo Type : " + cargoType);
				ZString packDescription = new CMRImportCargoTypes().GetDescriptionFromCode(cargoType);
				if (!packDescription.IsEmpty)
				{
					builder.Append("/" + packDescription);
				}

				builder.Append("\r\n");
			}
			if (packageCount > 0)
			{
				builder.Append("Manifested Packages : " + packageCount.ToString());
				if (!packageUnit.IsEmpty)
				{
					builder.Append(" " + packageUnit + "/" + new CMRPackageTypes().GetDescriptionFromCode(packageUnit));
				}
			}
			if (builder.Length > 0)
			{
				builder.Append("\r\n");
			}

			return builder.ToString();
		}

		ZString GetReportSeaDetails()
		{
			StringBuilder builder = new StringBuilder();
			ICusSCAContainerInformationProvider provider = this;
			if (provider != null)
			{
				builder.Append("Vessel Lloyds : " + provider.LloydsNumber + "\r\n");
				RefVessel vessel = RefVessel.LookupVesselByLloyds(provider.LloydsNumber, Factory);
				if (vessel != null)
				{
					builder.Append("Vessel Name   : " + vessel.RV_Code + "\r\n");
				}
				builder.Append("Voyage Number : " + provider.VoyageNumber + "\r\n");
			}
			for (int i = 1; i <= CUSRES.Group6.Count; i++)
			{
				ZString oceanBill = GetOceanBillOfLading(i);
				ZString container = GetContainerNumber(i);
				ZString houseBill = GetHouseBillOfLading(i);
				ZString cargoType = GetContainerMode(i);
				ZInt packageCount = GetNumberOfPackages(i);
				ZString packageUnit = GetPackageType(i);
				if (!oceanBill.IsEmpty)
				{
					builder.Append("Ocean Bill : " + oceanBill + "\r\n");
				}

				if (!container.IsEmpty)
				{
					builder.Append("Container  : " + container + "\r\n");
				}

				if (!houseBill.IsEmpty)
				{
					builder.Append("House Bill : " + houseBill + "\r\n");
				}

				builder.Append(PackageDetails(cargoType, packageCount, packageUnit));
			}
			return builder.ToString();
		}

		public ZString UnderbondMovementDetails()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("Mode of Movement : " + ModeOfMovement + "/" + new CMRUnderbondModeOfMovement().GetDescriptionFromCode(ModeOfMovement) + "\r\n");
			builder.Append("Request Reason   : " + RequestReason + "/" + new CMRUnderbondRequestCodes().GetDescriptionFromCode(RequestReason) + "\r\n");

			if (!UnderbondBySeaVesselID.IsEmpty)
			{
				RefVessel underbondBySeaVessel = RefVessel.LookupVesselByLloyds(UnderbondBySeaVesselID, Factory);
				if (underbondBySeaVessel != null)
				{
					builder.Append("Underbond By Sea Vessel      : " + underbondBySeaVessel.RV_Code + "\r\n");
				}

				builder.Append("Underbond By Sea Vessel Lloyds:" + UnderbondBySeaVesselID + "\r\n");
				builder.Append("Underbond By Sea Voyage Num   :" + UnderbondBySeaVoyage + "\r\n");
			}

			OrgAddress originOrgAddress = new OrgAddress.Loader(Factory).FromLocalPremiseID(OriginID);
			OrgAddress destinationOrgAddress = new OrgAddress.Loader(Factory).FromLocalPremiseID(DestinationID);
			builder.Append("Origin  - Premise ID : " + OriginID + "\r\n");
			builder.Append(AddressReportDetails(originOrgAddress));
			builder.Append("\r\nDestination - Premis ID : " + DestinationID + "\r\n");
			builder.Append(AddressReportDetails(destinationOrgAddress));

			return builder.ToString();
		}

		ZString AddressReportDetails(OrgAddress address)
		{
			if (address == null)
			{
				return "Address : *** Not on file***";
			}

			StringBuilder builder = new StringBuilder();
			if (address.Header != null)
			{
				builder.Append(address.Header.OH_Code + " " + address.Header.OH_FullNameTruncated + " \r\n");
			}

			builder.Append(address.OA_Address1 + "\r\n");
			if (!address.OA_Address2.IsEmpty)
			{
				builder.Append(address.OA_Address2 + "\r\n");
			}

			if (!address.OA_City.IsEmpty)
			{
				builder.Append(address.OA_City + " ");
			}

			if (!address.OA_State.IsEmpty)
			{
				builder.Append(address.OA_State + " ");
			}

			if (!address.OA_PostCode.IsEmpty)
			{
				builder.Append(address.OA_PostCode);
			}

			builder.Append(" \r\n");
			return builder.ToString();
		}

		ZString GetFullUnderbondDetails(CusUnderbond underbond)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Underbond ID: " + underbond.C4_SendersMessageReference + "\r\n");
			CusSCAContainer container = underbond.LinkedObject as CusSCAContainer;
			CommonContainer freightContainer = underbond.LinkedObject as CommonContainer;
			CommonShipment shipment = underbond.LinkedObject as CommonShipment;
			if (container != null)
			{
				builder.Append(GetContainerUnderbondDetails(container));
			}
			else if (freightContainer != null)
			{
				builder.Append(GetFreightContainerUnderbondDetails(freightContainer));
			}
			else if (shipment != null)
			{
				builder.Append(GetShipmentUnderbondDetails(shipment));
			}
			return builder.ToString();
		}

		ZString GetContainerUnderbondDetails(CusSCAContainer container)
		{
			StringBuilder builder = new StringBuilder();
			if (container != null)
			{
				if (container.OceanBill != null)
				{
					builder.Append("Ocean Bill : " + container.OceanBill.CB_OceanBill + "\r\n");
				}
			}
			return builder.ToString();
		}

		ZString GetFreightContainerUnderbondDetails(CommonContainer container)
		{
			StringBuilder builder = new StringBuilder();
			if (container != null)
			{
				if (container.Consol != null)
				{
					builder.Append("Load list ID: " + container.Consol.JK_UniqueConsignRef + "\r\n");
					builder.Append("Ocean Bill: " + container.Consol.JK_MasterBillNum + "\r\n");
				}
				if (!container.JC_ContainerJobID.IsEmpty)
				{
					builder.Append("Container Job ID: " + container.JC_ContainerJobID + "\r\n");
				}
			}
			return builder.ToString();
		}

		ZString GetShipmentUnderbondDetails(CommonShipment shipment)
		{
			StringBuilder builder = new StringBuilder();
			if (shipment != null)
			{
				if (shipment.ArrivalConsol != null)
				{
					builder.Append("Load list ID: " + shipment.ArrivalConsol.JK_UniqueConsignRef + "\r\n");
				}

				builder.Append("Shipment ID: " + shipment.JS_UniqueConsignRef);
			}
			return builder.ToString();
		}

		#endregion

		#region ICusHAWBInformationProvider Members

		ZString ICusHAWBInformationProvider.MAWB
		{
			get
			{
				ZString result = GetReference(CUSRES.Group6, ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber);
				if (result.IsEmpty)
				{
					result = GetReference(CUSRES.Group6, ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber);
				}

				return result;
			}
		}

		public ZString FlightNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Aircraft)
				{
					return tDT.Carrier.CarrierIdentification + tDT.ConveyanceReferenceNumber;
				}
				return ZString.Empty;
			}
		}

		public ZDateTime ArrivalDate
		{
			get
			{
				foreach (DTMSegment dTM in CUSRES.DTM)
				{
					if (dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated)
					{
						ZString dateString = dTM.DateTimePeriod.DateTimePeriodValue;
						return new ZDateTime(int.Parse(dateString.Substring(0, 4)), int.Parse(dateString.Substring(4, 2)), int.Parse(dateString.Substring(6, 2)));
					}
				}
				return ZDateTime.Empty;
			}
		}

		ZString ICusHAWBInformationProvider.HAWB
		{
			get
			{
				return GetReference(CUSRES.Group6, ReferenceFunctionCodeQualifierList.HouseWaybillNumber);
			}
		}

		#endregion

		#region ICusSCAContainerInformationProvider Members

		public ZString LloydsNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportStageCodeQualifier == TransportStageCodeQualifierList.MainCarriageTransport && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
				{
					return tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier;
				}
				return ZString.Empty;
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportStageCodeQualifier == TransportStageCodeQualifierList.MainCarriageTransport && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
				{
					return tDT.ConveyanceReferenceNumber;
				}
				return ZString.Empty;
			}
		}

		ZString ICusSCAContainerInformationProvider.ContainerNumber
		{
			get
			{
				ZString result = GetReference(CUSRES.Group6, ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber);
				return result;
			}
		}

		#endregion

		#region Status Code

		public ZString GetStatusCode()
		{
			if (UnderbondNoticeType.ToUpper() == ExpectedCargoArrival)
			{
				return CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			}
			else if (UnderbondNoticeType.ToUpper() == UnderbondApproval)
			{
				return CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			}
			else if (UnderbondNoticeType.ToUpper() == UnderbondApprovalRescind)
			{
				return CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived;
			}
			else if (UnderbondNoticeType.ToUpper() == ExpectedCargoArrivalRescind)
			{
				return CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			}

			return ZString.Empty;
		}

		public const string UnderbondApproval = "UNDERBOND APPROVAL";
		public const string UnderbondApprovalRescind = "UNDERBOND APPROVAL RESCIND NOTICE";
		public const string ExpectedCargoArrival = "EXPECTED CARGO ARRIVAL ADVICE";
		public const string ExpectedCargoArrivalRescind = "EXPECTED CARGO ARRIVAL RESCIND NTCE";

		#endregion

		#region Underbond Notice Type

		public ZString UnderbondNoticeType
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.ClearingReference);
			}
		}

		#endregion

		#region Is Expected Arrival

		public ZBool IsExpectedArrival
		{
			get
			{
				ZString statusCode = GetStatusCode();
				return statusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived || statusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			}
		}

		#endregion

		#region Is Approval

		public ZBool IsApproval
		{
			get
			{
				ZString statusCode = GetStatusCode();
				return statusCode == CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived || statusCode == CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived;
			}
		}

		#endregion

		#region Our Premise ID

		public ZString OurPremiseID
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsExpectedArrival)
				{
					result = DestinationID;
				}
				else if (IsApproval)
				{
					result = OriginID;
				}
				return result;
			}
		}

		#endregion

		#region Get Status Core

		protected override ZString GetStatusCore()
		{
			return Factory.GetCachedValue<CMRUnderbondStatuses>().GetDescriptionFromCode(GetStatusCode());
		}

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.UBMREQR;
		}

		#endregion

		#region ICMRDepotMessage Members

		ZString ICMRDepotMessage.OriginPremiseID
		{
			get { return this.OriginID; }
		}

		ZString ICMRDepotMessage.DestinationPremiseID
		{
			get { return this.DestinationID; }
		}

		CMRDepotMessageType ICMRDepotMessage.MessageType
		{
			get { return IsApproval ? CMRDepotMessageType.Approval : CMRDepotMessageType.ExpectedArrival; }
		}

		ICMRDepotMessageLine[] ICMRDepotMessage.Lines
		{
			get
			{
				ICMRDepotMessageLine[] result = new ICMRDepotMessageLine[Lines];
				for (int lineNumber = 0; lineNumber < Lines; lineNumber++)
				{
					result[lineNumber] = new CMRUnderbondDepotMessageLine(this, lineNumber + 1);
				}
				return result;
			}
		}

		ZString ICMRDepotMessage.OurPremiseID
		{
			get { return IsApproval ? ((ICMRDepotMessage)this).OriginPremiseID : ((ICMRDepotMessage)this).DestinationPremiseID; }
		}

		void ICMRDepotMessage.AddUnmatchedContainer(CARSTRecord carstRecord)
		{
			this.AddUnmatchedContainer(carstRecord, UnmatchedContainers);
		}

		public Dictionary<string, List<CARSTRecord>> UnmatchedContainers
		{
			get { return unmatchedContainers ?? (unmatchedContainers = new Dictionary<string, List<CARSTRecord>>()); }
		}
		Dictionary<string, List<CARSTRecord>> unmatchedContainers;

		#endregion
	}
}
