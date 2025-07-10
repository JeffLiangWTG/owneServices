using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class H7CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ManifestCommonResponseMessageProcessor<AsycudaBill, TResponseProvider>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected H7CommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected virtual string AcceptedResponseCode => "A";
		const string SyntaxError = "N";
		const string H7CusEntryNumLineReference = "H7";
		const string RiskAnalysisResultCodeL = "L";

		protected abstract ZString XsdSchemaEmbeddedResourceName { get; }

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

		protected override AsycudaBill FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			return sentBusinessObjects.FirstOrDefault() as AsycudaBill;
		}

		protected override void ProcessMessageCore(EDIMessage message, AsycudaBill linkedBusinessObject, TResponseProvider provider)
		{
			message.EM_MessageNum = ((ZString)provider.ServiceSegmentId).Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);
			var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider, message, linkedBusinessObject);

			if (provider.ResponseCode == AcceptedResponseCode)
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

		protected virtual void ProcessAcceptedResponse(TResponseProvider response, EDIMessage message, AsycudaBill bill)
		{
			bill.ABL_MessageStatus = LogicalStatusList.Codes.Accepted;
		}

		protected virtual void ProcessRejectedResponse(TResponseProvider response, EDIMessage message, AsycudaBill bill)
		{
			if (response is IH7CommonErrors h7Response)
			{
				if (h7Response.Errors.Any(x => x.Type == SyntaxError))
				{
					bill.ABL_MessageStatus = LogicalStatusList.Codes.Error;
				}
				else
				{
					bill.ABL_MessageStatus = LogicalStatusList.Codes.Invalid;
				}
			}
		}

		protected void SendH7QueryMessageIfMrnNotEmpty(string mrn, EDIMessage message, AsycudaBill bill)
		{
			if (!string.IsNullOrEmpty(mrn))
			{
				var broker = bill.Header.CustomsAgent;
				TriggerMessageSendingCommon(message, broker.GS_Code, broker, true, bill, SendH7QueryMessage, LogicalStatusList.Codes.Sent);
			}
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

		protected void SetMovementReferenceNumber(IH7DeclarationInfo declarationInfo, AsycudaBill bill)
		{
			if (!string.IsNullOrEmpty(declarationInfo.Mrn))
			{
				var entryStatus = GetEntryStatusFromRiskAnalysisResultCode(declarationInfo.RiskAnalysisResultCode);
				ZDateTime.TryParseExact(declarationInfo.PresentationDate + declarationInfo.PresentationTime, out var issueDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
				CreateOrUpdateCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, declarationInfo.Mrn, H7CusEntryNumLineReference, entryStatus, issueDate);
			}
		}

		ZString GetEntryStatusFromRiskAnalysisResultCode(string code) => code switch
		{
			RiskAnalysisResultCodeL => CircuitCodeList.Codes.YELLOW,
			_ => GetCircuitCodeFromText(code)
		};

		protected void SetReleaseDate(IH7DeclarationInfo declarationInfo, AsycudaBill bill)
		{
			if (!string.IsNullOrEmpty(declarationInfo.ReleaseDate))
			{
				ZDateTime.TryParseExact(declarationInfo.ReleaseDate, out var releaseDate, CustomsDateTimeExtension.DateFormat);
				bill.ABL_ReleaseDate = releaseDate.Date;
			}
		}

		protected void SetDocumentationRequired(IH7DeclarationInfo declarationInfo, AsycudaBill bill)
		{
			if (!string.IsNullOrEmpty(declarationInfo.DocumentationRequired))
			{
				bill.DocumentationRequired = declarationInfo.DocumentationRequired;
			}
		}

		protected void SetClearanceCSV(IH7DeclarationInfo declarationInfo, AsycudaBill bill, EDIMessage message)
		{
			if (!string.IsNullOrEmpty(declarationInfo.ReleaseCsvId))
			{
				ZDateTime.TryParseExact(declarationInfo.ReleaseDate + declarationInfo.ReleaseTime, out var issueDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
				CreateOrUpdateCusEntryNumber(bill, CusEntryNumberTypes.Spain.ClearanceCSV, declarationInfo.ReleaseCsvId, H7CusEntryNumLineReference, ZString.Empty, issueDate);
				TriggerMisingDocumentRequest(bill, message);
			}
		}

		protected sealed override CommonDocumentRequest<AsycudaBill> GetNewDocumentRequest(AsycudaBill businessObject, ZString certName, EDIMessage message)
		{
			return new H7DocumentRequest(businessObject, certName);
		}

		protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response, EDIMessage message, AsycudaBill bill);
	}
}
