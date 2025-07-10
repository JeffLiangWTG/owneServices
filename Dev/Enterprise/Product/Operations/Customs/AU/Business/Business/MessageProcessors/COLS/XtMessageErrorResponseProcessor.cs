using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class XtMessageErrorResponseProcessor : COLSMessageProcessor<BusinessObject, ZString>
	{
		public XtMessageErrorResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.XtMessageError, AUCOLSMessageTypeList.Descriptions.XtMessageError)
		{
		}

		protected override ZString DeserializeObjectCore(EDIMessage message)
		{
			return message.EM_MessageText;
		}

		protected override string ProcessCore(BusinessObject linkedObject, ZString messageData, EDIMessage message, EDIMessage originalMessage)
		{
			bool processed = false;

			var attachment = linkedObject as CusStorageDocPivot;
			var colsHeader = linkedObject as QuarantineColsHeader ?? attachment?.Parent as QuarantineColsHeader;
			if (colsHeader != null)
			{
				var messageType = originalMessage.EM_MessageType;
				switch (messageType)
				{
					case AUCOLSMessageTypeList.Codes.AddAttachment:
						if (UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, COLSHeaderStatusList.Codes.FailedAddAttachment, messageType))
						{
							processed = ProcessAddAttachment(colsHeader, attachment);
						}
						break;

					case AUCOLSMessageTypeList.Codes.AddNewLodgement:
						if (UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse, COLSHeaderStatusList.Codes.FailedAddLodgement, messageType))
						{
							colsHeader.DiscardPendingAttachmentMessages();
							processed = true;
						}
						break;

					case AUCOLSMessageTypeList.Codes.AddAdditionalDocument:
						if (UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingAddAdditionalDocumentResponse, COLSHeaderStatusList.Codes.FailedAddDocument, messageType))
						{
							colsHeader.DiscardPendingAttachmentMessages();
							processed = true;
						}
						break;

					case AUCOLSMessageTypeList.Codes.MakeAnEnquiry:
						processed = ProcessMakeAnEnquiry(colsHeader);
						break;

					case AUCOLSMessageTypeList.Codes.RequestAReassessment:
						processed = ProcessRequestAReassessment(colsHeader);
						break;

					case AUCOLSMessageTypeList.Codes.LodgementStatus:
						processed = UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingGetLodgementStatusResponse, COLSHeaderStatusList.Codes.FailedGetLodgementStatus, messageType);
						break;

					case AUCOLSMessageTypeList.Codes.PaymentStatus:
						processed = UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingGetPaymentResponse, COLSHeaderStatusList.Codes.FailedGetPaymentStatus, messageType);
						break;

					case AUCOLSMessageTypeList.Codes.SwitchAepLodgement:
						processed = UpdateHeaderMessageStatus(colsHeader, COLSHeaderStatusList.Codes.AwaitingSwitchAepRequest, COLSHeaderStatusList.Codes.FailedSwitchAepRequest, messageType);
						break;

					default:
						Logger.LogError($"Unknown outgoing message type: {messageType}");
						break;
				}

				if (processed)
				{
					message.EM_MessageSubType = originalMessage.EM_MessageSubType;
				}
			}
			else
			{
				Logger.LogError($"Unexpected Linked Object: {linkedObject?.GetType().FullName ?? "NULL"}");
			}

			return processed ? EDIMessageStatusList.Codes.Received : EDIMessageStatusList.Codes.Failed;
		}

		bool UpdateHeaderMessageStatus(QuarantineColsHeader colsHeader, string expectedStatus, string newStatus, string messageType)
		{
			if (colsHeader.QCH_MessageStatus == expectedStatus)
			{
				colsHeader.QCH_MessageStatus = newStatus;
				return true;
			}
			else
			{
				Logger.LogError($"COLS status {colsHeader.QCH_MessageStatus} is not the expected status {expectedStatus} for Message Type {messageType}. ");
				return false;
			}
		}

		bool ProcessAddAttachment(QuarantineColsHeader colsHeader, CusStorageDocPivot attachment)
		{
			bool result = true;

			switch (attachment.CSD_MessageStatus)
			{
				case COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse:
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedDocumentSentResponse;
					break;
				case COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse:
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedLastdocSentResponse;
					break;
				default:
					Logger.LogError($"Unknown Attachment Message Status: {attachment.CSD_MessageStatus}");
					result = false;
					break;
			}

			QueueNextAttachmentMessage(colsHeader);
			return result;
		}

		bool ProcessMakeAnEnquiry(QuarantineColsHeader colsHeader)
		{
			switch (colsHeader.QCH_MessageStatus)
			{
				case COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse:
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithoutDocs;
					break;
				case COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse:
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithDocs;
					colsHeader.DiscardPendingAttachmentMessages();
					break;
				default:
					Logger.LogError($"Unknown Header Message Status: {colsHeader.QCH_MessageStatus}");
					return false;
			}

			return true;
		}

		bool ProcessRequestAReassessment(QuarantineColsHeader colsHeader)
		{
			switch (colsHeader.QCH_MessageStatus)
			{
				case COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse:
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedReassessmentWithoutDocs;
					break;
				case COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse:
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedReassessmentWithDocs;
					colsHeader.DiscardPendingAttachmentMessages();
					break;
				default:
					Logger.LogError($"Unknown Header Message Status: {colsHeader.QCH_MessageStatus}");
					return false;
			}

			return true;
		}
	}
}
