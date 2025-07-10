using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class GrossWeightTransactionFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Gross Weight (transaction)";

	public GrossWeightTransactionFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddNumberRangeFilter(FilterDescription, CusTempStorageRegLineTransactionSchema.SRT_GrossWeight);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("939099C4-560E-44C4-AA44-D9622C5D706A", FilterDescription);
		filter.SubGroup = new CusTempStorageRegLineTransactionSubGroup();
	}
}
