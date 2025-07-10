using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PackUQList => RefCusCodeListTypes.GetCachedList(Parent.Factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
			ZDate.Today);
	}
}
