using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaIEOutgoingMessageProcessorTest : FROutgoingMessageProcessorBaseTest<DeltaIEOutgoingMessageProcessor>
	{
		protected override ZString MessageText => @"{""ImportOperation"":{""LRN"":""0000005856""}}";

		protected override ZString MessageType => MessageTypeList.Codes.DEC;

		protected override ZString MessageSubType => DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;

		protected override ZString ExpectedInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;

		protected override ZString ExpectedInterchangeBodyText(TestEDIMessage message) => @"{""SchemaId"":""IE415"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":{""LRN"":""0000005856""}}}";
	}
}
