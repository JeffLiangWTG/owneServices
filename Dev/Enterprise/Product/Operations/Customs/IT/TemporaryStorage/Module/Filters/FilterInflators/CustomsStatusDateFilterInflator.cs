using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class CustomsStatusDateFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Customs Status Date";

	public CustomsStatusDateFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsStatusDateFilter = filterCollection.AddDateFilter(FilterDescription, CusEntryNumSchema.CE_IssueDate);
		customsStatusDateFilter.Category = FilterCategories.Dates;
		customsStatusDateFilter.SubGroup = new CustomsStatusDateSubGroup();
		customsStatusDateFilter.MultilingualDescription = ResString.GetMultilingualString("F1E2D3C4-B5A6-7890-4321-09876ABCDE12", FilterDescription);
	}
}
