using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC917C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	abstract class CC917CProcessorTest<TMessageAttachee, TRelatedJob> : MessageAttacheeMessageProcessorTest<CC917CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC917CProvider, TMessageAttachee, TRelatedJob>
		where TMessageAttachee : BusinessObject, IMessageAttachee
		where TRelatedJob : BusinessObject, IRelatedJob
	{
		protected override ZString MessageFriendlyName => "CC917C: XML NACK";

		protected override CC917CProcessor Processor => new CC917CProcessor(logger, typeof(Cc917C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE917;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC917CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
