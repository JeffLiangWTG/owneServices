using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHPInterchangeProvider))]
sealed class CHPInterchangeProviderTest : CHInterchangeProviderTest
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return new CHPInterchangeProvider(collection);
	}

	protected override List<(string messageType, string messageSubType, string expectedDestinationCode)> GetDataForTest() => new[]
	{
			(MessageTypeCodeList.Codes.TRE, string.Empty, CustomsDestinationCodes.CustomsKeyMan),
			(MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarDeclaration, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarRequestDataJourney, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.NctsActivationAtDomicile, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarDeclaration, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarAmendment, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarWithdrawal, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.MSL, string.Empty, CustomsDestinationCodes.CustomsPassar),
			(MessageTypeCodeList.Codes.MSG, string.Empty, CustomsDestinationCodes.CustomsPassar),
		}.ToList();

	protected override Dictionary<string, string> GetExpectedEI_HeaderAttributes(EDIMessage message)
	{
		if (message.EM_MessageType.ToString() is MessageTypeCodeList.Codes.PassarNcts or MessageTypeCodeList.Codes.Export)
		{
			var expectedPassarMessageType = PassarMessageTypeList.GetPassarMessageType(message.EM_MessageType, message.EM_MessageSubType);
			AssertNotEquals("PassarMessageType", expectedPassarMessageType);
			return new Dictionary<string, string>()
					{
						{ CustomMsgAttributes.BpId, CompanyBID },
						{ CustomMsgAttributes.MessageType, expectedPassarMessageType },
						{ CustomMsgAttributes.MessageID, "APREF000001" },
					};
		}
		return null;
	}
}
