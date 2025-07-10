using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class G5MessageBuilderManager
{
	public G5MessageBuilderManager(G5TemporaryStorageMessageSendingObject objectToSend, ICertificateProvider certificate)
	{
		sendingObject = Argument.NotNull(objectToSend, nameof(objectToSend));
		header = Argument.NotNull(sendingObject.Header, nameof(sendingObject.Header));
		certificateData = Argument.NotNull(certificate, nameof(certificate));
		messageType = sendingObject.MessageType;
		messageSubType = sendingObject.MessageSubType;
	}

	readonly G5TemporaryStorageMessageSendingObject sendingObject;
	readonly TemporaryStorageHeader header;
	readonly ICertificateProvider certificateData;
	readonly ZString messageType;
	readonly ZString messageSubType;

	public IMessageBuilderBase NewMessageBuilder()
	{
		switch (messageType)
		{
			case DeclarationMessageTypeList.Codes.G5v1Expedition:
				return new ExpeditionG5MessageBuilder(new ExpeditionG5SendMessageWrapper(header, certificateData), messageType, messageSubType);
			case DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation:
				return new ExpCancelG5MessageBuilder(new CancelG5SendMessageWrapper(header, certificateData), messageType, messageSubType);
			case DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment:
				return new ExpAmendmentG5MessageBuilder(new ExpAmendmentG5SendMessageWrapper(header, certificateData), messageType, messageSubType);
			case DeclarationMessageTypeList.Codes.G5v1Reception:
				return new ReceptionG5MessageBuilder(new ReceptionG5SendMessageWrapper(header, certificateData), messageType, messageSubType);
			default:
				throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
		}
	}
}
