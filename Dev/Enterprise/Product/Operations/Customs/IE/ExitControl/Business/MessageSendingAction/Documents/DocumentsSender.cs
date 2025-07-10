using System;
using CargoWise.Customs.IE.MessageContracts.AES;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.ExitControl.Business.AES;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSender : ExitControlMessageSender
	{
		public DocumentsSender(DocumentsSendingAction sendingAction) : base(sendingAction)
		{
		}

		new DocumentsSendingAction SendingAction => (DocumentsSendingAction)base.SendingAction;

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage)
		{
			(Type builderType, Type providerType) types = (null, null);

			switch (relatingMessage.EM_MessageType)
			{
				case AESOutgoingMessageTypeList.Codes.DocumentUpload:
					types = (typeof(EX583MessageBuilder), typeof(EX583MessageProvider));
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
				if (types.providerType.GetInterface(nameof(IDocumentSendingMapper)) == null || types.providerType.GetConstructor(new[] { typeof(DocumentsSendingAction) }) == null)
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
