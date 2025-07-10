using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using WTG.Serialization.DataScience.Extensions;

namespace WTG.Serialization.DataScience.Audit
{
	public class HexByteArrayConverter : JsonConverter<byte[]>
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "const string is thread-safe")]
		public const string Prefix = ByteArrayExtensions.SerializationPrefix;

		public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			_ = reader ?? throw new ArgumentNullException(nameof(reader));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			switch (reader.TokenType)
			{
				case JsonToken.Null:
					return null;
				case JsonToken.String:
					var str = (string)reader.Value;
					if (!str.Substring(0, Prefix.Length).Equals(Prefix))
					{
						throw new FormatException($"Byte array does not begin with '{Prefix}': '{str.Substring(0, 100)}'");
					}
					str = str.Substring(Prefix.Length);
					return Enumerable.Range(0, str.Length / 2)
						.Select(i => byte.Parse(str.Substring(i * 2, 2), NumberStyles.HexNumber))
						.ToArray();
				default:
					throw new FormatException($"Unexpected JsonToken type in byte collection deserialization: {reader.TokenType}");
			}
		}

		public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
		{
			_ = writer ?? throw new ArgumentNullException(nameof(writer));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));

			if (value is null)
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteValue(value.ToHexString());
			}
		}
	}
}
