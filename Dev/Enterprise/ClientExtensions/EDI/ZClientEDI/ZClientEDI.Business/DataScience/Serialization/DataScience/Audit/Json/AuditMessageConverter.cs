using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using WTG.StaticAnalysis.Annotation;

namespace WTG.Serialization.DataScience.Audit
{
	[CodeAlive("Called by reflection based on attribute")]
	[AuditMessageJsonConverter("1.0.0")]
	public class AuditMessageConverter : JsonConverter<AuditMessage>
	{
		public override bool CanRead => false;

		public override AuditMessage ReadJson(JsonReader reader, Type objectType, AuditMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotSupportedException();

#pragma warning disable CS0162 // Unreachable code detected
			_ = reader ?? throw new ArgumentNullException(nameof(reader));
#pragma warning restore CS0162 // Unreachable code detected
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			if (reader.TokenType is JsonToken.Null)
			{
				return null;
			}

			var jObj = JObject.Load(reader);

			var messageFormatVersion = jObj
				.GetUncapitalized(nameof(AuditMessage.MessageFormatVersion), JTokenType.String)
				.Deserialize<string>(serializer);
			if (messageFormatVersion != AuditMessageJsonSerializerFactory.MessageFormatVersion)
			{
				throw new InvalidOperationException($"Cannot deserialize this object. {nameof(AuditMessage.MessageFormatVersion)} is {messageFormatVersion} but expecting version {AuditMessageJsonSerializerFactory.MessageFormatVersion}");
			}

			var messageCreatedTime = jObj
				.GetUncapitalized(nameof(AuditMessage.MessageCreatedTime), JTokenType.String)
				.Deserialize<DateTimeOffset>(serializer);

			var producerDetails = jObj
				.GetUncapitalized(nameof(AuditMessage.ProducerDetails), JTokenType.Object)
				.Deserialize<ProducerDetails>(serializer);

			var dataSource = jObj
				.GetUncapitalized(nameof(AuditMessage.DataSource), JTokenType.Object)
				.Deserialize<DataSource>(serializer);

			var changes = jObj
				.GetUncapitalized(nameof(AuditMessage.Changes), JTokenType.Array)
				.Deserialize<List<AuditRow>>(serializer);

			// TODO: Re-parse `changes[i].ColumnValues` values here depending on `dataSource.Columns` WI00551874

			return new AuditMessage(messageCreatedTime, producerDetails, dataSource, changes);
		}

		public override void WriteJson(JsonWriter writer, AuditMessage value, JsonSerializer serializer)
		{
			_ = writer ?? throw new ArgumentNullException(nameof(writer));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			if (value is null)
			{
				writer.WriteNull();
				return;
			}

			if (value.MessageFormatVersion != AuditMessageJsonSerializerFactory.MessageFormatVersion)
			{
				throw new InvalidOperationException($"Cannot serialize this object. {nameof(AuditMessage.MessageFormatVersion)} is {value.MessageFormatVersion} but expecting version {AuditMessageJsonSerializerFactory.MessageFormatVersion}");
			}

			var jObj = new JObject()
				.AddUncapitalized(serializer, value.MessageFormatVersion, nameof(value.MessageFormatVersion))
				.AddUncapitalized(serializer, value.MessageCreatedTime, nameof(value.MessageCreatedTime))
				.AddUncapitalized(serializer, value.ProducerDetails, nameof(value.ProducerDetails))
				.AddUncapitalized(serializer, value.DataSource, nameof(value.DataSource))
				.AddUncapitalized(serializer, value.Changes, nameof(value.Changes));

			serializer.Serialize(writer, jObj);
		}
	}
}
