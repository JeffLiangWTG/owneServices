using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class EUICS2BranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetApplicationTypeProcessor()
		{
			var messageTypesAndProcessors = new[]
			{
				new { MessageType = MessageTypes.Codes.N01, ProcessorType = typeof(IE3N01MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N02, ProcessorType = typeof(IE3N02MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N03, ProcessorType = typeof(IE3N03MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N04, ProcessorType = typeof(IE3N04MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N05, ProcessorType = typeof(IE3N05MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N08, ProcessorType = typeof(IE3N08MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N09, ProcessorType = typeof(IE3N09MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N10, ProcessorType = typeof(IE3N10MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N11, ProcessorType = typeof(IE3N11MessageProcessor) },
				new { MessageType = MessageTypes.Codes.N99, ProcessorType = typeof(IE3N99MessageProcessor) },
				new { MessageType = MessageTypes.Codes.R01, ProcessorType = typeof(IE3R01MessageProcessor) },
				new { MessageType = MessageTypes.Codes.R07, ProcessorType = typeof(IE3R07MessageProcessor) },
				new { MessageType = MessageTypes.Codes.R08, ProcessorType = typeof(IE3R08MessageProcessor) },
				new { MessageType = MessageTypes.Codes.Q01, ProcessorType = typeof(IE3Q01MessageProcessor) },
				new { MessageType = MessageTypes.Codes.Q02, ProcessorType = typeof(IE3Q02MessageProcessor) },
				new { MessageType = MessageTypes.Codes.Q03, ProcessorType = typeof(IE3Q03MessageProcessor) },
				new { MessageType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER, ProcessorType = typeof(XERMessageProcessor) },
			};
			CombineAssertions(() =>
			{
				foreach (var messageTypeAndProcessor in messageTypesAndProcessors)
				{
					var messageType = messageTypeAndProcessor.MessageType;
					AssertEquals($"MessageType: {messageType}; MessageSubType: empty", messageTypeAndProcessor.ProcessorType, GetApplicationTypeProcessor(messageType).GetType());
					AssertEquals($"MessageType: {messageType}; MessageSubType: XXX", typeof(SoapEnvelopeMessageProcessor), GetApplicationTypeProcessor(messageType, ICS2InboundEDIMessage.UndefinedSubType).GetType());
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new EUICS2BranchCustomsMessageProcessor();
			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IC2;
		}
		EUICS2BranchCustomsMessageProcessor processor;
		EDIMessage message;

		ApplicationTypeMessageProcessor GetApplicationTypeProcessor(ZString messageType, ZString messageSubType = new ZString())
		{
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			return processor.GetApplicationTypeProcessorCore(message);
		}
	}
}
