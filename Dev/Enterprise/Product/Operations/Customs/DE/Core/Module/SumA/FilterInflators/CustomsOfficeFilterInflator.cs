using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class CustomsOfficeFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Customs Office";

	public CustomsOfficeFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsOfficeFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_CustomsOffice, Lookups.CustomsOfficeList);
		customsOfficeFilter.Category = FilterCategories.Locations;
		customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("e5c3ad4c-c662-42ce-90d6-802d2a6204ea", FilterDescription);
	}
}
