using CargoWise.Types;

namespace Enterprise.Integration.Packing
{
	public interface IPackageTemplate
	{
		ZDecimal Length { get; }
		ZDecimal Width { get; }
		ZDecimal Height { get; }
		ZDecimal TareWeight { get; }
		ZDecimal? Volume { get; }

		ZString WeightUQ { get; }
		ZString DimensionUQ { get; }
		ZString VolumeUQ { get; }
	}
}
