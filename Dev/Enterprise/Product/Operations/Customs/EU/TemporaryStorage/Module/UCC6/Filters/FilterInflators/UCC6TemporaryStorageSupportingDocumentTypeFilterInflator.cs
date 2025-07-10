using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class UCC6TemporaryStorageSupportingDocumentTypeFilterInflator : UCC6TemporaryStorageFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Supporting Documents Type";

	public UCC6TemporaryStorageSupportingDocumentTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var supportingDocumentTypeFilter = filterCollection.AddNkFilter(FilterDescription, CusSupportingInfoSchema.CSI_Code, ModuleIDs.Customs.Universal.ZZRefCusCodeList, BizObj.Lookups.SupportingDocumentsType);
		supportingDocumentTypeFilter.SubGroup = new BillOrPackedItemCusSupportingInfoSubGroup(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
		supportingDocumentTypeFilter.Category = FilterCategories.NumbersAndReferences;
		supportingDocumentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("2FD08345-A27C-4220-9B69-301407CCC080", FilterDescription);
	}
}
