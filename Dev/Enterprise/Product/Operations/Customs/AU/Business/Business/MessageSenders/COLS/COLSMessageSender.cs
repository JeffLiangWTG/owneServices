using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSMessageSender
	{
		public COLSMessageSender(QuarantineColsHeader colsHeader, string additionalComment)
		{
			this.colsHeader = Argument.NotNull(colsHeader, "QuarantineColsHeader");
			this.additionalComment = additionalComment;
		}
		readonly QuarantineColsHeader colsHeader;
		readonly string additionalComment;
		List<EDIMessage> sendingMessages;
		List<ZPropertyInfo> changedFields;

		public string SendLodgementMessage(CusStorageDocPivot[] docsToBeSent)
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateLodgementMessage();
			GenerateAttachmentMessages(docsToBeSent, isPartOfOtherMessage: true);
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendAdditionalDocumentMessage(CusStorageDocPivot[] docsToBeSent)
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateAdditionalDocumentMessage();
			GenerateAttachmentMessages(docsToBeSent, isPartOfOtherMessage: true);
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAdditionalDocumentResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendLodgementStatusMessage()
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateLodgementStatusMessage();
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingGetLodgementStatusResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendPaymentStatusMessage()
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GeneratePaymentStatusMessage();
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingGetPaymentResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendAttachmentMessages(CusStorageDocPivot[] docsToBeSent)
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			var numberOfMessagesCreated = GenerateAttachmentMessages(docsToBeSent, isPartOfOtherMessage: false);
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return $"{numberOfMessagesCreated} attachment message(s) generated";
		}

		public string SendSwitchAEPLodgementMessage()
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateSwitchAEPLodgementMessage();
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingSwitchAepRequest;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendEnquiryMessage(COLSEnquiryAdditionalInformation additionalInformation, CusStorageDocPivot[] docsToBeSent)
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateEnquiryMessage(additionalInformation);
			var documentationRequired = additionalInformation.DocumentRequired;
			if (documentationRequired)
			{
				GenerateAttachmentMessages(docsToBeSent, isPartOfOtherMessage: true);
			}
			colsHeader.QCH_MessageStatus = documentationRequired ? COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse : COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public string SendReassessmentMessage(CusStorageDocPivot[] docsToBeSent, ZString reassessmentReason, ZBool documentationRequired)
		{
			sendingMessages = new List<EDIMessage>();
			changedFields = new List<ZPropertyInfo>();
			GenerateReassessmentMessage(reassessmentReason, documentationRequired);
			if (documentationRequired)
			{
				GenerateAttachmentMessages(docsToBeSent, isPartOfOtherMessage: true);
			}
			colsHeader.QCH_MessageStatus = documentationRequired ? COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse : COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
			changedFields.Add(colsHeader.QCH_MessageStatusInfo);
			return string.Empty;
		}

		public void RollBack()
		{
			sendingMessages?.ForEach(x => x.Delete());
			changedFields?.ForEach(x => x.Value = x.OriginalValue);
		}

		void GenerateLodgementMessage()
		{
			var message = new COLSLodgementMessageBuilder(colsHeader, additionalComment).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			sendingMessages.Add(message);
			colsHeader.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.StatusChange, reference: "|MST=COLS|NEW=SNT", eventTime: ZDateTimeOffset.Now, isEstimate: false));
		}

		void GenerateAdditionalDocumentMessage()
		{
			var message = new COLSAdditionalDocumentMessageBuilder(colsHeader, additionalComment).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = colsHeader.LRN;
			sendingMessages.Add(message);
		}

		void GeneratePaymentStatusMessage()
		{
			var message = new COLSPaymentStatusMessageBuilder(colsHeader).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = additionalComment;
			sendingMessages.Add(message);
		}

		void GenerateSwitchAEPLodgementMessage()
		{
			var message = new COLSSwitchAepLodgementMessageBuilder(colsHeader).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = colsHeader.LRN;
			sendingMessages.Add(message);
		}

		void GenerateReassessmentMessage(ZString reassessmentReason, ZBool documentationRequired)
		{
			var message = new COLSReassessmentMessageBuilder(colsHeader, additionalComment, reassessmentReason, documentationRequired).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			sendingMessages.Add(message);
		}

		int GenerateAttachmentMessages(CusStorageDocPivot[] docsToBeSent, bool isPartOfOtherMessage)
		{
			var docIndex = 1;
			var lastDocIndex = docsToBeSent.Length;
			foreach (var docPivot in docsToBeSent)
			{
				var message = new COLSAttachmentMessageBuilder(colsHeader
					, docPivot
					, isPartOfOtherMessage
					, isFirstAttachment: docIndex == 1
					, isLastAttachment: docIndex == lastDocIndex).CreateNewMessage();
				sendingMessages.Add(message);
				docIndex++;
			}
			return docsToBeSent.Length;
		}

		void GenerateLodgementStatusMessage()
		{
			var message = new COLSLodgementStatusMessageBuilder(colsHeader).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = colsHeader.LRN;
			sendingMessages.Add(message);
		}

		void GenerateEnquiryMessage(COLSEnquiryAdditionalInformation additionalInformation)
		{
			var message = new COLSEnquiryMessageBuilder(colsHeader, additionalInformation, additionalComment).CreateNewMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			sendingMessages.Add(message);
		}

		public MessageSendingValidation Validation => MessageSendingValidation.New(colsHeader, GetNewMessageErrorCollector(), Env.Security.CustomsDeclarationSendWithMessageErrors);

		IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new CustomsNotificationCollector(colsHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
		}
	}
}
