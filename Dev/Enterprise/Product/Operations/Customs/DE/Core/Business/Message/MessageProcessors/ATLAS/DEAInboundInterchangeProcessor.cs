using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public class DEAInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public DEAInboundInterchangeProcessor()
			: base(new ZString[] { EDIInterchange.ApplicationCodes.DECustomsAtlasSystem })
		{
		}

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new DEAInboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;
	}
}
