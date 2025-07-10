using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC599C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	abstract class CC599CProcessorTest<TMessageAttachee, TRelatedJob> : MessageAttacheeMessageProcessorTest<CC599CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC599CProvider, TMessageAttachee, TRelatedJob>
		where TMessageAttachee : BusinessObject, IMessageAttachee
		where TRelatedJob : BusinessObject, IRelatedJob
	{
		protected override ZString MessageFriendlyName => "CC599C: EXPORT NOTIFICATION";

		protected override CC599CProcessor Processor => new CC599CProcessor(logger, typeof(Cc599C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE599;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC599CText("A1", "LRN123456789", "21IEDUB11A782454R2");
	}
}
