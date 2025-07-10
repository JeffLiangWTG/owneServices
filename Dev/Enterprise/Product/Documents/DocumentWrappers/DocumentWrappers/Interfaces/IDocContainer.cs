
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocContainer
	{
		ZString ContainerNumber { get; }
		ZString ContainerCode { get; }
		ZString SealNumber { get; }
		ZString SealNumber2 { get; }
		ZString SealNumber3 { get; }
		ZString ContainerMode { get; }
		ZString ReleaseNum { get; }
		ZString TempRecorderSerialNo { get; }
		ZString AirVent { get; }
		ZString CommodityDescription { get; }
		ZString DescriptionAndStatus { get; }
		ZString Size { get; }
		ZString DeliveryMode { get; }
		ZString TareWeightWithUQ { get; }
		ZString GrossWeightWithUQ { get; }
		ZString SlotReference { get; }

		ZString ForwardingInstructionWeight { get; }
		ZString ForwardingInstructionVolume { get; }
		ZString ForwardingInstructionPackages { get; }

		ZDecimal TotalPackLineVolume { get; }
		ZDecimal TotalPackLineWeight { get; }
		ZDecimal TotalAllocatedShipmentWeight { get; }
		ZDecimal TotalAllocatedShipmentVolume { get; }
		ZString TotalAllocatedShipmentVolumeUQ { get; }
		ZBool IsControlledAtmosphere { get; }
		ZDecimal SetPointTemp { get; }
		ZString SetPointTempUnit { get; }
		ZString HumidityPercent { get; }

		ZDateTime ContainerAvailable { get; }
		ZDateTime LCLAvailable { get; }
		ZDateTime StorageCommences { get; }
		ZDateTime LCLStorageCommences { get; }
		ZDateTime EmptyRequired { get; }
		ZDateTime ContainerParkEmptyReturnGateIn { get; }
		ZDateTime EmptyReturnedBy { get; }
		ZDateTime FullPickDate { get; }

		ZInt TotalAllocatedShipmentPackages { get; }
		ZString TotalAllocatedShipmentPackagesPackType { get; }
		ZInt TotalPackLinePackages { get; }
		ZInt QuantityCount { get; set; }

		DocRefContainer Container { get; }
		DocDocAddress DepartureContainerParkAddress { get; }

		#region Additional CommonCartage Cover/Summary Sheet Requirements

		ZString ContainerType { get; }
		ZString SlotArrivalReference { get; }
		ZString SlotDepartureReference { get; }

		ZDateTime SlotArrivalTime { get; }
		ZDateTime SlotDepartureTime { get; }

		ZDecimal TareWeight { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal TotalVolume { get; }
		ZString TotalVolumeUnit { get; }

		ZString SlotArrivalDetails { get; }
		ZString SlotDepartureDetails { get; }
		ZString SlotAsArrivalOrDeparture { get; }

		#endregion

		ZString WeightUQ { get; }
	}
}
