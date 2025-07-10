using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC609CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC609CProvider>
	{
		public CC609CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("B9025619-E25A-45CD-91DF-1EEAB6EB7155", "CC609C: EXIT SUMMARY INVALIDATION/CANCELLATION");

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC609CProvider provider) => AESEntryStatusList.Codes.Cancelled;

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC609CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC609MessageInterpreter);
	}
}
