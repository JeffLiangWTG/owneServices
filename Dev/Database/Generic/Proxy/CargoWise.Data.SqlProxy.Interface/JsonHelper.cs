using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Data.SqlProxy.Interface.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.SqlProxy.Interface;

public static class JsonHelper
{
	[ThreadSafe]
	public static readonly JsonSerializerSettings JsonSettings = new ()
	{
		DateFormatHandling = DateFormatHandling.IsoDateFormat,
		TypeNameHandling = TypeNameHandling.None,
		FloatParseHandling = FloatParseHandling.Decimal,
		FloatFormatHandling = FloatFormatHandling.String,
		Converters =
		{
			new SqlGeographyConverter(),
			new SqlGeometryConverter(),
			new SqlHierarchyIdConverter()
		},
	};

	[ThreadSafe]
	public static readonly JsonSerializer CommonJsonSerializer = JsonSerializer.Create(JsonSettings);

	public static string Prettify(string jsonString)
	{
		try
		{
			var prettyJson = JToken.Parse(jsonString).ToString(Formatting.Indented);
			return Regex.Unescape(ReplaceNewLineCharacters(prettyJson));
		}
		catch (JsonException)
		{
			return Regex.Unescape((ReplaceNewLineCharacters(jsonString)));
		}
	}

	public static string ReplaceNewLineCharacters(string jsonString)
	{
		return jsonString.Replace(@"\\r\\n", Environment.NewLine).Replace(@"\\n", Environment.NewLine);
	}

	public static void Serialize<T>(T obj, TextWriter writer)
	{
		CommonJsonSerializer.Serialize(writer ?? throw new ArgumentNullException(nameof(writer)), obj);
	}

	public static T Deserialize<T>(TextReader reader)
	{
		return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), JsonSettings)!;
	}

	public static string? ToJson(object? obj)
	{
		if (obj == null)
		{
			return LazyJsonNull.Value;
		}

		if (obj is string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return LazyJsonEmptyObject.Value;
			}

			try
			{
				return JToken.Parse(str).Type == JTokenType.Object
					? LazyJsonEmptyObject.Value
					: str;
			}
			catch (JsonReaderException)
			{
				return JsonConvert.SerializeObject(str, JsonSettings);
			}
		}

		return JsonConvert.SerializeObject(obj, JsonSettings);
	}

	public static T? FromJson<T>(string? jsonString)
	{
		return (T?)FromJson(jsonString, typeof(T));
	}

	public static object? FromJson(string? jsonString, Type targetType, bool treatNullAsDbNull = false)
	{
		if (string.IsNullOrEmpty(jsonString)
			|| string.Equals(jsonString, LazyJsonNull.Value)
			|| string.Equals(jsonString, LazyJsonEmptyObject.Value))
		{
			return GetNullOrEmptyValueForTargetType();
		}

		var json = ParseJsonString();
		if (targetType == typeof(char))
		{
			if (json == null)
			{
				throw new JsonSerializationException($"Cannot deserialize JSON {jsonString} to type 'System.Char'. JSON must be a single character.");
			}

			return json.Trim('"')[0];
		}

		return json != null
			? JsonConvert.DeserializeObject(json, targetType, JsonSettings)
			: null;

		string? ParseJsonString()
		{
			var parseJsonString = jsonString;

			try
			{
				_ = JToken.Parse(jsonString!);
			}
			catch (JsonReaderException)
			{
				parseJsonString = ToJson(jsonString);
			}

			return parseJsonString;
		}

		object? GetNullOrEmptyValueForTargetType()
		{
			if (treatNullAsDbNull || targetType == typeof(DBNull))
			{
				return DBNull.Value;
			}

			return targetType == typeof(string)
				? string.IsNullOrEmpty(jsonString) ? string.Empty : null
				: null;
		}
	}

	public static string SerializeObjectArray(IEnumerable<object[]>? dataRows)
	{
		var jsonArray = new JArray();
		if (dataRows != null)
		{
			foreach (var row in dataRows)
			{
				jsonArray.Add(ConvertToJArray(row));
			}
		}

		return jsonArray.ToString();

		JArray ConvertToJArray(IEnumerable data)
		{
			var jArray = new JArray();
			var dataEnumerator = data.GetEnumerator();

			while (dataEnumerator.MoveNext())
			{
				jArray.Add(ToJson(dataEnumerator.Current));
			}

			if (dataEnumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}

			return jArray;
		}
	}

	public static IEnumerable<object?[]> DeserializeObjectArray(string jsonString, Type[] dataTypes, bool treatNullAsDbNull = false)
	{
		var jsonArray = JArray.Parse(jsonString);

		return jsonArray.Select(item =>
		{
			var deserializedResults = new List<object?>();

			foreach (var (parsedData, requiredDataType) in Enumerable.Zip((JArray)item, dataTypes, (i, t) => (i, t)))
			{
				var rawValue = ((JValue)parsedData).Value;
				var rawValueType = rawValue?.GetType();
				var isAlreadyDeserialized = rawValueType != typeof(string) && rawValueType == requiredDataType;

				var deserializedData = isAlreadyDeserialized ? rawValue : FromJson(parsedData.ToString(), requiredDataType, treatNullAsDbNull);
				deserializedResults.Add(deserializedData);
			}

			return deserializedResults.ToArray();
		});
	}

	public static void AddRow(DataTable dataTable, object?[] item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		if (dataTable.Columns.Count != item.Length)
		{
			throw new InvalidOperationException($"DataTable {dataTable.TableName} columns count: {dataTable.Columns.Count} and new item length {item.Length} mismatch.");
		}

		var row = dataTable.NewRow();
		dataTable.Rows.Add(row);

		for (var col = 0; col < dataTable.Columns.Count; col++)
		{
			row[col] = item[col] ?? DBNull.Value;
		}
	}

	public static Type ParseSqlType(string sqlTypeString)
	{
		if (string.IsNullOrEmpty(sqlTypeString))
		{
			return typeof(object);
		}

		if (Enum.TryParse(sqlTypeString, out SqlDataType typeCode))
		{
			return TypeMappingHelper.SqlDataTypeToCsTypeMappings[typeCode];
		}

		if (Enum.TryParse(sqlTypeString, out SqlDbType sqlDbType))
		{
			return TypeMappingHelper.SqlDbTypeToCsTypeMappings[sqlDbType];
		}

		throw new NotSupportedException($"SqlDataType: {sqlTypeString} is not recognized.");
	}

	public static Type FromSqlDbType(SqlDbType sqlDbType)
	{
		if (!TypeMappingHelper.SqlDbTypeToCsTypeMappings.TryGetValue(sqlDbType, out var type))
		{
			type = typeof(object);
		}

		return type;
	}

	static readonly Lazy<string> LazyJsonNull = new(() => JsonConvert.SerializeObject(null, JsonSettings));

	static readonly Lazy<string> LazyJsonEmptyObject = new(() => JsonConvert.SerializeObject(new object(), JsonSettings));
}
