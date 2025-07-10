using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class TransportTypeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Transport Type";

	public TransportTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var transportTypeFilter = filterCollection.AddTextFilter(FilterDescription, CusTransportMeansSchema.TPM_TypeOfIdentification, TemporaryStorageHeader.Lookups.TransportTypeList);
		transportTypeFilter.SubGroup = new TransportMeansSubGroup();
		transportTypeFilter.Category = FilterCategories.ModesAndTypes;
		transportTypeFilter.MultilingualDescription = ResString.GetMultilingualString("5239F9A6-D9A8-4E9A-95BC-35F937BE485B", FilterDescription);
	}
}
