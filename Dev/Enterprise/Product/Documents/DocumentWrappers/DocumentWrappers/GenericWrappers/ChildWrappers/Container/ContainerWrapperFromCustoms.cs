using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromCustoms : ContainerWrapperEmpty
	{
		public ContainerWrapperFromCustoms(BaseCusContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? factory.GetNull<BaseCusContainer>();
		}
		readonly BaseCusContainer ContainerBO;

		protected override FreightWrapper GetFreightJob()
		{
			var freightWrappers = FreightWrapper.New(ContainerBO.Declaration, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override CodeAndDescriptionWrapper GetMode()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.CO_FCL_LCL_AIR, ContainerBO.Lookups.CO_FCL_LCL_NCT_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetDeliveryMode()
		{
			CommonContainer jobContainer = ContainerBO.JobContainer;
			return jobContainer != null
				? new CodeAndDescriptionWrapper(jobContainer.JC_DeliveryMode, jobContainer.JC_DeliveryMode_List, Factory)
				: CodeAndDescriptionWrapper.Empty;
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(ContainerBO.Container, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(ContainerBO.TareWeight, Core.Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			return new WeightWrapper(ContainerBO.CO_Weight, ContainerBO.CO_WeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			return new WeightWrapper(ContainerBO.DunnageWeight, Core.Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			return new WeightWrapper(ContainerBO.GrossWeight, ContainerBO.GrossWeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override ValueAndUnitWrapper GetPackCount()
		{
			return ContainerBO == null ? new ValueAndUnitWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), Factory)
				: new ValueAndUnitWrapper(ContainerBO.CO_Calc_TotalPackages, ContainerBO.CO_Calc_TotalPackagesUnit, ContainerBO.Lookups.TotalPackagesUnit_List, Factory);
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(ContainerBO.JobContainer, Factory);
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return new ValueAndUnitWrapper(ContainerBO.AirVentFlow, ContainerBO.AirVentFlowRateUnit, 0, ContainerBO.Lookups.AirVentFlowRateUnit_List, Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return new ValueAndUnitWrapper(ContainerBO.SetPointTemp, ContainerBO.SetPointTempUnit, 1, ContainerBO.Lookups.TemperatureUnit_List, Factory);
		}

		protected override AddressWrapper GetArrivalContainerYardAddress()
		{
			return new AddressWrapper(ContainerBO.JobContainer.ArrivalContainerYardAddress, ContactType.Miscellaneous, Factory);
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

		protected override ZBool GetIsHazardous()
		{
			if (ContainerBO.JobContainer.ContainerCommodityCode != null && ContainerBO.JobContainer.ContainerCommodityCode.RH_IsHazardous)
			{
				return true;
			}
			else
			{
				foreach (PackLine pack in ContainerBO.JobContainer.PackLines)
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
			return ContainerBO.JobContainer.JC_IsRefrigerated;
		}

		protected override ZString GetContainerNo()
		{
			return ContainerBO.CO_ContainerNumber;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			if (!ContainerNo.IsEmpty)
			{
				return ContainerNo;
			}
			else if (!Type.Code.IsEmpty && ContainerBO.JobContainer != null)
			{
				return ZString.Format("{0} ({1})", Type.Code, ContainerBO.JobContainer.JC_ContainerCount);
			}

			return ZString.Empty;
		}

		protected override ZInt GetContainerCount()
		{
			return ContainerBO.JobContainer.JC_ContainerCount;
		}

		protected override ZString GetContainerJobID()
		{
			if (ContainerBO.JobContainer != null)
			{
				return ContainerBO.JobContainer.JC_ContainerJobID;
			}
			return ZString.Empty;
		}

		protected override ZString GetSealNo()
		{
			return ContainerBO.CO_Seal;
		}

		protected override ZString GetSealNo2()
		{
			return ContainerBO.CO_SecondSeal;
		}

		protected override ZString GetReleaseNumber()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZString.Empty : container.JC_ReleaseNum;
		}

		protected override ZString GetArrivalReleaseNumber()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZString.Empty : container.JC_ContainerImportDORelease;
		}

		protected override ZString GetArrivalCartageRef()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZString.Empty : container.JC_ArrivalCartageRef;
		}

		protected override ZString GetDepartureCartageRef()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZString.Empty : container.JC_DepartureCartageRef;
		}

		protected override ZDateTime GetEmptyReadyForReturn()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZDateTime.Empty : container.JC_EmptyReadyForReturn;
		}

		protected override ZDateTime GetEmptyReturnedBy()
		{
			return ContainerBO.EmptyReturnedBy;
		}

		protected override ZDateTime GetEmptyRequired()
		{
			return ContainerBO.EmptyRequired;
		}

		protected override ZString GetEmptyReturnReference()
		{
			var container = ContainerBO.JobContainer;
			return container == null ? ZString.Empty : container.JC_EmptyReturnReference;
		}

		protected override ZDateTime GetWharfGateOut()
		{
			CommonContainer container = ContainerBO.JobContainer;
			return container == null ? ZDateTime.Empty : container.JC_FCLWharfGateOut;
		}

		protected override ZDateTime GetContainerYardEmptyReturnGateIn()
		{
			return ContainerBO.ContainerYardEmptyReturnGateIn;
		}

		protected override ZString GetBookingReference()
		{
			return ContainerBO.JobContainer != null && ContainerBO.JobContainer.Consol != null ? ContainerBO.JobContainer.Consol.JK_BookingReference : ZString.Empty;
		}

		protected override ZDateTime GetArrivalEstimatedDelivery()
		{
			return ContainerBO.ArrivalEstimatedDelivery;
		}

		protected override ZString GetArrivalSlotReference()
		{
			return ContainerBO.ArrivalSlotReference;
		}

		protected override ZDateTime GetArrivalSlotTime()
		{
			return ContainerBO.ArrivalSlotDateTime;
		}

		protected override ZString GetDepartureSlotReference()
		{
			return ContainerBO.DepartureSlotReference;
		}

		protected override ZDateTime GetDepartureSlotTime()
		{
			return ContainerBO.DepartureSlotDateTime;
		}

		protected override ZString GetExportDepotCustomsReference()
		{
			CommonContainer jobContainer = ContainerBO.JobContainer;
			return jobContainer != null ? jobContainer.JC_ExportDepotCustomsReference : ZString.Empty;
		}

		protected override ZDateTime GetDepartureEstimatedPickup()
		{
			return ContainerBO.DepartureEstimatedPickup;
		}

		protected override ZDecimal GetLength()
		{
			return ContainerBO.TotalLength;
		}

		protected override ZDecimal GetWidth()
		{
			return ContainerBO.TotalWidth;
		}

		protected override ZDecimal GetHeight()
		{
			return ContainerBO.TotalHeight;
		}

		protected override ZBool GetDamaged()
		{
			CommonContainer jobContainer = ContainerBO.JobContainer;
			return jobContainer != null ? jobContainer.JC_IsDamaged : ZBool.False;
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
			return ContainerBO.IsControlledAtmosphere;
		}

		protected override ZByte GetHumidityPercentage()
		{
			return ContainerBO.HumidityPercent;
		}

		protected override ZString GetClipOnUnit()
		{
			return ContainerBO.RefrigGeneratorID;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(ContainerBO.JobContainer != null ? ContainerBO.JobContainer.Services : null, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return ContainerBO == null
				? new UNDGSubstanceWrapperCollection(null)
				: new UNDGSubstanceWrapperCollection(ContainerBO.Packages, Factory);
		}

		protected override DetentionWrapper GetImportDetention()
		{
			CommonContainer container = ContainerBO.JobContainer;

			if (container == null)
			{
				return DetentionWrapper.Empty;
			}
			else
			{
				return new DetentionWrapper(
					container.JC_FCLAvailable,
					container.JC_EmptyReturnedBy,
					container.JC_ContainerYardEmptyReturnGateIn,
					Factory);
			}
		}

		protected override ZString GetITReferencenumber()
		{
			var fwContainer = ContainerBO.JobContainer as ForwardingContainer;
			return fwContainer == null ? ZString.Empty : fwContainer.ITReferenceNumber;
		}

		protected override ZString GetAMSNumber()
		{
			var fwContainer = ContainerBO.JobContainer as ForwardingContainer;
			return fwContainer == null ? ZString.Empty : fwContainer.AMSNumber;
		}

		protected override ZString GetStowagePosition()
		{
			return ContainerBO.JobContainer.JC_StowagePosition;
		}

		protected override ZDateTime GetVGMVerifiedDate()
		{
			return ContainerBO.JobContainer.JC_GrossWeightVerificationDateTime;
		}

		protected override CodeAndDescriptionWrapper GetVGMMethod()
		{
			return new CodeAndDescriptionWrapper(ContainerBO.JobContainer.JC_GrossWeightVerificationType, ContainerBO.JobContainer.Lookups.GrossWeightVerificationTypeList, Factory);
		}

		protected override AddressWrapper GetVGMVerifiedByAddress()
		{
			return new AddressWrapper(ContainerBO.JobContainer.GrossWeightVerifiedByAddress, Factory);
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
