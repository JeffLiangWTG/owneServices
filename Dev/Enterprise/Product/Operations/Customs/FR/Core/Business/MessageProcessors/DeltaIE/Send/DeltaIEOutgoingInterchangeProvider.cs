using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagePacking;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEOutgoingInterchangeProvider : FRInterchangeProviderBase
	{
		public DeltaIEOutgoingInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;

		protected override ZString GetMessageTextToMessageBody(EDIInterchange interchange, EDIMessage message)
		{
			var schemaId = DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertMessageSubTypeToSchemaId(message.EM_MessageSubType);
			var transactionId = ((CusEntryHeader)message?.EM_LinkedObject)?.CorrelationID ?? string.Empty;
			var messageJson = base.GetMessageTextToMessageBody(interchange, message);
			var envelope = new DeltaIEMessageEnvelope
			{
				SchemaId = schemaId,
				TransactionId = transactionId,
				MessageJson = messageJson
			};
			return JsonSerializer.Serialize(envelope, SerializeOptions);
		}

		protected override void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			IMessagePackRule massageTypeRule = null;

			if (messageTextList.Count() > 1)
			{
				var messageSubType = interchange.ContainedMessages.Cast<EDIMessage>().Select(x => x.EM_MessageSubType).Distinct().SingleOrDefault();
				massageTypeRule = MessagePackRules.FirstOrDefault(x => x.MessageSubType == messageSubType);
			}

			if (massageTypeRule != null)
			{
				stringBuilder.Append(massageTypeRule.PackMessages(messageTextList));
			}
			else
			{
				base.AppendMessageTextToMessageBody(stringBuilder, messageTextList, interchange);
			}
		}

		static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		protected IMessagePackRule[] MessagePackRules => messagePackRules.ToArray();

		protected IEnumerable<IMessagePackRule> messagePackRules
		{
			get
			{
				yield return new IE414MessagePackRule();
			}
		}
	}
}
