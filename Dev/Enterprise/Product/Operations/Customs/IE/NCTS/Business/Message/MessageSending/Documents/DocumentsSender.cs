using System;
using CargoWise.Customs.IE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using MessageSender = Enterprise.Customs.IE.Business.MessageSender;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentsSender : MessageSender
	{
		public DocumentsSender(DocumentSendingAction sendingAction) : base(sendingAction)
		{
		}

		new DocumentSendingAction SendingAction => (DocumentSendingAction)base.SendingAction;

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<NCTSOutboundEDIMessage>();

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage)
		{
			(Type builderType, Type providerType) types = (null, null);
			var messageType = relatingMessage.EM_MessageType;
			switch (messageType)
			{
				case NCTSOutgoingMessageTypeList.Codes.UploadSupportingDocuments:
					types = (typeof(TR083MessageBuilder), typeof(TR083MessageProvider));
					break;
				default:
					break;
			}
			return CreateMessageBuilderWithMappedProvider(types, relatingMessage);
		}

		IXmlMessageBuilder CreateMessageBuilderWithMappedProvider((Type builderType, Type providerType) types, OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder result = null;

			if (types != (null, null))
			{
				if (types.providerType.GetInterface(nameof(IDocumentSendingMapper)) == null || types.providerType.GetConstructor(new[] { typeof(DocumentSendingAction) }) == null)
				{
					throw new DeveloperNotificationException($"Message type: {SendingAction.MessageType}, Provider Type: {types.providerType}: MessageProvider type must implement {typeof(IDocumentSendingMapper)} and have a Constructor with parameter DocumentsSendingAction");
				}

				var provider = Activator.CreateInstance(types.providerType, SendingAction);
				CreateAttachmentsAndMapWithMessageProvider((IDocumentSendingMapper)provider, outgoingMessage);

				result = (IXmlMessageBuilder)Activator.CreateInstance(types.builderType, provider);
			}

			return result;
		}

		void CreateAttachmentsAndMapWithMessageProvider(IDocumentSendingMapper messageProvider, OutboundEDIMessage relatedMessage)
		{
			foreach (AdditionalInfoSendingObject addInfoObject in SendingAction.AddInfoCollection)
			{
				messageProvider.AddAdditionalInfomation(addInfoObject);
			}

			foreach (DocumentSendingObject suppDocObj in SendingAction.SupportingDocuments)
			{
				messageProvider.AddSupportingDocument(suppDocObj);
				CreateAttach(suppDocObj, relatedMessage);
			}
		}

		void CreateAttach(SupportingDocSendingObject sendingObject, OutboundEDIMessage message)
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
