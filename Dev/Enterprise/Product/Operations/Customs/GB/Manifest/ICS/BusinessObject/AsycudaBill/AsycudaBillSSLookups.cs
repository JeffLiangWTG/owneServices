using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaBillSSLookups : AsycudaBillLookups
	{
		public AsycudaBillSSLookups(AsycudaBill parent) : base(parent)
		{ }

		protected override CodeDescriptionPairList PackageTypeListCore =>
			Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(
				Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDate.Today);

		protected override CodeDescriptionPairList GetPrepaidCollectListCore() => Factory.GetCachedValue<TransportChargesModeOfPayment>();

		protected override CodeDescriptionPairList SpecialMentionsListCore
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions); }
		}
	}
}
