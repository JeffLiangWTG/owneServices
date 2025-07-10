using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageProcessors.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	sealed class TP5OutgoingMessageProcessorTest : FROutgoingMessageProcessorBaseTest<TP5OutgoingMessageProcessor>
	{
		protected override ZString MessageType => FR.Business.MessageTypeList.Codes.TP5;

		protected override ZString MessageSubType => TP5MessageTypeList.Codes.CC015C;

		protected override ZString ExpectedInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
	}
}
