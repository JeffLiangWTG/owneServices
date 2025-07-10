using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using WTG.Statistics;

namespace Enterprise.ZArchitecture.Business
{
	internal class StatisticsCacheSerializer
	{
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			Culture = CultureInfo.InvariantCulture,
			TypeNameHandling = TypeNameHandling.None,
			Converters = new List<JsonConverter>()
			{
				new ObjectArrayJsonConverter(),
				new SqlGuidJsonConverter(),
				new DecimalJsonConverter()
			}
		};

		public byte[] SerializeHistograms(SqlHistogram[] histograms)
		{
			var jsonString = JsonConvert.SerializeObject(histograms, serializerSettings);
			return Encoding.Unicode.GetBytes(jsonString);
		}

		public SqlHistogram[] DeserializeHistograms(byte[] bytes)
		{
			var jsonHistogramString = Encoding.Unicode.GetString(bytes);

			return JsonConvert.DeserializeObject<SqlHistogram[]>(jsonHistogramString, serializerSettings);
		}

		public class ObjectArrayJsonConverter : JsonConverter
		{
			readonly Dictionary<string, Type> allowedItemTypes = new Dictionary<string, Type>() {
			{ nameof(Guid), typeof(Guid) },
			{ nameof(SqlGuid), typeof(SqlGuid) },
			{ nameof(String), typeof(string) },
			{ nameof(Byte), typeof(byte) },
			{ typeof(byte[]).Name, typeof(byte[]) },
			{ nameof(Int16), typeof(short) },
			{ nameof(Int32), typeof(int) },
			{ nameof(Int64), typeof(long) },
			{ nameof(Decimal), typeof(decimal) },
			{ nameof(Double), typeof(double) },
			{ nameof(Boolean), typeof(bool) }
			};

			public override bool CanConvert(Type objectType)
			{
				return objectType == typeof(object[]);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var results = new List<object>();
				if (reader.TokenType == JsonToken.StartObject)
				{
					reader.Read();
				}
				var strType = reader.ReadAsString();
				reader.Read();
				if (strType == null)
				{
					return Array.Empty<object>();
				}
				if (!allowedItemTypes.TryGetValue(strType, out var type))
				{
					throw new ArgumentException($"Type '{strType}' is not allowed to deserialize by {nameof(StatisticsCacheSerializer)}");
				}
				reader.Read();
				if (reader.TokenType == JsonToken.StartArray)
				{
					reader.Read();
					while (reader.TokenType != JsonToken.EndArray)
					{
						results.Add(serializer.Deserialize(reader, type));
						reader.Read();
					}
				}
				reader.Read();
				return results.ToArray();
			}

			[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Json property name")]
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("$itemType");
				Type itemType = null;
				if (value is object[] untypedArray && untypedArray.Length > 0)
				{
					itemType = untypedArray[0].GetType();
					writer.WriteValue($"{itemType.Name}");
				}
				else
				{
					writer.WriteNull();
				}
				writer.WritePropertyName("$values");
				writer.WriteStartArray();
				foreach (var item in value as object[])
				{
					if (item.GetType() != itemType)
					{
						throw new ArgumentException($"All items in the object array should be of the same type as the first item. First item type :{itemType}, Other item type : {item.GetType()}");
					}
					if (item != null)
					{
						serializer.Serialize(writer, item);
					}
					else
					{
						writer.WriteNull();
					}
				}
				writer.WriteEndArray();
				writer.WriteEndObject();
			}
		}

		// This is to handle SqlGuild.Null
		public class SqlGuidJsonConverter : JsonConverter<SqlGuid>
		{
			public override SqlGuid ReadJson(JsonReader reader, Type objectType, SqlGuid existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var strGuid = serializer.Deserialize<string>(reader);
				if (strGuid == null)
				{
					return SqlGuid.Null;
				}
				else
				{
					return SqlGuid.Parse(strGuid);
				}
			}

			public override void WriteJson(JsonWriter writer, SqlGuid value, JsonSerializer serializer)
			{
				if (value is SqlGuid sqlGuid && !sqlGuid.IsNull)
				{
					serializer.Serialize(writer, sqlGuid.Value);
				}
				else
				{
					writer.WriteNull();
				}
			}
		}

		// This is to handle Decimal.MinValue & Decimal.MaxValue
		public class DecimalJsonConverter : JsonConverter<decimal>
		{
			public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				return decimal.Parse((string)reader.Value, NumberStyles.Any);
			}

			public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
			{
				writer.WriteToken(JsonToken.String, value.ToString("G29", CultureInfo.InvariantCulture));
			}
		}
	}
}
