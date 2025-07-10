using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business;

public class CHOInterchangeProvider : CHInterchangeProvider
{
	public CHOInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
		: base(messageCollection)
	{
	}

	protected override void SetInterchangeValues(EDIInterchange interchange, EDIMessage message)
	{
		if (message.EM_MessageType == MessageTypeCodeList.Codes.REQ)
		{
			var messageType = ZString.Empty;
			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest:
					messageType = CustomMsgAttributes.MessageTypes.DocumentSearchRequest;
					break;
				case MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest:
					messageType = CustomMsgAttributes.MessageTypes.DocumentDeliveryRequest;
					break;
			}

			if (!messageType.IsEmpty)
			{
				var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes()
					.AddBpId()
					.AddMessageType(messageType)
					.AddMessageId(Guid.NewGuid().ToString());
				interchange.SetHeaderTextWithAttributeDictionary(headerAttributes);
			}
		}
	}

	protected override ZString GetEI_ToFromMessageType(EDIMessage message) => CustomsDestinationCodes.CustomsCharteraOutput;
}
