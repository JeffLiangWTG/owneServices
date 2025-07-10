using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class DeclarantFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	protected const string FilterDescription = "Declarant";

	public DeclarantFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var declarantFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.OrgAddresses, AsycudaManifestHeaderSchema.AMA_OA_Declarant, new OrgAddressCollection(Factory));
		declarantFilter.Category = FilterCategories.Organisations;
		declarantFilter.MultilingualDescription = ResString.GetMultilingualString("308B5871-A46C-479D-9C6B-FFB6092D4DE6", FilterDescription);
	}
}
