using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

public sealed class UniqueTransactionIdentifierRequestMessageCreationStrategy : IOutgoingCustomsMessageCreationStrategy
{
	public UniqueTransactionIdentifierRequestMessageCreationStrategy(BusinessObjectFactory factory, IUniqueTransactionIdentifierRequestContext messageCreationContext)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.messageCreationContext = Argument.NotNull(messageCreationContext, nameof(messageCreationContext));
	}

	readonly BusinessObjectFactory factory;
	readonly IUniqueTransactionIdentifierRequestContext messageCreationContext;

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		var soapEnvelope = UniqueTransactionIdentifierAidaSoapMessageBuilder.Create()
			.AddUniqueTransactionIdentifier(messageCreationContext.UniqueTransactionID)
			.Build();

		var requestMessage = factory.New<ITEDIMessage>();
		requestMessage.EM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade;
		requestMessage.EM_MessageType = requestMessage.EM_ApplicationReference = EDIMessageTypeList.Codes.UniqueTransactionId;
		requestMessage.EM_MessageSubType = "XXX";
		requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		requestMessage.EM_Status = EDIMessage.Status.Queued;
		requestMessage.EM_MessageText = soapEnvelope;
		requestMessage.EM_LinkedObject = messageCreationContext.RequestParent;
		requestMessage.EM_GP = messageCreationContext.MauCertificate?.PK ?? ZGuid.Empty;
		requestMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageCreationContext.MessageNumber);
		return requestMessage;
	}
}
