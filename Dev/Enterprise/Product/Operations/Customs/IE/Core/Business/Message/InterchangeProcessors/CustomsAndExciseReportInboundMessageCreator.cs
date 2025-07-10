using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	class CustomsAndExciseReportInboundMessageCreator : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var outgoingMessages = GetOutgoingMessage(interchange);
			foreach (var outgoingMessage in outgoingMessages)
			{
				CreateIncomingMessage(interchange, outgoingMessage);
			}
		}

		CustomsAndExciseReportOutboundMessage[] GetOutgoingMessage(EDIInterchange interchange)
		{
			CustomsAndExciseReportOutboundMessage[] result = null;
			var sessionGUID = interchange.EI_SessionGUID;
			if (sessionGUID.IsValid)
			{
				var interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
				interchangeQuery.AddToFilter(interchange.BuildOutgoingInterchangeQueryMatchingSessionGUID());

				var messageQuery = new ZDBOnlyQuery(typeof(CustomsAndExciseReportOutboundMessage));
				messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeQuery, JoinCondition.And);
				messageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Ascending;
				result = interchange.Factory.Load<CustomsAndExciseReportOutboundMessage>(messageQuery);
			}
			return result ?? Array.Empty<CustomsAndExciseReportOutboundMessage>();
		}

		BaseEDIMessage CreateIncomingMessage(EDIInterchange interchange, CustomsAndExciseReportOutboundMessage outgoingMessage)
		{
			var incomingMessage = interchange.ContainedMessages.AddNew(typeof(CustomsAndExciseReportInboundMessage));
			incomingMessage.EM_GB = outgoingMessage.EM_GB;
			incomingMessage.EM_LinkedObject = outgoingMessage;
			incomingMessage.EM_MessageType = outgoingMessage.EM_MessageType;
			incomingMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			incomingMessage.EM_MessageText = interchange.EI_BodyText;
			incomingMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength - 2) + messageNumberSuffix;
			return incomingMessage;
		}

		const string messageNumberSuffix = "01";
	}
}
