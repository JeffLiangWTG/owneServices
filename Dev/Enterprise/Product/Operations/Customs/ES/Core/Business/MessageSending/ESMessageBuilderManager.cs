using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ESMessageBuilderManager
	{
		public ESMessageBuilderManager(JobDeclarationMessageSendingObject objectToSend, ICertificateProvider certificate)
		{
			sendingObject = Argument.NotNull(objectToSend, nameof(objectToSend));

			entryHeader = Argument.NotNull(sendingObject.Header, nameof(sendingObject.Header));
			certificateData = Argument.NotNull(certificate, nameof(certificate));
			messageType = sendingObject.MessageType;
			messageSubType = sendingObject.MessageSubType;
		}

		public ESMessageBuilderManager(ZString messageType, ZString messageSubType, CusEntryHeader cusEntryHeader, ICertificateProvider certificate)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			certificateData = Argument.NotNull(certificate, nameof(certificate));
			this.messageType = messageType;
			this.messageSubType = messageSubType;
		}

		readonly JobDeclarationMessageSendingObject sendingObject;
		readonly CusEntryHeader entryHeader;
		readonly ICertificateProvider certificateData;
		readonly ZString messageType;
		readonly ZString messageSubType;

		public IMessageBuilderBase NewMessageBuilder()
		{
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.ExitSummaryDeclaration:
					return new EXSMessageBuilder(new EXSSendMessageWrapper(entryHeader, certificateData, messageSubType), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Export:
					return new DUAExportMessageBuilder(new DUAExportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportAmendment:
					return NewAmendmentDUAExportMessageBuilder();
				case DeclarationMessageTypeList.Codes.TypeXExport:
					return new ComplXExportMessageBuilder(new ComplXExportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration:
					return new PreDUAIncompleteImportMessageBuilder(new PreDUAIncompleteImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation:
					return new CANPreDUAImportMessageBuilder(new CANPreDUAImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration:
					return new DUACompleteImportMessageBuilder(new DUACompleteImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Box44Documents:
					return new Box44ImportMessageBuilder(new Box44ImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportAmendmentBox40:
					return new Box40AmendmentImportMessageBuilder(new Box40AmendmentImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lExpedition:
					return new ExpeditionMessageBuilder(new ExpeditionSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lExpeditionAmendment:
					return new ExpeditionAmendmentMessageBuilder(new ExpeditionAmendmentSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lReception:
					return new ReceptionMessageBuilder(new ReceptionSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lReceptionAmendment:
					return new ReceptionAmendmentMessageBuilder(new ReceptionAmendmentSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lClearance:
					return new ClearanceMessageBuilder(new ClearanceSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration:
					return new DUASimplifiedImportMessageBuilder(new DUASimplifiedImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.PendingSupportingDocuments:
					return new DJPImportMessageBuilder(new DJPImportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportUcc6:
				case DeclarationMessageTypeList.Codes.ExportPreDeclaration:
					return new DeclarationAESMessageBuilder(new DeclarationAESSendMessageWrapper(entryHeader, certificateData, sendingObject.SecurityFlag, messageType, messageSubType), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportQuery:
					return new QueryAESMessageBuilder(new QueryAESSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportAmendmentUcc6:
					return new AmendmentAESMessageBuilder(new AmendmentAESSendMessageWrapper(entryHeader, certificateData, sendingObject.SecurityFlag, messageSubType), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportNotification:
					return new GoodsNotificationAESMessageBuilder(new GoodsNotificationAESSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ExportCancellation:
					return new CancelAESMessageBuilder(new CancelAESSendMessageWrapper(entryHeader, certificateData, sendingObject.ReasonForCancellation), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.TypeXExportUcc6:
					return new ComplXAESMessageBuilder(new ComplXAESSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.DvdH2:
					return new DeclarationDVDMessageBuilder(new DeclarationDVDSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.DvdH2Cancellation:
					return new CancelDVDMessageBuilder(new CancelDVDSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.TypeXDvdH2:
					return new ComplXDVDMessageBuilder(new ComplXDVDSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lRequestPous:
					return new RequestT2LMessageBuilder(new RequestT2LPOUSSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lPresentationPous:
					return new PresentationT2LMessageBuilder(new PresentationT2LPOUSSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lQueryPous:
					return new QueryT2LMessageBuilder(new QueryT2LPOUSSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.T2lReceptionPous:
					return new ReceptionT2LMessageBuilder(new ReceptionT2LPOUSSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1:
					return new IncompleteImportH1MessageBuilder(new IncompleteImportH1SendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
				default:
					throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
			}
		}

		public IMessageBuilderBase NewT2LAnnexMessageBuilder(CusStorageDocPivot docPivot, ZBool isLast)
		{
			return new AnnexMessageBuilder(new AnnexSendMessageWrapper(entryHeader, certificateData, docPivot.Document, docPivot.CSD_Description, isLast), messageType, messageSubType);
		}

		public IMessageBuilderBase NewAESAnnexMessageBuilder(IEnumerable<CusStorageDocPivot> docPivotList, ZString requestDispatch)
		{
			return new AnnexAESMessageBuilder(new AnnexAESSendMessageWrapper(entryHeader, certificateData, docPivotList, requestDispatch), messageType, messageSubType);
		}

		public IMessageBuilderBase NewCommonAnnexMessageBuilder(CusStorageDocPivot docPivot, ZString requestDispatch)
		{
			return new CommonAnnexMessageBuilder(new CommonAnnexSendMessageWrapper(entryHeader, certificateData, docPivot, requestDispatch), messageType, messageSubType);
		}

		public IMessageBuilderBase NewDVDQueryBuilder()
		{
			return new QueryDVDMessageBuilder(new DVDQuerySendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
		}

		public IMessageBuilderBase NewDVDQueryBuilderCanaryIslands()
		{
			return new QueryDVDMessageBuilder(new DVDQuerySendMessageWrapperCanaryIslands(entryHeader, certificateData), messageType, messageSubType);
		}

		public IMessageBuilderBase NewImportQueryBuilder()
		{
			return new DUAImportQueryMessageBuilder(new ImportQuerySendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
		}

		public IMessageBuilderBase NewImportQueryBuilderCanaryIslands()
		{
			return new DUAImportQueryMessageBuilder(new ImportQuerySendMessageWrapperCanaryIslands(entryHeader, certificateData), messageType, messageSubType);
		}

		public IMessageBuilderBase NewQueryImportH1Builder(bool isCanaryIslands = false)
		{
			return new QueryImportH1MessageBuilder(new QueryImportH1SendMessageWrapper(entryHeader, certificateData, isCanaryIslands), messageType, messageSubType);
		}

		IMessageBuilderBase NewAmendmentDUAExportMessageBuilder()
		{
			var entrySubStyle = entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			if (entrySubStyle == EntrySubStyleList.Codes.B && entryHeader.CH_EntryStatus == EntryStatusCodes.Cleared)
			{
				return new DUAExportMessageBuilder(new AmendmentComplXExportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
			}
			else
			{
				return new DUAExportMessageBuilder(new AmendmentDUAExportSendMessageWrapper(entryHeader, certificateData), messageType, messageSubType);
			}
		}
	}
}
