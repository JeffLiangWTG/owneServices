using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public static class ProblemDetailsSerializer
	{
		public static string Serialize(this ProblemDetails details)
			=> JsonConvert.SerializeObject(details, SerializerSettings);

		static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new CamelCaseNamingStrategy
				{
					ProcessDictionaryKeys = true,
				},
			},
			NullValueHandling = NullValueHandling.Ignore,
		};
	}
}
