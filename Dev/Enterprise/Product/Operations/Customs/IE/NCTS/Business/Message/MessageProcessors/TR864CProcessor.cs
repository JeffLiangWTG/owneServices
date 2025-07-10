using System;
using System.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR864CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR864CProvider>
	{
		public TR864CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("43D88BC9-0A3E-460A-B985-4B1E56D61EFF", "TR864C: CANCEL INVALIDATION REQUEST");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR864CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR864CProvider provider) =>
			MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(
				messageAttachee: messageAttachee,
				incomingMessage: message,
				beforeMessageWithType: NCTSIncomingMessageTypeList.Codes.TR064C,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			);

		protected override Type MessageInterpreterType => typeof(TR864CMessageInterpreter);
	}
}
