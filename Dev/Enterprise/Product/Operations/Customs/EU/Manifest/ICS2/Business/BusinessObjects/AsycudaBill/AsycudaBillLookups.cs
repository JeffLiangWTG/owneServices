using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public CodeDescriptionPairList ScreeningAuthorizedPersonTypes => Factory.GetCachedValue<EUICS2ScreeningAuthorizedPersonTypes>();

		public CodeDescriptionPairList TransportDocumentTypeList => Factory.GetCachedValue<ASYCUDA.Business.TransportDocumentTypes>();

		public CodeDescriptionPairList PackUQList => Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);

		public CodeDescriptionPairList CodeList733 => RefCusCodeListTypes.GetCachedList(Parent.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL733, ZDate.Today);

		protected override CodeDescriptionPairList GetPrepaidCollectListCore() => Factory.GetCachedValue<EUICS2PaymentMethodList>();

		public CodeDescriptionPairList BuyerRegistrationNoTypeList
		{
			get { return GetRegistrationNoTypeList(Parent.BuyerRegNoTypes()); }
		}

		public new OrgHeaderCollection Sellers
		{
			get { return new AsycudaBillSellerCollection(Factory, Parent); }
		}
	}
}
