using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineSpecialMentionInfoUnloadingData
{
	readonly ISpecialMentionUnloadingDataInfo iSpecialMentionUnloadingDataInfo;

	public ETLineSpecialMentionInfoUnloadingData(ISpecialMentionUnloadingDataInfo iSpecialMentionUnloadingDataInfo)
	{
		this.iSpecialMentionUnloadingDataInfo = Argument.NotNull(iSpecialMentionUnloadingDataInfo, "iSpecialMentionUnloadingDataInfo");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 10, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZString CommodityCode => iSpecialMentionUnloadingDataInfo.CommodityCode;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(13, 5, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZDecimal? Quantity => iSpecialMentionUnloadingDataInfo.Quantity;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(13, 5, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZDecimal? SupplementaryUnit => iSpecialMentionUnloadingDataInfo.SupplementaryUnit;
}
