using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class BillNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Bill Number";

	public BillNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var billNumberFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaBillSchema.ABL_BillNumber);
		billNumberFilter.SubGroup = new BillNumberSubGroup();
		billNumberFilter.Category = FilterCategories.NumbersAndReferences;
		billNumberFilter.MultilingualDescription = ResString.GetMultilingualString("697A2DC7-6D44-40A5-B751-14F53D6FA2AE", FilterDescription);
	}
}
