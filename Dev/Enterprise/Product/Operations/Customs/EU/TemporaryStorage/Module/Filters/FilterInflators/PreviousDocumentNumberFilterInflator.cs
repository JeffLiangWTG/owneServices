using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class PreviousDocumentNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Previous Document Number";

	public PreviousDocumentNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var previousDocumentNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusSupportingInfoSchema.CSI_ReferenceNumber);
		previousDocumentNumberFilter.SubGroup = new BillOrPackedItemCusSupportingInfoSubGroup(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
		previousDocumentNumberFilter.Category = FilterCategories.NumbersAndReferences;
		previousDocumentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("91D52FF3-4EDC-453B-A875-FD55D29465E5", FilterDescription);
	}
}
