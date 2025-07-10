using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class CurrentDeclarationTypeFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Current Declaration Type";

	public CurrentDeclarationTypeFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var currentDeclarationType = filterCollection.AddTextFilter(FilterDescription, CusTempStorageDecSchema.STH_DeclarationType, Lookups.DeclarationTypeList);
		currentDeclarationType.MultilingualDescription = ResString.GetMultilingualString("405bd2c5-cd94-4e7d-bb0b-c19b7dc8440d", FilterDescription);
		currentDeclarationType.Category = FilterCategories.ModesAndTypes;
		currentDeclarationType.SubGroup = new MostRecentlyModifiedDeclarationSubGroup();
	}
}
