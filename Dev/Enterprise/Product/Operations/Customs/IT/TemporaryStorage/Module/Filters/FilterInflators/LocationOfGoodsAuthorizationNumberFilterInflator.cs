using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class LocationOfGoodsAuthorizationNumberFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Location of Goods: Authorization No.";

	public LocationOfGoodsAuthorizationNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var locationOfGoodsAuthorizationNumberFilter = filterCollection.AddTextFilter(FilterDescription, JobDocAddressSchema.E2_GovRegNum);
		locationOfGoodsAuthorizationNumberFilter.Category = FilterCategories.Locations;
		locationOfGoodsAuthorizationNumberFilter.SubGroup = new AuthorizationNumberSubGroup();
		locationOfGoodsAuthorizationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("FBCE26D8-E490-46DC-A497-6F567262493A", FilterDescription);
	}
}
