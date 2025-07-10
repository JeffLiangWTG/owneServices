using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("ContainerNo"), WrapperTypeName("Container")]
	public abstract class ContainerWrapper : GenericWrapper
	{
		protected ContainerWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public FreightWrapper FreightJob
		{
			get { return GetFreightJob(); }
		}
		protected abstract FreightWrapper GetFreightJob();

		public OrganisationWrapper CFSClient
		{
			get { return GetCFSClient(); }
		}
		protected abstract OrganisationWrapper GetCFSClient();

		public CodeAndDescriptionWrapper ContainerQuality
		{
			get { return GetContainerQuality(); }
		}
		protected abstract CodeAndDescriptionWrapper GetContainerQuality();

		public CodeAndDescriptionWrapper Mode
		{
			get { return fMode ?? (fMode = GetMode()); }
		}
		CodeAndDescriptionWrapper fMode;
		protected abstract CodeAndDescriptionWrapper GetMode();

		public CodeAndDescriptionWrapper DeliveryMode
		{
			get { return fDeliveryMode ?? (fDeliveryMode = GetDeliveryMode()); }
		}
		CodeAndDescriptionWrapper fDeliveryMode;
		protected abstract CodeAndDescriptionWrapper GetDeliveryMode();

		public ContainerTypeWrapper Type
		{
			get { return fType ?? (fType = GetTypeWrapper()); }
		}
		ContainerTypeWrapper fType;
		protected abstract ContainerTypeWrapper GetTypeWrapper();

		public WeightWrapper WeightTare
		{
			get { return fWeightTare ?? (fWeightTare = GetWeightTare()); }
		}
		WeightWrapper fWeightTare;
		protected abstract WeightWrapper GetWeightTare();

		public WeightWrapper WeightGoods
		{
			get { return fWeightGoods ?? (fWeightGoods = GetWeightGoods()); }
		}
		WeightWrapper fWeightGoods;
		protected abstract WeightWrapper GetWeightGoods();

		public WeightWrapper WeightDunnage
		{
			get { return fWeightDunnage ?? (fWeightDunnage = GetWeightDunnage()); }
		}
		WeightWrapper fWeightDunnage;
		protected abstract WeightWrapper GetWeightDunnage();

		public WeightWrapper WeightGross
		{
			get { return fWeightGross ?? (fWeightGross = GetWeightGross()); }
		}
		WeightWrapper fWeightGross;
		protected abstract WeightWrapper GetWeightGross();

		public VolumeWrapper VolumeGoods
		{
			get { return fVolumeGoods ?? (fVolumeGoods = GetVolumeGoods()); }
		}
		VolumeWrapper fVolumeGoods;
		protected abstract VolumeWrapper GetVolumeGoods();

		public ValueAndUnitWrapper PackCount
		{
			get { return fPackCount ?? (fPackCount = GetPackCount()); }
		}
		ValueAndUnitWrapper fPackCount;
		protected abstract ValueAndUnitWrapper GetPackCount();

		public CommodityWrapperCollection Commodities
		{
			get { return commodities ?? (commodities = GetCommodities()); }
		}
		CommodityWrapperCollection commodities;
		protected abstract CommodityWrapperCollection GetCommodities();

		public ValueAndUnitWrapper SetPointTemperature
		{
			get { return fSetPointTemperature ?? (fSetPointTemperature = GetSetPointTemperature()); }
		}
		ValueAndUnitWrapper fSetPointTemperature;
		protected abstract ValueAndUnitWrapper GetSetPointTemperature();

		public ValueAndUnitWrapper AirVentFlow
		{
			get { return fAirVentFlow ?? (fAirVentFlow = GetAirVentFlow()); }
		}
		ValueAndUnitWrapper fAirVentFlow;
		protected abstract ValueAndUnitWrapper GetAirVentFlow();

		public ServiceWrapperCollection Services
		{
			get { return fServices ?? (fServices = GetServices()); }
		}
		ServiceWrapperCollection fServices;
		protected abstract ServiceWrapperCollection GetServices();

		public UNDGSubstanceWrapperCollection UNDGSubstances
		{
			get { return fUNDGSubstances ?? (fUNDGSubstances = GetUNDGSubstances()); }
		}
		UNDGSubstanceWrapperCollection fUNDGSubstances;
		protected abstract UNDGSubstanceWrapperCollection GetUNDGSubstances();

		public AddressWrapper DepartureContainerYardAddress
		{
			get { return fDepartureContainerYardAddress ?? (fDepartureContainerYardAddress = GetDepartureContainerYardAddress()); }
		}
		AddressWrapper fDepartureContainerYardAddress;
		protected abstract AddressWrapper GetDepartureContainerYardAddress();

		public AddressWrapper ArrivalContainerYardAddress
		{
			get { return arrivalContainerYardAddress ?? (arrivalContainerYardAddress = GetArrivalContainerYardAddress()); }
		}
		AddressWrapper arrivalContainerYardAddress;
		protected abstract AddressWrapper GetArrivalContainerYardAddress();

		public CustomsEntryWrapperCollection CustomsEntries
		{
			get { return customsEntries ?? (customsEntries = GetCustomsEntries()); }
		}

		CustomsEntryWrapperCollection customsEntries;

		protected abstract CustomsEntryWrapperCollection GetCustomsEntries();

		public ZBool PrintTACImage
		{
			get { return GetPrintTACImage(); }
		}
		protected abstract ZBool GetPrintTACImage();

		public ZBool IsHazardous
		{
			get { return GetIsHazardous(); }
		}
		protected abstract ZBool GetIsHazardous();

		public ZBool IsReefer
		{
			get { return GetIsReefer(); }
		}
		protected abstract ZBool GetIsReefer();

		public ZString ContainerNo
		{
			get { return GetContainerNo(); }
		}
		protected abstract ZString GetContainerNo();

		public ZString ContainerNumberOrTypeCount
		{
			get { return GetContainerNumberOrTypeCount(); }
		}
		protected abstract ZString GetContainerNumberOrTypeCount();

		public ZInt ContainerCount
		{
			get { return GetContainerCount(); }
		}
		protected abstract ZInt GetContainerCount();

		public ZString ContainerJobID
		{
			get { return GetContainerJobID(); }
		}
		protected abstract ZString GetContainerJobID();

		public ZString SealNo
		{
			get { return GetSealNo(); }
		}
		protected abstract ZString GetSealNo();

		public ZString SealNo2
		{
			get { return GetSealNo2(); }
		}
		protected abstract ZString GetSealNo2();

		public ZString SealNo3
		{
			get { return GetSealNo3(); }
		}
		protected abstract ZString GetSealNo3();

		public ZString ArrivalCartageRef
		{
			get { return GetArrivalCartageRef(); }
		}
		protected abstract ZString GetArrivalCartageRef();

		public ZString DepartureCartageRef
		{
			get { return GetDepartureCartageRef(); }
		}
		protected abstract ZString GetDepartureCartageRef();

		public ZString ReleaseNumber
		{
			get { return GetReleaseNumber(); }
		}
		protected abstract ZString GetReleaseNumber();

		public ZString ArrivalReleaseNumber
		{
			get { return GetArrivalReleaseNumber(); }
		}
		protected abstract ZString GetArrivalReleaseNumber();

		public ZString ITReferenceNumber
		{
			get { return GetITReferencenumber(); }
		}
		protected abstract ZString GetITReferencenumber();

		public ZString AMSNumber
		{
			get { return GetAMSNumber(); }
		}
		protected abstract ZString GetAMSNumber();

		public ZDateTime EmptyReadyForReturn
		{
			get { return GetEmptyReadyForReturn(); }
		}
		protected abstract ZDateTime GetEmptyReadyForReturn();

		public ZDateTime EmptyReturnedBy
		{
			get { return GetEmptyReturnedBy(); }
		}
		protected abstract ZDateTime GetEmptyReturnedBy();

		public ZDateTime EmptyRequired
		{
			get { return GetEmptyRequired(); }
		}
		protected abstract ZDateTime GetEmptyRequired();

		public ZString EmptyReturnReference
		{
			get { return GetEmptyReturnReference(); }
		}
		protected abstract ZString GetEmptyReturnReference();

		public ZDateTime WharfGateOut
		{
			get { return GetWharfGateOut(); }
		}
		protected abstract ZDateTime GetWharfGateOut();

		public ZDateTime ContainerYardEmptyReturnGateIn
		{
			get { return GetContainerYardEmptyReturnGateIn(); }
		}
		protected abstract ZDateTime GetContainerYardEmptyReturnGateIn();

		public ZString BookingReference
		{
			get { return GetBookingReference(); }
		}
		protected abstract ZString GetBookingReference();

		public ZDateTime ArrivalEstimatedDelivery
		{
			get { return GetArrivalEstimatedDelivery(); }
		}
		protected abstract ZDateTime GetArrivalEstimatedDelivery();

		public ZString ArrivalSlotReference
		{
			get { return GetArrivalSlotReference(); }
		}
		protected abstract ZString GetArrivalSlotReference();

		public ZDateTime ArrivalSlotTime
		{
			get { return GetArrivalSlotTime(); }
		}
		protected abstract ZDateTime GetArrivalSlotTime();

		public ZString DepartureSlotReference
		{
			get { return GetDepartureSlotReference(); }
		}
		protected abstract ZString GetDepartureSlotReference();

		public ZDateTime DepartureSlotTime
		{
			get { return GetDepartureSlotTime(); }
		}
		protected abstract ZDateTime GetDepartureSlotTime();

		public ZString ExportDepotCustomsReference
		{
			get { return GetExportDepotCustomsReference(); }
		}
		protected abstract ZString GetExportDepotCustomsReference();

		public ZDateTime DepartureEstimatedPickup
		{
			get { return GetDepartureEstimatedPickup(); }
		}
		protected abstract ZDateTime GetDepartureEstimatedPickup();

		public ZDecimal Length
		{
			get { return GetLength(); }
		}
		protected abstract ZDecimal GetLength();

		public ZDecimal Width
		{
			get { return GetWidth(); }
		}
		protected abstract ZDecimal GetWidth();

		public ZDecimal Height
		{
			get { return GetHeight(); }
		}
		protected abstract ZDecimal GetHeight();

		public ZBool Damaged
		{
			get { return GetDamaged(); }
		}
		protected abstract ZBool GetDamaged();

		public ZBool Frozen
		{
			get { return GetFrozen(); }
		}
		protected abstract ZBool GetFrozen();

		public ZBool Chilled
		{
			get { return GetChilled(); }
		}
		protected abstract ZBool GetChilled();

		public ZBool ControlledAtmosphere
		{
			get { return GetControlledAtmosphere(); }
		}
		protected abstract ZBool GetControlledAtmosphere();

		public ZByte HumidityPercentage
		{
			get { return GetHumidityPercentage(); }
		}
		protected abstract ZByte GetHumidityPercentage();

		public ZString ClipOnUnit
		{
			get { return GetClipOnUnit(); }
		}
		protected abstract ZString GetClipOnUnit();

		public ZString IsPalletized
		{
			get
			{
				bool? isPalletized = GetIsPalletized();
				return isPalletized.HasValue ? (isPalletized.Value ? Res.GetString("689d2623-46e4-46d2-97f4-16794cacf560", "Yes") : Res.GetString("5108496b-2536-49da-8b00-acd71a1aeeca", "No")) : string.Empty;
			}
		}
		protected abstract bool? GetIsPalletized();

		public ZString IsChargeable
		{
			get
			{
				bool? isChargeable = GetIsIsChargeable();
				return isChargeable.HasValue ? (isChargeable.Value ? Res.GetString("689d2623-46e4-46d2-97f4-16794cacf560", "Yes") : Res.GetString("5108496b-2536-49da-8b00-acd71a1aeeca", "No")) : string.Empty;
			}
		}
		protected abstract bool? GetIsIsChargeable();

		public ZString Packages
		{
			get { return GetPackages(); }
		}
		protected abstract ZString GetPackages();

		public ZString Pallets
		{
			get { return GetPallets(); }
		}
		protected abstract ZString GetPallets();

		public ZString UnpackShed
		{
			get { return GetUnpackShed(); }
		}
		protected abstract ZString GetUnpackShed();

		public DetentionWrapper ImportDetention
		{
			get { return importDetention ?? (importDetention = GetImportDetention()); }
		}
		protected abstract DetentionWrapper GetImportDetention();
		DetentionWrapper importDetention;

		public DetentionWrapper ExportDetention
		{
			get { return exportDetention ?? (exportDetention = GetExportDetention()); }
		}
		protected abstract DetentionWrapper GetExportDetention();
		DetentionWrapper exportDetention;

		public ZString StowagePosition
		{
			get { return GetStowagePosition(); }
		}
		protected abstract ZString GetStowagePosition();

		public ZDateTime VGMVerifiedDate
		{
			get { return GetVGMVerifiedDate(); }
		}
		protected abstract ZDateTime GetVGMVerifiedDate();

		public CodeAndDescriptionWrapper VGMMethod
		{
			get { return vgmMethod ?? (vgmMethod = GetVGMMethod()); }
		}
		CodeAndDescriptionWrapper vgmMethod;
		protected abstract CodeAndDescriptionWrapper GetVGMMethod();

		public AddressWrapper VGMVerifiedByAddress
		{
			get { return vgmVerifiedByAddress ?? (vgmVerifiedByAddress = GetVGMVerifiedByAddress()); }
		}
		AddressWrapper vgmVerifiedByAddress;
		protected abstract AddressWrapper GetVGMVerifiedByAddress();

		public OrganisationWrapper Owner
		{
			get { return owner ?? (owner = GetOwner()); }
		}
		OrganisationWrapper owner;
		protected abstract OrganisationWrapper GetOwner();

		public CodeAndDescriptionWrapper Status
		{
			get { return status ?? (status = GetStatus()); }
		}
		CodeAndDescriptionWrapper status;
		protected abstract CodeAndDescriptionWrapper GetStatus();

		public ZDateTime GateInDate
		{
			get { return GetGateInDate(); }
		}
		protected abstract ZDateTime GetGateInDate();

		public ZDateTime GateOutDate
		{
			get { return GetGateOutDate(); }
		}
		protected abstract ZDateTime GetGateOutDate();

		public ZDateTime OffHireDate
		{
			get { return GetOffHireDate(); }
		}
		protected abstract ZDateTime GetOffHireDate();

		public ZDateTime OnHireDate
		{
			get { return GetOnHireDate(); }
		}
		protected abstract ZDateTime GetOnHireDate();

		public ZDateTime ManufactureDate
		{
			get { return GetManufactureDate(); }
		}
		protected abstract ZDateTime GetManufactureDate();

		public LocationWrapper OffHirePort
		{
			get { return offHirePort ?? (offHirePort = GetOffHirePort()); }
		}
		LocationWrapper offHirePort;
		protected abstract LocationWrapper GetOffHirePort();

		public LocationWrapper OnHirePort
		{
			get { return onHirePort ?? (onHirePort = GetOnHirePort()); }
		}
		LocationWrapper onHirePort;
		protected abstract LocationWrapper GetOnHirePort();

		public ZString CreatedByUserName
		{
			get { return GetCreatedByUserName(); }
		}
		protected abstract ZString GetCreatedByUserName();
	}
}
