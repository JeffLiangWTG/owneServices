using System.Text;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public abstract class DocumentManagementServiceRequestMessageCreationStrategy<TBusinessObject, TMessageParent> : IOutgoingCustomsMessageCreationStrategy, IDocumentManagementServiceRequestMessageCreationStrategy
	where TBusinessObject : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	protected DocumentManagementServiceRequestMessageCreationStrategy(BusinessObjectFactory factory, IDocumentManagementServiceRequestContext<TBusinessObject, TMessageParent> messageCreationContext)
	{
		Factory = Argument.NotNull(factory, nameof(factory));
		MessageCreationContext = Argument.NotNull(messageCreationContext, nameof(messageCreationContext));
	}

	protected BusinessObjectFactory Factory { get; }
	protected IDocumentManagementServiceRequestContext<TBusinessObject, TMessageParent> MessageCreationContext { get; }

	protected ITEDIMessage CreateEDIMessage(string type, string subType)
	{
		var message = Factory.New<ITEDIMessage>();
		message.EM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade;
		message.EM_MessageType = type;
		message.EM_MessageSubType = subType;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageText = GetSoapEnvelope();
		message.EM_LinkedObject = MessageCreationContext.MessageParent;
		message.EM_ApplicationReference = ZString.Empty;
		message.EM_GP = MessageCreationContext.MauCertificate?.PK ?? ZGuid.Empty;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		return message;
	}

	protected abstract ITEDIMessage AddNewEDIMessage();
	protected abstract CargoWise.Customs.Shared.MessageContracts.IXmlMessageBuilder CreateXmlMessageBuilder(IDocumentManagementServiceRequestContext<TBusinessObject, TMessageParent> context);

	string GetSoapEnvelope()
	{
		var xmlMessage = CreateXmlMessage();
		var signedXmlMessageBytes = SignXmlMessage(MessageCreationContext.CryptokiCertificate, MessageCreationContext.XmlSigner, xmlMessage);
		var soapEnvelope = CreateSoapEnvelope(signedXmlMessageBytes, MessageCreationContext.MauCertificate?.DeclarantTaxNumber ?? ZString.Empty, MessageCreationContext.ServiceId);
		return soapEnvelope;
	}

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		ValidateMessageCreationContext(MessageCreationContext);
		return AddNewEDIMessage();
	}

	string IDocumentManagementServiceRequestMessageCreationStrategy.CreateXmlMessage() => CreateXmlMessage();

	string CreateXmlMessage()
	{
		var messageBuilder = CreateXmlMessageBuilder(MessageCreationContext);
		return messageBuilder.GenerateXmlMessage().GetSerializedString();
	}

	string CreateSoapEnvelope(byte[] signedXmlMessageBytes, ZString declarantTaxNumber, string serviceId)
	{
		var soapMessage = AidaSoapMessageBuilder.Create(AidaSoapMessageNamespaceConstants.ElectronicFolderRequestTypeNamespace)
			.AddServiceId(serviceId)
			.AddDeclarantTaxNumber(declarantTaxNumber)
			.AddNonBase64EncodedXmlBytes(signedXmlMessageBytes)
			.Build();

		return soapMessage;
	}

	static byte[] SignXmlMessage(ICryptokiGlbExternalPassword cryptokiCertificate, IAidaXmlSigner xmlSigner, string xmlMessage)
	{
		var utf8Bytes = Encoding.UTF8.GetBytes(xmlMessage);

		if (GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword())
		{
			return utf8Bytes;
		}

#if DEBUG
		if (GlbStaff.CurrentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return utf8Bytes;
		}
#endif
		var signedBytes = xmlSigner.Sign(utf8Bytes, cryptokiCertificate, ZDateTime.UtcNow.ToDateTime());
		return signedBytes;
	}

	void ValidateMessageCreationContext(IDocumentManagementServiceRequestContext<TBusinessObject, TMessageParent> context)
	{
		Argument.NotNull(context.BusinessObject, nameof(context.BusinessObject));
		Argument.NotNull(context.MessageParent, nameof(context.MessageParent));
		Argument.NotNull(context.XmlSigner, nameof(context.XmlSigner));
#if DEBUG
		if (GlbStaff.CurrentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return;
		}
#endif
		Argument.NotNull(context.CryptokiCertificate, nameof(context.CryptokiCertificate));
	}
}

public abstract class DocumentManagementServiceRequestMessageCreationStrategy<TBusinessObject> : DocumentManagementServiceRequestMessageCreationStrategy<TBusinessObject, TBusinessObject>
	where TBusinessObject : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
{
	protected DocumentManagementServiceRequestMessageCreationStrategy(BusinessObjectFactory factory, IDocumentManagementServiceRequestContext<TBusinessObject, TBusinessObject> messageCreationContext) : base(factory, messageCreationContext)
	{
	}
}
