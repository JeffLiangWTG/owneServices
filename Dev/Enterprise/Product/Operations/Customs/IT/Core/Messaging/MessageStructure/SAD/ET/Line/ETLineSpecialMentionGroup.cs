using CargoWise.Common;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineSpecialMentionGroup
{
	readonly IETLineSpecialMentionGroup iETLineSpecialMentionGroup;

	public ETLineSpecialMentionGroup(IETLineSpecialMentionGroup iETLineSpecialMentionGroup)
	{
		this.iETLineSpecialMentionGroup = Argument.NotNull(iETLineSpecialMentionGroup, "iETLineSpecialMentionGroup");
	}

	[MessageLayout(Order = 0)]
	public ETLineSpecialMentionInfoEori Eori => new ETLineSpecialMentionInfoEori(iETLineSpecialMentionGroup.Eori);

	[MessageLayout(Order = 1)]
	public ETLineSpecialMentionInfoUnloadingData UnloadingData => new ETLineSpecialMentionInfoUnloadingData(iETLineSpecialMentionGroup.UnloadingData);
}
