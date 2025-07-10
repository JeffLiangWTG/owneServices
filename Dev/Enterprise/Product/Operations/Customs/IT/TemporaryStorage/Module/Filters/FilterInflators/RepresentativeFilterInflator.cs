using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class RepresentativeFilterInflator : EU.TemporaryStorage.Module.RepresentativeFilterInflator
{
	public RepresentativeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var representativeFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
		representativeFilter.Category = FilterCategories.Organisations;
		representativeFilter.SubGroup = new OrganisationSubGroup(AsycudaManifestHeaderSchema.AMA_OA_Representative);
		representativeFilter.MultilingualDescription = ResString.GetMultilingualString("999103F3-E0F2-451F-95C6-83355BA0EC86", FilterDescription);
	}
}
