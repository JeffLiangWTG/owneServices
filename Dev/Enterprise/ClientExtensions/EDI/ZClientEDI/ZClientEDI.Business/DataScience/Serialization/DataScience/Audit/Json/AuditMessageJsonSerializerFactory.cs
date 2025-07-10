using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace WTG.Serialization.DataScience.Audit
{
	public static class AuditMessageJsonSerializerFactory
	{
		public const string MessageFormatVersion = "1.0.0";

		public static JsonSerializer CreateJsonSerializer() => JsonSerializer.Create(CreateJsonSerializerSettings());

		public static JsonSerializerSettings CreateJsonSerializerSettings() => new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new CamelCaseNamingStrategy(),
			},
			Converters = new[] { new StringEnumConverter() }
				.Concat(JsonExtensions.GetAuditMessageJsonConverters(MessageFormatVersion))
				.ToList(),
			DateParseHandling = DateParseHandling.None,
			Formatting = Formatting.None,
		};
	}
}
