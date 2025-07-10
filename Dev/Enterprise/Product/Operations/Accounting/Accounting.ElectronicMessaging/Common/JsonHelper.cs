using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using Json.Pointer;
using Json.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using WTG.StaticAnalysis.Annotation;
using JsonSerialization = Newtonsoft.Json.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class JsonHelper
	{
		public static bool ValidateJSON(this JSchema schema, string jsonText, INotifications notifications)
		{
			var result = false;
			try
			{
				var jobject = JObject.Parse(jsonText);
				result = jobject.IsValid(schema, out IList<string> messages);
				messages?.ToList().ForEach(m => notifications.AddError(m));
			}
			catch (JsonSerializationException ex)
			{
				notifications.AddError(ex.Message);
			}

			return result;
		}

		[CodeAlive("Production usage chunked into future workflow")]
		public static bool ValidateJSON(this Json.Schema.JsonSchema schema, string jsonText, INotifications notifications)
		{
			JsonDocument jsonDoc;
			try
			{
				jsonDoc = JsonDocument.Parse(jsonText ?? string.Empty);
			}
			catch (System.Text.Json.JsonException ex)
			{
				notifications.AddError(ex.Message);
				return false;
			}

			var evaluationResult = schema.Evaluate(jsonDoc, new EvaluationOptions() { OutputFormat = OutputFormat.List });
			var errors = evaluationResult.Details
							.Where(x => x.HasErrors)
							.SelectMany(x => x.Errors, (x, xs) => x.EvaluationPath.ToHumanReadablePath() + ": " + xs.Value);
			foreach (var e in errors)
			{
				notifications.AddError(e);
			}
			return !errors.Any();
		}

		public static JsonDeserializationResult<T> TryDeserialize<T>(this JSchema schema, string jsonText)
		{
			T result = default;

			var isValid = false;
			var notifications = new NotificationBuffer();

			try
			{
				isValid = schema.ValidateJSON(jsonText, notifications);
				if (isValid)
				{
					var settings = new JsonSerializerSettings
					{
						Error = (object sender, JsonSerialization.ErrorEventArgs args) =>
						{
							notifications.AddError(args.ErrorContext.Error.Message);
							args.ErrorContext.Handled = true;
							isValid = false;
						}
					};
					result = JsonConvert.DeserializeObject<T>(jsonText, settings);
					if (!isValid)
					{
						result = default;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddError(ex.Message);
			}

			return new JsonDeserializationResult<T> { IsSuccessful = isValid, DeserializedObject = result, ErrorMessage = notifications.AsString };
		}

		public class JsonDeserializationResult<T>
		{
			public bool IsSuccessful { get; set; }

			public string ErrorMessage { get; set; }

			public T DeserializedObject { get; set; }
		}
	}

	static class JsonPointerExtensions
	{
		internal static string ToHumanReadablePath(this JsonPointer path)
		{
			var asString = path.ToString();
			if (string.IsNullOrEmpty(asString))
			{
				return "/";
			}
			return asString.Replace("/properties", "");
		}
	}
}
