using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class Container : IDataObject,
		ICustomizedFieldContainer,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ITransportLogisticsCostCollectionParent,
		IMilestoneCollectionParent
	{
		public Container()
		{
		}

		public Container(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZDecimal? AirVentFlow { get; set; }
		public CodeDescriptionPair AirVentFlowRateUnit { get; set; }
		public ZDateTime? ArrivalCartageAdvised { get; set; }
		public ZDateTime? ArrivalCartageComplete { get; set; }
		public ZDecimal? ArrivalCartageDemurrageCharge { get; set; }
		public ZDecimal? ArrivalTruckWaitCost { get; set; }
		public ZDateTime? ArrivalCartageDemurrageTime { get; set; }
		public ZDateTime? ArrivalTruckWaitTime { get; set; }
		[MaxLength(20)]
		public ZString? ArrivalCartageRef { get; set; }
		public ZDateTime? ArrivalDeliveryRequiredBy { get; set; }
		public ZDateTime? ArrivalEstimatedDelivery { get; set; }
		public ZBool? ArrivalPickupByRail { get; set; }
		public ZDateTime? ArrivalSlotDateTime { get; set; }
		[MaxLength(15)]
		public ZString? ArrivalSlotReference { get; set; }
		public Commodity Commodity { get; set; }
		public Commodity RatingCommodity { get; set; }
		public ZDecimal? CarbonGasLevel { get; set; }
		public CodeDescriptionPair CarbonGasLevelUnit { get; set; }
		public ZInt? ContainerCount { get; set; }
		public ZDecimal? ContainerDetentionCharge { get; set; }
		public ZDecimal? ArrivalCarrierDetentionCost { get; set; }
		public ZByte? ContainerDetentionDays { get; set; }
		public ZByte? ArrivalCarrierDetentionDays { get; set; }
		[MaxLength(50)]
		public ZString? ContainerImportDORelease { get; set; }
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
		[MaxLength(20)]
		public ZString? ContainerJobID { get; set; }
		public ZDateTime? ContainerParkEmptyPickupGateOut { get; set; }
		public ZDateTime? ContainerParkEmptyReturnGateIn { get; set; }
		public CodeDescriptionPair ContainerQuality { get; set; }
		public CodeDescriptionPair2Char CustomsContainerSize { get; set; }
		public ContainerType ContainerType { get; set; }
		public CodeDescriptionPair ContainerStatus { get; set; }
		[MaxLength(7)]
		public ZString? DeliveryMode { get; set; }
		public ZShort? DeliverySequence { get; set; }
		public ZDateTime? DepartureCartageAdvised { get; set; }
		public ZDateTime? DepartureCartageComplete { get; set; }
		public ZDecimal? DepartureCartageDemurrageCharge { get; set; }
		public ZDecimal? DepartureTruckWaitCost { get; set; }
		public ZDateTime? DepartureCartageDemurrageTime { get; set; }
		public ZDateTime? DepartureTruckWaitTime { get; set; }
		[MaxLength(20)]
		public ZString? DepartureCartageRef { get; set; }
		public ZBool? DepartureDeliveryByRail { get; set; }
		[MaxLength(20)]
		public ZString? DepartureDockReceipt { get; set; }
		public ZDateTime? DepartureEstimatedPickup { get; set; }
		public ZDateTime? DepartureSlotDateTime { get; set; }
		[MaxLength(15)]
		public ZString? DepartureSlotReference { get; set; }
		public ZDecimal? DunnageWeight { get; set; }
		public ZDateTime? EmptyReadyForReturn { get; set; }
		public ZDateTime? EmptyRequired { get; set; }
		public ZDateTime? EmptyReturnedBy { get; set; }
		[MaxLength(20)]
		public ZString? EmptyReturnRef { get; set; }
		[MaxLength(20)]
		public ZString? ExportDepotCustomsReference { get; set; }
		public ContainerMode FCL_LCL_AIR { get; set; }
		public ZDateTime? FCLAvailable { get; set; }
		public ZBool? FCLHeldInTransitStaging { get; set; }
		public ZDateTime? FCLOnBoardVessel { get; set; }
		public ZBool? FCLStorageArrivedUnderbond { get; set; }
		public ZDecimal? FCLStorageCharge { get; set; }
		public ZDecimal? ArrivalCTOStorageCost { get; set; }
		public ZDateTime? FCLStorageCommences { get; set; }
		public ZDateTime? ArrivalCTOStorageStartDate { get; set; }
		public ZByte? FCLStorageDays { get; set; }
		public ZByte? ArrivalCTOStorageDays { get; set; }
		[MaxLength(20)]
		public ZString? FCLStorageModuleOnlyMaster { get; set; }
		public ZDateTime? FCLStorageUnderbondCleared { get; set; }
		public ZDateTime? FCLUnloadFromVessel { get; set; }
		public ZDateTime? FCLWharfGateIn { get; set; }
		public ZDateTime? FCLWharfGateOut { get; set; }
		[MaxLength(35)]
		public ZString? GoodsDescription { get; set; }
		public ZDecimal? GoodsValue { get; set; }
		public Currency GoodsValueCurrency { get; set; }
		public ZDecimal? GrossWeight { get; set; }
		public ZDateTime? GrossWeightVerificationDateTime { get; set; }
		public CodeDescriptionPair GrossWeightVerificationType { get; set; }
		[MaxLength(15)]
		public ZString? HarmonisedCode { get; set; }
		public ZByte? HumidityPercent { get; set; }
		[MaxLength(20)]
		public ZString? ImportDepotCustomsReference { get; set; }
		public ZBool? IsChargeable { get; set; }
		public ZBool? IsCFSRegistered { get; set; }
		public ZBool? IsControlledAtmosphere { get; set; }
		public ZBool? IsDamaged { get; set; }
		public ZBool? IsEmptyContainer { get; set; }
		public ZBool? IsNonOperating { get; set; }
		public ZBool? IsPalletised { get; set; }
		public ZBool? IsSealOk { get; set; }
		public ZBool? IsShipperOwned { get; set; }
		public ZInt? ItemCount { get; set; }
		public ZDateTime? LCLAvailable { get; set; }
		public ZDateTime? LCLStorageCommences { get; set; }
		public ZDateTime? LCLUnpack { get; set; }
		public ZInt? Link { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public ZDecimal? NitrogenGasLevel { get; set; }
		public CodeDescriptionPair NitrogenGasLevelUnit { get; set; }
		public ZBool? NonOperatingReefer { get; set; }
		public ZBool? OverrideFCLAvailableStorage { get; set; }
		public ZBool? OverrideLCLAvailableStorage { get; set; }
		public ZDecimal? OxygenGasLevel { get; set; }
		public CodeDescriptionPair OxygenGasLevelUnit { get; set; }
		public ZDateTime? PackDate { get; set; }
		public ZInt? PalletCount { get; set; }
		[MaxLength(17)]
		public ZString? RefrigGeneratorID { get; set; }
		[MaxLength(20)]
		public ZString? ReleaseNum { get; set; }
		[MaxLength(20)]
		public ZString? Seal { get; set; }
		public CodeDescriptionPair SealPartyType { get; set; }
		[MaxLength(20)]
		public ZString? SecondSeal { get; set; }
		public CodeDescriptionPair SecondSealPartyType { get; set; }
		public ZDecimal? SetPointTemp { get; set; }
		[MaxLength(1)]
		public ZString? SetPointTempUnit { get; set; }
		[MaxLength(25)]
		public ZString? StowagePosition { get; set; }
		public ZDecimal? TareWeight { get; set; }
		[MaxLength(20)]
		public ZString? TempRecorderSerialNo { get; set; }
		[MaxLength(20)]
		public ZString? ThirdSeal { get; set; }
		public CodeDescriptionPair ThirdSealPartyType { get; set; }
		public ZDecimal? TotalHeight { get; set; }
		public ZDecimal? TotalLength { get; set; }
		public UnitOfLength LengthUnit { get; set; }
		public ZDecimal? TotalWidth { get; set; }
		public ZDecimal? OverhangFront { get; set; }
		public ZDecimal? OverhangBack { get; set; }
		public ZDecimal? OverhangLeft { get; set; }
		public ZDecimal? OverhangRight { get; set; }
		public ZDecimal? OverhangHeight { get; set; }
		[MaxLength(10)]
		public ZString? TrainWagonNumber { get; set; }
		[MaxLength(35)]
		public ZString? TransportReference { get; set; }
		[MaxLength(10)]
		public ZString? UnpackGang { get; set; }
		[MaxLength(10)]
		public ZString? UnpackShed { get; set; }
		public ZDecimal? VolumeCapacity { get; set; }
		public UnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? GoodsWeight { get; set; }
		public UnitOfWeight WeightUnit { get; set; }
		public ZDecimal? WeightCapacity { get; set; }
		public ZDateTime? GateInDate { get; set; }
		public ZDateTime? GateOutDate { get; set; }
		public ZDecimal? PivotBreak { get; set; }
		public ZBool? IsGrossWeightOverridden { get; set; }
		public ZBool? IsCheckedWeighedCubed { get; set; }
		public ZBool? Pillaged { get; set; }
		public ZBool? Fumigated { get; set; }
		public ZBool? HeatTreated { get; set; }
		public ZBool? RequiresTemperatureControl { get; set; }
		public ZDecimal? RequiredTemperatureMinimum { get; set; }
		public ZDecimal? RequiredTemperatureMaximum { get; set; }
		public CodeDescriptionPair1Char RequiredTemperatureUnit { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? MarksAndNos { get; set; }

		#region Collections

		public List<UNDG> UNDGCollection { get; private set; }
		public void SetUNDGCollection(List<UNDG> value) => UNDGCollection = value;
		public List<AddInfo> AddInfoCollection { get; set; }
		public void SetAddInfoCollection(List<AddInfo> value) => AddInfoCollection = value;
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public void SetAddInfoGroupCollection(List<AddInfoGroup> value) => AddInfoGroupCollection = value;
		public List<CustomizedField> CustomizedFieldCollection { get; set; }
		public void SetCustomizedFieldCollection(List<CustomizedField> value) => CustomizedFieldCollection = value;
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public void SetCustomsReferenceCollection(List<CustomsReference> value) => CustomsReferenceCollection = value;
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
		public void SetOrganizationAddressCollection(List<OrganizationAddress> value) => OrganizationAddressCollection = value;
		public DataObjectList<AdditionalService> AdditionalServiceCollection { get; private set; }
		public void SetAdditionalServiceCollection(DataObjectList<AdditionalService> value) => AdditionalServiceCollection = value;
		public List<PackingLine> PackingLineCollection { get; private set; }
		public void SetPackingLineCollection(List<PackingLine> value) => PackingLineCollection = value;
		public List<TransportLogisticsCost> TransportLogisticsCostCollection { get; private set; }
		public void SetTransportLogisticsCostCollection(List<TransportLogisticsCost> value) => TransportLogisticsCostCollection = value;
		public List<Milestone> MilestoneCollection { get; set; }
		public void SetMilestoneCollection(List<Milestone> value) => MilestoneCollection = value;
		public DataObjectList<AdditionalReference> AdditionalReferenceCollection { get; private set; }
		public void SetAdditionalReferenceCollection(DataObjectList<AdditionalReference> value) => AdditionalReferenceCollection = value;

		public List<ContainerPenalty> ContainerPenaltyCollection { get; set; }
		public void SetContainerPenalty(List<ContainerPenalty> value) => ContainerPenaltyCollection = value;
		public List<SealNumber> AdditionalSealNumberCollection { get; private set; }
		public List<Seal> SealCollection { get; set; }
		public List<AdditionalAddressInfo> AdditionalAddressInfoCollection { get; private set; }

		#endregion
	}
}
