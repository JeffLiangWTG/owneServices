using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromFreight : ContainerWrapperEmpty
	{
		public ContainerWrapperFromFreight(CommonContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? factory.GetNull<CommonContainer>();
		}

		public ContainerWrapperFromFreight(CommonContainer containerBO, CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? factory.GetNull<CommonContainer>();
			ShipmentBO = shipmentBO ?? factory.GetNull<CommonShipment>();
		}
		readonly CommonShipment ShipmentBO;
		readonly CommonContainer ContainerBO;

		protected override FreightWrapper GetFreightJob()
		{
			var freightWrappers = FreightWrapper.New(ContainerBO.Consol, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override OrganisationWrapper GetCFSClient()
		{
			OrgHeader organisation = Factory.Load<OrgHeader>(ContainerBO.JC_OH_CFSClient);
			return new OrganisationWrapper(OrganisationUsageType.Client, organisation, ContactType.Miscellaneous, Factory);
		}

		public DocPickupDeliveryConfirm OriginConfirm
		{
			get { return DocPickupDeliveryConfirm.New(ContainerBO.OriginConfirm, Factory); }
		}

		protected override CodeAndDescriptionWrapper GetContainerQuality()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.JC_ContainerQuality, ContainerBO.Lookups.ContainerQualities, Factory);
		}

		protected override CodeAndDescriptionWrapper GetMode()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.JC_ContainerMode, ContainerBO.JC_ContainerMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetDeliveryMode()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.JC_DeliveryMode, ContainerBO.JC_DeliveryMode_List, Factory);
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(ContainerBO.Container, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_TareWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(ContainerBO.JC_TareWeight, ContainerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_Calc_TotalWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			if (ShipmentBO == null
				|| (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV) && ShipmentBO.JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead))
			{
				return new WeightWrapper(ContainerBO.JC_Calc_TotalWeight, ContainerBO.JC_Calc_TotalWeightUnit, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
			}
			else if (ShipmentBO.CoLoadShipments.Any())
			{
				return new WeightWrapper(((IPackLineCollection)new OuterPackLineCollectionView(ShipmentBO.OuterPackLines, ContainerBO)).Totals.TotalWeight, ContainerBO.JC_Calc_TotalWeightUnit, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
			}
			else
			{
				return new WeightWrapper(ContainerBO.GetTotalWeightByShipment(ShipmentBO), ContainerBO.JC_Calc_TotalWeightUnit, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
			}
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_DunnageWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(ContainerBO.JC_DunnageWeight, ContainerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_GrossWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(ContainerBO.JC_GrossWeight, ContainerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_Calc_TotalVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			if (ShipmentBO == null
				|| (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV) && ShipmentBO.JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead))
			{
				return new VolumeWrapper(ContainerBO.JC_Calc_TotalVolume, ContainerBO.JC_Calc_TotalVolumeUnit, decimals, ContainerBO.TotalVolumeUnit_List, Factory);
			}
			else if (ShipmentBO.CoLoadShipments.Any())
			{
				return new VolumeWrapper(((IPackLineCollection)new OuterPackLineCollectionView(ShipmentBO.OuterPackLines, ContainerBO)).Totals.TotalVolume, ContainerBO.JC_Calc_TotalWeightUnit, decimals, ContainerBO.TotalVolumeUnit_List, Factory);
			}
			else
			{
				return new VolumeWrapper(ContainerBO.GetTotalVolumeByShipment(ShipmentBO), ContainerBO.JC_Calc_TotalVolumeUnit, decimals, ContainerBO.TotalVolumeUnit_List, Factory);
			}
		}

		protected override ValueAndUnitWrapper GetPackCount()
		{
			if (ShipmentBO == null
				|| (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV) && ShipmentBO.JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead))
			{
				return new ValueAndUnitWrapper(ContainerBO.JC_Calc_TotalPackages, ContainerBO.JC_Calc_TotalPackagesUnit, ContainerBO.TotalPackagesUnit_List, Factory);
			}
			else if (ShipmentBO.CoLoadShipments.Any())
			{
				return new ValueAndUnitWrapper(((IPackLineCollection)new OuterPackLineCollectionView(ShipmentBO.OuterPackLines, ContainerBO)).Totals.TotalPackages, ContainerBO.JC_Calc_TotalPackagesUnit, ContainerBO.TotalPackagesUnit_List, Factory);
			}
			else
			{
				return new ValueAndUnitWrapper(ContainerBO.GetTotalPackagesByShipment(ShipmentBO), ContainerBO.JC_Calc_TotalPackagesUnit, ContainerBO.TotalPackagesUnit_List, Factory);
			}
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(ContainerBO, Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return new ValueAndUnitWrapper(ContainerBO.JC_SetPointTemp, ContainerBO.JC_SetPointTempUnit, 1, ContainerBO.JC_TemperatureUnit_List, Factory);
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return new ValueAndUnitWrapper(ContainerBO.JC_AirVentFlow, ContainerBO.JC_AirVentFlowRateUnit, 0, ContainerBO.BindToLists.AirVentFlowRateUnits, Factory);
		}

		protected override AddressWrapper GetArrivalContainerYardAddress()
		{
			return new AddressWrapper(ContainerBO.ArrivalContainerYardAddress, ContactType.LocalTransport, Factory);
		}

		protected override AddressWrapper GetDepartureContainerYardAddress()
		{
			return new AddressWrapper(Factory.Load<OrgAddress>(ContainerBO.JC_OA_DepartureContainerYardAddress), ContactType.LocalTransport, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ContainerBO, Factory);
		}

		protected override ZBool GetIsHazardous()
		{
			if (ContainerBO.ContainerCommodityCode != null && ContainerBO.ContainerCommodityCode.RH_IsHazardous)
			{
				return true;
			}
			else
			{
				foreach (PackLine pack in ContainerBO.PackLines)
				{
					if (pack.CommodityCode != null && pack.CommodityCode.RH_IsHazardous)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected override ZBool GetIsReefer()
		{
			return ContainerBO.JC_IsRefrigerated;
		}

		protected override ZString GetContainerNo()
		{
			return ContainerBO.JC_ContainerNum;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerBO.JC_ContainerCode;
		}

		protected override ZInt GetContainerCount()
		{
			return ContainerBO.JC_ContainerCount;
		}

		protected override ZString GetContainerJobID()
		{
			return ContainerBO.JC_ContainerJobID;
		}

		protected override ZString GetSealNo()
		{
			return ContainerBO.JC_SealNum;
		}

		protected override ZString GetSealNo2()
		{
			return ContainerBO.JC_AdditionalSealNum;
		}

		protected override ZString GetSealNo3()
		{
			return ContainerBO.JC_Additional2SealNum;
		}

		protected override ZString GetReleaseNumber()
		{
			return ContainerBO.JC_ReleaseNum;
		}

		protected override ZString GetArrivalReleaseNumber()
		{
			return ContainerBO.JC_ContainerImportDORelease;
		}

		protected override ZString GetArrivalCartageRef()
		{
			return ContainerBO.JC_ArrivalCartageRef;
		}

		protected override ZString GetDepartureCartageRef()
		{
			return ContainerBO.JC_DepartureCartageRef;
		}

		protected override ZDateTime GetEmptyReadyForReturn()
		{
			return ContainerBO.JC_EmptyReadyForReturn;
		}

		protected override ZDateTime GetEmptyReturnedBy()
		{
			return ContainerBO.JC_EmptyReturnedBy;
		}

		protected override ZDateTime GetEmptyRequired()
		{
			return ContainerBO.JC_EmptyRequired;
		}

		protected override ZString GetEmptyReturnReference()
		{
			return ContainerBO.JC_EmptyReturnReference;
		}

		protected override ZDateTime GetWharfGateOut()
		{
			return ContainerBO.JC_FCLWharfGateOut;
		}

		protected override ZDateTime GetContainerYardEmptyReturnGateIn()
		{
			return ContainerBO.JC_ContainerYardEmptyReturnGateIn;
		}

		protected override ZString GetBookingReference()
		{
			return ContainerBO.Consol != null ? ContainerBO.Consol.JK_BookingReference : ZString.Empty;
		}

		protected override ZDateTime GetArrivalEstimatedDelivery()
		{
			return ContainerBO.JC_ArrivalEstimatedDelivery;
		}

		protected override ZString GetArrivalSlotReference()
		{
			return ContainerBO.JC_ArrivalSlotReference;
		}

		protected override ZDateTime GetArrivalSlotTime()
		{
			return ContainerBO.JC_ArrivalSlotDateTime;
		}

		protected override ZString GetDepartureSlotReference()
		{
			return ContainerBO.JC_DepartureSlotReference;
		}

		protected override ZDateTime GetDepartureSlotTime()
		{
			return ContainerBO.JC_DepartureSlotDateTime;
		}

		protected override ZString GetExportDepotCustomsReference()
		{
			return ContainerBO.JC_ExportDepotCustomsReference;
		}

		protected override ZDateTime GetDepartureEstimatedPickup()
		{
			return ContainerBO.JC_DepartureEstimatedPickup;
		}

		protected override ZDecimal GetLength()
		{
			return ContainerBO.JC_TotalLength;
		}

		protected override ZDecimal GetWidth()
		{
			return ContainerBO.JC_TotalWidth;
		}

		protected override ZDecimal GetHeight()
		{
			return ContainerBO.JC_TotalHeight;
		}

		protected override ZBool GetDamaged()
		{
			return ContainerBO.JC_IsDamaged;
		}

		protected override ZBool GetFrozen()
		{
			return ContainerBO.IsFreezer;
		}

		protected override ZBool GetChilled()
		{
			return ContainerBO.IsChiller;
		}

		protected override ZBool GetControlledAtmosphere()
		{
			return ContainerBO.JC_IsControlledAtmosphere;
		}

		protected override ZByte GetHumidityPercentage()
		{
			return ContainerBO.JC_HumidityPercent;
		}

		protected override ZString GetClipOnUnit()
		{
			return ContainerBO.JC_RefrigGeneratorID;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(ContainerBO.Services, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection((PackLine[])ContainerBO.PackLines.ToArray(typeof(PackLine)), Factory);
		}

		protected override ZString GetUnpackShed()
		{
			return ContainerBO.JC_UnpackShed;
		}

		protected override DetentionWrapper GetImportDetention()
		{
			return new DetentionWrapper(
				ContainerBO.JC_FCLAvailable,
				ContainerBO.JC_EmptyReturnedBy,
				ContainerBO.JC_ContainerYardEmptyReturnGateIn,
				Factory);
		}

		protected override ZString GetITReferencenumber()
		{
			var fwContainer = ContainerBO as ForwardingContainer;
			return fwContainer != null ? fwContainer.ITReferenceNumber : ZString.Empty;
		}

		protected override ZString GetAMSNumber()
		{
			var fwContainer = ContainerBO as ForwardingContainer;
			return fwContainer != null ? fwContainer.AMSNumber : ZString.Empty;
		}

		protected override ZString GetStowagePosition()
		{
			return ContainerBO.JC_StowagePosition;
		}

		protected override ZDateTime GetVGMVerifiedDate()
		{
			return ContainerBO.JC_GrossWeightVerificationDateTime;
		}

		protected override CodeAndDescriptionWrapper GetVGMMethod()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.JC_GrossWeightVerificationType, ContainerBO.Lookups.GrossWeightVerificationTypeList, Factory);
		}

		protected override AddressWrapper GetVGMVerifiedByAddress()
		{
			return new AddressWrapper(ContainerBO.GrossWeightVerifiedByAddress, Factory);
		}
	}
}
