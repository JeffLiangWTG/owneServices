using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public static class ESExitControlTestFileReader
	{
		public static string GetEmbeddedFileText(string embeddedResourcePath, string fileName)
		{
			var fileReader = new TestFileReader(typeof(ESExitControlTestFileReader));
			return fileReader.GetEmbeddedFileText(embeddedResourcePath, fileName);
		}
	}
}
