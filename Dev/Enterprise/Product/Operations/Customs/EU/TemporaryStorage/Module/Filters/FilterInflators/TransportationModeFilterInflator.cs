using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class TransportationModeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Transportation Mode";

	public TransportationModeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var transportationModeFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_TransportMode, TemporaryStorageHeader.Lookups.TransportModeList);
		transportationModeFilter.Category = FilterCategories.ModesAndTypes;
		transportationModeFilter.MultilingualDescription = ResString.GetMultilingualString("F0B3E4EE-F981-4E97-9B55-BD3CD29D9935", FilterDescription);
	}
}
