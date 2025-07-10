using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using TemporaryStorageHeader = Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class MessageStatusFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Message Status";

	public MessageStatusFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	new TemporaryStorageHeader TemporaryStorageHeader => Factory.GetNull<TemporaryStorageHeader>();

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var messageStatusFilter = filterCollection.AddTextFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_MessageStatus, TemporaryStorageHeader.Lookups.PNTSMessageStatusList);
		messageStatusFilter.Category = FilterCategories.StatusAndFlags;
		messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0", FilterDescription);
	}
}
