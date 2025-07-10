using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineSpecialMentionInfoUnloadingData
{
	public IMLineSpecialMentionInfoUnloadingData(ISpecialMentionUnloadingDataInfo specialMentionInfoUnloadingData)
	{
		this.specialMentionInfoUnloadingData = Argument.NotNull(specialMentionInfoUnloadingData, nameof(specialMentionInfoUnloadingData));
	}

	readonly ISpecialMentionUnloadingDataInfo specialMentionInfoUnloadingData;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CommodityCode => specialMentionInfoUnloadingData.CommodityCode;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O", "TRN0002")]
	public ZDecimal? Quantity => specialMentionInfoUnloadingData.Quantity;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("O", "TRN0002")]
	[MessageFieldDepositoRules("O", "TRN0002")]
	public ZDecimal? SupplementaryUnit => specialMentionInfoUnloadingData.SupplementaryUnit;
}
