using System;
using System.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM864Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM864Provider>
	{
		public IM864Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("65015308-A24A-4618-8C8E-A9B468BE754F", "IM864: Declaration Invalidation Request Cancellation");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM864Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM864Provider provider) =>
			MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(
				messageAttachee: messageAttachee,
				incomingMessage: message,
				beforeMessageWithType: AISInterchangeTypeList.Codes.IM464,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override Type MessageInterpreterType => typeof(IM864MessageInterpreter);
	}
}
