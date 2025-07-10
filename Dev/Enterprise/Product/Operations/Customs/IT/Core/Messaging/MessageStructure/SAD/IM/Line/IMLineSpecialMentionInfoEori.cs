using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineSpecialMentionInfoEori
{
	public IMLineSpecialMentionInfoEori(ISpecialMentionEoriInfo specialMentionInfoEori)
	{
		this.specialMentionInfoEori = Argument.NotNull(specialMentionInfoEori, nameof(specialMentionInfoEori));
	}

	readonly ISpecialMentionEoriInfo specialMentionInfoEori;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 16, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString FirstEoriCode => specialMentionInfoEori.FirstEoriCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 16, false)]
	[MessageFieldImportRules("D", "CN13")]
	[MessageFieldDepositoRules("D", "CN13")]
	public ZString SecondEoriCode => specialMentionInfoEori.SecondEoriCode;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(17, 2, false)]
	[MessageFieldImportRules("D", "CN13", "TRN0001")]
	[MessageFieldDepositoRules("D", "CN13", "TRN0001")]
	public ZDecimal? PreviousInvoiceAmount => specialMentionInfoEori.PreviousInvoiceAmount;
}
