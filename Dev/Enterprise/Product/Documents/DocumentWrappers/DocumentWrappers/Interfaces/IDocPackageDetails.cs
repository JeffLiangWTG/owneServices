using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	interface IDocPackageDetails
	{
		ZString ReferenceNumber { get; }
		ZInt Count { get; }
		ZString PackType { get; }

		ZString Description { get; }
		ZString DetailedDescription { get; }
		ZString MarksAndNumbers { get; }

		ZDecimal Weight { get; }
		ZString WeightUnit { get; }

		ZDecimal Volume { get; }
		ZString VolumeUnit { get; }

		ZDecimal Length { get; }
		ZDecimal Height { get; }
		ZDecimal Width { get; }
		ZString DimensionUnit { get; }

		DocCommodity Commodity { get; }
		ZString HarmonisedCode { get; }
		UNDGSubstanceWrapper[] UNDGs { get; }

		ZString VehicleColour { get; }
		ZString VehicleMake { get; }
		ZString VehicleModel { get; }
		ZByte VehicleNumberOfDoors { get; }
		ZString VehicleTransmission { get; }
		ZShort VehicleYear { get; }
	}
}
