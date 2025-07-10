using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSAttachmentMessageBuilder : COLSMessageBuilder<COLSAttachmentMessage>
	{
		public COLSAttachmentMessageBuilder(QuarantineColsHeader colsHeader, CusStorageDocPivot docPivot, bool isPartOfOtherMessage, bool isFirstAttachment, bool isLastAttachment)
			: base(colsHeader)
		{
			this.docPivot = Argument.NotNull(docPivot, nameof(docPivot));
			this.isPartOfOtherMessage = isPartOfOtherMessage;
			this.isFirstAttachment = isFirstAttachment;
			this.isLastAttachment = isLastAttachment;
		}
		readonly CusStorageDocPivot docPivot;
		readonly bool isPartOfOtherMessage;
		readonly bool isFirstAttachment;
		readonly bool isLastAttachment;

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.AddAttachment;

		protected override BusinessObject MessageParent => docPivot;

		protected override COLSAttachmentMessage GetMessageData()
		{
			return new COLSAttachmentMessage()
			{
				file = docPivot.FileName,
				docType = docPivot.CSD_DocType,
				docReference = docPivot.CSD_Description,
				lastDoc = isLastAttachment
			};
		}

		protected override void DoAdditionalProcessing(COLSMessage message)
		{
			message.EM_Status = isFirstAttachment && !isPartOfOtherMessage
				? Messaging.Integration.EDIMessageStatusList.Codes.Queued
				: Messaging.Integration.EDIMessageStatusList.Codes.Pending;
			message.EM_MessageSubType = isLastAttachment
				? AUCOLSMessageSubTypeList.Codes.LastdocAttachment
				: AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			message.EM_ApplicationReference = colsHeader.LRN;
			docPivot.CSD_MessageStatus = isLastAttachment
				? COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse
				: COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;

			AddAttachmentToMessage(message);
		}

		void AddAttachmentToMessage(COLSMessage message)
		{
			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = docPivot.FileName;
			attachment.EG_StorageDocsGuid = docPivot.Document?.UniqueKey ?? ZGuid.Empty;
		}
	}
}
