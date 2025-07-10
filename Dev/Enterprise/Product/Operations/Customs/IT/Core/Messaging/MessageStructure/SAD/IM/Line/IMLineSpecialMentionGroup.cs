using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineSpecialMentionGroup
{
	public IMLineSpecialMentionGroup(IIMLineSpecialMentionGroup iMLineSpecialMentionGroup)
	{
		this.iMLineSpecialMentionInfo = Argument.NotNull(iMLineSpecialMentionGroup, nameof(iMLineSpecialMentionGroup));
	}

	readonly IIMLineSpecialMentionGroup iMLineSpecialMentionInfo;

	[MessageLayout(Order = 0)]
	public IMLineSpecialMentionInfoEori Eori => new IMLineSpecialMentionInfoEori(iMLineSpecialMentionInfo.Eori);

	[MessageLayout(Order = 1)]
	public IMLineSpecialMentionInfoUnloadingData UnloadingData => new IMLineSpecialMentionInfoUnloadingData(iMLineSpecialMentionInfo.UnloadingData);

	[MessageLayout(Order = 2)]
	public IMLineSpecialMentionInfoPreviousProcedure PreviousProcedure => new IMLineSpecialMentionInfoPreviousProcedure(iMLineSpecialMentionInfo.PreviousProcedure);

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString SteelType => iMLineSpecialMentionInfo.SteelType;
}
