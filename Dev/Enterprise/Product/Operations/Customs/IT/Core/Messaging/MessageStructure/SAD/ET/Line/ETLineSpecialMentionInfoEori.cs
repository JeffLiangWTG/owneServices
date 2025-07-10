using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineSpecialMentionInfoEori
{
	readonly ISpecialMentionEoriInfo iSpecialMentionEoriInfo;

	public ETLineSpecialMentionInfoEori(ISpecialMentionEoriInfo iSpecialMentionEoriInfo)
	{
		this.iSpecialMentionEoriInfo = Argument.NotNull(iSpecialMentionEoriInfo, "iSpecialMentionEoriInfo");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportRules("D", "CN12")]
	[MessageFieldExportWithTransitRules("D", "CN12")]
	[MessageFieldTransitRules("D", "CN12")]
	public ZString FirstEoriCode => iSpecialMentionEoriInfo.FirstEoriCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportRules("D", "CN13")]
	[MessageFieldExportWithTransitRules("D", "CN13")]
	[MessageFieldTransitRules("D", "CN13")]
	public ZString SecondEoriCode => iSpecialMentionEoriInfo.SecondEoriCode;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(14, 2, false, true)]
	[MessageFieldExportRules("D", "CN13a")]
	[MessageFieldExportWithTransitRules("D", "CN13a")]
	[MessageFieldTransitRules("D", "CN13a")]
	public ZDecimal? PreviousInvoiceAmount => iSpecialMentionEoriInfo.PreviousInvoiceAmount;
}
