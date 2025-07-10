using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ResponseEDIMessagePrettyFormatter : ITEDIMessagePrettyFormatter
{
	public Ucc6ResponseEDIMessagePrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		var deserializer = GetDeserializer();
		IResponseDataProvider content = deserializer.GetDeserializedBody(originalText, overridesCreationFactory: null);
		var xmlContent = content?.DataString;
		if (string.IsNullOrWhiteSpace(xmlContent))
		{
			return originalText;
		}

		return xmlContent;
	}

	ISoapMessageBodyDeserializer<recuperaEsitoResponse> GetDeserializer()
		=> new SoapMessageBodyDeserializer<recuperaEsitoResponse>(typeNamespace: null);
}
