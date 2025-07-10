using Newtonsoft.Json.Converters;

namespace Enterprise.DocumentScanning.Integration
{
	public enum ShipamaxEDocsChangeType
	{
		DocType = 1,
		DocFormat,
		DocEditTime,
		ParsingEnabled,
		Deleted,
		DocumentPK
	}

	public class ShipamaxEDocsChange
	{
		[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
		public ShipamaxEDocsChangeType Field { get; set; }

		public object OldValue { get; set; }

		public object NewValue { get; set; }
	}
}
