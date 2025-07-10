using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class CountryFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Country";

	public CountryFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var countryFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_RN_NKCountry);
		countryFilter.Property = GlbCompany.CurrentCompany.Country.Code;
		countryFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
	}
}
