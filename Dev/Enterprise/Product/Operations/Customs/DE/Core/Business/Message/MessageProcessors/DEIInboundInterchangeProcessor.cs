using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class DEIInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public DEIInboundInterchangeProcessor() : base(new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAcknowledgementSystem })
		{
		}

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return customsInformationProcessor ?? (customsInformationProcessor = new DEICustomsAcknowledgementProcessor());
		}
		IInboundMessageCreator customsInformationProcessor;
	}
}
