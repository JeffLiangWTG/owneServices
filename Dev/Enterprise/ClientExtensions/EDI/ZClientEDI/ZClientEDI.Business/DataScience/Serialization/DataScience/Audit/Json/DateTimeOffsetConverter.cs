using System;
using System.Globalization;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace WTG.Serialization.DataScience.Audit
{
	[CodeAlive("Called by reflection based on attribute")]
	[AuditMessageJsonConverter("1.0.0")]
	public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
	{
		public const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";

		public override DateTimeOffset ReadJson(JsonReader reader, Type objectType, DateTimeOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			_ = reader ?? throw new ArgumentNullException(nameof(reader));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			if (reader.TokenType is JsonToken.Null)
			{
				throw new InvalidOperationException("DateTimeOffset cannot be null");
			}

			if (serializer.DateParseHandling != DateParseHandling.None)
			{
				throw new InvalidOperationException($"{serializer.GetType().Name}.{nameof(serializer.DateParseHandling)} must be set to {DateParseHandling.None} to use {GetType().Name}");
			}

			var str = serializer.Deserialize<string>(reader);
			if (DateTimeOffset.TryParseExact(str, DateTimeOffsetFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return result;
			}
			throw new InvalidOperationException($"Cannot parse {nameof(DateTimeOffset)} `{str}` as `{DateTimeOffsetFormat}`");
		}

		public override void WriteJson(JsonWriter writer, DateTimeOffset value, JsonSerializer serializer)
		{
			_ = writer ?? throw new ArgumentNullException(nameof(writer));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			serializer.Serialize(writer, value.ToString(DateTimeOffsetFormat));
		}
	}
}
