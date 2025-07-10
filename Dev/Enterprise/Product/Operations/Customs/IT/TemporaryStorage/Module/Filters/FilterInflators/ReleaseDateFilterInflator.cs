using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class ReleaseDateFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Release Date";

	public ReleaseDateFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var releaseDateFilter = filterCollection.AddDateFilter(FilterDescription, CusEntryNumSchema.CE_IssueDate);
		releaseDateFilter.Category = FilterCategories.Dates;
		releaseDateFilter.SubGroup = new BillCusEntryNumModuleFilterSubGroup(CusEntryNumberTypes.EU.CustomsRegistry);
		releaseDateFilter.MultilingualDescription = ResString.GetMultilingualString("499F39AE-37D2-4189-8F88-6F5B029B8E8D", FilterDescription);
	}
}
