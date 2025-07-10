using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaPackSSLookups : AsycudaPackLookups
	{
		public AsycudaPackSSLookups(AsycudaPack parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PackUQList =>
			Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(
				Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDate.Today);
	}
}
