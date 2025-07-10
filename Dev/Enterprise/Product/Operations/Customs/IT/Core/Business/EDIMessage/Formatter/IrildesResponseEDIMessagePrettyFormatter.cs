using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class IrildesResponseEDIMessagePrettyFormatter : ITEDIMessagePrettyFormatter
{
	public IrildesResponseEDIMessagePrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		var soapMessageDeserializer = GetSoapMessageDeserializer();
		var responseDataProvider = (IResponseDataProvider)soapMessageDeserializer.GetDeserializedBody(originalText, overridesCreationFactory: null);
		var xmlContent = responseDataProvider?.DataString;
		return !string.IsNullOrWhiteSpace(xmlContent) ? xmlContent : originalText;
	}

	static ISoapMessageBodyDeserializer<Risposta> GetSoapMessageDeserializer()
		=> new SoapMessageBodyDeserializer<Risposta>(AidaSoapMessageNamespaceConstants.NctsServiceTypeNamespace);
}
