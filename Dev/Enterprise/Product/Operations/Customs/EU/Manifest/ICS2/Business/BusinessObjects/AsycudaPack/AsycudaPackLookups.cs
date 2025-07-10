using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PackUQList => Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
	}
}
