using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromAgency : ContainerWrapperEmpty
	{
		public ContainerWrapperFromAgency(AgencyShipmentContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			this.containerBO = containerBO ?? factory.GetNull<AgencyShipmentContainer>();
		}

		protected override FreightWrapper GetFreightJob()
		{
			var freightWrappers = FreightWrapper.New(containerBO, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override CodeAndDescriptionWrapper GetContainerQuality()
		{
			return new CodeAndDescriptionWrapper(containerBO.JC_ContainerQuality, containerBO.Lookups.ContainerQualities, Factory);
		}

		protected override CodeAndDescriptionWrapper GetMode()
		{
			return new CodeAndDescriptionWrapper(containerBO.JC_ContainerMode, containerBO.JC_ContainerMode_List, Factory);
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(containerBO.Container, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			int decimals = (int)MetaData.GetMetaData(containerBO, containerBO.JC_TareWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(containerBO.JC_TareWeight, containerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			int decimals = (int)MetaData.GetMetaData(containerBO, containerBO.JC_Calc_NetWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(containerBO.JC_Calc_NetWeight - containerBO.JC_DunnageWeight, containerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			int decimals = (int)MetaData.GetMetaData(containerBO, containerBO.JC_DunnageWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(containerBO.JC_DunnageWeight, containerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			int decimals = (int)MetaData.GetMetaData(containerBO, containerBO.JC_GrossWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(containerBO.JC_GrossWeight, containerBO.JC_GrossWeightUQ, decimals, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			int decimals = (int)MetaData.GetMetaData(containerBO, containerBO.JC_Calc_TotalVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(containerBO.JC_Calc_TotalVolume, containerBO.JC_Calc_TotalVolumeUnit, decimals, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override ValueAndUnitWrapper GetPackCount()
		{
			return new ValueAndUnitWrapper(containerBO.JC_Calc_TotalPackages, containerBO.JC_Calc_TotalPackagesUnit, containerBO.TotalPackagesUnit_List, Factory);
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(containerBO, Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return new ValueAndUnitWrapper(containerBO.JC_SetPointTemp, containerBO.JC_SetPointTempUnit, 1, containerBO.JC_TemperatureUnit_List, Factory);
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return new ValueAndUnitWrapper(containerBO.JC_AirVentFlow, containerBO.JC_AirVentFlowRateUnit, 0, containerBO.BindToLists.AirVentFlowRateUnits, Factory);
		}

		protected override AddressWrapper GetArrivalContainerYardAddress()
		{
			return new AddressWrapper(containerBO.ArrivalContainerYardAddress, ContactType.Miscellaneous, Factory);
		}

		protected override AddressWrapper GetDepartureContainerYardAddress()
		{
			return new AddressWrapper(Factory.Load<OrgAddress>(containerBO.JC_OA_DepartureContainerYardAddress), ContactType.Miscellaneous, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(containerBO, Factory);
		}

		protected override ZBool GetPrintTACImage()
		{
			ZBool result = false;

			FreightWrapperFromAgencyShipment shipment = new FreightWrapperFromAgencyShipment(containerBO.Booking, Factory);
			AgencyShipment agencyShipment = containerBO.Booking;

			if (agencyShipment != null)
			{
				AgencyShipmentContainerDependentCollection containers =
					(containerBO.JC_Purpose == ContainerBookedStatus.Codes.Real ? agencyShipment.RealContainers : agencyShipment.BookedContainers);

				result = shipment.HasTACImage && containers.Count > 0 && containers[0].PK == containerBO.PK;
			}

			return result;
		}

		protected override ZBool GetIsHazardous()
		{
			if (containerBO.ContainerCommodityCode != null && containerBO.ContainerCommodityCode.RH_IsHazardous)
			{
				return true;
			}
			else
			{
				foreach (PackLine pack in containerBO.PackLines)
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
			return containerBO.JC_IsRefrigerated;
		}

		protected override ZString GetContainerNo()
		{
			return containerBO.JC_ContainerNum;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return containerBO.JC_ContainerCode;
		}

		protected override ZInt GetContainerCount()
		{
			return containerBO.JC_ContainerCount;
		}

		protected override ZString GetContainerJobID()
		{
			return containerBO.JC_ContainerJobID;
		}

		protected override ZString GetSealNo()
		{
			return containerBO.JC_SealNum;
		}

		protected override ZString GetSealNo2()
		{
			return containerBO.JC_AdditionalSealNum;
		}

		protected override ZString GetSealNo3()
		{
			return containerBO.JC_Additional2SealNum;
		}

		protected override ZString GetReleaseNumber()
		{
			return containerBO.JC_ReleaseNum;
		}

		protected override ZString GetArrivalReleaseNumber()
		{
			return containerBO.JC_ContainerImportDORelease;
		}

		protected override ZString GetArrivalCartageRef()
		{
			return containerBO.JC_ArrivalCartageRef;
		}

		protected override ZString GetDepartureCartageRef()
		{
			return containerBO.JC_DepartureCartageRef;
		}

		protected override ZDateTime GetEmptyReadyForReturn()
		{
			return containerBO.JC_EmptyReadyForReturn;
		}

		protected override ZDateTime GetEmptyReturnedBy()
		{
			return containerBO.JC_EmptyReturnedBy;
		}

		protected override ZDateTime GetEmptyRequired()
		{
			return containerBO.JC_EmptyRequired;
		}

		protected override ZString GetEmptyReturnReference()
		{
			return containerBO.JC_EmptyReturnReference;
		}

		protected override ZDateTime GetWharfGateOut()
		{
			return containerBO.JC_FCLWharfGateOut;
		}

		protected override ZDateTime GetContainerYardEmptyReturnGateIn()
		{
			return containerBO.JC_ContainerYardEmptyReturnGateIn;
		}

		protected override ZString GetBookingReference()
		{
			return containerBO.Booking != null ? containerBO.Booking.JS_UniqueConsignRef : ZString.Empty;
		}

		protected override ZDateTime GetArrivalEstimatedDelivery()
		{
			return containerBO.JC_ArrivalEstimatedDelivery;
		}

		protected override ZString GetArrivalSlotReference()
		{
			return containerBO.JC_ArrivalSlotReference;
		}

		protected override ZDateTime GetArrivalSlotTime()
		{
			return containerBO.JC_ArrivalSlotDateTime;
		}

		protected override ZString GetDepartureSlotReference()
		{
			return containerBO.JC_DepartureSlotReference;
		}

		protected override ZDateTime GetDepartureSlotTime()
		{
			return containerBO.JC_DepartureSlotDateTime;
		}

		protected override ZString GetExportDepotCustomsReference()
		{
			return containerBO.JC_ExportDepotCustomsReference;
		}

		protected override ZDateTime GetDepartureEstimatedPickup()
		{
			return containerBO.JC_DepartureEstimatedPickup;
		}

		protected override ZDecimal GetLength()
		{
			return containerBO.JC_TotalLength;
		}

		protected override ZDecimal GetWidth()
		{
			return containerBO.JC_TotalWidth;
		}

		protected override ZDecimal GetHeight()
		{
			return containerBO.JC_TotalHeight;
		}

		protected override ZBool GetDamaged()
		{
			return containerBO.JC_IsDamaged;
		}

		protected override ZBool GetFrozen()
		{
			return containerBO.IsFreezer;
		}

		protected override ZBool GetChilled()
		{
			return containerBO.IsChiller;
		}

		protected override ZBool GetControlledAtmosphere()
		{
			return containerBO.JC_IsControlledAtmosphere;
		}

		protected override ZByte GetHumidityPercentage()
		{
			return containerBO.JC_HumidityPercent;
		}

		protected override ZString GetClipOnUnit()
		{
			return containerBO.JC_RefrigGeneratorID;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(containerBO.Services, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(containerBO.PackLines.ToArray<PackLine>(), Factory);
		}

		protected override DetentionWrapper GetImportDetention()
		{
			AgencyShipment shipment = containerBO.Booking;
			Transport transport = shipment == null ? null : shipment.TransportsIncludingRelated.ArrivalTransport;

			var emptyReturnedBy = containerBO.JC_EmptyReturnedBy;
			if (containerBO != null && emptyReturnedBy == ZDate.Empty)
			{
				var strategy = containerBO.NewContainerDefaultingStrategy();
				emptyReturnedBy = strategy.CalculateRequiredBy().returnedBy;
			}

			return new DetentionWrapper(
				transport == null ? ZDateTime.Empty : transport.JW_TerminalAvailabilityDate,
				emptyReturnedBy,
				containerBO.JC_ContainerYardEmptyReturnGateIn,
				containerBO.Booking != null ? containerBO.Booking.JS_RL_NKDestination : ZString.Empty,
				Factory);
		}

		protected override DetentionWrapper GetExportDetention()
		{
			return new DetentionWrapper(
				containerBO.JC_ContainerYardEmptyPickupGateOut,
				containerBO.JC_Calc_ExportDetentionFreeDays,
				containerBO.JC_FCLWharfGateIn,
				containerBO.Booking != null ? containerBO.Booking.JS_RL_NKOrigin : ZString.Empty,
				Factory);
		}

		protected override ZString GetStowagePosition()
		{
			return containerBO.JC_StowagePosition;
		}

		#region Implementation

		readonly AgencyShipmentContainer containerBO;

		#endregion

		protected override ZDateTime GetVGMVerifiedDate()
		{
			return containerBO.JC_GrossWeightVerificationDateTime;
		}

		protected override CodeAndDescriptionWrapper GetVGMMethod()
		{
			return new CodeAndDescriptionWrapper(containerBO.JC_GrossWeightVerificationType, containerBO.Lookups.GrossWeightVerificationTypeList, Factory);
		}

		protected override AddressWrapper GetVGMVerifiedByAddress()
		{
			return new AddressWrapper(containerBO.GrossWeightVerifiedByAddress, Factory);
		}
	}
}
