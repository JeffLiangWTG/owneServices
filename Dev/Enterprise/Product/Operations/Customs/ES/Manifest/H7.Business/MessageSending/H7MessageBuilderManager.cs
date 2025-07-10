using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7MessageBuilderManager
	{
		public H7MessageBuilderManager(IH7CommonMessageSendingObject objectToSend, ICertificateProvider certificateProvider)
		{
			sendingObject = Argument.NotNull(objectToSend, nameof(objectToSend));
			this.certificateProvider = Argument.NotNull(certificateProvider, nameof(certificateProvider));
		}

		readonly IH7CommonMessageSendingObject sendingObject;
		readonly ICertificateProvider certificateProvider;
		AsycudaBill bill => sendingObject.Bill;
		ZString messageType => sendingObject.Action;
		string messageSubType => DeclarationMessageSubTypeList.Codes.OriginalDeclaration;

		public IMessageBuilderBase NewMessageBuilder()
		{
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.H7Cancellation:
					return new CancelH7MessageBuilder(new CancelH7SendMessageWrapper(bill, certificateProvider), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.H7Query:
					return new QueryH7MessageBuilder(new QueryH7SendMessageWrapper(bill, certificateProvider), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.H7ReExport:
					return new ReexportH7MessageBuilder(new ReexportH7SendMessageWrapper(bill, certificateProvider, ((H7MessageSendingObject)sendingObject).OperationCode), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.H7Declaration:
					return new DeclarationH7MessageBuilder(new DeclarationH7MessageWrapper(bill, certificateProvider), messageType, messageSubType);
				default:
					throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
			}
		}

		public CommonAnnexMessageBuilder NewCommonAnnexMessageBuilder(IeDoc eDoc, AdditionalInfoSendingObject addInfoSendingObject)
		{
			return new CommonAnnexMessageBuilder(new CommonAnnexH7SendMessageWrapper(bill, certificateProvider, eDoc, addInfoSendingObject, (UploadDocumentsSendingAction)sendingObject), messageType, messageSubType);
		}
	}
}
