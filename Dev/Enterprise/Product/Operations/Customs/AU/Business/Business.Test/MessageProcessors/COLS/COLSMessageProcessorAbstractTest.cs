using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class COLSMessageProcessorAbstractTest : TestCaseWithFactory
	{
		protected abstract COLSMessageProcessor GetMessageProcessor();

		protected EDIMessage CreateColsMessage(BusinessObject messageParent)
		{
			var colsMessage = messageParent.Factory.New<COLSMessage>();
			colsMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			colsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			colsMessage.EM_Status = EDIMessage.Status.Queued;
			colsMessage.EM_LinkedObject = messageParent;

			return colsMessage;
		}

		protected EDIMessage CreateColsAttachmentMessage(CusStorageDocPivot docPivot)
		{
			var attachmentMessage = CreateColsMessage(docPivot);
			attachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			attachmentMessage.EM_Status = EDIMessage.Status.Pending;

			var attachment = attachmentMessage.MessageAttachments.AddNew();
			attachment.EG_FileName = docPivot.FileName;
			attachment.EG_StorageDocsGuid = docPivot.Document?.UniqueKey ?? ZGuid.Empty;

			return attachmentMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			processor = GetMessageProcessor();
		}

		protected JobDeclaration declaration;
		protected COLSMessageProcessor processor;
		protected QuarantineColsHeader colsHeader;
	}
}
