using System.Xml.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6IvistoResponseEDIMessagePrettyFormatter : ITEDIMessagePrettyFormatter
{
	public Ucc6IvistoResponseEDIMessagePrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		var soapDeserializer = (ISoapMessageBodyDeserializer<Risposta>)new SoapMessageBodyDeserializer<Risposta>(typeNamespace: null);
		var responseDataProvider = (IResponseDataProvider)soapDeserializer.GetDeserializedBody(originalText, overridesCreationFactory: null);
		var dataString = responseDataProvider?.DataString;
		if (!string.IsNullOrWhiteSpace(dataString))
		{
			var xDocument = XDocument.Parse(dataString);
			return new ZStringBuilder()
				.Append((NoResString)"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>")
				.Append(xDocument.ToString())
				.ToStringWithNewLineBetweenAppends();
		}

		return originalText;
	}
}
