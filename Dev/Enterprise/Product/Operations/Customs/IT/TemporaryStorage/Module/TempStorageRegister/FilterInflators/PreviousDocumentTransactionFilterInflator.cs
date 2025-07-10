using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class PreviousDocumentTransactionFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Previous Document (transaction)";

	public PreviousDocumentTransactionFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("22333FBD-7E44-43B1-A1D2-FFBA08DB84C6", FilterDescription);
		filter.SubGroup = new CusTempStorageRegLineTransactionSubGroup();
	}
}
