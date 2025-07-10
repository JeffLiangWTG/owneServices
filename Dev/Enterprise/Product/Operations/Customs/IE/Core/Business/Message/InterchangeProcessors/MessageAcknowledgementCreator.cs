using System;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	class MessageAcknowledgementCreator : InboundMessageCreator<MessageAcknowledgement>
	{
		const string messageNumberSuffix = "01";

		public MessageAcknowledgementCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override void HandleDataWhenItIsNotASOAPEnvelope(EDIInterchange interchange)
		{
			if (GetOutgoingMessage(interchange) is BaseEDIMessage outgoingMessage)
			{
				CreateIncomingMessageForInvalidData(interchange, outgoingMessage);
			}
			else
			{
				base.HandleDataWhenItIsNotASOAPEnvelope(interchange);
			}
		}

		BaseEDIMessage GetOutgoingMessage(EDIInterchange interchange)
		{
			BaseEDIMessage result = null;
			var sessionGUID = interchange.EI_SessionGUID;
			if (sessionGUID.IsValid)
			{
				var interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
				interchangeQuery.AddToFilter(interchange.BuildOutgoingInterchangeQueryMatchingSessionGUID());

				var messageQuery = new ZDBOnlyQuery(typeof(BaseEDIMessage));
				messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeQuery, JoinCondition.And);
				messageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Ascending;
				result = interchange.Factory.LoadTop1<BaseEDIMessage>(messageQuery);
			}
			return result;
		}

		void CreateIncomingMessageForInvalidData(EDIInterchange interchange, BaseEDIMessage outgoingMessage)
		{
			var incomingMessage = CreateIncomingMessage(interchange, outgoingMessage.EM_ApplicationCode, outgoingMessage.EM_MessageType, EDIMessage.Status.Error, outgoingMessage.EM_GB, EDIMessage.Status.PreProcessedOK, interchange.EI_BodyText);
			MessageCreator.SetPreProcessData(incomingMessage, outgoingMessage);
			incomingMessage.EM_ApplicationReference = outgoingMessage.EM_ApplicationReference;
			if (outgoingMessage.EM_LinkedObject is IMessageAttachee attachee)
			{
				attachee.LogicalStatus = LogicalStatusList.Codes.Failed;
			}
			logger.Log(string.Format("Created error message ({0}, {1}, {2}, {3}, {4}).", incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, EDIMessage.Direction.Receive, incomingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference));
		}

		BaseEDIMessage CreateIncomingMessage(EDIInterchange interchange, ZString applicationCode, ZString messageType, ZString messageSubType, ZGuid branchPK, ZString status, ZString messageText)
		{
			var incomingMessage = interchange.ContainedMessages.AddNew();
			incomingMessage.EM_ApplicationCode = applicationCode;
			incomingMessage.EM_GB = branchPK;
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_MessageSubType = messageSubType;
			incomingMessage.EM_Status = status;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
			incomingMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength - 2) + messageNumberSuffix;
			return incomingMessage;
		}

		protected override void CreateMessagesForInterchange(EDIInterchange interchange, MessageAcknowledgement response)
		{
			var incomingMessage = CreateIncomingMessage(interchange, interchange.EI_ApplicationCode, interchange.EI_InterchangeType, CommonInterchangeTypeList.Codes.MessageAcknowledge, interchange.EI_GB, EDIMessage.Status.Queued, IEXmlObjectSerializer.Serialize(response));
			var transactionId = response.TransactionId;
			if (string.IsNullOrEmpty(transactionId))
			{
				if (GetOutgoingMessage(interchange) is BaseEDIMessage outgoingMessage)
				{
					MessageCreator.SetPreProcessData(incomingMessage, outgoingMessage);
					transactionId = outgoingMessage.EM_ApplicationReference;
					incomingMessage.EM_ApplicationReference = transactionId;
				}
			}
			else
			{
				incomingMessage.EM_ApplicationReference = transactionId;
				if (InboundEDIMessage.GetOriginalMessage(incomingMessage.Factory, incomingMessage.EM_ApplicationCode, transactionId) is BaseEDIMessage originalMessage)
				{
					MessageCreator.SetPreProcessData(incomingMessage, originalMessage);
				}
			}
			logger.Log(string.Format("Created message ({0}, {1}, {2}, {3}{4}).", incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, EDIMessage.Direction.Receive, incomingMessage.EM_MessageNum, string.IsNullOrEmpty(transactionId) ? string.Empty : ", " + transactionId));
		}

		protected override string GetErrorLog(Exception e)
		{
			return (NoResString)"Invalid message type. Expected: MessageAcknowledgement";
		}
	}
}
