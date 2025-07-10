namespace XMLSchemaExtractor
{
	class Program
	{
		static void Main(string[] args)
		{
			var extractor = new XMLExtractor();
			extractor.GenerateUniversalXmlSchemas();
			extractor.GenerateNativeXmlSchemas();
		}
	}
}
