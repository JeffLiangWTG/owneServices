using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IContainer
	{
		#region General

		ZString Number { get; set; }
		ZString DeliveryMode { get; }

		ZBool IsDamaged { get; }
		ZBool IsEmpty { get; set; }
		ZBool IsPartOf { get; set; }
		ZBool IsSealOk { get; }
		ZBool IsShipperOwned { get; set; }
		ZBool IsNonOperativeReefer { get; set; }

		ICodeDescription Commodity { get; }
		ICodeDescription ContainerMode { get; }
		ICodeDescription ContainerQuality { get; }
		ICodeDescription ContainerStatus { get; }
		IContainerType Type { get; }

		IMeasurement GoodsValue { get; }

		ZInt ContainerCount { get; set; }

		ZInt PackCount { get; }
		ICodeDescription PackType { get; }

		ZShort DeliverySequence { get; }

		#endregion

		#region Seals

		ZString Seal { get; set; }
		ICodeDescription SealPartyType { get; }
		ZString SecondSeal { get; set; }
		ICodeDescription SecondSealPartyType { get; }
		ZString ThirdSeal { get; set; }
		ICodeDescription ThirdSealPartyType { get; }

		#endregion

		#region Weight And Volume

		IMeasurement Dunnage { get; }
		IMeasurement GoodsWeight { get; }
		IMeasurement GrossWeight { get; }
		IMeasurement NetWeight { get; }
		IMeasurement TareWeight { get; }
		IMeasurement Volume { get; }
		IMeasurement VolumeCapacity { get; }
		IMeasurement WeightCapacity { get; }

		#endregion

		#region Refrigeration

		IMeasurement AirVentFlow { get; }
		IMeasurement Humidity { get; }
		IMeasurement SetTemperature { get; }
		ZBool HasControlledAtmosphere { get; set; }
		ZString RefrigGeneratorID { get; }
		ZString TemperatureRecorderSerialNumber { get; set; }
		ZBool Genset { get; }

		#endregion

		#region Measures

		IMeasurement OverhangBack { get; }
		IMeasurement OverhangFront { get; }
		IMeasurement OverhangHeight { get; }
		IMeasurement OverhangLeft { get; }
		IMeasurement OverhangRight { get; }
		IMeasurement TotalHeight { get; }
		IMeasurement TotalLength { get; }
		IMeasurement TotalWidth { get; }

		#endregion

		#region Export Info

		IAddress DepartureContainerYard { get; }

		ZBool DepartureDeliveryByRail { get; }

		ZDateTime ContainerParkEmptyPickupGateOut { get; }
		ZDateTime DepartureCartageAdvised { get; }
		ZDateTime DepartureCartageComplete { get; }
		ZDateTime DepartureTruckWaitTime { get; }
		ZDateTime DepartureEstimatedPickup { get; }
		ZDateTime DepartureSlotDateTime { get; }
		ZDateTime EmptyRequired { get; }
		ZDateTime FCLOnBoardVessel { get; }
		ZDateTime FCLWharfGateIn { get; }

		ZDecimal DepartureTruckWaitCost { get; }

		ZString DepartureCartageReference { get; }
		ZString DepartureSlotReference { get; }
		ZString ExportDepotCustomsReference { get; }
		ZString ImportDepotCustomsReference { get; }
		ZString ReleaseNumber { get; }

		#endregion

		#region Import Info

		ZBool ArrivalPickupByRail { get; }
		ZBool FCLHeldInTransitStaging { get; }

		ZByte ArrivalCarrierDetentionDays { get; }
		ZByte ArrivalCTOStorageDays { get; }

		ZDateTime ArrivalCartageAdvised { get; }
		ZDateTime ArrivalCartageComplete { get; }
		ZDateTime ArrivalTruckWaitTime { get; }
		ZDateTime ArrivalDeliveryRequiredBy { get; }
		ZDateTime ArrivalEstimatedDelivery { get; }
		ZDateTime ArrivalSlotDateTime { get; }
		ZDateTime ContainerParkEmptyReturnGateIn { get; }
		ZDateTime EmptyReadyForReturn { get; }
		ZDateTime EmptyReturnedBy { get; }
		ZDateTime FCLAvailable { get; }
		ZDateTime ArrivalCTOStorageStartDate { get; }
		ZDateTime FCLUnloadFromVessel { get; }
		ZDateTime FCLWharfGateOut { get; }
		ZDateTime LCLAvailable { get; }
		ZDateTime LCLStorageCommences { get; }
		ZDateTime LCLUnpack { get; }
		ZDateTime PackDate { get; }

		ZDecimal ArrivalTruckWaitCost { get; }
		ZDecimal ArrivalCarrierDetentionCost { get; }
		ZDecimal ArrivalCTOStorageCost { get; }

		ZString ArrivalCartageReference { get; }
		ZString ArrivalSlotReference { get; }
		ZString ContainerImportDORelease { get; }
		ZString EmptyReturnReference { get; }

		#endregion

		#region Verification

		IAddress VerifiedByAddress { get; }
		ICodeDescription VerifiedMethod { get; }
		ICodeDescription VerifiedStatus { get; }
		ZDateTime VerifiedDate { get; set; }

		#endregion

		#region PackingLines

		IReadOnlyCollection<IPackingLine> PackingLines { get; }

		#endregion

		#region Numbers

		IReadOnlyCollection<IReferenceNumber> Numbers { get; }

		ZString CustomerLoadReference { get; set; }

		#endregion

		#region AdditionalServices

		IReadOnlyCollection<IAdditionalService> AdditionalServices { get; }

		#endregion

		#region Milestones

		IReadOnlyCollection<IMilestone> Milestones { get; }

		#endregion
	}
}
