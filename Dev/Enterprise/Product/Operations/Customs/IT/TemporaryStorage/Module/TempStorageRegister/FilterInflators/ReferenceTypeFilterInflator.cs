using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class ReferenceTypeFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Reference Type";

	public ReferenceTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageRegLineTransactionSchema.SRT_ReferenceType, Factory.GetNull<CusTempStorageRegLineTransaction>().Lookups.ReferenceTypeList);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("25FD66C3-8044-4CE1-B9FE-FD933067B45E", FilterDescription);
		filter.SubGroup = new CusTempStorageRegLineTransactionSubGroup();
	}
}
