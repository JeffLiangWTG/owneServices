using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class IrildesRequestMessageCreationStrategy : IOutgoingCustomsMessageCreationStrategy
{
	public IrildesRequestMessageCreationStrategy(IIrildesRequestContext messageCreationContext)
	{
		this.messageCreationContext = Argument.NotNull(messageCreationContext, nameof(messageCreationContext));
	}

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		var nctsHeader = Argument.NotNull(messageCreationContext.NctsHeader, nameof(messageCreationContext.NctsHeader));
		var movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		var mauCertificate = messageCreationContext.MauCertificate;

		var xmlMessage = CreateXmlMessage(nctsHeader);
		var soapEnvelope = CreateSoapEnvelope(xmlMessage, mauCertificate?.DeclarantTaxNumber ?? ZString.Empty);
		return AddNewEDIMessage(soapEnvelope, movementHeader, mauCertificate);
	}

	ITEDIMessage AddNewEDIMessage(string soapEnvelope, NctsDepartureMovementHeader movementHeader, IGlbExternalPassword mauCertificate)
	{
		var statusRequestMessage = movementHeader.Messages.AddNew();
		statusRequestMessage.EM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade;
		statusRequestMessage.EM_MessageType = EDIMessageTypeList.Codes.IrildesRequest;
		statusRequestMessage.EM_MessageSubType = "XXX";
		statusRequestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		statusRequestMessage.EM_Status = EDIMessage.Status.Queued;
		statusRequestMessage.EM_MessageText = soapEnvelope;
		statusRequestMessage.EM_ApplicationReference = ZString.Empty;
		statusRequestMessage.EM_GP = mauCertificate?.PK ?? ZGuid.Empty;
		statusRequestMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		return statusRequestMessage;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant prefix tran")]
	string CreateSoapEnvelope(ZString xmlContent, ZString declarantTaxNumber)
	{
		const string typePrefix = "tran";
		return AidaSoapMessageBuilder.Create(AidaSoapMessageNamespaceConstants.NctsServiceTypeNamespace, typePrefix: typePrefix)
			.AddServiceId(Ucc6XmlConstants.CustomsClearRequests.ServiceId.Irildes)
			.AddDeclarantTaxNumber(declarantTaxNumber)
			.AddNonBase64EncodedXmlContent(xmlContent)
			.Build();
	}

	string CreateXmlMessage(NctsHeader nctsHeader)
	{
		var messageWrapper = new MrnMessageWrapper(nctsHeader);
		IXmlMessageBuilder xmlMessageBuilder = new CustomsClearRequestMessageBuilder(messageWrapper);
		var xmlMessage = xmlMessageBuilder.GenerateXmlMessage();
		return xmlMessage.GetSerializedString();
	}

	readonly IIrildesRequestContext messageCreationContext;
}
