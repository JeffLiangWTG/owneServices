using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR862CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR862CProvider>
	{
		public TR862CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR862CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR862CProvider provider)
		{
			(string status, NCTSInboundEDIMessage historyMessage) result = (null, null);

			do
			{
				result = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(
					messageAttachee: messageAttachee,
					incomingMessage: message,
					beforeMessageWithType: result.historyMessage?.EM_MessageType ?? NCTSIncomingMessageTypeList.Codes.TR062C,
					additionalComparer: GetAdditionalComparer(ListSortDirection.Descending)
				);
			} while (result.historyMessage != null && result.status == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested);

			return result.status;
		}

		internal protected virtual IComparer<Enterprise.Messaging.Business.EDIMessage> GetAdditionalComparer(ListSortDirection direction) =>
			new IEEDIMessageComparer(direction);

		protected override string MessageFriendlyNameCore => (NoResString)"TR862C: DECLARATION AMENDMENT REQUEST CANCELLATION";

		protected override Type MessageInterpreterType => typeof(TR862CMessageInterpreter);
	}
}
