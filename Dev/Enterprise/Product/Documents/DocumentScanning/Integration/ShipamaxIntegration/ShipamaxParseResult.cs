namespace Enterprise.DocumentScanning.Integration
{
	public class ShipamaxParseResult
	{
		public ShipamaxParseStatus ParseStatus { get; set; }

		public string XmlParseResult { get; set; }

		public string JsonParseResult { get; set; }
	}
}
