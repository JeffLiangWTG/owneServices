using System;
using System.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM862Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM862Provider>
	{
		public IM862Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("BA3AD8B9-AE96-45A3-B77D-389CB33C7079", "IM862: Declaration Amendment Request Cancellation");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM862Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM862Provider provider) =>
			MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(
				messageAttachee: messageAttachee,
				incomingMessage: message,
				beforeMessageWithType: AISInterchangeTypeList.Codes.IM462,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override Type MessageInterpreterType => typeof(IM862MessageInterpreter);
	}
}
