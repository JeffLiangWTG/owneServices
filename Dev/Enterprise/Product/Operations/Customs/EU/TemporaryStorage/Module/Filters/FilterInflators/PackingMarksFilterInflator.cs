using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class PackingMarksFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Packing Marks";

	public PackingMarksFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var packingMarksFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaPackSchema.APA_MarksAndNumbers);
		packingMarksFilter.SubGroup = new PackSubGroup();
		packingMarksFilter.Category = FilterCategories.NumbersAndReferences;
		packingMarksFilter.MultilingualDescription = ResString.GetMultilingualString("FE74C555-76CF-4C49-BE53-3F5FF6BCDB7F", FilterDescription);
	}
}
