using System.Text;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class IvistoRequestMessageCreationStrategy : IOutgoingCustomsMessageCreationStrategy
{
	public IvistoRequestMessageCreationStrategy(BusinessObjectFactory factory, IIvistoRequestContext messageCreationContext)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.messageCreationContext = Argument.NotNull(messageCreationContext, nameof(messageCreationContext));
	}

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		ValidateMessageCreationContext(messageCreationContext);

		var mauCertificate = messageCreationContext.MauCertificate;
		var xmlMessageBytes = Encoding.UTF8.GetBytes(CreateXmlMessage(messageCreationContext.EntryHeader));
		var soapEnvelope = CreateSoapEnvelope(xmlMessageBytes, mauCertificate?.DeclarantTaxNumber ?? ZString.Empty);
		return AddNewEDIMessage(soapEnvelope, messageCreationContext.EntryHeader, mauCertificate);
	}

	ITEDIMessage AddNewEDIMessage(string soapEnvelope, CusEntryHeader entryHeader, IGlbExternalPassword mauCertificate)
	{
		var statusRequestMessage = factory.New<ITEDIMessage>();
		statusRequestMessage.EM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade;
		statusRequestMessage.EM_MessageType = EDIMessageTypeList.Codes.IvistoRequest;
		statusRequestMessage.EM_MessageSubType = "XXX";
		statusRequestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		statusRequestMessage.EM_Status = EDIMessage.Status.Queued;
		statusRequestMessage.EM_MessageText = soapEnvelope;
		statusRequestMessage.EM_LinkedObject = entryHeader;
		statusRequestMessage.EM_ApplicationReference = ZString.Empty;
		statusRequestMessage.EM_GP = mauCertificate?.PK ?? ZGuid.Empty;
		statusRequestMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		return statusRequestMessage;
	}

	string CreateSoapEnvelope(byte[] xmlMessageBytes, ZString declarantTaxNumber)
	{
		return AidaSoapMessageBuilder.Create(AidaSoapMessageNamespaceConstants.ExportServiceTypeNamespace)
			.AddServiceId(Ucc6XmlConstants.CustomsClearRequests.ServiceId.Ivisto)
			.AddDeclarantTaxNumber(declarantTaxNumber)
			.AddNonBase64EncodedXmlBytes(xmlMessageBytes)
			.Build();
	}

	string CreateXmlMessage(CusEntryHeader entryHeader)
	{
		var messageWrapper = new MrnMessageWrapper(entryHeader);
		var message = ((IXmlMessageBuilder)new CustomsClearRequestMessageBuilder(messageWrapper)).GenerateXmlMessage();
		return message.GetSerializedString();
	}

	void ValidateMessageCreationContext(IIvistoRequestContext context)
	{
		Argument.NotNull(context.EntryHeader, nameof(context.EntryHeader));
	}

	readonly BusinessObjectFactory factory;
	readonly IIvistoRequestContext messageCreationContext;
}
