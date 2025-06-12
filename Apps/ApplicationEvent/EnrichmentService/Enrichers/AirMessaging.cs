using System.Text.RegularExpressions;

namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public partial class AirMessaging(IConfiguration configuration) : IEnricherSingle
{
	private readonly IConfiguration configuration = configuration;

	public Task<JsonObject> EnrichMessageSingleAsync(JsonObject messageEvent, string criteria, CancellationToken cancellationToken = default)
	{
		var enrichedData = new JsonObject();
		var regex = AggregateMessageInformationFormat ??= CreateAggregateMessageInformationFormat(configuration);
		var match = regex.Match(messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["FileName"]?.ToString() ?? "");
		foreach (var name in FieldNames.Where(f => match.Groups[f].Success && match.Groups[f].Length > 0))
		{
			enrichedData[name] = match.Groups[name].Value;
		}
		if (messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["EmailSubject"]?.ToString() is string email)
			enrichedData["SenderPIMA"] = PimaWithTrailingZeroRegex().IsMatch(email) ? email[..^1] : email;

		return Task.FromResult(enrichedData);
	}

	private static Regex CreateAggregateMessageInformationFormat(IConfiguration configuration)
	{
		lock (Lock_AggregateMessageInformationFormat)
		{
			return AggregateMessageInformationFormat
				??= new Regex(configuration.GetValue<string>("Enrichers:Air:AggregateMessageInformationFormat")!, RegexOptions.Compiled);
		}
	}
	private static Regex AggregateMessageInformationFormat = null!;
	private static readonly object Lock_AggregateMessageInformationFormat = new();
	private readonly string[] FieldNames = ["MessageType", "MAWB", "HAWB", "Carrier", "RecipientPIMA"];

	[GeneratedRegex(@"/[A-Z]{3}[0-9]{2}0$")]
	private static partial Regex PimaWithTrailingZeroRegex();
}
