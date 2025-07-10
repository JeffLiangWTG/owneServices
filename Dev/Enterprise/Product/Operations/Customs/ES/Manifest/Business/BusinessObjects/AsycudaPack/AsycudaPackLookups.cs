using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}
		public override CodeDescriptionPairList PackUQList => Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes);
	}
}
