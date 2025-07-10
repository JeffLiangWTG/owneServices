using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CHCInterchangeProviderTest : CHInterchangeProviderTest
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return new CHCInterchangeProvider(collection);
	}

	protected override List<(string messageType, string messageSubType, string expectedDestinationCode)> GetDataForTest() => new[]
	{
			(MessageTypeCodeList.Codes.Import, "", CustomsDestinationCodes.CustomsEdecSoap),
			(MessageTypeCodeList.Codes.Export, "", CustomsDestinationCodes.CustomsEdecSoap),
			(MessageTypeCodeList.Codes.EBD, "", CustomsDestinationCodes.CustomsEbdSoap),
			(MessageTypeCodeList.Codes.ECM, "", CustomsDestinationCodes.CustomsEComSoap),
			(MessageTypeCodeList.Codes.EVV, "", CustomsDestinationCodes.CustomsEvvSoap),
			(MessageTypeCodeList.Codes.BOR, "", CustomsDestinationCodes.CustomsBordereauSoap),
		}.ToList();

	protected override ZString GetExpectedEI_BodyText(EDIMessage message)
	{
		return SoapPrefix + message.EM_MessageText + SoapSuffix;
	}

	const string SoapPrefix = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""><soapenv:Header /><soapenv:Body>";
	const string SoapSuffix = @"</soapenv:Body></soapenv:Envelope>";
}
