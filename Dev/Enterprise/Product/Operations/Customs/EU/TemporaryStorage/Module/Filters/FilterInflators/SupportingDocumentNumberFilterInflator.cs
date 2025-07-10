using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class SupportingDocumentNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Supporting Documents Number";

	public SupportingDocumentNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var supportingDocumentNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusSupportingInfoSchema.CSI_ReferenceNumber);
		supportingDocumentNumberFilter.SubGroup = new BillOrPackedItemCusSupportingInfoSubGroup(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
		supportingDocumentNumberFilter.Category = FilterCategories.NumbersAndReferences;
		supportingDocumentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("B2B42326-24E3-43E6-AFBB-8E31805D0221", FilterDescription);
	}
}
