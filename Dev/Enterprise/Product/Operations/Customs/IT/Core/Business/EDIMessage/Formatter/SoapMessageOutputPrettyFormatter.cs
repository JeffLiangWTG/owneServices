using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class SoapMessageOutputPrettyFormatter : ITEDIMessagePrettyFormatter
{
	public SoapMessageOutputPrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		var deserializer = GetDeserializer();
		IResponseDataProvider content = deserializer.GetDeserializedBody(originalText, new RispostaOutputXmlOverridesCreationFactory());
		var xmlContent = content?.DataString;
		if (string.IsNullOrWhiteSpace(xmlContent))
		{
			return originalText;
		}

		return xmlContent;
	}

	ISoapMessageBodyDeserializer<Risposta> GetDeserializer()
		=> new SoapMessageBodyDeserializer<Risposta>(AidaSoapMessageNamespaceConstants.ElectronicFolderRequestTypeNamespace);
}
