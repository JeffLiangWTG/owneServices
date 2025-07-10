using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public static class ESNctsTestFileReader
	{
		public static string GetEmbeddedFileText(string embeddedResourcePath, string fileName)
		{
			var fileReader = new TestFileReader(typeof(ESNctsTestFileReader));
			return fileReader.GetEmbeddedFileText(embeddedResourcePath, fileName);
		}
	}
}
