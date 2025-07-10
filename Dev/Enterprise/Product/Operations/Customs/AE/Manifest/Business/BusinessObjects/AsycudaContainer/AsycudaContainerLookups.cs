using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
{
	public AsycudaContainerLookups(AsycudaContainer parent) : base(parent)
	{
	}

	public CodeDescriptionPairList AEContainerTemperatureUnitCodes => Factory.GetCachedValue<AEContainerTemperatureUnitCodes>();
}
