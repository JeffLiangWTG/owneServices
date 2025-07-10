using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class UCC6TemporaryStorageFilterLookups
	{
		public UCC6TemporaryStorageFilterLookups(FilterStripBusinessObject filterBizObj)
		{
			this.FilterBizObj = Argument.NotNull(filterBizObj, nameof(filterBizObj));
		}

		protected FilterStripBusinessObject FilterBizObj { get; }

		public ZZRefCusCodeListCombinedCollection SupportingDocumentsType => FilterBizObj.Factory.GetCachedValue("EUUCC6TemporaryStorageFilterLookupsSupportingDocumentTypeList",
			() => GetCachedCollectionOfType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6));

		public ZZRefCusCodeListCombinedCollection PreviousDocumentsType => FilterBizObj.Factory.GetCachedValue("EUUCC6TemporaryStorageFilterLookupsPreviousDocumentTypeList",
			() => GetPreviousDocumentsType);

		protected virtual ZZRefCusCodeListCombinedCollection GetPreviousDocumentsType => GetCachedCollectionOfType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS);

		ZZRefCusCodeListCombinedCollection GetCachedCollectionOfType(ZString codeType) =>
			ZZRefCusCodeListCombinedCollection.GetCachedCollection(FilterBizObj.Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, codeType, ZDateTime.Today);
	}
}
