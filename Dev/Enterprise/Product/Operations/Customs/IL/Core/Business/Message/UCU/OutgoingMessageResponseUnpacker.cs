using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.GEN.RES_910.NG_9101_MSG_OutgoingMessageResponse;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	sealed class OutgoingMessageResponseUnpacker : IFeedbackMessageUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, ZString bodyText, ZString responseHeaderText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, Integration.BatchProcessor.ILoggingInformation logger)
		{
			var unsupportedServiceMessageCount = 0;
			var correlationAlreadyExistMessageCount = 0;
			var totalMessagesCreated = 0;
			var ediMessageList = new List<EDIMessage>();
			var correlationIdList = new List<ZString>();
			var outgoingMessageResponse = new MessageDeserializer<Ng9101MsgOutgoingMessageResponse>().DeserializeMessage(bodyText);
			var factory = interchange.Factory;
			var dCAParameters = ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.GetFallBackValueAtAllLevels(interchange.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var needToSendSyncAcknowledgementMessage = (dCAParameters.PeekWay == PeekWayList.Codes._2);
			IUniversalCustomsInterchangeUnpackerResult result = null;

			foreach (var currentOutgoingMessage in outgoingMessageResponse?.OutgoingMessage ?? new System.Collections.ObjectModel.Collection<Ng9101MsgOutgoingMessageResponseOutgoingMessage>())
			{
				var currentOutgoingMessageCorrelationId = currentOutgoingMessage.CorrelationId;
				if (DoesCorrelationIdExist(factory, currentOutgoingMessageCorrelationId))
				{
					correlationAlreadyExistMessageCount++;
					correlationIdList.Add(currentOutgoingMessageCorrelationId);
					continue;
				}

				var msg = currentOutgoingMessage.Msg;
				if (msg.IsNullOrEmpty())
				{
					return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeBodyTextIsEmpty);
				}

				var doc = Extensions.TryParseXML(msg);
				if (doc is null)
				{
					return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeBodyTextIsNotValidXML);
				}

				var bodyContent = (ZString)doc.Root?.Element(XName.Get(Unpacker.ResponseBody))?.LastNode.ToString();
				if (string.IsNullOrEmpty(bodyContent))
				{
					return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeBodyTextIsEmpty);
				}

				var customsFeedbackMessageName = Extensions.GetCustomsFeedbackMessageTagName(bodyContent);
				var feedbackMessageUnpacker = FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker(customsFeedbackMessageName);
				if (feedbackMessageUnpacker == null)
				{
					unsupportedServiceMessageCount++;
					continue;
				}

				var unpackResult = feedbackMessageUnpacker.Unpack(interchange, bodyContent, string.Empty, currentOutgoingMessageCorrelationId, outgoingInterchange, outgoingMessage, logger);
				if (!unpackResult.IsSuccess)
				{
					result = unpackResult;
					continue;
				}
				totalMessagesCreated++;
				correlationIdList.Add(currentOutgoingMessageCorrelationId);
				ediMessageList.AddRange(unpackResult.EdiMessages);
			}

			logger.Log($"ILC a-sync: New EDI Messages created - {totalMessagesCreated} \r\nNumber of discarded messages (Correlation already exist) – {correlationAlreadyExistMessageCount} \r\nNumber of discarded messages (Service not supported) – {unsupportedServiceMessageCount}");

			if (needToSendSyncAcknowledgementMessage && correlationIdList.Count > 0)
			{
				var messageBuilder = new ILGEN920MessageBuilder(interchange.Company, correlationIdList);
				((IMessageBuilder)messageBuilder).PopulateMessages();
				factory.Save();
			}

			if (result != null)
			{
				return result;
			}
			return new EDIInterchangeUnpackerResult(ediMessageList);
		}

		bool DoesCorrelationIdExist(BusinessObjectFactory factory, string currentOutgoingMessageCorrelationId)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ILCustoms);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, currentOutgoingMessageCorrelationId);
			var message = factory.LoadTop1<EDIMessage>(query);
			return message != null;
		}
	}
}
