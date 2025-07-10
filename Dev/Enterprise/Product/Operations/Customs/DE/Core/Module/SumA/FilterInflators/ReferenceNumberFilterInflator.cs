using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class ReferenceNumberFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Reference Number";

	public ReferenceNumberFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var referenceNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_ReferenceNumber);
		referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0277cb67-dbf1-4826-aa58-4d6b0af84874", FilterDescription);
	}
}
