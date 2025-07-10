using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class TransportModeFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Transport Mode";

	public TransportModeFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var transportModeFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_TransportMode, Lookups.TransportTypeList);
		transportModeFilter.Category = FilterCategories.ModesAndTypes;
		transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("6f6f3cd5-ef50-44ef-86bb-b9e9d6423a16", FilterDescription);
	}
}
