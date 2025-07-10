using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class G3CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ManifestCommonResponseMessageProcessor<AsycudaManifestHeader, TResponseProvider>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode, IG3Lrn, IG3CommonErrors
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected G3CommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		readonly string acceptedResponseCode = (NoResString)"Ac";
		const string g3EntryLineReference = "G3";
		const string h7EntryLineReference = "H7";

		readonly List<string> syntaxErrorCodes = ["900", "901", "902", "911", "912", "914", "999"];

		protected abstract ZString XsdSchemaEmbeddedResourceName { get; }

		protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response, EDIMessage message, AsycudaManifestHeader header);

		protected override TResponseProvider GetMessageProviderCore(EDIMessage message)
		{
			return DeserializeMessage(message, XsdSchemaEmbeddedResourceName);
		}

		protected TResponseProvider DeserializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<TResponseProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: false);
			}
		}

		protected override void ProcessMessageCore(EDIMessage message, AsycudaManifestHeader linkedBusinessObject, TResponseProvider provider)
		{
			message.EM_MessageNum = ((ZString)provider.ServiceSegmentId).Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);
			var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider, message, linkedBusinessObject);

			if (provider.ResponseCode == acceptedResponseCode)
			{
				ProcessAcceptedResponse(provider, message, linkedBusinessObject);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted(string.Empty);
				SetMessageSubTypeAsAccepted(message);
			}
			else
			{
				ProcessRejectedResponse(provider, message, linkedBusinessObject);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
				SetMessageSubTypeAsRejected(message);
			}
			SetMessageStatusAsReceived(message);
		}

		protected virtual void ProcessAcceptedResponse(TResponseProvider response, EDIMessage message, AsycudaManifestHeader header)
		{
			var matchedG3Bills = GetBillsByG3Lrn(message.Factory, response.Lrn, false);
			foreach (var bill in matchedG3Bills)
			{
				bill.ABL_MessageStatus = LogicalStatusList.Codes.Accepted;
			}

			SendH7QueryMessageIfBillH7MrnNotEmpty(response.Lrn, message, header);
		}

		protected void ProcessRejectedResponse(TResponseProvider response, EDIMessage message, AsycudaManifestHeader header)
		{
			var matchedG3Bills = GetBillsByG3Lrn(message.Factory, response.Lrn, false);
			var messageStatus = response.Errors.Any(x => syntaxErrorCodes.Contains(x.Code)) 
				? LogicalStatusList.Codes.Error 
				: LogicalStatusList.Codes.Invalid;

			foreach (var bill in matchedG3Bills)
			{
				bill.ABL_MessageStatus = messageStatus;
			}
		}

		void SendH7QueryMessageIfBillH7MrnNotEmpty(string lrn, EDIMessage message, AsycudaManifestHeader header)
		{
			var broker = header.CustomsAgent;
			var billsWithH7Mrn = GetBillsByG3Lrn(message.Factory, lrn, true);
			TriggerMessageSending(message, broker.GS_Code, broker, billsWithH7Mrn, SendH7QueryMessage, LogicalStatusList.Codes.Sent);
		}

		List<ESEDIMessage> SendH7QueryMessage(AsycudaBill bill, CertificateObject certificateObject)
		{
			var messages = new List<ESEDIMessage>();
				var messageSendingObject = new H7MessageSendingObject(bill) { Action = DeclarationMessageTypeList.Codes.H7Query };
			var messageSender = new H7MessageSender(new List<H7MessageSendingObject> { messageSendingObject });
			var messageBuilders = messageSender.GetMessageBuildersData();

			if (!messageBuilders.IsNullOrEmpty())
			{
				messageSender.Send(messageBuilders, messages);
				var h7QMessage = messages.FirstOrDefault();

				if (h7QMessage != null)
				{
					h7QMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddSeconds(150);
				}
			}

			return messages;
		}

		protected ZBool TriggerMessageSending(EDIMessage message, ZString sentBrokerCode, GlbStaff broker, IEnumerable<AsycudaBill> bills, Func<AsycudaBill, CertificateObject, List<ESEDIMessage>> sendDeclaration, string messageStatus)
		{
			var sentMessagesCorrectly = false;

			var certificateObject = GetCertificateObject(message, sentBrokerCode, broker);

			if (certificateObject != null)
			{
				sentMessagesCorrectly = SendAndProcessMessages(bills, sendDeclaration, certificateObject, messageStatus);
			}

			return sentMessagesCorrectly;
		}

		protected bool SendAndProcessMessages(IEnumerable<AsycudaBill> bills, Func<AsycudaBill, CertificateObject, List<ESEDIMessage>> sendDeclaration, CertificateObject certificateObject, string messageStatus)
		{
			try
			{
				foreach (var bill in bills)
				{
					var messages = sendDeclaration(bill, certificateObject);
					if (messages.Count > 0)
					{
						((IESResponseBOMessageStatus)bill).MessageStatus = messageStatus;
					}
				}

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.LogWarning(Res.GetString("9660E33F-0797-4707-B430-40DD7F2CF371", "Error when creating G3 messages"));
			}

			return false;
		}

		protected AsycudaBill[] GetBillsByG3Lrn(BusinessObjectFactory factory, string g3Lrn, bool andHasH7Mrn)
		{
			var query = new ZDBOnlyQuery(typeof(AsycudaBill));

			var cusEntrySubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.LocalReferenceNumber);
			cusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, g3Lrn);
			cusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, g3EntryLineReference);
			query.AddSubQuery(cusEntrySubQuery, JoinCondition.And);

			if (andHasH7Mrn)
			{
				var h7CusEntrySubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				h7CusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				h7CusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, h7EntryLineReference);
				h7CusEntrySubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.AddSubQuery(h7CusEntrySubQuery, JoinCondition.And);
			}

			return factory.Load<AsycudaBill>(query);
		}
	}
}
