using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX564Processor : AESMessageProcessor<AESInboundEDIMessage, EX564Provider>
	{
		public EX564Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("93F2EDC6-5BC5-473D-B41F-AAEA0FE802C5", "EX564: REQUEST DECLARATION INVALIDATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX564Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX564Provider provider) => AESEntryStatusList.Codes.CancellationRequestedByCustoms;

		protected override Type MessageInterpreterType => typeof(EX564MessageInterpreter);
	}
}
