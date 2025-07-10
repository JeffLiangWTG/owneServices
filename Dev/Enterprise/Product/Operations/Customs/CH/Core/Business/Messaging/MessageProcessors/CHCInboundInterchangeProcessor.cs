using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class CHCInboundInterchangeProcessor : BranchInboundInterchangeProcessor
{
	public CHCInboundInterchangeProcessor() : base(new[] { EDIInterchange.ApplicationCodes.CHCustomsEdec, EDIInterchange.ApplicationCodes.CHCustomsPassar, EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput }) { }

	protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => InboundMessageCreatorFactory.GetNew(Logger, interchange);
}
