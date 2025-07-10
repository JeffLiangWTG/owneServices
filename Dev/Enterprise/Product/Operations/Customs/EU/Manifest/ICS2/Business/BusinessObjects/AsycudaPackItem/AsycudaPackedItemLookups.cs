using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public ZZRefCusCodeListCombinedCollection ChemicalSubstanceCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, ZDateTime.Today);

		public CodeDescriptionPairList TypeOfGoodsList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG);
	}
}
