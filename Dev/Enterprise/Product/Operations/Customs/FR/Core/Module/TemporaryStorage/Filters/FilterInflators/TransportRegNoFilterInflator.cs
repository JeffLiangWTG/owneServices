using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

public sealed class TransportRegNoFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Transport Reg. No.";

	public TransportRegNoFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var transportRegNoFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_TransportRegNo);
		transportRegNoFilter.Category = FilterCategories.NumbersAndReferences;
		transportRegNoFilter.MultilingualDescription = ResString.GetMultilingualString("AAC82526-43DB-4525-9606-9A93CE79F3B6", FilterDescription);
	}
}
