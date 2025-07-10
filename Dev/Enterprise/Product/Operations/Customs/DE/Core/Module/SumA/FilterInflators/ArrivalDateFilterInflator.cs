using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class ArrivalDateFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Arrival Date";

	public ArrivalDateFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var arrivalDate = filterCollection.AddDateFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_ArrivalDate);
		arrivalDate.Category = FilterCategories.Dates;
		arrivalDate.MultilingualDescription = ResString.GetMultilingualString("36df38e0-c064-47ff-bae4-6d4ef7654369", FilterDescription);
	}
}
