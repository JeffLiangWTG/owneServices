using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class DEMInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public DEMInboundInterchangeProcessor() : base(new ZString[] { EDIInterchange.ApplicationCodes.DECustomsEmcsSystem })
		{
		}

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new DEMInboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;
	}
}
