using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class MessageTypeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Message Type";

	public MessageTypeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var messageTypeFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_MessageType, TemporaryStorageHeader.Lookups.MessageTypeList);
		messageTypeFilter.Category = FilterCategories.ModesAndTypes;
		messageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("40A3C04A-D51F-4F6D-9A41-1FAFB1B81024", FilterDescription);
	}
}
