using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class LocalReferenceNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "LRN";

	public LocalReferenceNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var localReferenceNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryNum);
		localReferenceNumberFilter.SubGroup = GetFilterSubGroup();
		localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		localReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("F94B3BC6-1691-4D06-8C09-D1CB91889A9D", FilterDescription);
	}

	protected virtual ModuleFilterSubGroup GetFilterSubGroup() => new ReferenceNumberSubGroup(CusEntryNumberTypes.Standard.LocalReferenceNumber);
}
