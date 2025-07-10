using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WTG.Serialization.DataScience.Audit
{
	public static class JsonExtensions
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Auto-initialised getter of immutable type")]
		public static IReadOnlyCollection<string> SupportedMessageFormatVersions { get; } = new HashSet<string> { "1.0.0" };

		public static IEnumerable<JsonConverter> GetAuditMessageJsonConverters(string messageFormatVersion)
		{
			if (!SupportedMessageFormatVersions.Contains(messageFormatVersion))
			{
				throw new NotSupportedException($"{nameof(messageFormatVersion)}: {messageFormatVersion}");
			}

			return typeof(AuditMessageJsonConverterAttribute)
				.Assembly
				.DefinedTypes
				.Where(t => t.BaseType?.BaseType == typeof(JsonConverter))
				.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(AuditMessageJsonConverterAttribute)))
				.Where(t => t
					.GetCustomAttributes(typeof(AuditMessageJsonConverterAttribute), false)
					.Cast<AuditMessageJsonConverterAttribute>()
					.Any(a => a.MessageFormatVersion == messageFormatVersion))
				.Select(converterInfo => converterInfo
					.GetConstructor(Array.Empty<Type>())
					?.Invoke(Array.Empty<object>()) as JsonConverter
					?? throw new InvalidOperationException($"Cannot create `{converterInfo.Name}` object"));
		}

		public static JObject AddUncapitalized<T>(
			this JObject jObject,
			JsonSerializer serializer,
			T value,
			string propertyName)
		{
			_ = jObject ?? throw new ArgumentNullException(nameof(jObject));
			_ = serializer ?? throw new ArgumentNullException(nameof(serializer));
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException($"{nameof(propertyName)} must not be empty", nameof(propertyName));
			}

			using (var tw = new JTokenWriter())
			{
				serializer.Serialize(tw, value);

				jObject.Add(propertyName.ToUncapitalized(), tw.Token);

				return jObject;
			}
		}

		public static JToken GetUncapitalized(
			this JObject jObject,
			string propertyName,
			JTokenType tokenType)
		{
			_ = jObject ?? throw new ArgumentNullException(nameof(jObject));
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException($"{nameof(propertyName)} can not be empty", nameof(propertyName));
			}

			propertyName = propertyName.ToUncapitalized();

			return jObject
				.Properties()
				.Single(
					p => p.Name.Equals(propertyName, StringComparison.Ordinal)
						&& p.Value.Type == tokenType)
				.Value;
		}

		public static JToken GetUncapitalized(
			this JObject jObject,
			string propertyName)
		{
			_ = jObject ?? throw new ArgumentNullException(nameof(jObject));
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException($"{nameof(propertyName)} must not be empty", nameof(propertyName));
			}

			propertyName = propertyName.ToUncapitalized();

			return jObject
				.Properties()
				.Single(p => p.Name.Equals(propertyName, StringComparison.Ordinal))
				.Value;
		}

		public static T Deserialize<T>(this JToken token, JsonSerializer serializer)
		{
			return serializer.Deserialize<T>(token.CreateReader());
		}

		public static string ToUncapitalized(this string str)
		{
			return char.IsUpper(str[0])
				? char.ToLower(str[0]) + str.Substring(1)
				: str;
		}
	}
}
