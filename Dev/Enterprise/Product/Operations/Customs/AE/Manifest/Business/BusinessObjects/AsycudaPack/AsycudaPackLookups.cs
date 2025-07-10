using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
{
	public AsycudaPackLookups(ASYCUDA.Business.AsycudaPack parent) : base(parent)
	{
	}

	public new AsycudaPack Parent => (AsycudaPack)base.Parent;

	public override CodeDescriptionPairList PackUQList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedArabEmirates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
}
