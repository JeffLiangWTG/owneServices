using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCARSTMessage : CMRCUSRESMessage, ICusHAWBInformationProvider, ICTOCusHAWBInformationProvider, ICusSCAHouseInfoProvider, ICusSeaManOBLHeaderInfoProvider, ICMRDepotMessage, ICMRDepotMessageLine, Integration.Customs.AU.ICMRCARSTMessage
	{
		#region enum FTXSegmentAnswerCode

		public abstract class FTXSegmentAnswerCode
		{
			public const string NotAvailable = "N/A";
			public const string No = "NO";
			public const string Yes = "YES";
		}

		#endregion

		public CMRCARSTMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Loader : CMRCUSRESMessage.Loader
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public ZQuery GetApplicationAndOwnerReferenceQuery(ZString applicationRef, ZString ownerRef)
			{
				var result = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationReference, applicationRef.Left(EDIMessage.Schema.EM_ApplicationReferenceMaxLength));
				result.AddToFilter(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.CARST);
				result.AddToFilter(EDIMessageSchema.EM_MessageOwner, ownerRef.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength));
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CMRCARSTMessage);
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.CARST;
		}

		#endregion

		#region All Creators

		protected internal CARSTBusinessObjectLoaderOrCreator[] AllCreators
		{
			get
			{
				ArrayList result = new ArrayList();
				//Result.Add(new StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator());
				result.Add(new StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator());
				//Result.Add(new UnpackDepotSeaCargoCARSTBusinessObjectLoaderOrCreator());
				result.Add(new AirCTOCARSTBusinessObjectLoaderOrCreator());
				result.Add(new VoyageManifestCARSTBusinessObjectLoaderOrCreator());
				result.Add(new BrokerageCARSTBusinessObjectLoader());
				result.Add(new SeaCargoCARSTBusinessObjectLoader());
				return (CARSTBusinessObjectLoaderOrCreator[])result.ToArray(typeof(CARSTBusinessObjectLoaderOrCreator));
			}
		}

		#endregion

		#region Sender's Reference

		public ZString CARSTSendersReference
		{
			get { return GetReferenceFromSendersReference(); }
		}

		#endregion

		#region Get Wrapped Object

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;

			foreach (CARSTBusinessObjectLoaderOrCreator loaderOrCreator in AllCreators)
			{
				BusinessObject[] factoryProcessingResults = loaderOrCreator.LoadOrCreateRecordForMessage(this);

				if (factoryProcessingResults != null)
				{
					foreach (BusinessObject factoryProcessingResult in factoryProcessingResults)
					{
						if ((bool)Env.Registry.RawRegistry.AUCAutoSendUnderbondOnCARST.Value)
						{
							ICusUnderbondUnionCollectionParent underbondParent = factoryProcessingResult as ICusUnderbondUnionCollectionParent;
							if (underbondParent != null)
							{
								CMRAutoUnderbondSender.CheckUnderbondsAndSend(underbondParent.AllUnderbonds.Cast<CusUnderbond>());
							}
						}

						if (result == null)
						{
							result = factoryProcessingResult;
						}
					}
				}
			}

			CMRSeaDepotMessageProcessor depotMessageProcessor = new CMRSeaDepotMessageProcessor(this);
			depotMessageProcessor.Process();

			if (result == null && EM_LinkedObject == null)
			{
				MakeOrphanMarks();
			}

			return result;
		}

		protected override CMRCUSRESMessage LinkOrCloneMessageCore(EDIMessageCollection messages)
		{
			var result = base.LinkOrCloneMessageCore(messages);
			if (EM_LinkTable == CusMAWB.Schema.TableName && IsAir && GetFTXSegmentInfo("CONSOLIDATED STATUS") == "SUBUBMOV" && Logs.MostRecentLogByEventTime(Events.SubjectToUnderbondMovement) == null)
			{
				Logs.AddNew(Events.SubjectToUnderbondMovement);
			}
			return result;
		}

		public void MakeOrphanMarks()
		{
			if (IsSea && !OceanBillNumber.IsEmpty && !HouseBillNumber.IsEmpty)
			{
				EM_ApplicationReference = OceanBillNumber;
				EM_MessageOwner = HouseBillNumber.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
				EM_LinkTable = CusSCAOceanBill.Schema.TableName;
			}
		}

		#endregion

		public bool IsUnmatchedHouseReportRequired { get; set; }

		public override ZString AdditionalInfoForStatusSectionOfReport()
		{
			ZString status = ConsolidatedStatus;
			ZStringBuilder result = new ZStringBuilder();
			result.Append("***CONSOLIDATED CARGO STATUS: ***");
			result.Append(status);
			result.Append("***\r\n");
			ZStringBuilder label = new ZStringBuilder();
			ZStringBuilder value = new ZStringBuilder();

			ZString reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber);
			if (!reference.IsEmpty)
			{
				label.Append(" / HBL");
				value.Append(" / ");
				value.Append(reference);
			}
			reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.HouseWaybillNumber);
			if (!reference.IsEmpty)
			{
				label.Append(" / HAWB");
				value.Append(" / ");
				value.Append(reference);
			}
			reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber);
			if (!reference.IsEmpty)
			{
				label.Append(" / OBL");
				value.Append(" / ");
				value.Append(reference);
			}
			reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber);
			if (!reference.IsEmpty)
			{
				label.Append(" / MAWB");
				value.Append(" / ");
				value.Append(reference);
			}
			reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber);
			if (!reference.IsEmpty)
			{
				label.Append(" / Container");
				value.Append(" / ");
				value.Append(reference);
			}
			if (!label.IsEmpty)
			{
				result.Append(label.ToString().Substring(3));
				result.Append(": ");
				result.Append(value.ToString().Substring(3));
				result.Append("\r\n");
			}
			result.Append("ACSDec/ACSCR/AQISDec/AQISCR: ");
			result.Append(AbbreviatedStatusDescriptionForTransportLineCore(0));
			result.Append("\r\n");
			return result.ToString();
		}

		protected override ZString AbbreviatedStatusDescriptionForTransportLineCore(ZShort cargoLineNumber)
		{
			ZString aCSDec = "N";
			ZString aCSCR = "N";
			ZString aQISDec = "N";
			ZString aQISCR = "N";
			if (ConsolidatedStatus == CMRConsolidatedCargoStatuses.ShortDescriptions.Clear.ToString() ||
				 ConsolidatedStatus == CMRConsolidatedCargoStatuses.ShortDescriptions.Clearhrm.ToString() ||
				 ConsolidatedStatus == CMRConsolidatedCargoStatuses.ShortDescriptions.Condclear.ToString())
			{
				aCSDec = "Y";
				aCSCR = "Y";
				aQISDec = "Y";
				aQISCR = "Y";
			}
			ZString tempValue = GetFTXSegmentInfo("IMPORT DECLARATION ACS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aCSDec = tempValue.Left(1);
			}

			if (aCSDec == "Y" && GetFTXSegmentInfo("IMPORT DECLARATION PAID") == "NO")
			{
				aCSDec = "$";
			}

			tempValue = GetFTXSegmentInfo("CARGO REPORT ACS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aCSCR = tempValue.Left(1);
			}

			tempValue = GetFTXSegmentInfo("IMPORT DECLARATION AQIS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aQISDec = tempValue.Left(1);
			}

			tempValue = GetFTXSegmentInfo("CARGO REPORT AQIS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aQISCR = tempValue.Left(1);
			}

			return aCSDec + "/" + aCSCR + "/" + aQISDec + "/" + aQISCR;
		}

		protected override bool ShowExtendedStatus
		{
			get { return true; }
		}

		public ZString TranshipmentNumber
		{
			get { return GetFTXSegmentInfo("TRANSHIPMENT NUMBER"); }
		}

		public ZString ConsolidatedStatus
		{
			get
			{
				if (consolidatedStatus.IsEmpty)
				{
					consolidatedStatus = GetFTXSegmentInfo("CONSOLIDATED STATUS");
				}
				return consolidatedStatus;
			}
		}
		ZString consolidatedStatus;

		public ZString[] ACSAQISImpedimentDetails
		{
			get { return GetFTXSegmentInfos("ACS/AQIS IMPEDIMENT DETAILS"); }
		}

		public ZString AQISCargoReportEvaluationComplete
		{
			get { return GetFTXSegmentInfo("AQIS CARGO REPORT EVALUATION COMPLETE"); }
		}

		public ZString GetFTXSegmentInfo(string identifier)
		{
			if (CUSRES != null)
			{
				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					if (fTX.TextLiteral.FreeTextValue1 == identifier)
					{
						return fTX.TextLiteral.FreeTextValue2;
					}
				}
			}

			return ZString.Empty;
		}

		public ZString[] GetFTXSegmentInfos(string identifier)
		{
			ArrayList result = new ArrayList();

			if (CUSRES != null)
			{
				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					if (fTX.TextLiteral.FreeTextValue1 == identifier)
					{
						result.Add(new ZString(fTX.TextLiteral.FreeTextValue2));
					}
				}
			}

			return (ZString[])result.ToArray(typeof(ZString));
		}

		public ZString ContainerNumber
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber);
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				foreach (FTXSegment fTX in CUSRES.Group6[0].FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsDescription)
					{
						return fTX.TextLiteral.FreeTextValue1;
					}
				}
				return ZString.Empty;
			}
		}

		public ZString MarksAndNumbers
		{
			get
			{
				foreach (FTXSegment fTX in CUSRES.Group6[0].FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.AdditionalMarksNumbersInformation)
					{
						return fTX.TextLiteral.FreeTextValue1;
					}
				}
				return ZString.Empty;
			}
		}

		#region Container Modes

		public ZString ContainerMode
		{
			get
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					foreach (PACSegment pAC in group6.PAC)
					{
						if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.TypeOfPackage)
						{
							return pAC.PackageType.PackageTypeDescriptionCode;
						}
					}
				}
				return ZString.Empty;
			}
		}

		public bool IsFCL
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoad; }
		}

		public bool IsFCX
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills; }
		}

		public bool IsLCL
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.LessThanContainerLoad; }
		}

		public bool IsBulk
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.Bulk; }
		}

		public bool IsBreakBulk
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.BreakBulk; }
		}

		#endregion

		#region Number of Packages

		public ZInt NumberOfPackages
		{
			get
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					foreach (PACSegment pAC in group6.PAC)
					{
						if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.ItemType || string.IsNullOrEmpty(pAC.PackageType.CodeListIdentificationCode.ToString()))
						{
							ZInt result;
							ZInt.TryParse(pAC.NumberOfPackages, out result);
							return result;
						}
					}
				}
				return 0;
			}
		}

		#endregion

		#region Package Type

		public ZString PackageType
		{
			get
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					foreach (PACSegment pAC in group6.PAC)
					{
						if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.ItemType)
						{
							return pAC.PackageType.PackageTypeDescriptionCode;
						}
					}
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region ICusHAWBInformationProvider Members

		public ZString MAWB
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber);
			}
		}

		public ZString HAWB
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.HouseWaybillNumber);
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

		public ZString PortOfDischarge
		{
			get
			{
				LOCSegment lOC = GetLOCSegment(CUSRES.LOC, LocationFunctionCodeQualifierList.PortOfDischarge);
				return lOC != null ? (ZString)lOC.LocationIdentification.LocationNameCode : ZString.Empty;
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

		#endregion

		#region ICusSCAHouseInfoProvider Members

		public ZString LloydsNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
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
				if (tDT != null && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
				{
					return tDT.ConveyanceReferenceNumber;
				}
				return ZString.Empty;
			}
		}

		public ZString OceanBillNumber
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber);
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber);
			}
		}

		public ZString PremiseID
		{
			get
			{
				LOCSegment lOC = GetLOCSegment(CUSRES.LOC, LocationFunctionCodeQualifierList.GoodsReceiptPlace);
				return lOC != null ? (ZString)lOC.LocationIdentification.LocationNameCode : ZString.Empty;
			}
		}

		#endregion

		#region ToStrings

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			if (!PremiseID.IsEmpty)
			{
				result.Append("Premise ID     : " + PremiseID);
			}

			if (!PortOfDischarge.IsDefault)
			{
				result.Append("Discharge      : " + PortOfDischarge);
			}

			if (IsSea)
			{
				result.Append(ToSeaString());
			}
			else
			{
				result.Append(ToAirString());
			}

			if (NumberOfPackages != 0)
			{
				result.Append("Package Count  : " + NumberOfPackages);
			}

			if (!PackageType.IsEmpty)
			{
				result.Append("Package Type   : " + PackageType);
			}

			return result.ToString();
		}

		string ToAirString()
		{
			StringBuilder result = new StringBuilder();
			if (!FlightNumber.IsEmpty)
			{
				result.Append("Flight No.     : " + FlightNumber);
			}

			if (!MAWB.IsEmpty)
			{
				result.Append("MAWB           : " + MAWB);
			}

			if (!HAWB.IsEmpty)
			{
				result.Append("HAWB           : " + HAWB);
			}

			return result.ToString();
		}

		string ToSeaString()
		{
			StringBuilder result = new StringBuilder();
			if (!LloydsNumber.IsEmpty)
			{
				result.Append("Lloyds IMO     : " + LloydsNumber);
			}

			if (!VoyageNumber.IsEmpty)
			{
				result.Append("Voyage         : " + VoyageNumber);
			}

			if (!OceanBillNumber.IsEmpty)
			{
				result.Append("Ocean Bill     : " + OceanBillNumber);
			}

			if (!ContainerNumber.IsEmpty)
			{
				result.Append("Container #    : " + ContainerNumber);
			}

			if (!ContainerMode.IsEmpty)
			{
				result.Append("Container Mode : " + ContainerMode);
			}

			if (!HouseBillNumber.IsEmpty)
			{
				result.Append("House Bill     : " + HouseBillNumber);
			}

			return result.ToString();
		}

		#endregion
		#region ICMRDepotMessage Members

		ZString ICMRDepotMessage.LloydsNumber
		{
			get { return LloydsNumber; }
		}

		ZString ICMRDepotMessage.VoyageNumber
		{
			get { return VoyageNumber; }
		}

		ZString ICMRDepotMessage.DestinationPremiseID
		{
			get { return PremiseID; }
		}

		ZString ICMRDepotMessage.OurPremiseID
		{
			get { return ((ICMRDepotMessage)this).DestinationPremiseID; }
		}

		ICMRDepotMessageLine[] ICMRDepotMessage.Lines
		{
			get { return new ICMRDepotMessageLine[] { this }; }
		}

		ZString ICMRDepotMessage.OriginPremiseID
		{
			get { return ZString.Empty; }
		}

		CMRDepotMessageType ICMRDepotMessage.MessageType
		{
			get { return CMRDepotMessageType.Status; }
		}

		ICMRDepotMessage ICMRDepotMessageLine.Parent
		{
			get { return this; }
		}

		ZDecimal ICMRDepotMessageLine.ActualWeight
		{
			get { return ZDecimal.Zero; }
		}

		ZString ICMRDepotMessageLine.WeightUnits
		{
			get { return ZString.Empty; }
		}

		ZDecimal ICMRDepotMessageLine.ActualVolume
		{
			get { return ZDecimal.Zero; }
		}

		ZString ICMRDepotMessageLine.VolumeUnits
		{
			get { return ZString.Empty; }
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
