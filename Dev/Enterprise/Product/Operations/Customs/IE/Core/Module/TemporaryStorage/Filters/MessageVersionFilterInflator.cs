using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Module;

public sealed class MessageVersionFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Message Version";

	public MessageVersionFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var messageVersionFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_ManifestType, TemporaryStorageHeader.Lookups.ManifestTypeList);
		messageVersionFilter.Category = FilterCategories.StatusAndFlags;
		messageVersionFilter.MultilingualDescription = ResString.GetMultilingualString("057458B8-DB30-476C-8FC1-1BE9C031D264", FilterDescription);
	}
}
