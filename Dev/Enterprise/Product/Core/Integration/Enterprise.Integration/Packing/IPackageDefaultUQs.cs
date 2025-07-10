using CargoWise.Types;

namespace Enterprise.Integration.Packing
{
	public interface IPackageDefaultUQs
	{
		ZString DefaultDimensionUnit { get; }
		ZString DefaultVolumeUnit { get; }
		ZString DefaultWeightUnit { get; }
	}
}
