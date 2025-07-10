using System;
using System.Globalization;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace WTG.Serialization.DataScience.Audit
{
	[CodeAlive("Called by reflection based on attribute")]
	[AuditMessageJsonConverter("1.0.0")]
	public class DateTimeConverter : JsonConverter<DateTime>
	{
		public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";

		public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			_ = reader ?? throw new ArgumentNullException(nameof(reader));
			_ = serializer ?? throw new ArgumentNullException(nameof(reader));

			if (reader.TokenType is JsonToken.Null)
			{
				throw new InvalidOperationException("DateTime cannot be null");
			}

			if (serializer.DateParseHandling != DateParseHandling.None)
			{
				throw new InvalidOperationException($"{serializer.GetType().Name}.{nameof(serializer.DateParseHandling)} must be set to {DateParseHandling.None} to use {GetType().Name}");
			}

			var str = serializer.Deserialize<string>(reader);
			if (DateTime.TryParseExact(str, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return result;
			}
			throw new InvalidOperationException($"Cannot parse {nameof(DateTime)} `{str}` as `{DateTimeFormat}`");
		}

		public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
		{
			_ = writer ?? throw new ArgumentNullException(nameof(writer));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			serializer.Serialize(writer, value.ToString(DateTimeFormat));
		}
	}
}
