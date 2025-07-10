using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessagePacking;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEInboundMessageCreator : InboundMessageCreator
	{
		protected override List<FREDIMessage> CreateMessagesFromInterchange(EDIInterchange interchange)
		{
			var envelope = JsonSerializer.Deserialize<DeltaIEMessageEnvelope>(interchange.EI_BodyText);
			if (envelope is not null and { MessageJson: not null })
			{
				var messageSubType = DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertSchemaIdToMessageSubType(envelope.SchemaId);
				var originalMessageJsonText = envelope.MessageJson;
				if (MessageUnpackRules.FirstOrDefault(r => r.MessageSubType == messageSubType) is IMessageUnpackRule unpackRule)
				{
					return unpackRule.UnpackMessage(originalMessageJsonText).Select(singleMessageText => CreateNewMessage(singleMessageText)).ToList();
				}
				else
				{
					return new List<FREDIMessage> { CreateNewMessage(originalMessageJsonText) };
				}

				FREDIMessage CreateNewMessage(ZString messageText)
				{
					var message = interchange.Factory.New<DeltaIEFREDIMessage>();
					message.EM_MessageText = messageText;
					message.EM_MessageSubType = messageSubType;
					return message;
				}
			}
			else
			{
				return Array.Empty<FREDIMessage>().ToList();
			}
		}

		IEnumerable<IMessageUnpackRule> MessageUnpackRules
		{
			get
			{
				yield return new IE456MessageUnpackRule();
			}
		}
	}
}
