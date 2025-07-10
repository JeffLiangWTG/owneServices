using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public static class MessageAttacheeMessageHistoryHandler
	{
		public static string GetEntryStatusFromMessageHistory<TMessage>(
			IMessageAttachee messageAttachee,
			TMessage incomingMessage,
			string beforeMessageWithType = null,
			IComparer<Enterprise.Messaging.Business.EDIMessage> additionalComparer = null
		) where TMessage : InboundEDIMessage =>
			GetEntryStatusAndMessageFromMessageHistory(messageAttachee, incomingMessage, beforeMessageWithType, additionalComparer).status;

		public static (string status, TMessage historyMessage) GetEntryStatusAndMessageFromMessageHistory<TMessage>(
			IMessageAttachee messageAttachee,
			TMessage incomingMessage,
			string beforeMessageWithType = null,
			IComparer<Enterprise.Messaging.Business.EDIMessage> additionalComparer = null
		) where TMessage : InboundEDIMessage
		{
			string entryStatus = null;
			TMessage resultMessage = null;
			var messageHistoryQuery = GetMessageHistoryCreateTimeDescendingQuery(messageAttachee, incomingMessage);
			var messageHistory = messageAttachee.Factory.Load<TMessage>(messageHistoryQuery);
			messageHistory = messageHistory
				.GroupBy(message => message.EM_SystemCreateTimeUtc)
				.Select(grouped =>
					additionalComparer == null
					? grouped.AsEnumerable()
					: grouped.AsEnumerable().OrderBy(message => message, additionalComparer)
				)
				.SelectMany(group => group)
				.ToArray();

			int indexToFind = -1;
			if (beforeMessageWithType == null)
			{
				indexToFind = 0;
			}
			else
			{
				var messageIndexToRevert = Array.FindIndex(messageHistory, message => message.EM_MessageType == beforeMessageWithType);
				if (messageIndexToRevert != -1)
				{
					indexToFind = messageIndexToRevert + 1;
				}
			}

			TMessage messageToFind = null;
			while (indexToFind > -1 && indexToFind < messageHistory.Length && entryStatus == null)
			{
				messageToFind = messageHistory[indexToFind++];
				entryStatus = GetEntryStatusByMessage(messageAttachee, messageToFind);
			}

			if (!string.IsNullOrEmpty(entryStatus))
			{
				resultMessage = messageToFind;
			}

			return (entryStatus, resultMessage);
		}

		public static ZQuery GetMessageHistoryCreateTimeDescendingQuery<TMessage>(IMessageAttachee messageAttachee, TMessage incomingMessage) where TMessage : InboundEDIMessage
		{
			return new ZQuery(EDIMessageSchema.EM_ApplicationCode, incomingMessage.EM_ApplicationCode) { OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending }
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, InboundEDIMessage.Direction.Receive)
				.AddToFilter(EDIMessageSchema.EM_LinkTable, messageAttachee.TableName)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, messageAttachee.PK)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.ProcessedOK)
				.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, incomingMessage.EM_SystemCreateTimeUtc);
		}

		static string GetEntryStatusByMessage<TMessage>(IMessageAttachee messageAttachee, TMessage historicalMessage) where TMessage : InboundEDIMessage
		{
			string result = null;

			if(GetProcessor(historicalMessage) is object processor && GetDataProvider(historicalMessage, processor) is object dataProvider)
			{
				result = (string)processor.GetType()
					.GetMethod(nameof(MessageAttacheeMessageProcessor<TMessage, object>.GetEntryStatus), BindingFlags.Instance | BindingFlags.Public)?
					.Invoke(processor, new[] { historicalMessage, messageAttachee, dataProvider });
			}
			return result;
		}

		public static object GetProcessor<TMessage>(this TMessage message) where TMessage : InboundEDIMessage
		{
			object processor = null;
			var responseDetail = ResponseMessageDetails.GetResponseDetail(message.EM_ApplicationCode, message.EM_MessageType, message.EM_MessageSubType, message.EM_MessageText);
			if (responseDetail?.ProcessorType != null)
			{
				var processorType = responseDetail.ProcessorType;
				processor = Activator.CreateInstance(processorType, null, responseDetail.XmlObjectType);
			}
			return processor;
		}

		public static object GetDataProvider<TMessage>(this TMessage message) where TMessage : InboundEDIMessage => GetDataProvider(message, GetProcessor(message));

		public static object GetDataProvider<TMessage>(this TMessage message, object processor) where TMessage : InboundEDIMessage
			=> processor.GetType()
					.GetMethod(nameof(MessageAttacheeMessageProcessor<TMessage, object>.GetDataProvider), BindingFlags.Instance | BindingFlags.NonPublic)?
					.Invoke(processor, new object[] { message, false });
	}
}
