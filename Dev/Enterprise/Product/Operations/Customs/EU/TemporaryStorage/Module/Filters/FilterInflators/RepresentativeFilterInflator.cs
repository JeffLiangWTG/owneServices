using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class RepresentativeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	protected const string FilterDescription = "Representative";

	public RepresentativeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var representativeFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.OrgAddresses, AsycudaManifestHeaderSchema.AMA_OA_Representative, new OrgAddressCollection(Factory));
		representativeFilter.Category = FilterCategories.Organisations;
		representativeFilter.MultilingualDescription = ResString.GetMultilingualString("5EA6F99D-7D8E-4176-99DF-E6334F4867EA", FilterDescription);
	}
}
