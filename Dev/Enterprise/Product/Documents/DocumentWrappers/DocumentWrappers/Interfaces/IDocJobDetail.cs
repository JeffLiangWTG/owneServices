using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocJobDetail
	{
		ZString OrderNumbersForInvoice { get; }
		ZString OurReference { get; }
		ZString SupplierAsString { get; }
		ZString VesselAndVoyage { get; }
		ZString MasterBillNumber { get; }
		ZString ETAPortName { get; }
		ZString ETDPortName { get; }
		ZString Service { get; }
		ZString PackageQuantity { get; }
		ZString PackageType { get; }
		ZString ConsolDepot { get; }
		ZString WeightAsString { get; }
		ZString VolumeAsString { get; }
		ZString Note { get; }
		ZString ConsignorAsString { get; }
		ZString ConsigneeAsString { get; }
		ZString ShortContainerAndSealNumbersForInvoice { get; }
		ZString LongContainerAndSealNumbersForInvoice { get; }
		ZString MarksAndNumbersForInvoice { get; }
		ZString ShortGoodsDescriptionForInvoice { get; }
		ZString LongGoodsDescriptionForInvoice { get; }

		ZDateTime ETADate { get; }
		ZDateTime ETDDate { get; }

		ZBool TransportModeIsAir { get; }
		ZBool TransportModeIsSea { get; }

		ZInt NumberOfContainers { get; }
	}
}