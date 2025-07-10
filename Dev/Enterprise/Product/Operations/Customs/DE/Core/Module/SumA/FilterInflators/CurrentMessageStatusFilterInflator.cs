using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class CurrentMessageStatusFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Current Message Status";

	public CurrentMessageStatusFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var currentMessageStatus = filterCollection.AddTextFilter(FilterDescription, CusTempStorageDecSchema.STH_MessageStatus, Lookups.MessageStatusList);
		currentMessageStatus.MultilingualDescription = ResString.GetMultilingualString("f150d92f-0ce0-47a6-b82f-5140f8b4dbe6", FilterDescription);
		currentMessageStatus.Category = FilterCategories.ModesAndTypes;
		currentMessageStatus.SubGroup = new MostRecentlyModifiedDeclarationSubGroup();
	}
}
