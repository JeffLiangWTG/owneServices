using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class ContainerWrapperEmpty : ContainerWrapper
	{
		protected ContainerWrapperEmpty(BusinessObject objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
		}

		protected override FreightWrapper GetFreightJob() => null;

		protected override OrganisationWrapper GetCFSClient() => null;

		protected override CodeAndDescriptionWrapper GetContainerQuality() => CodeAndDescriptionWrapper.Empty;

		protected override CodeAndDescriptionWrapper GetMode() => CodeAndDescriptionWrapper.Empty;

		protected override CodeAndDescriptionWrapper GetDeliveryMode() => CodeAndDescriptionWrapper.Empty;

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(null, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(ZDecimal.Zero, Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			return new WeightWrapper(ZDecimal.Zero, ZString.Empty, 3, new CodeDescriptionPairList(), null);
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			return new WeightWrapper(ZDecimal.Zero, ZString.Empty, 3, new CodeDescriptionPairList(), null);
		}

		protected override WeightWrapper GetWeightGross()
		{
			return new WeightWrapper(ZDecimal.Zero, ZString.Empty, 3, new CodeDescriptionPairList(), null);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			return new VolumeWrapper(ZDecimal.Zero, ZString.Empty, new CodeDescriptionPairList(), null);
		}

		protected override ValueAndUnitWrapper GetPackCount() => ValueAndUnitWrapper.Empty;

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature() => ValueAndUnitWrapper.Empty;

		protected override ValueAndUnitWrapper GetAirVentFlow() => ValueAndUnitWrapper.Empty;

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection((GenericWrapper)null, null);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override AddressWrapper GetDepartureContainerYardAddress()
		{
			return AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetArrivalContainerYardAddress()
		{
			return AddressWrapper.Empty(Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(Factory);
		}

		protected override ZBool GetPrintTACImage() => ZBool.False;

		protected override ZBool GetIsHazardous() => ZBool.False;

		protected override ZBool GetIsReefer() => ZBool.False;

		protected override ZString GetContainerNo() => ZString.Empty;

		protected override ZString GetContainerNumberOrTypeCount() => ZString.Empty;

		protected override ZInt GetContainerCount() => ZInt.Zero;

		protected override ZString GetContainerJobID() => ZString.Empty;

		protected override ZString GetSealNo() => ZString.Empty;

		protected override ZString GetSealNo2() => ZString.Empty;

		protected override ZString GetSealNo3() => ZString.Empty;

		protected override ZString GetArrivalCartageRef() => ZString.Empty;

		protected override ZString GetDepartureCartageRef() => ZString.Empty;

		protected override ZString GetReleaseNumber() => ZString.Empty;

		protected override ZString GetArrivalReleaseNumber() => ZString.Empty;

		protected override ZString GetITReferencenumber() => ZString.Empty;

		protected override ZString GetAMSNumber() => ZString.Empty;

		protected override ZDateTime GetEmptyReadyForReturn() => ZDateTime.Empty;

		protected override ZDateTime GetEmptyReturnedBy() => ZDateTime.Empty;

		protected override ZDateTime GetEmptyRequired() => ZDateTime.Empty;

		protected override ZString GetEmptyReturnReference() => ZString.Empty;

		protected override ZDateTime GetWharfGateOut() => ZDateTime.Empty;

		protected override ZDateTime GetContainerYardEmptyReturnGateIn() => ZDateTime.Empty;

		protected override ZString GetBookingReference() => ZString.Empty;

		protected override ZDateTime GetArrivalEstimatedDelivery() => ZDateTime.Empty;

		protected override ZString GetArrivalSlotReference() => ZString.Empty;

		protected override ZDateTime GetArrivalSlotTime() => ZDateTime.Empty;

		protected override ZString GetDepartureSlotReference() => ZString.Empty;

		protected override ZDateTime GetDepartureSlotTime() => ZDateTime.Empty;

		protected override ZString GetExportDepotCustomsReference() => ZString.Empty;

		protected override ZDateTime GetDepartureEstimatedPickup() => ZDateTime.Empty;

		protected override ZDecimal GetLength() => ZDecimal.Zero;

		protected override ZDecimal GetWidth() => ZDecimal.Zero;

		protected override ZDecimal GetHeight() => ZDecimal.Zero;

		protected override ZBool GetDamaged() => ZBool.False;

		protected override ZBool GetFrozen() => ZBool.False;

		protected override ZBool GetChilled() => ZBool.False;

		protected override ZBool GetControlledAtmosphere() => ZBool.False;

		protected override ZByte GetHumidityPercentage() => ZByte.Zero;

		protected override ZString GetClipOnUnit() => ZString.Empty;

		protected override bool? GetIsPalletized() => false;

		protected override bool? GetIsIsChargeable() => false;

		protected override ZString GetPackages() => ZInt.Zero.ToString();

		protected override ZString GetPallets() => ZInt.Zero.ToString();

		protected override ZString GetUnpackShed() => ZString.Empty;

		protected override DetentionWrapper GetImportDetention() => DetentionWrapper.Empty;

		protected override DetentionWrapper GetExportDetention() => DetentionWrapper.Empty;

		protected override ZString GetStowagePosition() => ZString.Empty;

		protected override ZDateTime GetVGMVerifiedDate() => ZDateTime.Empty;

		protected override CodeAndDescriptionWrapper GetVGMMethod() => CodeAndDescriptionWrapper.Empty;

		protected override AddressWrapper GetVGMVerifiedByAddress()
		{
			return AddressWrapper.Empty(Factory);
		}

		protected override OrganisationWrapper GetOwner()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetStatus() => CodeAndDescriptionWrapper.Empty;

		protected override ZDateTime GetGateInDate() => ZDateTime.Empty;

		protected override ZDateTime GetGateOutDate() => ZDateTime.Empty;

		protected override ZDateTime GetOffHireDate() => ZDateTime.Empty;

		protected override ZDateTime GetOnHireDate() => ZDateTime.Empty;

		protected override ZDateTime GetManufactureDate() => ZDateTime.Empty;

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
