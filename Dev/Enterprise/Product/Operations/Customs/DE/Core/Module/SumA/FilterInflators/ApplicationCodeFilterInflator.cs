using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class ApplicationCodeFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Application Code";

	public ApplicationCodeFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var applicationCodeFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_AppCode, Lookups.ApplicationCodeList);
		applicationCodeFilter.Category = FilterCategories.ModesAndTypes;
		applicationCodeFilter.MultilingualDescription = ResString.GetMultilingualString("b7715dc2-1777-4cce-a2ff-5a4bb23fa3d2", FilterDescription);
	}
}
