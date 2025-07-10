using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

public sealed class DdtNumberFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "TSD Number";

	public DdtNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var ddtNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryNum);
		ddtNumberFilter.SubGroup = new DDTNumberSubGroup();
		ddtNumberFilter.Category = FilterCategories.NumbersAndReferences;
		ddtNumberFilter.MultilingualDescription = ResString.GetMultilingualString("9F575504-6B6C-4FA9-A585-D2D2E411C933", FilterDescription);
	}
}
