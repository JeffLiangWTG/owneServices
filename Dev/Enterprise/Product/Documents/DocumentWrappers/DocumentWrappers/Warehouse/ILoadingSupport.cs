using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface ILoadingSupport
	{
		ZGuid LoadPK { get; set; }
		ZString CommonLoadWeightUQ { get; set; }
		ZString CommonLoadVolumeUQ { get; set; }
	}
}
