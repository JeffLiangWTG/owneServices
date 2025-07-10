using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromCartage : ContainerWrapperEmpty
	{
		public ContainerWrapperFromCartage(FreightWrapperFromCartage parentCartageWrapper, CommonContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? factory.GetNull<CommonContainer>();
			ParentCartageBO = parentCartageWrapper != null ? parentCartageWrapper.Cartage : Factory.GetNull<CommonCartage>();
			ParentCartageWrapper = parentCartageWrapper;
		}
		readonly CommonContainer ContainerBO;
		readonly CommonCartage ParentCartageBO;
		readonly FreightWrapperFromCartage ParentCartageWrapper;

		bool IsEmptyCartageLeg
		{
			get
			{
				bool result = false;

				if (ParentCartageWrapper != null)
				{
					var legs = ParentCartageWrapper.LocalTransportLegs;
					if (legs.Count == 1)
					{
						result = legs[0].LegBO.JU_IsEmptyContainer;
					}
				}

				return result;
			}
		}

		protected override FreightWrapper GetFreightJob()
		{
			FreightWrapper[] freightWrappers;

			if (ContainerBO.Consol != null)
			{
				freightWrappers = FreightWrapper.New(ContainerBO.Consol, Factory);
			}
			else if (ParentCartageBO.HasParent)
			{
				freightWrappers = FreightWrapper.New((BusinessObject)ParentCartageBO.CartageParent, Factory);
			}
			else
			{
				freightWrappers = FreightWrapper.New(ParentCartageBO, Factory);
			}

			return freightWrappers.Length > 0 ? freightWrappers[0] : null;
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
			return new WeightWrapper(ContainerBO.JC_TareWeight, ContainerBO.JC_GrossWeightUQ, decimals, BindToLists.GetCachedLists(Factory).WeightUnits, Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			if (IsEmptyCartageLeg)
			{
				return WeightWrapper.Empty;
			}
			else
			{
				int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_GrossWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
				return new WeightWrapper(WeightGross.Value - WeightTare.Value - WeightDunnage.Value, BookedMove.EW_WeightUQ, decimals, BindToLists.GetCachedLists(Factory).WeightUnits, Factory);
			}
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_DunnageWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(ContainerBO.JC_DunnageWeight, ContainerBO.JC_GrossWeightUQ, decimals, BindToLists.GetCachedLists(Factory).WeightUnits, Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			if (IsEmptyCartageLeg)
			{
				return WeightTare;
			}
			else
			{
				int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_GrossWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
				return new WeightWrapper(ContainerBO.JC_GrossWeight, ContainerBO.JC_GrossWeightUQ, decimals, BindToLists.GetCachedLists(Factory).WeightUnits, Factory);
			}
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			int decimals = (int)MetaData.GetMetaData(ContainerBO, ContainerBO.JC_Calc_TotalVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(ContainerBO.JC_Calc_TotalVolume, ContainerBO.JC_Calc_TotalVolumeUnit, decimals, ContainerBO.TotalVolumeUnit_List, Factory);
		}

		protected override ValueAndUnitWrapper GetPackCount()
		{
			return IsEmptyCartageLeg
				? ValueAndUnitWrapper.Empty
				: new ValueAndUnitWrapper(BookedMove.EW_BookedPackCount, BookedMove.EW_F3_NKPackType, BookedMove.Lookups.PackTypes, Factory);
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(ContainerBO, Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return new ValueAndUnitWrapper(ContainerBO.JC_SetPointTemp, ContainerBO.JC_SetPointTempUnit, 1, BindToLists.GetCachedLists(Factory).TemperatureUnits, Factory);
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return new ValueAndUnitWrapper(ContainerBO.JC_AirVentFlow, ContainerBO.JC_AirVentFlowRateUnit, 0, BindToLists.GetCachedLists(Factory).AirVentFlowRateUnits, Factory);
		}

		protected override AddressWrapper GetArrivalContainerYardAddress()
		{
			return new AddressWrapper(ContainerBO.ArrivalContainerYardAddress, ContactType.Miscellaneous, Factory);
		}

		protected override AddressWrapper GetDepartureContainerYardAddress()
		{
			JobDocAddress docAddress = null;
			var jobContainer = ContainerBO.JobContainer;
			if (jobContainer != null && !jobContainer.JC_OA_DepartureContainerYardAddress.IsEmpty)
			{
				docAddress = Factory.Load<JobDocAddress>(jobContainer.JC_OA_DepartureContainerYardAddress);
			}

			return new AddressWrapper(docAddress, Factory);
		}

		protected override ZBool GetPrintTACImage()
		{
			return ZBool.False;
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
			UNDGSubstanceWrapperCollection result = new UNDGSubstanceWrapperCollection(Factory);
			foreach (UNDGDataItem dgItem in BookedMove.UNDGs)
			{
				if (dgItem.Substance != null || !dgItem.DI_IMOClass.IsEmpty)
				{
					result.Add(new UNDGSubstanceWrapper(dgItem, Factory));
				}
			}
			return result;
		}

		CommonBookedCtgMove BookedMove
		{
			get
			{
				return ParentCartageBO.GetBookedMoves(ContainerBO).Length > 0 ? ParentCartageBO.GetBookedMoves(ContainerBO)[0] : Factory.GetNull<CommonBookedCtgMove>();
			}
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

		protected override LocationWrapper GetOffHirePort()
		{
			return new LocationWrapper(ZString.Empty, Factory);
		}

		protected override LocationWrapper GetOnHirePort()
		{
			return new LocationWrapper(ZString.Empty, Factory);
		}

		protected override ZString GetCreatedByUserName() => ZString.Empty;
	}
}
