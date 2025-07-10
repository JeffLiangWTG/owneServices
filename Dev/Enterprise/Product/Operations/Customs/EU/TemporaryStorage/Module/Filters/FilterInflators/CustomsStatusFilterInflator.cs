using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class CustomsStatusFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Customs Status";

	public CustomsStatusFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsStatusFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryStatus, TemporaryStorageHeader.Lookups.CustomsStatusList);
		customsStatusFilter.SubGroup = new ReferenceNumberSubGroup(CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
		customsStatusFilter.Category = FilterCategories.StatusAndFlags;
		customsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("5797752B-CE72-4597-9DEA-F8A418258944", FilterDescription);
	}
}
