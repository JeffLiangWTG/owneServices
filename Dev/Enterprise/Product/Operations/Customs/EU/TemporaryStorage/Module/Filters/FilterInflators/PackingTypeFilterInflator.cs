using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class PackingTypeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Packing Type";

	public PackingTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var packingTypeFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaPackSchema.APA_PackUQ, TemporaryStoragePack.Lookups.PackUQList);
		packingTypeFilter.SubGroup = new PackSubGroup();
		packingTypeFilter.Category = FilterCategories.NumbersAndReferences;
		packingTypeFilter.MultilingualDescription = ResString.GetMultilingualString("B9218E26-25EA-4862-8761-32F1D7AD48DA", FilterDescription);
	}
}
