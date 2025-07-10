using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class LoadingFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Loading";

	public LoadingFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var loadingFilter = filterCollection.AddNkFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_RL_NKLoading, ModuleIDs.RefUNLOCO, Lookups.LoadingList);
		loadingFilter.Category = FilterCategories.Locations;
		loadingFilter.MultilingualDescription = ResString.GetMultilingualString("cd4e04be-c911-486b-bd13-083a50caaa73", FilterDescription);
	}
}
