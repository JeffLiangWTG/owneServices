using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business;

public class CHPInterchangeProvider : CHInterchangeProvider
{
	public CHPInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
		: base(messageCollection)
	{
	}

	protected override void SetInterchangeValues(EDIInterchange interchange, EDIMessage message)
	{
		switch (message.EM_MessageType)
		{
			case MessageTypeCodeList.Codes.PassarNcts:
			case MessageTypeCodeList.Codes.Export:
				var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes()
					.AddBpId()
					.AddMessageType(PassarMessageTypeList.GetPassarMessageType(message.EM_MessageType, message.EM_MessageSubType))
					.AddMessageId(message.EM_ApplicationReference);
				interchange.SetHeaderTextWithAttributeDictionary(headerAttributes);
				break;
		}
	}

	protected override ZString GetEI_ToFromMessageType(EDIMessage message)
	{
		switch (message.EM_MessageType)
		{
			case MessageTypeCodeList.Codes.TRE:
				return CustomsDestinationCodes.CustomsKeyMan;
			default:
				return CustomsDestinationCodes.CustomsPassar;
		}
	}
}
