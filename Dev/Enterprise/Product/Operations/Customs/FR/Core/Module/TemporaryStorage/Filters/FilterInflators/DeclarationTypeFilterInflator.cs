using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

public sealed class DeclarationTypeFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Declaration Type";

	public DeclarationTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var declarationTypeFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_ManifestType);
		declarationTypeFilter.Category = FilterCategories.ModesAndTypes;
		declarationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("5BB4C5F8-5115-4045-A913-956C0F50B12C", FilterDescription);
	}
}
