using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CHOInterchangeProviderTest : CHInterchangeProviderTest
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsCharteraOutput;

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return new CHOInterchangeProvider(collection);
	}

	protected override List<(string messageType, string messageSubType, string expectedDestinationCode)> GetDataForTest() => new[]
	{
			(MessageTypeCodeList.Codes.MSL, "", CustomsDestinationCodes.CustomsCharteraOutput),
			(MessageTypeCodeList.Codes.MSG, "",CustomsDestinationCodes.CustomsCharteraOutput),
			(MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest, CustomsDestinationCodes.CustomsCharteraOutput),
			(MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest,CustomsDestinationCodes.CustomsCharteraOutput),
		}.ToList();

	protected override Dictionary<string, string> GetExpectedEI_HeaderAttributes(EDIMessage message)
	{
		if (message.EM_MessageType == MessageTypeCodeList.Codes.REQ)
		{
			if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest)
			{
				return new Dictionary<string, string>()
					{
						{ CustomMsgAttributes.BpId, CompanyBID },
						{ CustomMsgAttributes.MessageType, CustomMsgAttributes.MessageTypes.DocumentDeliveryRequest },
						{ CustomMsgAttributes.MessageID, JsonTestHelper.AnyGuid },
					};
			}
			if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest)
			{
				return new Dictionary<string, string>()
					{
						{ CustomMsgAttributes.BpId, CompanyBID },
						{ CustomMsgAttributes.MessageType, CustomMsgAttributes.MessageTypes.DocumentSearchRequest },
						{ CustomMsgAttributes.MessageID, JsonTestHelper.AnyGuid },
					};
			}
		}
		return null;
	}
}
