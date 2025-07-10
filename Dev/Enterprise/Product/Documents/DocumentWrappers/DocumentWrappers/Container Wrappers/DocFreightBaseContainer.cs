using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocFreightBaseContainer : DocBaseWrapper, IDocSimpleContainer, IDocContainer
	{
		public DocFreightBaseContainer(CommonContainer commonContainer, BusinessObjectFactory factoryToWrap)
			: base(commonContainer, factoryToWrap)
		{
			QuantityCount = commonContainer.JC_ContainerCount;
		}

		#region Overrides

		public override string ToString()
		{
			return ContainerNumber;
		}

		public ZInt MarksAndNumbersWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 1); }
		}

		public ZInt GoodsDescWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 1); }
		}

		#endregion

		#region Virtual Fields

		public virtual DocUNLOCO PortOfDischarge
		{
			get { return DocUNLOCO.New(Factory, CommonContainer.JC_JB_NKPortOfDischarge); }
		}

		public virtual DocUNLOCO PortOfLoading
		{
			get { return DocUNLOCO.New(Factory, CommonContainer.JC_JA_NKPortOfLoading); }
		}

		public virtual ZString IMOClass
		{
			get { return ZString.Empty; }
		}

		public virtual ZString UNDG_Num
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Abstract Fields

		#region IDocContainer Members

		public abstract ZString ForwardingInstructionWeight { get; }
		public abstract ZString ForwardingInstructionVolume { get; }
		public abstract ZString ForwardingInstructionPackages { get; }
		public abstract ZString DescriptionAndStatus { get; }
		public abstract ZDecimal TotalAllocatedShipmentWeight { get; }
		public abstract ZDecimal TotalAllocatedShipmentVolume { get; }
		public abstract ZString TotalAllocatedShipmentVolumeUQ { get; }
		public abstract ZInt TotalAllocatedShipmentPackages { get; }
		public abstract ZString TotalAllocatedShipmentPackagesPackType { get; }
		public abstract ZDecimal TotalPackLineVolume { get; }
		public abstract ZDecimal TotalPackLineWeight { get; }
		public abstract ZInt TotalPackLinePackages { get; }

		#endregion

		#endregion

		#region ZString Fields

		#region Cartage Advice

		public ZString EmailSubjectNumber
		{
			get { return ContainerNumber; }
		}

		#endregion

		public ZString ContainerNumberOrTypeCount
		{
			get { return CommonContainer.JC_ContainerCode; }
		}

		public ZString TotalVolumeUnit
		{
			get { return CommonContainer.JC_Calc_TotalVolumeUnit; }
		}

		public ZString TotalWeightUnit
		{
			get { return CommonContainer.JC_Calc_TotalWeightUnit; }
		}

		public ZString TotalPackagesUnit
		{
			get { return CommonContainer.JC_Calc_TotalPackagesUnit; }
		}

		public ZString AdditionalSealNum // dont use this, use SealNumber2
		{
			get { return CommonContainer.JC_AdditionalSealNum; }
		}

		public ZString AirVent
		{
			get
			{
				ZString result;

				if (CommonContainer.JC_AirVentFlow == 0)
				{
					result = Res.GetString("db4f86ba-a264-4785-b47f-f5153978b919", "CLOSED");
				}
				else
				{
					result = CommonContainer.JC_AirVentFlow.ToString() + " " + CommonContainer.JC_AirVentFlowRateUnit;
				}

				return result;
			}
		}

		public ZString ArrivalTransportMode
		{
			get
			{
				return "ROA";
			}
		}

		public virtual ZString ArrivalTruckDriversLicense
		{
			get
			{
				ZString result = "";
				if (ArrivalCFSConfirm != null)
				{
					result = ArrivalCFSConfirm.DriversLicense;
				}
				return result;
			}
		}

		public ZString ArrivalTruckRegistration
		{
			get
			{
				ZString result = "";
				if (ArrivalCFSConfirm != null)
				{
					result = ArrivalCFSConfirm.TruckRegistration;
				}
				return result;
			}
		}

		public ZString BookingReference
		{
			get { return CommonContainer.Consol != null ? CommonContainer.Consol.JK_BookingReference : ZString.Empty; }
		}

		public ZString ClipOnUnit
		{
			get { return CommonContainer.JC_RefrigGeneratorID; }
		}

		public ZString ContainerJobID
		{
			get { return CommonContainer.JC_ContainerJobID; }
		}

		public ZString ContainerMode
		{
			get { return CommonContainer.JC_ContainerMode; }
		}

		public ZString CarrierBookingRef
		{
			get { return Consol != null ? Consol.BookingReference : ZString.Empty; }
		}

		public ZString ClientReference
		{
			get { return ZString.Empty; }
		}

		public ZString ContainerNumber
		{
			get { return CommonContainer.JC_ContainerNum; }
		}

		public ZString ContainerCode
		{
			get { return CommonContainer.JC_ContainerCode; }
		}

		public ZString ContainerRating
		{
			get { return CommonContainer.JC_ContainerRating; }
		}

		public ZString ContainerStatus
		{
			get { return CommonContainer.JC_ContainerStatus; }
		}

		public ZString DeliveryMode
		{
			get { return CommonContainer.JC_DeliveryMode; }
		}

		public ZString DeliveryModeDescription
		{
			get
			{
				CodeAndDescriptionWrapper deliveryModeWrapper = new CodeAndDescriptionWrapper(CommonContainer.JC_DeliveryMode, CommonContainer.JC_DeliveryMode_List, Factory);
				return deliveryModeWrapper.Description;
			}
		}

		public virtual ZString DepartureTruckDriversLicense
		{
			get
			{
				ZString result = "";
				if (DepartureCFSConfirm != null)
				{
					result = DepartureCFSConfirm.DriversLicense;
				}
				return result;
			}
		}

		public virtual ZString DepartureTruckRegistration
		{
			get
			{
				ZString result = "";
				if (DepartureCFSConfirm != null)
				{
					result = DepartureCFSConfirm.TruckRegistration;
				}
				return result;
			}
		}

		public ZString DetentionLiabilityWarningText
		{
			get { return DocumentsDataRegistry.Instance.ContainerLiabilityStatementLiabilityWarningText.Value; }
		}

		public ZString DetentionLiabilityAcceptanceText
		{
			get { return DocumentsDataRegistry.Instance.ContainerLiabilityAcceptanceText.Value; }
		}

		public ZString ExportDepotCustomsReference
		{
			get { return CommonContainer.JC_ExportDepotCustomsReference; }
		}

		public ZString GrossWeightUQ
		{
			get { return CommonContainer.ContainerWeightUnit; }
		}

		public ZString MasterBillNumber
		{
			get
			{
				if (Consol != null)
				{
					return Consol.MasterBillNum;
				}
				return ZString.Empty;
			}
		}

		#region Seal Numbers

		public ZString SealNumber
		{
			get { return CommonContainer.JC_SealNum; }
		}

		public ZString SealNumber2
		{
			get { return CommonContainer.JC_AdditionalSealNum; }
		}

		public ZString SealNumber3
		{
			get { return CommonContainer.JC_Additional2SealNum; }
		}

		#endregion

		public ZDecimal SetPointTemp
		{
			get { return CommonContainer.JC_SetPointTemp; }
		}

		public ZString SetPointTempUnit
		{
			get { return CommonContainer.JC_SetPointTempUnit; }
		}

		public ZString Size
		{
			get { return (Container != null) ? Container.Code : ZString.Empty; }
		}

		public ZString TempRecorderSerialNo
		{
			get
			{
				return CommonContainer.JC_TempRecorderSerialNo;
			}
		}

		public ZString SlotReference
		{
			get { return CommonContainer.JC_ArrivalSlotReference; }
		}

		public ZString Purpose
		{
			get { return CommonContainer.JC_Purpose; }
		}

		public ZString UnpackGang
		{
			get { return CommonContainer.JC_UnpackGang; }
		}

		public ZString VolumeCapacityUQ
		{
			get { return CommonContainer.JC_VolumeCapacityUQ; }
		}

		public ZString WeightCapacityUQ
		{
			get { return CommonContainer.JC_WeightCapacityUQ; }
		}

		public ZString ReleaseNum
		{
			get { return CommonContainer.JC_ReleaseNum; }
		}

		public ZString ArrivalReleaseNum
		{
			get { return CommonContainer.JC_ContainerImportDORelease; }
		}

		public ZString TotalWeight
		{
			get { return FormatNumber(CommonContainer.JC_Calc_TotalWeight); }
		}

		public ZString BBKTotalWeight
		{
			get
			{
				if (Consol != null && (Consol.ConsolMode == Core.Constants.ContainerModes.BreakBulk || Consol.ConsolMode == Core.Constants.ContainerModes.Bulk || Consol.ConsolMode == Core.Constants.ContainerModes.Liquid))
				{
					return FormatNumber(CommonContainer.JC_Calc_TotalWeight / 1000M);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString HumidityPercent
		{
			get
			{
				ZString result = ZString.Empty;

				if (CommonContainer.JC_HumidityPercent != 0)
				{
					result = CommonContainer.JC_HumidityPercent.ToString();
				}

				return result;
			}
		}

		public ZString OLength
		{
			get
			{
				ZDecimal oLength = CommonContainer.JC_TotalLength - CommonContainer.JC_Calc_Length;
				return (oLength > 0) ? ZArchitecture.Core.Utilities.Round(Foot2Centimetre(oLength), 1).ToString() : "";
			}
		}

		public ZString OWidth
		{
			get
			{
				ZDecimal oWidth = CommonContainer.JC_TotalWidth - CommonContainer.JC_Calc_Width;
				return (oWidth > 0) ? ZArchitecture.Core.Utilities.Round(Foot2Centimetre(oWidth), 1).ToString() : "";
			}
		}

		public ZString OHeight
		{
			get
			{
				ZDecimal oHeight = CommonContainer.JC_TotalHeight - CommonContainer.JC_Calc_Height;
				return (oHeight > 0) ? ZArchitecture.Core.Utilities.Round(Foot2Centimetre(oHeight), 1).ToString() : "";
			}
		}

		public ZString CommodityDescription
		{
			get { return (Commodity != null) ? Commodity.Description : CommonContainer.JC_RH_NKContainerCommodityCode; }
		}

		public ZString UnpackShed
		{
			get { return CommonContainer.JC_UnpackShed; }
		}

		public ZString ContainerStorageLocation
		{
			get { return CommonContainer.JC_ContainerStorageLocation; }
		}

		public ZString TareWeightWithUQ
		{
			get { return ((ZDecimal)ZArchitecture.Core.Utilities.Round(TareWeight, 3)).ToStringTrimZeros() + " " + GrossWeightUQ; }
		}

		public ZString GrossWeightWithUQ
		{
			get { return ((ZDecimal)ZArchitecture.Core.Utilities.Round(GrossWeight, 3)).ToStringTrimZeros() + " " + GrossWeightUQ; }
		}

		public ZString ContainerPacklinesDescription
		{
			get
			{
				ZString result = "";

				foreach (PackLine packLine in CommonContainer.PackLines)
				{
					ZString packlineInfo = "";

					if (packLine.JL_PackageCount > 0)
					{
						packlineInfo += packLine.JL_PackageCount;
					}
					if (!packLine.JL_Description.IsEmpty)
					{
						if (!packlineInfo.IsEmpty)
						{
							packlineInfo += " ";
						}

						packlineInfo += packLine.JL_Description;
					}
					if (!packLine.JL_HarmonisedCode.IsEmpty)
					{
						if (!packlineInfo.IsEmpty)
						{
							packlineInfo += " ";
						}

						packlineInfo += packLine.JL_HarmonisedCode;
					}

					if (!packlineInfo.IsEmpty)
					{
						packlineInfo += "\n";
						result += packlineInfo;
					}
				}

				return result;
			}
		}

		public ZString WeightUQ
		{
			get { return CommonContainer.ContainerWeightUnit; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal TotalVolume
		{
			get { return CommonContainer.JC_Calc_TotalVolume; }
		}

		public ZDecimal ContainerCapacity
		{
			get { return CommonContainer.JC_Calc_ContainerCapacity; }
		}

		public ZDecimal MaxGrossWeight
		{
			get { return CommonContainer.JC_Calc_MaxGrossWeight; }
		}

		public ZDecimal CalcTareWeight
		{
			get { return CommonContainer.JC_Calc_TareWeight; }
		}

		public ZDecimal TareWeight
		{
			get { return CommonContainer.JC_TareWeight; }
		}

		public ZString TareWeightUQ
		{
			get { return CommonContainer.ContainerWeightUnit; }
		}

		public ZDecimal NetWeight
		{
			get { return CommonContainer.JC_Calc_NetWeight; }
		}

		public ZDecimal Length
		{
			get { return CommonContainer.JC_Calc_Length; }
		}

		public ZDecimal Width
		{
			get { return CommonContainer.JC_Calc_Width; }
		}

		public ZDecimal Height
		{
			get { return CommonContainer.JC_Calc_Height; }
		}

		public ZDecimal Demurrage
		{
			get { return CommonContainer.ArrivalTruckWaitCost; }
		}

		public ZDecimal DunnageWeight
		{
			get { return CommonContainer.JC_DunnageWeight; }
		}

		public ZDecimal GrossWeight
		{
			get { return CommonContainer.JC_GrossWeight; }
		}

		public ZDecimal GrossWeightKG
		{
			get { return Core.Constants.Weight.ConvertSafe(GrossWeight, GrossWeightUQ, "KG"); }
		}

		public ZDecimal TotalHeight
		{
			get { return CommonContainer.JC_TotalHeight; }
		}

		public ZDecimal TotalLength
		{
			get { return CommonContainer.JC_TotalLength; }
		}

		public ZDecimal TotalWidth
		{
			get { return CommonContainer.JC_TotalWidth; }
		}

		public ZDecimal VolumeCapacity
		{
			get { return CommonContainer.JC_VolumeCapacity; }
		}

		public ZDecimal WeightCapacity
		{
			get { return CommonContainer.JC_WeightCapacity; }
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalPackages
		{
			get { return CommonContainer.JC_Calc_TotalPackages; }
		}

		public ZShort BookingContainerCount
		{
			get { return CommonContainer.JC_ContainerCount; }
		}

		protected ZInt fQuantityCount;
		public ZInt QuantityCount
		{
			get { return fQuantityCount; }
			set { fQuantityCount = value; }
		}
		#endregion

		#region ZDateTime Fields

		#region Cartage Advice Fields

		public ZDateTime DropOffEmpty
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsExport)
				{
					result = EmptyRequired;
				}

				return result;
			}
		}

		public ZDateTime ReturnEmpty
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsImport)
				{
					result = EmptyReturnedBy;
				}

				return result;
			}
		}

		public ZDateTime PickUpFull
		{
			get { return IsExport ? DepartureEstimatedPickup : ZDateTime.Empty; }
		}

		#endregion

		public ZDateTime ArrivalCartageAdvised
		{
			get { return CommonContainer.JC_ArrivalCartageAdvised; }
		}

		public ZDateTime ArrivalCartageComplete
		{
			get { return CommonContainer.JC_ArrivalCartageComplete; }
		}

		public ZDateTime ContainerAvailable
		{
			get { return CommonContainer.JC_FCLAvailable; }
		}

		public ZDateTime DepartureCartageAdvised
		{
			get { return CommonContainer.JC_DepartureCartageAdvised; }
		}

		public ZDateTime DepartureCartageComplete
		{
			get { return CommonContainer.JC_DepartureCartageComplete; }
		}

		public ZDateTime DepartureEstimatedPickup
		{
			get { return CommonContainer.JC_DepartureEstimatedPickup; }
		}

		public ZDateTime ContainerParkEmptyReturnGateIn
		{
			get { return CommonContainer.JC_ContainerYardEmptyReturnGateIn; }
		}

		public ZDateTime EmptyRequired
		{
			get { return CommonContainer.JC_EmptyRequired; }
		}

		public ZDateTime EmptyReturnedBy
		{
			get { return CommonContainer.JC_EmptyReturnedBy; }
		}

		public ZDateTime EmptyReady
		{
			get { return CommonContainer.JC_EmptyReadyForReturn; }
		}

		public ZDateTime EstimatedDelivery
		{
			get { return CommonContainer.JC_ArrivalEstimatedDelivery; }
		}

		public ZDateTime FullPickDate
		{
			get { return CommonContainer.JC_DepartureEstimatedPickup; }
		}

		public ZDateTime LCLAvailable
		{
			get { return CommonContainer.JC_LCLAvailable; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return CommonContainer.JC_LCLStorageCommences; }
		}

		public ZDateTime LCLUnpack
		{
			get { return CommonContainer.JC_LCLUnpack; }
		}

		public ZDateTime PackDate
		{
			get { return CommonContainer.JC_PackDate; }
		}

		public ZDateTime SlotDate
		{
			get
			{
				return CommonContainer.JC_ArrivalSlotDateTime;
			}
		}

		public ZDateTime ArrivalSlotDate
		{
			get
			{
				return CommonContainer.JC_ArrivalSlotDateTime;
			}
		}

		public ZDateTime DepartureSlotDate
		{
			get
			{
				return CommonContainer.JC_DepartureSlotDateTime;
			}
		}

		public ZDateTime StorageCommences
		{
			get { return CommonContainer.JC_ArrivalCTOStorageStartDate; }
		}

		#endregion

		#region ZShort Fields

		public ZShort ContainerCount
		{
			get { return CommonContainer.JC_ContainerCount; }
		}

		#endregion

		#region ZBool Fields

		public ZBool IsCFSRegistered
		{
			get { return CommonContainer.JC_IsCFSRegistered; }
		}

		public ZBool IsChiller
		{
			get { return CommonContainer.IsChiller; }
		}

		public ZBool IsCleaningRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Cleaning); }
		}

		public ZBool IsControlledAtmosphere
		{
			get { return CommonContainer.JC_IsControlledAtmosphere; }
		}

		public ZBool IsCustomsHold
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		public ZBool IsDamaged
		{
			get { return CommonContainer.JC_IsDamaged; }
		}

		public ZBool IsEmptyContainer
		{
			get { return CommonContainer.JC_IsEmptyContainer; }
		}

		public ZBool IsExtraInspectionRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.ExtraInspection); }
		}

		public ZBool IsFrozen
		{
			get { return CommonContainer.IsFreezer; }
		}

		public ZBool IsFumigationRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		public ZBool IsQuarantineRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		public ZBool IsQuarantineUnpackRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.QuarantineUnpack); }
		}

		public ZBool IsSealOk
		{
			get { return CommonContainer.JC_IsSealOk; }
		}

		public ZBool IsSteamCleanRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.SteamCleaning); }
		}

		public ZBool IsTailgateRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Tailgate); }
		}

		public ZBool IsWashingRequired
		{
			get { return CommonContainer.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Washing); }
		}

		#endregion

		#region Wrapper Fields

		public DocForwardingConsol Consol
		{
			get
			{
				return fConsol ?? (CommonContainer.JC_JK.IsValid
														? (fConsol = DocForwardingConsol.New(CommonContainer.Factory, CommonContainer.JC_JK))
														: null);
			}
		}
		DocForwardingConsol fConsol;

		public DocSailing Sailing
		{
			get { return DocSailing.New(CommonContainer.Sailing, Factory); }
		}

		#region Addresses

		public DocDocAddress ArrivalContainerParkAddress
		{
			get { return DocDocAddress.New(CommonContainer.ArrivalContainerYardAddress, Factory); }
		}

		public DocDocAddress ArrivalCTOAddress
		{
			get { return (Consol != null) ? Consol.ArrivalCTOAddress : null; }
		}

		public DocDocAddress ArrivalUnpackAddress
		{
			get { return (Consol != null) ? Consol.UnpackDepotAddress : null; }
		}

		public DocDocAddress DepartureContainerParkAddress
		{
			get { return DocDocAddress.New(CommonContainer.DepartureContainerYardAddress, Factory); }
		}

		public DocDocAddress DepartureCTOAddress
		{
			get { return (Consol != null) ? Consol.DepartureCTOAddress : null; }
		}

		public DocDocAddress DeparturePackAddress
		{
			get { return (Consol != null) ? Consol.PackDepotAddress : null; }
		}

		#endregion

		public virtual DocOrganisation ArrivalTransport
		{
			get
			{
				DocOrganisation result = null;
				if (ArrivalCFSConfirm != null)
				{
					result = ArrivalCFSConfirm.TransportCo;
				}
				return result;
			}
		}

		public virtual DocOrganisation DepartureTransport
		{
			get
			{
				DocOrganisation result = null;
				if (DepartureCFSConfirm != null)
				{
					result = DepartureCFSConfirm.TransportCo;
				}
				return result;
			}
		}

		public DocPickupDeliveryConfirm ArrivalCFSConfirm
		{
			get { return DocPickupDeliveryConfirm.New(IsExport ? CommonContainer.OriginCFSArrival : CommonContainer.DestinationCFSArrival, Factory); }
		}

		public DocPickupDeliveryConfirm DepartureCFSConfirm
		{
			get { return DocPickupDeliveryConfirm.New(IsExport ? CommonContainer.OriginCFSDeparture : CommonContainer.DestinationCFSDeparture, Factory); }
		}

		public DocOrganisation CFSClient
		{
			get { return DocOrganisation.New(CommonContainer.CFSClient, Factory); }
		}

		public DocRefContainer Container
		{
			get { return DocRefContainer.New(CommonContainer.RefContainer, Factory); }
		}

		public DocCommodity Commodity
		{
			get { return DocCommodity.New(CommonContainer.Factory, CommonContainer.JC_RH_NKContainerCommodityCode); }
		}

		public DocOrganisation ShippingLine
		{
			get
			{
				if (Consol != null)
				{
					return Consol.ShippingLine;
				}
				return null;
			}
		}

		#endregion

		#region Implementation

		protected CommonContainer CommonContainer
		{
			get { return (CommonContainer)WrappedObject; }
		}

		ZDecimal Foot2Centimetre(ZDecimal value)
		{
			return value * 30.48M;
		}

		public ZBool IsExport
		{
			get { return IsExportDocument; }
		}

		public ZBool IsImport
		{
			get { return IsImportDocument; }
		}

		#endregion

		#region IDocSimpleContainer Members

		public ZString ClientRef
		{
			get { return ZString.Empty; }
		}

		public ZString Type
		{
			get { return ContainerMode; }
		}

		public ZInt TotalAllocatedJobPackages
		{
			get { return TotalAllocatedShipmentPackages; }
		}

		public ZDecimal TotalAllocatedJobWeight
		{
			get { return TotalAllocatedShipmentWeight; }
		}

		public ZDecimal TotalAllocatedJobVolume
		{
			get { return TotalAllocatedShipmentVolume; }
		}
		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return ContainerNumber; }
		}

		#endregion

		#region Additional CommonCartage Cover/Summary Sheet Requirements
		public ZString ContainerType
		{
			get { return (CommonContainer.Container != null) ? CommonContainer.Container.RC_Code : ContainerMode; }
		}

		public ZString SlotArrivalReference
		{
			get { return CommonContainer.JC_ArrivalSlotReference; }
		}

		public ZDateTime SlotArrivalTime
		{
			get { return CommonContainer.JC_ArrivalSlotDateTime; }
		}

		public ZString SlotDepartureReference
		{
			get { return CommonContainer.JC_DepartureSlotReference; }
		}

		public ZDateTime SlotDepartureTime
		{
			get { return CommonContainer.JC_DepartureSlotDateTime; }
		}

		public ZString SlotArrivalDetails
		{
			get
			{
				ZString result = SlotArrivalReference.IsEmpty ? (ZString)" - " : SlotArrivalReference;
				if (SlotArrivalTime.IsValid)
				{
					result += " / " + SlotArrivalTime.ToShortDateString() + " " + SlotArrivalTime.ToShortTimeString();
				}
				return result;
			}
		}

		public ZString SlotDepartureDetails
		{
			get
			{
				ZString result = SlotDepartureReference.IsEmpty ? (ZString)" - " : SlotDepartureReference;
				if (SlotDepartureTime.IsValid)
				{
					result += " / " + SlotDepartureTime.ToShortDateString() + " " + SlotDepartureTime.ToShortTimeString();
				}
				return result;
			}
		}

		public ZString SlotAsArrivalOrDeparture
		{
			get
			{
				string result = " - ";

				if (IsExport)
				{
					result = "D";
				}
				else if (IsImport)
				{
					result = "A";
				}
				return result;
			}
		}

		#endregion

		#region IDoc Cartage Advice

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("8b105814-1c6d-452d-b18f-648637b44c03", "PICKUP");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5d30c886-c081-4fe2-a318-4e1c1c26b4e8", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7d2e8414-d7f2-4d3b-b5b3-d15458a2395b", "FULL"));
				}

				if (IsExport)
				{
					if (!ReleaseNum.IsEmpty)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4de3bcc6-183c-4ca2-ad51-67a80e7745e2", "REF. {0}", ReleaseNum));
					}
				}
				else if (IsImport && (!SlotArrivalReference.IsEmpty || SlotArrivalTime.IsValid))
				{
					string date = SlotArrivalTime.IsValid ? SlotArrivalTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("0202aa75-dc68-43b8-817b-034f52616617", "SLOT REF. {0} / {1}", SlotArrivalReference, date));
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("29d53b35-39d4-42a6-858f-2b6dbfbf52bb", "DELIVER TO");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5d30c886-c081-4fe2-a318-4e1c1c26b4e8", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7d2e8414-d7f2-4d3b-b5b3-d15458a2395b", "FULL"));
				}

				if (IsExport && DropOffEmpty.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("c1d89099-e00d-4daf-a039-eb7d3662bd81", "DATE {0}", DropOffEmpty.ToLongTimeString()));
				}
				else if (IsImport && EstimatedDelivery.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("c1d89099-e00d-4daf-a039-eb7d3662bd81", "DATE {0}", EstimatedDelivery.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("8b105814-1c6d-452d-b18f-648637b44c03", "PICKUP");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5d30c886-c081-4fe2-a318-4e1c1c26b4e8", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7d2e8414-d7f2-4d3b-b5b3-d15458a2395b", "FULL"));
				}

				if (IsExport && PickUpFull.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("c1d89099-e00d-4daf-a039-eb7d3662bd81", "DATE {0}", PickUpFull.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("094b629f-36b7-49f6-96bc-e04258ea8d58", "DELIVER TO");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5d30c886-c081-4fe2-a318-4e1c1c26b4e8", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7d2e8414-d7f2-4d3b-b5b3-d15458a2395b", "FULL"));
				}

				if (IsExport && (!SlotDepartureReference.IsEmpty || SlotDepartureTime.IsValid))
				{
					string date = SlotDepartureTime.IsValid ? SlotDepartureTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("a49c9601-aee2-45e0-975b-837bb4b4f097", "SLOT REF. {0} / {1}", SlotDepartureReference, date));
				}
				else if (IsImport && ReturnEmpty.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("c1d89099-e00d-4daf-a039-eb7d3662bd81", "DATE {0}", ReturnEmpty.ToLongTimeString()));
				}

				return result;
			}
		}

		#endregion

		#region Addresses (Base)

		#region JourneyOneDeliverToAddress

		public virtual DocDocAddress JourneyOneDeliverToAddress
		{
			get
			{
				if (fJourneyOneDeliverToAddress == null)
				{
					if (IsExport)
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForExport;
					}
					else if (IsImport)
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForImport;
					}
				}
				return fJourneyOneDeliverToAddress;
			}
		}
		DocDocAddress fJourneyOneDeliverToAddress;

		#endregion

		#region JourneyTwoPickUpAddress

		public virtual DocDocAddress JourneyTwoPickUpAddress
		{
			get
			{
				if (fJourneyTwoPickUpAddress == null)
				{
					if (IsExport)
					{
						fJourneyTwoPickUpAddress = JourneyTwoPickUpAddressForExport;
					}
					else if (IsImport)
					{
						fJourneyTwoPickUpAddress = JourneyTwoPickUpAddressForImport;
					}
				}
				return fJourneyTwoPickUpAddress;
			}
		}
		DocDocAddress fJourneyTwoPickUpAddress;

		#endregion

		#endregion

		#region Addresses (Abstract)

		public abstract DocDocAddress JourneyOnePickUpAddress { get; }
		public abstract DocDocAddress JourneyOneDeliverToAddressForExport { get; }
		public abstract DocDocAddress JourneyOneDeliverToAddressForImport { get; }
		public abstract DocDocAddress JourneyTwoPickUpAddressForExport { get; }
		public abstract DocDocAddress JourneyTwoPickUpAddressForImport { get; }
		public abstract DocDocAddress JourneyTwoDeliverToAddress { get; }

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#endregion

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);

				if (JourneyOnePickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
				}

				if (JourneyOneDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
				}

				if (JourneyTwoPickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
				}

				if (JourneyTwoDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
				}

				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public virtual DocOrganisation Consignee
		{
			get { return null; }
		}

		public virtual DocOrganisation Consignor
		{
			get { return (IsCFSCartageAdvice) ? CFSClient : null; }
		}

		public ZBool PrintAsContainers
		{
			get { return true; }
		}

		public ZBool PrintTwoJourneys
		{
			get
			{
				return true;
			}
		}

		public ZBool IsAir
		{
			get { return Consol != null && Consol.IsAir; }
		}

		#region Implementation

		public ZBool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsExport : IsImport);
		}

		public ZBool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsImport : IsExport);
		}

		#endregion

		#region Other

		public ZBool IsCFSCartageAdvice
		{
			get { return ReportName.StartsWith("CFS"); }
		}

		#endregion

		#endregion
	}
}
