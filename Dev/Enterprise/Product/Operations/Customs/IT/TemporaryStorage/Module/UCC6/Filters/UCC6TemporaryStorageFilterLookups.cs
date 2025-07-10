using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class UCC6TemporaryStorageFilterLookups : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterLookups
{
	public UCC6TemporaryStorageFilterLookups(FilterStripBusinessObject filterBizObj) : base(filterBizObj)
	{
	}

	protected override ZZRefCusCodeListCombinedCollection GetPreviousDocumentsType =>
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			FilterBizObj.Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
			[Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS],
			ZDateTime.Today,
			[],
			includeParentDataGroupings: false);
}
