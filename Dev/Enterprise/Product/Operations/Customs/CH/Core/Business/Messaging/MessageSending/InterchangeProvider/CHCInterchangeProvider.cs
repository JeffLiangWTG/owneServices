using CargoWise.Types;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business;

public class CHCInterchangeProvider : CHInterchangeProvider
{
	public CHCInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
		: base(messageCollection)
	{
	}

	protected override void SetInterchangeValues(EDIInterchange interchange, EDIMessage message)
	{
		interchange.EI_BodyText = GetBodyTextWithSoapEnvelope(interchange);
	}

	string GetBodyTextWithSoapEnvelope(EDIInterchange interchange)
	{
		const string XmlDeclarationEndTag = "?>";
		var bodyText = interchange.EI_BodyText;

		var positionOfEndOfXmlDeclaration = bodyText.IndexOf(XmlDeclarationEndTag);
		if (positionOfEndOfXmlDeclaration == -1)
		{
			positionOfEndOfXmlDeclaration = 0;
		}
		else
		{
			positionOfEndOfXmlDeclaration += XmlDeclarationEndTag.Length;
		}
		return bodyText.Insert(positionOfEndOfXmlDeclaration, SoapEnvelope) + SoapEnvelopeEnd;
	}

	protected override ZString GetEI_ToFromMessageType(EDIMessage message)
	{
		switch (message.EM_MessageType)
		{
			case MessageTypeCodeList.Codes.EBD:
				return CustomsDestinationCodes.CustomsEbdSoap;
			case MessageTypeCodeList.Codes.EVV:
				return CustomsDestinationCodes.CustomsEvvSoap;
			case MessageTypeCodeList.Codes.ECM:
				return CustomsDestinationCodes.CustomsEComSoap;
			case MessageTypeCodeList.Codes.BOR:
				return CustomsDestinationCodes.CustomsBordereauSoap;
			default:
				return CustomsDestinationCodes.CustomsEdecSoap;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string SoapEnvelope = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""><soapenv:Header /><soapenv:Body>";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string SoapEnvelopeEnd = @"</soapenv:Body></soapenv:Envelope>";
}
