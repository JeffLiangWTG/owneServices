using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PackUQList => Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.PackageTypes);
	}
}
