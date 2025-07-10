using System;
using System.Data;
using System.Text.Json;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class PBNInboundEDIMessage : InboundEDIMessage
{
	public PBNInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new PBNInboundEDIMessageLookups Lookups => (PBNInboundEDIMessageLookups)base.Lookups;
	protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new PBNInboundEDIMessageLookups(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsPBN;
	}

	protected override TDataProvider GetDataProviderCore<TDataProvider>(Type messageObjectType, bool withSchemaValidations)
	{
		if (EM_MessageText.IsEmpty)
		{
			EM_Status = EDIMessage.Status.Failed;
			return default;
		}
		var textToDeserialize = EM_MessageText;
		if (typeof(TDataProvider) == typeof(ROSErrorProvider))
		{
			textToDeserialize = GetMessageTextInJsonFormat(EM_MessageText,messageObjectType);
		}
		var dataObject = JsonSerializer.Deserialize(textToDeserialize, messageObjectType);
		return (TDataProvider)Activator.CreateInstance(typeof(TDataProvider), dataObject);
	}

	static string GetMessageTextInJsonFormat(string text, Type messageObjectType)
	{
		try
		{
			if (JsonSerializer.Deserialize(text, messageObjectType) is not null)
			{
				return text;
			}
		}
		catch (Exception ex) when (ex is JsonException || ex.InnerException is JsonException)
		{
			text = UniversalInterchangeXMLHelper.TryGetReasonNodeFromUniversalInterchangeTypeXML(text);
		}
		return text;
	}
}
