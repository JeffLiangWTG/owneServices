using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Send.IE414;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessagePacking;

class IE414MessagePackRule : IMessagePackRule
{
	ZString IMessagePackRule.MessageSubType => DeltaIESendMessageSubTypeList.Codes.Invalidation;
	ZString IMessagePackRule.PackMessages(IEnumerable<ZString> messageTextList)
	{
		var result = new StringBuilder();
		var schemaID = ZString.Empty;
		var transactionId = ZString.Empty;
		var newJObject = new JsonObject();
		var index = 1;

		var importOperationName = CC414BType.ImportOperationJsonPropertyName;
		var sequenceNumberName = MCciOperationType08FR.SequenceNumberJsonPropertyName;

		foreach (var messageText in messageTextList)
		{
			var envelope = JsonSerializer.Deserialize<DeltaIEMessageEnvelope>(messageText, SerializeOptions);
			if (envelope != null)
			{
				JsonObject jObject = JsonNode.Parse(envelope.MessageJson)?.AsObject();
				if (index == 1)
				{
					schemaID = envelope.SchemaId;
					transactionId = envelope.TransactionId;
					newJObject = jObject;
				}
				else
				{
					if (jObject[importOperationName] is JsonArray importOperationArray)
					{
						var importOperation = importOperationArray.FirstOrDefault() as JsonObject;
						if (importOperation != null)
						{
							importOperation[sequenceNumberName] = index.ToString();
							if (newJObject[importOperationName] is JsonArray newImportOperationArray)
							{
								newImportOperationArray.Add(importOperation.DeepClone());
							}
						}
					}
				}
			}
			index++;
		}

		if (!schemaID.IsEmpty)
		{
			var newEnvelope = new DeltaIEMessageEnvelope
			{
				SchemaId = schemaID,
				TransactionId = transactionId,
				MessageJson = newJObject?.ToString()
			};
			result.Append(JsonSerializer.Serialize(newEnvelope, SerializeOptions));
		}
		return result.ToString();
	}

	static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};
}
