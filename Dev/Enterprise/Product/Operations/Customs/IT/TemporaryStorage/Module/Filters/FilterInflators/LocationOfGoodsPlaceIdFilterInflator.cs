using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class LocationOfGoodsPlaceIdFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Location of Goods: Place ID";

	public LocationOfGoodsPlaceIdFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var locationOfGoodsPlaceIdFilter = filterCollection.AddTextFilter(FilterDescription, CusGoodsLocationSchema.CGL_AdditionalIdentifier);
		locationOfGoodsPlaceIdFilter.Category = FilterCategories.Locations;
		locationOfGoodsPlaceIdFilter.SubGroup = new PlaceIdSubGroup();
		locationOfGoodsPlaceIdFilter.MultilingualDescription = ResString.GetMultilingualString("A5FFEDE5-B055-4AEF-834E-02677958E8B4", FilterDescription);
	}
}
