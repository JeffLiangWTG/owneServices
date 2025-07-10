using System;
using System.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX864Processor : AESMessageProcessor<AESInboundEDIMessage, EX864Provider>
	{
		public EX864Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("ACB94F93-3CB3-415D-8338-1F1E1BDDC388", "EX864: Response message advising to DECLARATION INVALIDATION REQUEST CANCELLATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX864Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX864Provider provider) =>
			MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(
				messageAttachee: messageAttachee,
				incomingMessage: message,
				beforeMessageWithType: AESIncomingMessageTypeList.Codes.EX564,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			);

		protected override Type MessageInterpreterType => typeof(EX864MessageInterpreter);
	}
}
