using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class InboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public InboundInterchangeProcessor() : base(new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow }) { }

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new InboundMessageCreator(Logger);
		}
	}
}
