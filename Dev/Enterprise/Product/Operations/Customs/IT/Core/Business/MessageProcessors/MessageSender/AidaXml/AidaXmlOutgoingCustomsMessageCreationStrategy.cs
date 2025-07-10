using System.Text;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class AidaXmlOutgoingCustomsMessageCreationStrategy : IOutgoingCustomsMessageCreationStrategy
{
	public AidaXmlOutgoingCustomsMessageCreationStrategy(BusinessObjectFactory factory, IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider valuesProvider)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.valuesProvider = Argument.NotNull(valuesProvider, nameof(valuesProvider));
	}

	readonly BusinessObjectFactory factory;
	readonly IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider valuesProvider;

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		ValidateMessageCreationContext(valuesProvider);

		var mauCertificate = valuesProvider.MauCertificate;
		var cryptokiCertificate = valuesProvider.HasValidAutomaticSignature ? null : valuesProvider.CryptokiCertificate;
		var localRefNumber = valuesProvider.LocalReferenceNumberGenerator.Generate();

		var message = factory.New<ITEDIMessage>();
		message.EM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade;
		message.EM_MessageType = valuesProvider.GetMessageType();
		message.EM_MessageSubType = valuesProvider.GetSubType();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageText = GetMessageText(cryptokiCertificate, localRefNumber, mauCertificate);
		message.EM_LinkedObject = valuesProvider.Parent;
		message.EM_ApplicationReference = valuesProvider.GetApplicationReference();
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(localRefNumber);
		message.EM_GP = mauCertificate?.PK ?? ZGuid.Empty;
		return message;
	}

	#region Implementation

	void ValidateMessageCreationContext(IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider context)
	{
		Argument.NotNull(context.Parent, nameof(context.Parent));
		Argument.NotNull(context.XmlSigner, nameof(context.XmlSigner));
#if DEBUG
		if (GlbStaff.CurrentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return;
		}
#endif
		if (!valuesProvider.HasValidAutomaticSignature)
		{
			Argument.NotNull(context.CryptokiCertificate, nameof(context.CryptokiCertificate));
		}
	}

	string GetMessageText(ICryptokiGlbExternalPassword cryptokiCertificate, string localRefNumber, IGlbMauExternalPassword mauCertificate)
	{
		var serializedXml = valuesProvider.CustomsMessageText;
		serializedXml = ReplaceLrnPlaceholderWithValue(serializedXml, localRefNumber);
		var signedXmlBytes = GetSignedXml(serializedXml, cryptokiCertificate);
		return GetSoapEnvelopedMessage(signedXmlBytes, mauCertificate);
	}

	string ReplaceLrnPlaceholderWithValue(string serializedXml, string localRefNumber)
	{
		return serializedXml.Replace(ITEDIMessage.ITMessageNumberPlaceholder, localRefNumber);
	}

	string GetSoapEnvelopedMessage(byte[] signedXmlBytes, IGlbMauExternalPassword mauCertificate)
	{
		return AidaSoapMessageBuilder.Create(valuesProvider.ServiceTypeNamespace, valuesProvider.ServiceTypePrefix)
			.AddServiceId(valuesProvider.GetServiceId())
			.AddDeclarantTaxNumber(mauCertificate?.DeclarantTaxNumber ?? ZString.Empty)
			.AddNonBase64EncodedXmlBytes(signedXmlBytes)
			.Build();
	}

	byte[] GetSignedXml(string serializedXml, ICryptokiGlbExternalPassword cryptokiCertificate)
	{
		var serializedBytes = Encoding.UTF8.GetBytes(serializedXml);
		return GetSignedXmlBytes(serializedBytes, cryptokiCertificate);
	}

	byte[] GetSignedXmlBytes(byte[] serializedBytes, ICryptokiGlbExternalPassword cryptokiCertificate)
	{
#if DEBUG
		if (GlbStaff.CurrentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return serializedBytes;
		}
#endif
		if (valuesProvider.HasValidAutomaticSignature)
		{
			return serializedBytes;
		}

		return valuesProvider.XmlSigner.Sign(
			serializedBytes,
			cryptokiCertificate,
			ZDateTime.UtcNow.ToDateTime());
	}

	#endregion
}
