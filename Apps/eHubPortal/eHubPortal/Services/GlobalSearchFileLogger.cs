using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using log4net;

namespace eServices.eHubPortal.Services;

public sealed class GlobalSearchFileLogger(string name, ILog logger, IExternalScopeProvider? scopeProvider) : ILogger
{
	private readonly string name = name;
	private readonly ILog logger = logger;

	internal IExternalScopeProvider? ScopeProvider { get; set; } = scopeProvider;
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => ScopeProvider?.Push(state) ?? default!;

	public void Log<TState>(
	   LogLevel logLevel,
	   EventId eventId,
	   TState state,
	   Exception? exception,
	   Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		var jsonMessage = new JsonObject();
		jsonMessage["@timestamp"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
		jsonMessage["CategoryName"] = name;
		jsonMessage["Severity"] = logLevel switch
		{
			LogLevel.Trace => "Trace",
			LogLevel.Debug => "Debug",
			LogLevel.Information => "Info",
			LogLevel.Warning => "Warn",
			LogLevel.Error => "Error",
			LogLevel.Critical => "Fatal",
			_ => "Info"
		};
		var body = formatter(state, exception);
		if (body.Length <= 1024)
		{
			jsonMessage["Body"] = body;
		}
		else
		{
			jsonMessage["Body"] = body[..1024];
			jsonMessage["Attributes"] = new JsonObject();
			jsonMessage["Attributes"]!["OriginalMessage"] = body;
		}

		if (state is IEnumerable<KeyValuePair<string, object?>> attributes)
		{
			jsonMessage["Attributes"] ??= new JsonObject();
			foreach (var attribute in attributes)
			{
				jsonMessage["Attributes"]![attribute.Key.Trim('{', '}')] = attribute.Value?.ToString();
			}
		}

		if (exception is not null)
			jsonMessage["Exception"] = exception.ToString();

		var jsonScopeValues = new JsonObject();
		var scopeId = 0;
		ScopeProvider?.ForEachScope((scope, state) =>
		{
			if (scope is IEnumerable<KeyValuePair<string, object?>> scopeProperties)
			{
				var jsonScopeProperties = new JsonObject();
				foreach (var property in scopeProperties)
				{
					jsonScopeValues[property.Key] = property.Value?.ToString();
				}
			}
			else
			{
				jsonScopeValues[$"Scope.{scopeId}"] = scope?.ToString();
			}
			scopeId++;
		}, jsonScopeValues);
		if (scopeId > 0)
			jsonMessage["ScopeValues"] = jsonScopeValues;

		var jsonOptions = new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		logger.Info(jsonMessage.ToJsonString(jsonOptions));
	}


	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
}