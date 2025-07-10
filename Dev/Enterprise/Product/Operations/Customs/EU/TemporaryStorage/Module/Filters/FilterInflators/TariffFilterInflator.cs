using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class TariffFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Tariff";

	public TariffFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var tariffFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaPackedItemSchema.API_Tariff);
		tariffFilter.SubGroup = new TariffSubGroup();
		tariffFilter.Category = FilterCategories.NumbersAndReferences;
		tariffFilter.MultilingualDescription = ResString.GetMultilingualString("AE7B3954-9CAE-48E6-BF3F-2A9AF7EEF133", FilterDescription);
	}
}
