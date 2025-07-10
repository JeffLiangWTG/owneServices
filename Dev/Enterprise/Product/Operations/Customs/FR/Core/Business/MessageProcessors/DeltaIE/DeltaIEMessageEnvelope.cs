using System.Text.Json.Serialization;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEMessageEnvelope
	{
		public string SchemaId { get; set; } = string.Empty;
		public string TransactionId { get; set; } = string.Empty;
		[JsonConverter(typeof(RawJsonStringConverter))]
		public string MessageJson { get; set; }
	}
}
