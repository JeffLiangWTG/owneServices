
namespace BusinessObjectSecurityCheckpointExtractor
{
	class Program
	{
		static void Main(string[] args)
		{
			var extractor = new CheckpointExtractor();
			var businessObjectCheckpoints = Utility.CleanDictionary(extractor.GenerateBusinessObjectSecurityCheckpoints());
			Utility.WriteDictionaryToFile(businessObjectCheckpoints, extractor.OutputFilename);
		}
	}
}
