using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class XTradeInboundMessageCreatorAbstractTest : InboundMessageCreatorBaseTest
{
	protected sealed override IInboundMessageCreator GetInboundMessageCreator()
		=> new XTradeInboundMessageCreator();
}
