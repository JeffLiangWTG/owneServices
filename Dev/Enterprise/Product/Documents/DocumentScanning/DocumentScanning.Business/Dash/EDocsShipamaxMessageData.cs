using System.Text.Json.Serialization;

namespace Enterprise.DocumentScanning.Business
{
	public class EDocsShipamaxMessageData
	{
		[JsonConstructor]
		public EDocsShipamaxMessageData(string parseResultXml, string parseDataJson)
		{
			ParseResultXml = parseResultXml;
			ParseDataJson = parseDataJson;
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string ParseResultXml { get; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string ParseDataJson { get; }
	}
}
