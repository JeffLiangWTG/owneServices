using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business
{
	public class UploadDocumentsSender : MessageSender
	{
		public UploadDocumentsSender(UploadDocumentsSendingAction sendingAction) : base(sendingAction)
		{
		}

		new UploadDocumentsSendingAction SendingAction => (UploadDocumentsSendingAction)base.SendingAction;

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => SendingAction.IsUCC5 ? factory.New<AISUCC5OutboundEDIMessage>() : factory.New<AISOutboundEDIMessage>();

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder builder = null;

			if (SendingAction.IsUCC5)
			{
				switch (outgoingMessage.EM_MessageType)
				{
					case AISUploadDocumentsMessageTypeList.Codes.IM483:
						var im483HeaderProvider = new AIS.UCC5.IM483HeaderProvider(SendingAction);
						CreateAttachmentsAndMapWithMessageProvider(im483HeaderProvider, outgoingMessage);
						builder = new CargoWise.Customs.IE.MessageContracts.AIS.UCC5.IM483MessageBuilder(im483HeaderProvider);
						break;
				}
			}
			else
			{
				switch (outgoingMessage.EM_MessageType)
				{
					case AISUploadDocumentsMessageTypeList.Codes.IM483:
						var im483HeaderProvider = new IM483HeaderProvider(SendingAction);
						CreateAttachmentsAndMapWithMessageProvider(im483HeaderProvider, outgoingMessage);
						builder = new IM483MessageBuilder(im483HeaderProvider);
						break;
					case AISUploadDocumentsMessageTypeList.Codes.IM446:
						var im446MessageProvider = new IM446MessageProvider(SendingAction);
						CreateAttachmentsAndMapWithMessageProvider(im446MessageProvider, outgoingMessage);
						builder = new IM446MessageBuilder(im446MessageProvider);
						break;
				}
			}

			return builder;
		}

		void CreateAttachmentsAndMapWithMessageProvider(IUploadingDocumentMapper messageProvider, OutboundEDIMessage relatedMessage)
		{
			foreach (AdditionalInfoSendingObject addInfoObject in SendingAction.AddInfoCollection)
			{
				var list = new List<IeDoc>();
				foreach (DocumentSendingObject suppDocObj in addInfoObject.EDocsCollection)
				{
					CreateAttach(suppDocObj, relatedMessage);
					if (suppDocObj.Document is IeDoc document)
					{
						list.Add(document);
					}
				}
				messageProvider.AddData(addInfoObject, list);
			}
		}

		void CreateAttachmentsAndMapWithMessageProvider(IDocumentSendingMapper messageProvider, OutboundEDIMessage relatedMessage)
		{
			foreach (AdditionalInfoSendingObject addInfoObject in SendingAction.AddInfoCollection)
			{
				messageProvider.AddAdditionalInfomation(addInfoObject);

				foreach (DocumentSendingObject suppDocObj in addInfoObject.EDocsCollection)
				{
					messageProvider.AddSupportingDocument(suppDocObj);
					CreateAttach(suppDocObj, relatedMessage);
				}
			}
		}

		void CreateAttach(DocumentSendingObject sendingObject, OutboundEDIMessage message)
		{
			if (sendingObject.Document is IeDoc ieDoc)
			{
				var attachment = message.MessageAttachments.AddNew();
				attachment.EG_StorageDocsGuid = ieDoc.UniqueKey;
				attachment.EG_FileName = ieDoc.FileName;
			}
		}
	}
}
