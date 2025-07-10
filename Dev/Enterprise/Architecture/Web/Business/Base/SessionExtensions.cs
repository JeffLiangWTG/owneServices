#if NET
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class SessionExtensions
	{
#nullable enable
		static JsonSerializerOptions CreateJsonParsingOptions()
		{
			var options = new JsonSerializerOptions()
			{
				IncludeFields = true,
				PropertyNameCaseInsensitive = true
			};
			options.Converters.Add(new OrgHeaderMinimalJsonConverter());
			options.Converters.Add(new WebUserPolymorphicJsonConverter());
			options.Converters.Add(new ZStringJsonConverter());
			options.Converters.Add(new ZGuidJsonConverter());
			options.Converters.Add(new ZBoolJsonConverter());
			options.Converters.Add(new ZDateTimeJsonConverter());
			options.Converters.Add(new MultilingualStringJsonConverter());
			options.Converters.Add(new OrgContactMinimalJsonConverter());
			options.Converters.Add(new WebContactMinimalJsonConverter());
			return options;
		}

		public static void SetObject<T>(this ISession session, string key, T value)
		{
			session.Remove(key);
			var json = JsonSerializer.Serialize(value, CreateJsonParsingOptions());
			session.SetString(key, json);
		}

		public static T? GetObject<T>(this ISession session, string key)
		{
			var json = session.GetString(key);
			return json == null ? default : JsonSerializer.Deserialize<T>(json, CreateJsonParsingOptions());
		}
#nullable restore
	}
}
#endif
