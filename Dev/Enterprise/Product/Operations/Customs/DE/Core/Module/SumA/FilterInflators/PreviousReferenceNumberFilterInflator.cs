using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class PreviousReferenceNumberFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Previous Reference Number";

	public PreviousReferenceNumberFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var previousReferenceNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_PreviousReferenceNumber);
		previousReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		previousReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("3d58d9f5-f3b6-41e6-b767-833cf7266a60", FilterDescription);
	}
}
