using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class SoapMessageInputPrettyFormatter : ITEDIMessagePrettyFormatter
{
	public SoapMessageInputPrettyFormatter(BusinessObjectFactory factory, string soapNamespace) : base(factory)
	{
		this.soapNamespace = Argument.NotNullOrEmpty(soapNamespace, nameof(soapNamespace));
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		var soapMessageDeserializer = GetSoapMessageDeserializer();
		var deserializedBody = soapMessageDeserializer.GetDeserializedBody(originalText, new SoapMessageInputBodyXmlOverridesCreationFactory());
		var xmlContent = deserializedBody?.Data?.XmlContentString;

		return !string.IsNullOrWhiteSpace(xmlContent)
			? xmlContent
			: originalText;
	}

	ISoapMessageBodyDeserializer<SoapMessageInput> GetSoapMessageDeserializer() => new SoapMessageBodyDeserializer<SoapMessageInput>(soapNamespace);

	readonly string soapNamespace;
}
