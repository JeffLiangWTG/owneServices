using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class MovementReferenceNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "MRN";

	public MovementReferenceNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var movementReferenceNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryNum);
		movementReferenceNumberFilter.SubGroup = GetFilterSubGroup();
		movementReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		movementReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("C634252F-6BF0-4193-92ED-2AB08611A8A6", FilterDescription);
	}

	protected virtual ModuleFilterSubGroup GetFilterSubGroup() => new ReferenceNumberSubGroup(CusEntryNumberTypes.Standard.MovementReferenceNumber);
}
