using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class InboundMessageCreator : IInboundMessageCreator
{
	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var singleMessageCreator = new InboundSingleMessageCreator();
		_ = singleMessageCreator.CreateMessageForInterchange(interchange);
	}
}
