using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class DeclarantFilterInflator : EU.TemporaryStorage.Module.DeclarantFilterInflator
{
	public DeclarantFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var declarantFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
		declarantFilter.Category = FilterCategories.Organisations;
		declarantFilter.SubGroup = new OrganisationSubGroup(AsycudaManifestHeaderSchema.AMA_OA_Declarant);
		declarantFilter.MultilingualDescription = ResString.GetMultilingualString("A6591CCD-DE04-4B57-A22D-29FE296DC0AC", FilterDescription);
	}
}
