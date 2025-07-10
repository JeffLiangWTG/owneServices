using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class JobReferenceTransactionFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Job Reference (transaction)";

	public JobReferenceTransactionFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageRegLineTransactionSchema.SRT_Reference);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("4C0101F0-11A0-4703-AE64-55D689ABE945", FilterDescription);
		filter.SubGroup = new CusTempStorageRegLineTransactionSubGroup();
	}
}
