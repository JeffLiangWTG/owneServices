using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.CDS.Messaging.MessageManagers;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class MessageSender : Integration.Customs.GB.GBH7.IH7MessageSender
	{
		public MessageSender(IEnumerable<MessageSendingObject> sendingObjects)
		{
			this.sendingObjects = sendingObjects;
		}

		public MessageSender(IEnumerable<MessageSendingObject> sendingObjects, Action<int, int> updateProgressCallback)
			: this(sendingObjects)
		{
			this.updateProgressCallback = updateProgressCallback;
		}

		readonly IEnumerable<MessageSendingObject> sendingObjects;
		readonly Action<int, int> updateProgressCallback;

		public int Send()
		{
			var messagesSent = 0;
			var messagesToSend = sendingObjects.Count();
			var errorCollector = new ErrorCollector();

			foreach (var sendingObject in sendingObjects)
			{
				var messageType = GetEDIMessageType(sendingObject);
				var message = sendingObject.Bill.Messages.AddNew(messageType.EdiMessageBizOType);
				if (sendingObject.Action == H7EDIMessageTypeList.Codes.QueryDeclaration)
				{
					message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
					notificationCollection ??= new MessageSendingNotificationCollection();
					if (SendQueryMessage(sendingObject, message))
					{
						sendingObject.Bill.Messages.Reload(false);
						sendingObject.MessageCreated(message.EM_MessageText);
						messagesSent++;
						updateProgressCallback?.Invoke(messagesSent, messagesToSend);
					}
				}
				else
				{
					var messageBuilder = GetMessageBuilder(messageType.NewAmendDelete, sendingObject, errorCollector);
					message.EM_MessageText = messageBuilder.Build();
					sendingObject.Bill.ABL_MessageStatus = CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(messageType.EdiMessageBizOType, messageType.NewAmendDelete);
					sendingObject.MessageCreated(message.EM_MessageText);
					messagesSent++;
					updateProgressCallback?.Invoke(messagesSent, messagesToSend);
				}
			}

			return messagesSent;
		}

		MessageSendingNotificationCollection notificationCollection;

		bool SendQueryMessage(MessageSendingObject sendingObject, Enterprise.Messaging.Business.EDIMessage message)
		{
			var querySendingObject = GetQuerySendingObject(sendingObject);
			var universalEvent = new CDSQueryUniversalEventBuilder(querySendingObject).BuildUniversalEvent(Guid.Empty);
			return CDSQuerySendingHelper.Deliver(sendingObject.Bill.Factory, universalEvent, sendingObject.Bill, EHubID, notificationCollection, message);
		}

		BaseCDSQuerySendingObject GetQuerySendingObject(MessageSendingObject sendingObject)
		{
			var bill = (AsycudaBill)sendingObject.Bill;
			return (string)sendingObject.QueryType switch
			{
				H7QueryTypeList.Codes.MRNSummary => new CDSH7QueryMRNSendingObject(bill, H7QueryTypeList.Codes.MRNSummary),
				H7QueryTypeList.Codes.MRNSnapshot => new CDSH7QueryMRNSendingObject(bill, H7QueryTypeList.Codes.MRNSnapshot),
				H7QueryTypeList.Codes.DUCR => new CDSH7QueryDUCRSendingObject(bill),
				H7QueryTypeList.Codes.UCR => new CDSH7QueryUCRSendingObject(bill),
				_ => null,
			};
		}

		(Type EdiMessageBizOType, CusdecMessageFunction NewAmendDelete) GetEDIMessageType(MessageSendingObject sendingObject)
		{
			(Type EdiMessageBizOType, CusdecMessageFunction NewAmendDelete) result = (null, null);

			switch (sendingObject.Action)
			{
				case H7EDIMessageTypeList.Codes.NewDeclaration:
					result = (typeof(CDSNewDeclarationEDIMessage), new CusdecMessageFunction.New());
					break;
				case H7EDIMessageTypeList.Codes.ArrivalNotification:
					result = (typeof(CDSArrivalAmendmentDeclarationEDIMessage), new CusdecMessageFunction.Amended());
					break;
				case H7EDIMessageTypeList.Codes.CancelDeclaration:
					result = (typeof(CDSArrivalAmendmentDeclarationEDIMessage), new CusdecMessageFunction.Deleted());
					break;
				case H7EDIMessageTypeList.Codes.QueryDeclaration:
					result = (typeof(XmlEDIMessage), null);
					break;
			}

			return result;
		}

		IGbCDSMessageBuilder GetMessageBuilder(CusdecMessageFunction newAmendDelete, MessageSendingObject sendingObject, ErrorCollector errorCollector)
		{
			var functionCode = newAmendDelete.GetCdsFunctionCode();

			if (newAmendDelete is CusdecMessageFunction.New)
			{
				if (sendingObject.Bill.Header.PortOfDischarge?.IsInNorthernIreland == true)
				{
					return new H7MessageBuilder(sendingObject, errorCollector, functionCode);
				}
				else
				{
					return new C21BMessageBuilder(sendingObject, errorCollector, functionCode);
				}
			}
			else if (newAmendDelete is CusdecMessageFunction.Amended)
			{
				return new H7ArrivalMessageBuilder(sendingObject, functionCode);
			}
			else if (newAmendDelete is CusdecMessageFunction.Deleted)
			{
				return new H7CancelMessageBuilder(sendingObject, functionCode);
			}

			return null;
		}

		const string EHubID = "GBCustoms";
	}
}
