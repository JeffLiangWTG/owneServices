using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class UCC6TemporaryStoragePreviousDocumentTypeFilterInflator : UCC6TemporaryStorageFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Previous Document Type";

	public UCC6TemporaryStoragePreviousDocumentTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var previousDocumentTypeFilter = filterCollection.AddNkFilter(FilterDescription, CusSupportingInfoSchema.CSI_Code, ModuleIDs.Customs.Universal.ZZRefCusCodeList, BizObj.Lookups.PreviousDocumentsType);
		previousDocumentTypeFilter.SubGroup = new BillOrPackedItemCusSupportingInfoSubGroup(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
		previousDocumentTypeFilter.Category = FilterCategories.NumbersAndReferences;
		previousDocumentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("C7288FD1-70E4-4769-B834-01942F473E24", FilterDescription);
	}
}
