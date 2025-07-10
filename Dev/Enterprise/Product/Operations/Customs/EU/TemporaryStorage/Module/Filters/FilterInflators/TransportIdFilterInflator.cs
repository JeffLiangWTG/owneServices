using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class TransportIdFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Transport ID";

	public TransportIdFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var transportIDFilter = filterCollection.AddTextFilter(FilterDescription, CusTransportMeansSchema.TPM_IdentificationNumber);
		transportIDFilter.SubGroup = new TransportMeansSubGroup();
		transportIDFilter.Category = FilterCategories.ModesAndTypes;
		transportIDFilter.MultilingualDescription = ResString.GetMultilingualString("D380E34C-CBC0-4308-BF15-093A59C701B3", FilterDescription);
	}
}
