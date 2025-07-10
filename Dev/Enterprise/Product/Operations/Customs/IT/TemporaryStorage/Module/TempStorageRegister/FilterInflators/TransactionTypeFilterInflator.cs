using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class TransactionTypeFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Transaction Type";

	public TransactionTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, Factory.GetNull<CusTempStorageRegLineTransaction>().Lookups.TransactionTypeList);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("B5589085-D86E-4E77-805F-7DAEBBA3B27E", FilterDescription);
		filter.SubGroup = new CusTempStorageRegLineTransactionSubGroup();
	}
}
