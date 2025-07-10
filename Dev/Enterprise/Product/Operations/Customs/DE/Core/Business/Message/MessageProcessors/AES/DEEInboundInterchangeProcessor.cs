using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEEInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public DEEInboundInterchangeProcessor()
			: base(new ZString[] { EDIInterchange.ApplicationCodes.DECustomsAesSystem })
		{
		}

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new DEEInboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;
	}
}
